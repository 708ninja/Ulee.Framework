//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : DLL Import Class
//------------------------------------------------------------------------------
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Ulee.DllImport.Win32
{
    public static class Win32
    {
        public const int WM_USER = 0x0400;

        [DllImport("kernel32")]
        public static extern void SwitchToThread();

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(
            string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(
            string section, string key, string val, string filePath);

        [DllImport("User32.dll")]
        public extern static bool PostMessage(
            IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
