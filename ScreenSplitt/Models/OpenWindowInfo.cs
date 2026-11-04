// FILE_ID: MOD-001 | FILE_PATH: ScreenSplitt/Models/OpenWindowInfo.cs
using System;

namespace ScreenSplitt.Models;

public class OpenWindowInfo
{
    public IntPtr Handle { get; set; }
    public string Title { get; set; } = string.Empty;
    public uint ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public override string ToString() => Title;
}
