// FILE_ID: DLG-002 | FILE_PATH: ScreenSplitt/Views/SelectWindowDialog.xaml.cs
using System.Windows;
using ScreenSplitt.Models;
using ScreenSplitt.Services;
using Wpf.Ui.Controls;

namespace ScreenSplitt.Views;

public partial class SelectWindowDialog : UiWindow
{
    public OpenWindowInfo? SelectedWindow { get; private set; }
    
    /// <summary>
    /// فیلتر دسته‌بندی برای نمایش فقط پنجره‌های مربوطه
    /// </summary>
    public string? CurrentCategoryFilter { get; set; }

    public SelectWindowDialog()
    {
        InitializeComponent();
        WindowsListBox.SelectionChanged += (s, e) =>
        {
            SelectButton.IsEnabled = WindowsListBox.SelectedItem != null;
        };
        LoadWindowsList();
    }

    private void LoadWindowsList()
    {
        var windows = WindowEnumerator.GetAllOpenWindows(CurrentCategoryFilter);
        WindowsListBox.ItemsSource = windows;

        if (windows.Count == 0)
        {
            WindowsListBox.Items.Add("هیچ پنجره‌ای در این دسته‌بندی یافت نشد!");
            WindowsListBox.IsEnabled = false;
        }
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
