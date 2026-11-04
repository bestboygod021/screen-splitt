// FILE_ID: SVC-001 | FILE_PATH: ScreenSplitt/Services/WindowEnumerator.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ScreenSplitt.Models;
using ScreenSplitt.Native;

namespace ScreenSplitt.Services;

/// <summary>
/// سرویس لیست کردن تمام پنجره‌های قابل نمایش باز در سیستم
/// </summary>
public static class WindowEnumerator
{
    public static List<OpenWindowInfo> GetAllOpenWindows()
    {
        var windows = new List<OpenWindowInfo>();
        var ownProcessId = (uint)Process.GetCurrentProcess().Id;

        Win32Api.EnumWindows((hWnd, lParam) =>
        {
            // فقط پنجره‌های قابل مشاهده را در نظر بگیر
            if (!Win32Api.IsWindowVisible(hWnd))
                return true;

            // پنجره‌های بدون عنوان را رد کن
            int length = Win32Api.GetWindowTextLength(hWnd);
            if (length == 0)
                return true;

            // عنوان پنجره را بخوان
            var stringBuilder = new StringBuilder(length + 1);
            Win32Api.GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
            string title = stringBuilder.ToString().Trim();
            
            if (string.IsNullOrWhiteSpace(title))
                return true;

            // شناسه پردازه را دریافت کن
            Win32Api.GetWindowThreadProcessId(hWnd, out uint processId);
            
            // پنجره خود برنامه را از لیست حذف کن
            if (processId == ownProcessId)
                return true;

            // پنجره‌های سیستمی خاص که معمولا خالی هستند را رد کن
            if (title == "Program Manager" || title == "Windows Input Experience")
                return true;

            windows.Add(new OpenWindowInfo
            {
                Handle = hWnd,
                Title = title,
                ProcessId = processId
            });

            return true;
        }, IntPtr.Zero);

        return windows;
    }
}
