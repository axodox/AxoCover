// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <vector>
#pragma comment (lib, "User32.lib")
#include "easyhook.h"

#if _WIN64
#pragma comment (lib, "EasyHook64.lib")
#else
#pragma comment (lib, "EasyHook32.lib")
#endif
