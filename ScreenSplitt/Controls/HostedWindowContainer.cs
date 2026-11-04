// FILE_ID: CTRL-002 | FILE_PATH: ScreenSplitt/Controls/HostedWindowContainer.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using ScreenSplitt.Models;
using ScreenSplitt.Native;

namespace ScreenSplitt.Controls;

/// <summary>
/// کانتینر میزبانی یک پنجره خارجی
/// </summary>
public class HostedWindowContainer : Border
{
    private const int PaneHeaderHeight = 32;
    
    private IntPtr _hostedHwnd = IntPtr.Zero;
    private int _originalWindowStyle = 0;
    private readonly Grid _rootGrid;
    private readonly Border _addButtonOverlay;
    private readonly Border _paneHeader;
    private readonly TextBlock _windowTitleTextBlock;
    
    public static readonly DependencyProperty HostedWindowProperty = DependencyProperty.Register(
        nameof(HostedWindow), typeof(OpenWindowInfo), typeof(HostedWindowContainer),
        new PropertyMetadata(null, OnHostedWindowChanged));

    public OpenWindowInfo? HostedWindow
    {
        get => (OpenWindowInfo)GetValue(HostedWindowProperty);
        set => SetValue(HostedWindowProperty, value);
    }

    /// <summary>
    /// رویداد درخواست اضافه کردن پنجره
    /// </summary>
    public event RoutedEventHandler? AddWindowRequested;
    
    /// <summary>
    /// رویداد درخواست بستن پنجره
    /// </summary>
    public event RoutedEventHandler? CloseWindowRequested;

    private static void OnHostedWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HostedWindowContainer container)
        {
            if (e.OldValue is OpenWindowInfo oldWindow)
            {
                container.DetachWindow(oldWindow.Handle);
            }
            
            if (e.NewValue is OpenWindowInfo newWindow)
            {
                container.AttachWindow(newWindow.Handle);
                container._windowTitleTextBlock.Text = newWindow.Title;
            }
            
            container.UpdateOverlayState();
        }
    }

    public HostedWindowContainer()
    {
        Background = new SolidColorBrush(Color.FromArgb(15, 128, 128, 128));
        BorderBrush = new SolidColorBrush(Color.FromArgb(30, 128, 128, 128));
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(4);
        ClipToBounds = true;
        
        _rootGrid = new Grid();
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(PaneHeaderHeight) });
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        
        // هدر اختصاصی پنل
        _paneHeader = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)),
            VerticalAlignment = VerticalAlignment.Top,
            Height = PaneHeaderHeight,
            Padding = new Thickness(8, 0, 4, 0),
            Visibility = Visibility.Collapsed
        };

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _windowTitleTextBlock = new TextBlock
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetColumn(_windowTitleTextBlock, 0);
        headerGrid.Children.Add(_windowTitleTextBlock);

        // دکمه بستن پنجره
        var closeButton = new Button
        {
            Width = 24,
            Height = 24,
            Content = "✕",
            FontSize = 10,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center
        };
        closeButton.Click += (s, e) => 
        {
            CloseWindowRequested?.Invoke(this, new RoutedEventArgs());
            ClearHostedWindow();
        };
        Grid.SetColumn(closeButton, 1);
        headerGrid.Children.Add(closeButton);

        _paneHeader.Child = headerGrid;
        Grid.SetRow(_paneHeader, 0);
        _rootGrid.Children.Add(_paneHeader);
        
        // لایه خوش‌آمدگویی و دکمه اضافه کردن
        _addButtonOverlay = new Border
        {
            Background = Brushes.Transparent,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var addButtonStack = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        var addIcon = new System.Windows.Shapes.Path
        {
            Width = 24,
            Height = 24,
            Data = Geometry.Parse("M12 4V20M20 12H4"),
            Stroke = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)),
            StrokeThickness = 2,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        
        var addButton = new Button
        {
            Content = "اضافه کردن پنجره",
            HorizontalAlignment = HorizontalAlignment.Center,
            Style = (Style)FindResource("UiButtonSecondary") ?? Application.Current?.FindResource("UiButton"),
            Opacity = 0.8
        };
        addButton.Click += (s, e) => AddWindowRequested?.Invoke(this, new RoutedEventArgs());
        
        addButtonStack.Children.Add(addIcon);
        addButtonStack.Children.Add(addButton);
        _addButtonOverlay.Child = addButtonStack;
        
        Grid.SetRow(_addButtonOverlay, 1);
        Grid.SetRowSpan(_addButtonOverlay, 2);
        _rootGrid.Children.Add(_addButtonOverlay);
        
        Child = _rootGrid;
        
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
        MouseLeftButtonDown += (s, e) =>
        {
            if (!HasHostedWindow)
            {
                AddWindowRequested?.Invoke(this, new RoutedEventArgs());
            }
        };
        
        UpdateOverlayState();
    }

    private void UpdateOverlayState()
    {
        bool hasWindow = _hostedHwnd != IntPtr.Zero;
        _addButtonOverlay.Visibility = hasWindow ? Visibility.Collapsed : Visibility.Visible;
        _paneHeader.Visibility = hasWindow ? Visibility.Visible : Visibility.Collapsed;
        Background = hasWindow ? Brushes.Transparent : new SolidColorBrush(Color.FromArgb(15, 128, 128, 128));
        BorderThickness = hasWindow ? new Thickness(0) : new Thickness(1);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (HostedWindow != null)
        {
            AttachWindow(HostedWindow.Handle);
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_hostedHwnd != IntPtr.Zero)
        {
            UpdateHostedWindowPosition();
        }
    }

    /// <summary>
    /// الحاق پنجره خارجی به این کانتینر
    /// </summary>
    private void AttachWindow(IntPtr hwndToHost)
    {
        if (hwndToHost == IntPtr.Zero) return;

        _hostedHwnd = hwndToHost;
        
        // گرفتن هندل میزبان WPF ما
        var hostHwndSource = (HwndSource)PresentationSource.FromVisual(this);
        if (hostHwndSource == null) return;
        
        IntPtr hostHandle = hostHwndSource.Handle;

        // ذخیره استایل اصلی پنجره تا موقع جداسازی برگردانیم
        _originalWindowStyle = Win32Api.GetWindowLong(hwndToHost, Win32Api.GWL_STYLE);

        // تغییر والد پنجره به کانتینر ما
        Win32Api.SetParent(hwndToHost, hostHandle);

        // حذف نوار عنوان و حاشیه‌های پنجره مهمان
        int newStyle = _originalWindowStyle;
        newStyle &= ~(Win32Api.WS_CAPTION | Win32Api.WS_THICKFRAME | Win32Api.WS_MINIMIZEBOX | Win32Api.WS_MAXIMIZEBOX | Win32Api.WS_POPUP);
        newStyle |= Win32Api.WS_CHILD | Win32Api.WS_VISIBLE;
        
        Win32Api.SetWindowLong(hwndToHost, Win32Api.GWL_STYLE, newStyle);

        // تنظیم اندازه و موقعیت اولیه
        UpdateHostedWindowPosition();

        // به روز رسانی نمایش
        Win32Api.UpdateWindow(hwndToHost);
    }

    /// <summary>
    /// به روز رسانی موقعیت و سایز پنجره میهمان متناسب با کانتینر
    /// </summary>
    private void UpdateHostedWindowPosition()
    {
        if (_hostedHwnd == IntPtr.Zero) return;

        // محاسبه سایز بر اساس DPI صفحه
        var source = PresentationSource.FromVisual(this);
        if (source == null) return;
        
        double dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
        double dpiScaleY = source.CompositionTarget.TransformToDevice.M22;

        int width = (int)(ActualWidth * dpiScaleX);
        int headerHeight = (int)(PaneHeaderHeight * dpiScaleY);
        int height = (int)((ActualHeight - PaneHeaderHeight) * dpiScaleY);

        // پنجره میهمان را زیر هدر قرار بده
        Win32Api.MoveWindow(_hostedHwnd, 0, headerHeight, width, height, true);
    }

    /// <summary>
    /// جداسازی پنجره از کانتینر (برگرداندن به والد اصلی)
    /// </summary>
    private void DetachWindow(IntPtr hwndToDetach)
    {
        if (hwndToDetach == IntPtr.Zero) return;

        // بازگرداندن استایل اصلی پنجره
        Win32Api.SetWindowLong(hwndToDetach, Win32Api.GWL_STYLE, _originalWindowStyle);
        
        // بازگرداندن والد به دسکتاپ
        Win32Api.SetParent(hwndToDetach, IntPtr.Zero);
        
        _hostedHwnd = IntPtr.Zero;
    }

    /// <summary>
    /// بررسی اینکه آیا این کانتینر هنوز پنجره‌ای میزبانی می‌کند
    /// </summary>
    public bool HasHostedWindow => _hostedHwnd != IntPtr.Zero;

    /// <summary>
    /// انتقال فوکوس ورودی به پنجره میهمان
    /// </summary>
    public void FocusHostedWindow()
    {
        if (_hostedHwnd == IntPtr.Zero) return;
        
        try
        {
            uint currentThread = Win32Api.GetCurrentThreadId();
            Win32Api.GetWindowThreadProcessId(_hostedHwnd, out uint targetThread);
            
            Win32Api.AttachThreadInput(currentThread, targetThread, true);
            Win32Api.SetForegroundWindow(_hostedHwnd);
            Win32Api.SendMessage(_hostedHwnd, Win32Api.WM_ACTIVATE, Win32Api.WA_ACTIVE, IntPtr.Zero);
            Win32Api.AttachThreadInput(currentThread, targetThread, false);
        }
        catch
        {
            // در صورت خطا در انتقال فوکوس چشم پوشی کن
        }
    }

    /// <summary>
    /// بستن و جداسازی پنجره میهمان
    /// </summary>
    public void ClearHostedWindow()
    {
        if (_hostedHwnd != IntPtr.Zero)
        {
            DetachWindow(_hostedHwnd);
        }
        HostedWindow = null;
    }
}
