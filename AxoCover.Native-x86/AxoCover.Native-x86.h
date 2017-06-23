// AxoCover.Runner.Native.h

#pragma once

#pragma unmanaged
LPCWSTR* _mappedPaths;
LPCWSTR _root;

BOOL FileExists(LPCWSTR filePath);
BOOL DirectoryExists(LPCWSTR dirPath);

void MapFile(LPCWSTR &filePath)
{
  if (filePath)
  {
    WCHAR dir[MAX_PATH];
    memset(dir, 0, sizeof(WCHAR) * MAX_PATH);
    auto fileName = wcsrchr(filePath, '\\');
    memcpy(dir, filePath, (BYTE*)fileName - (BYTE*)filePath);

    if (!DirectoryExists(dir) || (!FileExists(filePath) && _wcsicmp(dir, _root))) return;

    for (auto mappedPath = &_mappedPaths[0]; *mappedPath != nullptr; mappedPath++)
    {
      auto mappedName = wcsrchr(*mappedPath, '\\');
      if (_wcsicmp(fileName, mappedName) == 0 && _wcsicmp(filePath, *mappedPath) != 0)
      {
        filePath = *mappedPath;
        break;
      }
    }
  }
}

typedef HANDLE(WINAPI *CreateFileWCallback)(
  LPCWSTR lpFileName,
  DWORD dwDesiredAccess,
  DWORD dwShareMode,
  LPSECURITY_ATTRIBUTES lpSecurityAttributes,
  DWORD dwCreationDisposition,
  DWORD dwFlagsAndAttributes,
  HANDLE hTemplateFile
  );

CreateFileWCallback OriginalCreateFileW;

HANDLE WINAPI OnCreateFileW(
  LPCWSTR lpFileName,
  DWORD dwDesiredAccess,
  DWORD dwShareMode,
  LPSECURITY_ATTRIBUTES lpSecurityAttributes,
  DWORD dwCreationDisposition,
  DWORD dwFlagsAndAttributes,
  HANDLE hTemplateFile)
{
  MapFile(lpFileName);

  auto result = OriginalCreateFileW(
    lpFileName,
    dwDesiredAccess,
    dwShareMode,
    lpSecurityAttributes,
    dwCreationDisposition,
    dwFlagsAndAttributes,
    hTemplateFile);

  return result;
}

typedef DWORD (WINAPI *GetFileAttributesWCallback)(
  LPCWSTR lpFileName);

GetFileAttributesWCallback OriginalGetFileAttributesW;

DWORD WINAPI OnGetFileAttributesW(
  LPCWSTR lpFileName)
{
  MapFile(lpFileName);

  auto result = OriginalGetFileAttributesW(
    lpFileName);
  
  return result;
}

typedef BOOL (WINAPI *GetFileAttributesExWCallback)(
  LPCWSTR lpFileName,
  GET_FILEEX_INFO_LEVELS fInfoLevelId,
  LPVOID lpFileInformation);

GetFileAttributesExWCallback OriginalGetFileAttributesExW;

BOOL WINAPI OnGetFileAttributesExW(
  LPCWSTR lpFileName,
  GET_FILEEX_INFO_LEVELS fInfoLevelId,
  LPVOID lpFileInformation)
{
  MapFile(lpFileName);

  auto result = OriginalGetFileAttributesExW(
    lpFileName,
    fInfoLevelId,
    lpFileInformation);

  return result;
}

BOOL FileExists(LPCWSTR filePath)
{
  DWORD attribs = OriginalGetFileAttributesW(filePath);
  if (attribs == INVALID_FILE_ATTRIBUTES)
  {
    return false;
  }
  else
  {
    return !(attribs & FILE_ATTRIBUTE_DIRECTORY);
  }
}

BOOL DirectoryExists(LPCWSTR dirPath)
{
  auto attribs = OriginalGetFileAttributesW(dirPath);
  if (attribs == INVALID_FILE_ATTRIBUTES)
  {
    return false;
  }
  else
  {
    return (attribs & FILE_ATTRIBUTE_DIRECTORY);
  }
}

#pragma managed
using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace System::Collections::Concurrent;
using namespace System::Runtime::InteropServices;
using namespace System::Reflection;

namespace AxoCoverRunnerNative {

  public ref class FileRemapper
  {
  private:
    static bool _isHooking;
  public:
    static void RedirectFiles(IList<String^>^ mappedFiles)
    {
      auto root = Path::GetDirectoryName(Assembly::GetEntryAssembly()->Location);
      _root = (LPCWSTR)Marshal::StringToHGlobalUni(root).ToPointer();

      _mappedPaths = new LPCWSTR[mappedFiles->Count + 1];
      auto mappedPath = &_mappedPaths[0];
      for each (String^ mapping in mappedFiles)
      {
        *mappedPath++ = (LPCWSTR)Marshal::StringToHGlobalUni(mapping).ToPointer();
      }
      *mappedPath++ = nullptr;

      if (!_isHooking)
      {
        _isHooking = true;
        Hook(L"Kernel32.dll", "CreateFileW", OnCreateFileW, OriginalCreateFileW);
        Hook(L"Kernel32.dll", "GetFileAttributesW", OnGetFileAttributesW, OriginalGetFileAttributesW);
        Hook(L"Kernel32.dll", "GetFileAttributesExW", OnGetFileAttributesExW, OriginalGetFileAttributesExW);
      }
    }

    template <typename TCallback>
    static void Hook(LPCWSTR moduleName, LPCSTR procName, TCallback hook, TCallback &original)
    {
      //Locate function address to redirect
      auto moduleHandle = GetModuleHandle(moduleName);
      auto procAddress = GetProcAddress(moduleHandle, procName);

      //Prepare unconditional jump opcodes
      const int jmpLength = 6;
      BYTE jmpOpcodes[jmpLength] = { 0xE9, 0x90, 0x90, 0x90, 0x90, 0xC3 };
      auto jmpDistance = (DWORD)hook - (DWORD)procAddress - jmpLength + 1;

      //Unlock original method for reading and writing
      DWORD procProtectionMode, tempProtectionMode;
      VirtualProtect((LPVOID)procAddress, jmpLength, PAGE_EXECUTE_READWRITE, &procProtectionMode);

      //Back-up original opcodes
      const int backupLength = jmpLength;
      BYTE* backupOpcodes = new BYTE[backupLength + jmpLength];
      CopyMemory(backupOpcodes, procAddress, backupLength);
      CopyMemory(backupOpcodes + backupLength, jmpOpcodes, jmpLength);
      auto backDistance = (DWORD)procAddress - (DWORD)backupOpcodes - jmpLength + 1;
      CopyMemory(backupOpcodes + backupLength + 1, &backDistance, sizeof(backDistance));
      VirtualProtect((LPVOID)backupOpcodes, backupLength + jmpLength, PAGE_EXECUTE_READWRITE, &tempProtectionMode);
      original = (TCallback)backupOpcodes;

      //Override original opcodes with unconditional jump to callback
      CopyMemory(&jmpOpcodes[1], &jmpDistance, sizeof(jmpDistance));
      CopyMemory(procAddress, jmpOpcodes, jmpLength);

      //Restore memory protection
      VirtualProtect((LPVOID)procAddress, jmpLength, procProtectionMode, &tempProtectionMode);
    }
  };
}
