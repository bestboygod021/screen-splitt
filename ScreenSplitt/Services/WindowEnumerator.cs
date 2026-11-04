// FILE_ID: SVC-001 | FILE_PATH: ScreenSplitt/Services/WindowEnumerator.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ScreenSplitt.Models;
using ScreenSplitt.Native;

namespace ScreenSplitt.Services;

public static class WindowEnumerator
{
    // لیست پردازه‌های مربوط به هر دسته‌بندی
    private static readonly Dictionary<string, string[]> CategoryProcesses = new()
    {
        {"folder", new[] {"explorer"}},
        {"note", new[] {"notepad", "notepad++", "winword", "wordpad", "onenote", "obsidian", "code", "vscode"}},
        {"browser", new[] {"chrome", "msedge", "firefox", "opera", "brave", "iexplore", "safari"}},
        {"photo", new[] {"ms-photos", "photos", "paint", "mspaint", "irfanview", "xnview", "photoshop", "discord", "zoom"}}
    };

    public static List<OpenWindowInfo> GetAllOpenWindows(string? categoryFilter = null)
    {
        var windows = new List<OpenWindowInfo>();
        var ownProcessId = (uint)Process.GetCurrentProcess().Id;

        Win32Api.EnumWindows((hWnd, lParam) =>
        {
            if (!Win32Api.IsWindowVisible(hWnd))
                return true;

            int length = Win32Api.GetWindowTextLength(hWnd);
            if (length == 0)
                return true;

            var sb = new StringBuilder(length + 1);
            Win32Api.GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString().Trim();

            if (string.IsNullOrWhiteSpace(title))
                return true;

            Win32Api.GetWindowThreadProcessId(hWnd, out uint processId);

            if (processId == ownProcessId)
                return true;

            if (title == "Program Manager" || title == "Windows Input Experience" || title == "Setup" || title == "Default IME")
                return true;

            try
            {
                var proc = Process.GetProcessById((int)processId);
                string procName = proc.ProcessName.ToLowerInvariant();

                // اگر فیلتر دسته‌بندی داریم فقط پردازه‌های مربوطه را برگردان
                if (categoryFilter != null && CategoryProcesses.ContainsKey(categoryFilter))
                {
                    if (!CategoryProcesses[categoryFilter].Contains(procName))
                        return true;
                }

                windows.Add(new OpenWindowInfo
                {
                    Handle = hWnd,
                    Title = title,
                    ProcessId = processId,
                    ProcessName = proc.ProcessName
                });
            }
            catch
            {
                // در صورت عدم دسترسی به پردازه رد کن
            }

            return true;
        }, IntPtr.Zero);

        return windows.OrderBy(w => w.ProcessName).ThenBy(w => w.Title).ToList();
    }
}
