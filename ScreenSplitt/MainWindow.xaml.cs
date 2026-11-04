// FILE_ID: WIN-002 | FILE_PATH: ScreenSplitt/MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ScreenSplitt.Controls;
using ScreenSplitt.Models;
using ScreenSplitt.Native;
using ScreenSplitt.Views;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace ScreenSplitt;

/// <summary>
/// پنجره اصلی نرم‌افزار
/// </summary>
public partial class MainWindow : UiWindow
{
    private bool _isMaximized;
    private LayoutType _currentLayout = LayoutType.SinglePane;
    private readonly List<HostedWindowContainer> _allPanes = new();
    private readonly SettingsService _settingsService = new();

    public MainWindow()
    {
        InitializeComponent();

        // فعال کردن حالت همیشه در بالا و تنظیمات اولیه
        Loaded += (s, e) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetAlwaysOnTop(hwnd);
            
            // اعمال تم ذخیره شده
            if (!_settingsService.Settings.UseDarkTheme)
            {
                ThemeManager.Apply(ThemeType.Light);
            }
            
            // فعال کردن افکت مدرن میکا برای ویندوز 11 (فقط در WPF-UI 2.x)
            try
            {
                var backdrop = new WindowBackdrop(this, Wpf.Ui.Appearance.BackdropType.Mica);
                backdrop.Apply();
            }
            catch
            {
                // اگر سیستم‌عامل از میکا پشتیبانی نمی‌کند یا نسخه کتابخانه متفاوت است، چشم پوشی کن
            }
            
            InitializePanesList();
            ApplyLayout((LayoutType)_settingsService.Settings.LastUsedLayout);
        };

        // آزاد کردن تمام پنجره‌های میهمان هنگام بستن برنامه
        Closed += (s, e) =>
        {
            foreach (var pane in _allPanes)
            {
                pane.ClearHostedWindow();
            }
        };
    }

    /// <summary>
    /// مقداردهی اولیه لیست تمام پنل‌های موجود
    /// </summary>
    private void InitializePanesList()
    {
        _allPanes.Clear();
        _allPanes.Add(Pane1);
        _allPanes.Add(PaneVLeft);
        _allPanes.Add(PaneVRight);
        _allPanes.Add(PaneHTop);
        _allPanes.Add(PaneHBottom);
        _allPanes.Add(PaneTopLeft);
        _allPanes.Add(PaneTopRight);
        _allPanes.Add(PaneBottomLeft);
        _allPanes.Add(PaneBottomRight);

        // اتصال رویدادهای تمام پنل‌ها
        foreach (var pane in _allPanes)
        {
            pane.AddWindowRequested += (s, e) =>
            {
                if (s is HostedWindowContainer clickedPane)
                {
                    OpenSelectWindowDialog(clickedPane);
                }
            };

            pane.CloseWindowRequested += (s, e) =>
            {
                if (s is HostedWindowContainer closedPane)
                {
                    closedPane.ClearHostedWindow();
                }
            };
        }
    }

    /// <summary>
    /// اعمال چیدمان جدید
    /// </summary>
    private void ApplyLayout(LayoutType newLayout)
    {
        // آزاد کردن تمام پنجره‌های قبلی هنگام تغییر چیدمان
        foreach (var pane in _allPanes)
        {
            pane.ClearHostedWindow();
        }

        // مخفی کردن همه کانتینرها
        Pane1.Visibility = Visibility.Collapsed;
        VerticalSplitContainer.Visibility = Visibility.Collapsed;
        HorizontalSplitContainer.Visibility = Visibility.Collapsed;
        Grid4Container.Visibility = Visibility.Collapsed;

        _currentLayout = newLayout;
        _settingsService.Settings.LastUsedLayout = (int)newLayout;
        _settingsService.SaveSettings();

        // به روز رسانی وضعیت فعال دکمه‌ها
        UpdateLayoutButtonsState();

        // ریست کردن تقسیم‌بندی‌ها
        VerticalSplitContainer.ResetSplit();
        HorizontalSplitContainer.ResetSplit();

        // مخفی کردن پیام خوش آمدگویی
        EmptyStatePanel.Visibility = Visibility.Collapsed;

        switch (newLayout)
        {
            case LayoutType.SinglePane:
                Pane1.Visibility = Visibility.Visible;
                break;
            case LayoutType.SplitVertical:
                VerticalSplitContainer.Visibility = Visibility.Visible;
                break;
            case LayoutType.SplitHorizontal:
                HorizontalSplitContainer.Visibility = Visibility.Visible;
                break;
            case LayoutType.Grid4:
                Grid4Container.Visibility = Visibility.Visible;
                break;
        }
    }

    /// <summary>
    /// باز کردن پنجره تنظیمات
    /// </summary>
    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_settingsService)
        {
            Owner = this
        };
        settingsWindow.ShowDialog();
    }

    /// <summary>
    /// به روز رسانی ظاهر دکمه‌های چیدمان
    /// </summary>
    private void UpdateLayoutButtonsState()
    {
        // در این نسخه برای سادگی، حالت پیش‌فرض دکمه‌ها حفظ می‌شود
        LayoutSingleButton.Opacity = _currentLayout == LayoutType.SinglePane ? 1 : 0.6;
        LayoutVerticalButton.Opacity = _currentLayout == LayoutType.SplitVertical ? 1 : 0.6;
        LayoutHorizontalButton.Opacity = _currentLayout == LayoutType.SplitHorizontal ? 1 : 0.6;
        LayoutGridButton.Opacity = _currentLayout == LayoutType.Grid4 ? 1 : 0.6;
    }



    /// <summary>
    /// باز کردن دیالوگ انتخاب پنجره برای یک پنل خاص
    /// </summary>
    private void OpenSelectWindowDialog(HostedWindowContainer targetPane)
    {
        // اگر این پنل قبلاً پنجره‌ای دارد، اول آن را رها کن
        if (targetPane.HasHostedWindow)
        {
            targetPane.ClearHostedWindow();
        }

        var selectDialog = new SelectWindowDialog
        {
            Owner = this
        };

        var result = selectDialog.ShowDialog();
        if (result == true && selectDialog.SelectedWindow != null)
        {
            targetPane.HostedWindow = selectDialog.SelectedWindow;
        }
    }

    /// <summary>
    /// دکمه اضافه کردن پنجره از هدر (برای حالت تک پنجره)
    /// </summary>
    private void AddWindowButton_OnClick(object sender, RoutedEventArgs e)
    {
        HostedWindowContainer targetPane = _currentLayout switch
        {
            LayoutType.SinglePane => Pane1,
            LayoutType.SplitVertical => PaneVLeft,
            LayoutType.SplitHorizontal => PaneHTop,
            LayoutType.Grid4 => PaneTopLeft,
            _ => Pane1
        };
        
        OpenSelectWindowDialog(targetPane);
    }

    // دکمه‌های تغییر چیدمان
    private void LayoutSingleButton_OnClick(object sender, RoutedEventArgs e) => ApplyLayout(LayoutType.SinglePane);
    private void LayoutVerticalButton_OnClick(object sender, RoutedEventArgs e) => ApplyLayout(LayoutType.SplitVertical);
    private void LayoutHorizontalButton_OnClick(object sender, RoutedEventArgs e) => ApplyLayout(LayoutType.SplitHorizontal);
    private void LayoutGridButton_OnClick(object sender, RoutedEventArgs e) => ApplyLayout(LayoutType.Grid4);

    /// <summary>
    /// فعال کردن قابلیت همیشه در بالاترین سطح برای پنجره
    /// </summary>
    private void SetAlwaysOnTop(IntPtr hWnd)
    {
        Win32Api.SetWindowPos(hWnd,
            Win32Api.HWND_TOPMOST,
            0, 0, 0, 0,
            Win32Api.SWP_NOMOVE | Win32Api.SWP_NOSIZE | Win32Api.SWP_SHOWWINDOW);
    }

    /// <summary>
    /// جابجایی پنجره با کشیدن هدر
    /// </summary>
    private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            Win32Api.ReleaseCapture();
            Win32Api.SendMessage(new WindowInteropHelper(this).Handle, 
                Win32Api.WM_NCLBUTTONDOWN, new IntPtr(Win32Api.HT_CAPTION), IntPtr.Zero);
        }
    }

    /// <summary>
    /// بزرگ کردن/برگرداندن پنجره با دابل کلیک روی هدر
    /// </summary>
    private void TitleBar_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        ToggleMaximize();
    }

    /// <summary>
    /// دکمه کوچک کردن پنجره
    /// </summary>
    private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// دکمه بزرگ کردن/تمام صفحه
    /// </summary>
    private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    /// <summary>
    /// دکمه بستن پنجره
    /// </summary>
    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// منطق تغییر حالت بین تمام صفحه و عادی
    /// </summary>
    private void ToggleMaximize()
    {
        if (_isMaximized)
        {
            WindowState = WindowState.Normal;
            WindowBorder.CornerRadius = new CornerRadius(8);
            WindowBorder.BorderThickness = new Thickness(1);
            _isMaximized = false;
            MaximizeButton.Icon = Wpf.Ui.Common.SymbolRegular.Square16;
        }
        else
        {
            WindowState = WindowState.Maximized;
            WindowBorder.CornerRadius = new CornerRadius(0);
            WindowBorder.BorderThickness = new Thickness(0);
            _isMaximized = true;
            MaximizeButton.Icon = Wpf.Ui.Common.SymbolRegular.SquareMultiple16;
        }
    }
}
