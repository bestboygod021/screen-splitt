// FILE_ID: SVC-002 | FILE_PATH: ScreenSplitt/Services/SettingsService.cs
using System;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace ScreenSplitt.Services;

/// <summary>
/// سرویس مدیریت تنظیمات برنامه و ذخیره‌سازی پایدار
/// </summary>
public class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ScreenSplitt",
        "settings.json");

    public AppSettings Settings { get; private set; } = new AppSettings();

    public SettingsService()
    {
        LoadSettings();
    }

    /// <summary>
    /// بارگذاری تنظیمات از فایل
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            Settings = new AppSettings();
        }
    }

    /// <summary>
    /// ذخیره تنظیمات در فایل
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // در صورت خطا در ذخیره‌سازی، چشم پوشی کن
        }
    }

    /// <summary>
    /// تنظیم یا لغو شروع خودکار با ویندوز
    /// </summary>
    public void SetAutoStart(bool enable)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            
            if (key == null) return;

            if (enable)
            {
                string exePath = Environment.ProcessPath ?? AppDomain.CurrentDomain.BaseDirectory + "ScreenSplitt.exe";
                key.SetValue("ScreenSplitt", exePath);
            }
            else
            {
                if (key.GetValue("ScreenSplitt") != null)
                {
                    key.DeleteValue("ScreenSplitt");
                }
            }

            Settings.AutoStartEnabled = enable;
            SaveSettings();
        }
        catch
        {
            // در صورت عدم دسترسی رجیستری چشم پوشی کن
        }
    }

    /// <summary>
    /// بررسی وضعیت فعلی شروع خودکار
    /// </summary>
    public bool IsAutoStartEnabled()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            
            return key?.GetValue("ScreenSplitt") != null;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// مدل تنظیمات برنامه
/// </summary>
public class AppSettings
{
    public bool AutoStartEnabled { get; set; } = false;
    public bool UseDarkTheme { get; set; } = true;
    public int LastUsedLayout { get; set; } = 0;
}
