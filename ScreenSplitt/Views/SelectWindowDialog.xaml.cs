// FILE_ID: DLG-002 | FILE_PATH: ScreenSplitt/Views/SelectWindowDialog.xaml.cs
using System.Windows;
using ScreenSplitt.Models;
using ScreenSplitt.Services;
using Wpf.Ui.Controls;

namespace ScreenSplitt.Views;

public partial class SelectWindowDialog : UiWindow
{
    public OpenWindowInfo? SelectedWindow { get; private set; }

    public SelectWindowDialog()
    {
        InitializeComponent();
        LoadWindowsList();
        WindowsListBox.SelectionChanged += (s, e) =>
        {
            SelectButton.IsEnabled = WindowsListBox.SelectedItem != null;
        };
    }

    /// <summary>
    /// بارگذاری لیست پنجره‌های باز
    /// </summary>
    private void LoadWindowsList()
    {
        var windows = WindowEnumerator.GetAllOpenWindows();
        WindowsListBox.ItemsSource = windows;
    }

    private void SelectButton_OnClick(object sender, RoutedEventArgs e)
    {
        SelectedWindow = WindowsListBox.SelectedItem as OpenWindowInfo;
        DialogResult = true;
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
