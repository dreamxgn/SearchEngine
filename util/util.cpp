// util.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include <Windows.h>
#include <stdlib.h>


typedef struct
{
	HWND hWnd;
	DWORD dwPid;
}WNDINFO;

BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam)
{
	WNDINFO* pInfo = (WNDINFO*)lParam;
	DWORD dwProcessId = 0;
	GetWindowThreadProcessId(hWnd, &dwProcessId);

	if (dwProcessId == pInfo->dwPid)
	{
		pInfo->hWnd = hWnd;
		return FALSE;
	}
	return TRUE;
}

HWND GetHwndByProcessId(DWORD dwProcessId)
{
	WNDINFO info = { 0 };
	info.hWnd = NULL;
	info.dwPid = dwProcessId;
	EnumWindows(EnumWindowsProc, (LPARAM)&info);
	return info.hWnd;
}

extern "C" _declspec(dllexport) BOOL HidePhantomjs(DWORD pid)
{
	HWND hwnd = GetHwndByProcessId(pid);
	if (hwnd == NULL)
	{
		return FALSE;
	}
	return ShowWindow(hwnd, SW_HIDE);
}