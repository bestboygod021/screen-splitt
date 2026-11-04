// FILE_ID: DLG-004 | FILE_PATH: ScreenSplitt/Views/SettingsWindow.xaml.cs
using System.Windows;
using ScreenSplitt.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace ScreenSplitt.Views;

public partial class SettingsWindow : UiWindow
{
    private readonly SettingsService _settingsService;
    private bool _initializing = true;

    public SettingsWindow(SettingsService settingsService)
    {
        _settingsService = settingsService;
        InitializeComponent();
        
        // مقداردهی اولیه سویچ‌ها بر اساس تنظیمات ذخیره شده
        AutoStartToggle.IsChecked = _settingsService.IsAutoStartEnabled();
        DarkThemeToggle.IsChecked = _settingsService.Settings.UseDarkTheme;
        _initializing = false;
    }

    private void AutoStartToggle_Checked(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        _settingsService.SetAutoStart(true);
    }

    private void AutoStartToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        _settingsService.SetAutoStart(false);
    }

    private void DarkThemeToggle_Checked(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        _settingsService.Settings.UseDarkTheme = true;
        _settingsService.SaveSettings();
        Theme.Apply(ThemeType.Dark);
    }

    private void DarkThemeToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        _settingsService.Settings.UseDarkTheme = false;
        _settingsService.SaveSettings();
        Theme.Apply(ThemeType.Light);
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
