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

  auto result = CreateFileW(
    lpFileName,
    dwDesiredAccess,
    dwShareMode,
    lpSecurityAttributes,
    dwCreationDisposition,
    dwFlagsAndAttributes,
    hTemplateFile);

  return result;
}

DWORD WINAPI OnGetFileAttributesW(
  LPCWSTR lpFileName)
{
  MapFile(lpFileName);

  auto result = GetFileAttributesW(
    lpFileName);
  
  return result;
}

BOOL WINAPI OnGetFileAttributesExW(
  LPCWSTR lpFileName,
  GET_FILEEX_INFO_LEVELS fInfoLevelId,
  LPVOID lpFileInformation)
{
  MapFile(lpFileName);

  auto result = GetFileAttributesExW(
    lpFileName,
    fInfoLevelId,
    lpFileInformation);

  return result;
}

BOOL FileExists(LPCWSTR filePath)
{
  DWORD attribs = GetFileAttributesW(filePath);
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
  auto attribs = GetFileAttributesW(dirPath);
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
        EasyHook(L"Kernel32.dll", "CreateFileW", OnCreateFileW);
        EasyHook(L"Kernel32.dll", "GetFileAttributesW", OnGetFileAttributesW);
        EasyHook(L"Kernel32.dll", "GetFileAttributesExW", OnGetFileAttributesExW);
      }
    }

    template <typename TCallback>
    static void EasyHook(LPCWSTR moduleName, LPCSTR procName, TCallback hook)
    {
      auto moduleHandle = GetModuleHandle(moduleName);
      auto procAddress = GetProcAddress(moduleHandle, procName);

      HOOK_TRACE_INFO hookHandle = { 0 };
      NTSTATUS result = LhInstallHook(procAddress,
        hook,
        NULL,
        &hookHandle);
      if (FAILED(result))
      {
        Console::WriteLine("Failed to hook " + Marshal::PtrToStringUni(IntPtr((void*)procName)) + "!");
        return;
      }
      
      LhSetExclusiveACL(nullptr, 0, &hookHandle);
    }
  };
}
