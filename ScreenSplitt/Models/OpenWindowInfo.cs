// FILE_ID: MOD-001 | FILE_PATH: ScreenSplitt/Models/OpenWindowInfo.cs
using System;

namespace ScreenSplitt.Models;

/// <summary>
/// مدل نمایش اطلاعات یک پنجره باز در سیستم
/// </summary>
public class OpenWindowInfo
{
    /// <summary>
    /// هندل پنجره (HWND)
    /// </summary>
    public IntPtr Handle { get; set; }
    
    /// <summary>
    /// عنوان پنجره
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// شناسه پردازه مالک پنجره
    /// </summary>
    public uint ProcessId { get; set; }

    public override string ToString()
    {
        return Title;
    }
}
