// FILE_ID: CTRL-001 | FILE_PATH: ScreenSplitt/Controls/WindowResizer.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using ScreenSplitt.Native;

namespace ScreenSplitt.Controls;

/// <summary>
/// کنترل حاشیه پنجره برای تغییر سایز از یک جهت مشخص
/// </summary>
public class WindowResizer : Border
{
    public static readonly DependencyProperty ResizeDirectionProperty = DependencyProperty.Register(
        nameof(ResizeDirection), typeof(int), typeof(WindowResizer), new PropertyMetadata(Win32Api.HTBOTTOMRIGHT));

    public int ResizeDirection
    {
        get => (int)GetValue(ResizeDirectionProperty);
        set => SetValue(ResizeDirectionProperty, value);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Background = System.Windows.Media.Brushes.Transparent;
        
        // تنظیم مناسب کورسور موس بسته به جهت تغییر سایز
        Cursor = ResizeDirection switch
        {
            Win32Api.HTLEFT or Win32Api.HTRIGHT => Cursors.SizeWE,
            Win32Api.HTTOP or Win32Api.HTBOTTOM => Cursors.SizeNS,
            Win32Api.HTTOPLEFT or Win32Api.HTBOTTOMRIGHT => Cursors.SizeNWSE,
            Win32Api.HTTOPRIGHT or Win32Api.HTBOTTOMLEFT => Cursors.SizeNESW,
            _ => Cursors.Arrow
        };
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        
        var parentWindow = Window.GetWindow(this);
        if (parentWindow == null || parentWindow.WindowState == WindowState.Maximized) return;

        // شروع عملیات تغییر سایز با پیام‌های خود ویندوز
        Win32Api.ReleaseCapture();
        Win32Api.SendMessage(new WindowInteropHelper(parentWindow).Handle, 
            Win32Api.WM_NCLBUTTONDOWN, new IntPtr(ResizeDirection), IntPtr.Zero);
    }
}

