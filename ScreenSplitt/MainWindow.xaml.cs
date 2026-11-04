// FILE_ID: WIN-002 | FILE_PATH: ScreenSplitt/MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ScreenSplitt.Controls;
using ScreenSplitt.Models;
using ScreenSplitt.Native;
using ScreenSplitt.Services;
using ScreenSplitt.Views;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace ScreenSplitt;

public partial class MainWindow : UiWindow
{
    private bool _isMaximized;
    private LayoutType _currentLayout = LayoutType.SinglePane;
    private string _currentCategory = "folder";
    private readonly List<HostedWindowContainer> _allPanes = new();
    private readonly SettingsService _settingsService = new();

    // ذخیره لیست پنجره‌های باز در هر دسته‌بندی
    private readonly Dictionary<string, List<OpenWindowInfo>> _categoryWindows = new()
    {
        {"folder", new List<OpenWindowInfo>()},
        {"note", new List<OpenWindowInfo>()},
        {"browser", new List<OpenWindowInfo>()},
        {"photo", new List<OpenWindowInfo>()}
    };

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetAlwaysOnTop(hwnd);

            try
            {
                if (_settingsService.Settings.UseDarkTheme)
                    Theme.Apply(ThemeType.Dark);
                else
                    Theme.Apply(ThemeType.Light);
            }
            catch { }

            InitializePanesList();
            ApplyLayout(LayoutType.SinglePane);
        };

        Closed += (s, e) =>
        {
            foreach (var pane in _allPanes)
                pane.ClearHostedWindow();
        };
    }

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

        foreach (var pane in _allPanes)
        {
            pane.AddWindowRequested += (s, e) =>
            {
                if (s is HostedWindowContainer clickedPane)
                    OpenSelectWindowDialog(clickedPane);
            };

            pane.CloseWindowRequested += (s, e) =>
            {
                if (s is HostedWindowContainer closedPane)
                    closedPane.ClearHostedWindow();
            };
        }
    }

    private void ApplyLayout(LayoutType newLayout)
    {
        foreach (var pane in _allPanes)
            pane.ClearHostedWindow();

        Pane1.Visibility = Visibility.Collapsed;
        VerticalSplitContainer.Visibility = Visibility.Collapsed;
        HorizontalSplitContainer.Visibility = Visibility.Collapsed;
        Grid4Container.Visibility = Visibility.Collapsed;
        EmptyStatePanel.Visibility = Visibility.Visible;

        _currentLayout = newLayout;
        _settingsService.Settings.LastUsedLayout = (int)newLayout;
        _settingsService.SaveSettings();

        VerticalSplitContainer.ResetSplit();
        HorizontalSplitContainer.ResetSplit();
        UpdateLayoutButtonOpacity();

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

    private void UpdateLayoutButtonOpacity()
    {
        LayoutSingleButton.Opacity = _currentLayout == LayoutType.SinglePane ? 1 : 0.5;
        LayoutVerticalButton.Opacity = _currentLayout == LayoutType.SplitVertical ? 1 : 0.5;
        LayoutHorizontalButton.Opacity = _currentLayout == LayoutType.SplitHorizontal ? 1 : 0.5;
        LayoutGridButton.Opacity = _currentLayout == LayoutType.Grid4 ? 1 : 0.5;
    }

    private void OpenSelectWindowDialog(HostedWindowContainer targetPane)
    {
        if (targetPane.HasHostedWindow)
            targetPane.ClearHostedWindow();

        var selectDialog = new SelectWindowDialog
        {
            Owner = this,
            CurrentCategoryFilter = _currentCategory
        };

        var result = selectDialog.ShowDialog();
        if (result == true && selectDialog.SelectedWindow != null)
        {
            targetPane.HostedWindow = selectDialog.SelectedWindow;
            EmptyStatePanel.Visibility = Visibility.Collapsed;
        }
    }

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

    /// <summary>
    /// تغییر دسته‌بندی فعال (فولدر/یادداشت/مرورگر/عکس)
    /// </summary>
    private void CategoryTab_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string category)
        {
            _currentCategory = category;
        }
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var sw = new SettingsWindow(_settingsService) { Owner = this };
        sw.ShowDialog();
    }

    // دکمه‌های چیدمان
    private void LayoutSingleButton_OnClick(object sender, RoutedEventArgs e) => ApplyLayout(LayoutType.SinglePane);
    private void LayoutVerticalButton_OnClick(object sender, RoutedEventArgs e) => ApplyLayout(LayoutType.SplitVertical);
    private void LayoutHorizontalButton_OnClick(object sender, RoutedEventArgs e) => ApplyLayout(LayoutType.SplitHorizontal);
    private void LayoutGridButton_OnClick(object sender, RoutedEventArgs e) => ApplyLayout(LayoutType.Grid4);

    // تنظیمات پنجره
    private void SetAlwaysOnTop(IntPtr hWnd)
    {
        Win32Api.SetWindowPos(hWnd, Win32Api.HWND_TOPMOST, 0, 0, 0, 0,
            Win32Api.SWP_NOMOVE | Win32Api.SWP_NOSIZE | Win32Api.SWP_SHOWWINDOW);
    }

    private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
                e.Handled = true;
                return;
            }
            Win32Api.ReleaseCapture();
            Win32Api.SendMessage(new WindowInteropHelper(this).Handle,
                Win32Api.WM_NCLBUTTONDOWN, new IntPtr(Win32Api.HT_CAPTION), IntPtr.Zero);
        }
    }

    private void MinimizeButton_OnClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeButton_OnClick(object sender, RoutedEventArgs e) => ToggleMaximize();
    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();

    private void ToggleMaximize()
    {
        if (_isMaximized)
        {
            WindowState = WindowState.Normal;
            WindowBorder.CornerRadius = new CornerRadius(12);
            WindowBorder.BorderThickness = new Thickness(1);
            _isMaximized = false;
            MaximizeIcon.Text = "▢";
        }
        else
        {
            WindowState = WindowState.Maximized;
            WindowBorder.CornerRadius = new CornerRadius(0);
            WindowBorder.BorderThickness = new Thickness(0);
            _isMaximized = true;
            MaximizeIcon.Text = "▣";
        }
    }
}
