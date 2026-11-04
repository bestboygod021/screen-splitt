// FILE_ID: APP-002 | FILE_PATH: ScreenSplitt/App.xaml.cs
using System;
using System.Windows;
using System.Windows.Threading;

namespace ScreenSplitt;

/// <summary>
/// کلاس اصلی برنامه
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // هندل کردن تمام خطاهای زمان اجرا
        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show($"خطایی در اجرای برنامه رخ داد:\n\n{args.Exception.Message}\n\nجزئیات:\n{args.Exception}", 
                "خطا در اجرای Screen Splitt", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"خطای بحرانی:\n\n{ex.Message}", "خطای بحرانی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        // باز کردن پنجره اصلی
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
