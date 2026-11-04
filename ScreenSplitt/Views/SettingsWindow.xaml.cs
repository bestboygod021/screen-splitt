// FILE_ID: DLG-004 | FILE_PATH: ScreenSplitt/Views/SettingsWindow.xaml.cs
using System.Windows;
using ScreenSplitt.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace ScreenSplitt.Views;

public partial class SettingsWindow : UiWindow
{
    private readonly SettingsService _settingsService;

    public SettingsWindow(SettingsService settingsService)
    {
        _settingsService = settingsService;
        InitializeComponent();
        
        // مقداردهی اولیه سویچ‌ها بر اساس تنظیمات ذخیره شده
        AutoStartToggle.IsOn = _settingsService.IsAutoStartEnabled();
        DarkThemeToggle.IsOn = _settingsService.Settings.UseDarkTheme;
    }

    private void AutoStartToggle_OnIsOnChanged(object sender, RoutedEventArgs e)
    {
        _settingsService.SetAutoStart(AutoStartToggle.IsOn);
    }

    private void DarkThemeToggle_OnIsOnChanged(object sender, RoutedEventArgs e)
    {
        _settingsService.Settings.UseDarkTheme = DarkThemeToggle.IsOn;
        _settingsService.SaveSettings();
        
        // اعمال تم فوری
        ThemeManager.Apply(DarkThemeToggle.IsOn ? ThemeType.Dark : ThemeType.Light);
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
