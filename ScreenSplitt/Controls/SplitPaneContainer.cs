// FILE_ID: CTRL-003 | FILE_PATH: ScreenSplitt/Controls/SplitPaneContainer.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace ScreenSplitt.Controls;

/// <summary>
/// کانتینر اسپلیت دو قسمتی با نوار جداکننده قابل کشیدن
/// </summary>
public class SplitPaneContainer : Grid
{
    private readonly GridLength _defaultSplitSize = new GridLength(1, GridUnitType.Star);
    private GridSplitter? _splitter;
    private bool _isVertical = true;

    public static readonly DependencyProperty FirstPaneProperty = DependencyProperty.Register(
        nameof(FirstPane), typeof(UIElement), typeof(SplitPaneContainer), new PropertyMetadata(null, OnPaneChanged));

    public UIElement? FirstPane
    {
        get => (UIElement)GetValue(FirstPaneProperty);
        set => SetValue(FirstPaneProperty, value);
    }

    public static readonly DependencyProperty SecondPaneProperty = DependencyProperty.Register(
        nameof(SecondPane), typeof(UIElement), typeof(SplitPaneContainer), new PropertyMetadata(null, OnPaneChanged));

    public UIElement? SecondPane
    {
        get => (UIElement)GetValue(SecondPaneProperty);
        set => SetValue(SecondPaneProperty, value);
    }

    public static readonly DependencyProperty IsVerticalProperty = DependencyProperty.Register(
        nameof(IsVertical), typeof(bool), typeof(SplitPaneContainer), new PropertyMetadata(true, OnOrientationChanged));

    public bool IsVertical
    {
        get => (bool)GetValue(IsVerticalProperty);
        set => SetValue(IsVerticalProperty, value);
    }

    private static void OnPaneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitPaneContainer container)
        {
            container.BuildLayout();
        }
    }

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitPaneContainer container)
        {
            container._isVertical = (bool)e.NewValue;
            container.BuildLayout();
        }
    }

    public SplitPaneContainer()
    {
        Background = Brushes.Transparent;
        _isVertical = IsVertical;
        Loaded += (s, e) => BuildLayout();
    }

    private void BuildLayout()
    {
        Children.Clear();
        RowDefinitions.Clear();
        ColumnDefinitions.Clear();

        if (_isVertical)
        {
            // چیدمان عمودی (دو ستون)
            ColumnDefinitions.Add(new ColumnDefinition { Width = _defaultSplitSize });
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            ColumnDefinitions.Add(new ColumnDefinition { Width = _defaultSplitSize });

            // پنل اول
            if (FirstPane != null)
            {
                SetColumn(FirstPane, 0);
                Children.Add(FirstPane);
            }

            // جداکننده
            _splitter = new GridSplitter
            {
                Width = 6,
                Background = Brushes.Transparent,
                Cursor = Cursors.SizeWE,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            SetColumn(_splitter, 1);
            Children.Add(_splitter);

            // پنل دوم
            if (SecondPane != null)
            {
                SetColumn(SecondPane, 2);
                Children.Add(SecondPane);
            }
        }
        else
        {
            // چیدمان افقی (دو سطر)
            RowDefinitions.Add(new RowDefinition { Height = _defaultSplitSize });
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            RowDefinitions.Add(new RowDefinition { Height = _defaultSplitSize });

            // پنل اول
            if (FirstPane != null)
            {
                SetRow(FirstPane, 0);
                Children.Add(FirstPane);
            }

            // جداکننده
            _splitter = new GridSplitter
            {
                Height = 6,
                Background = Brushes.Transparent,
                Cursor = Cursors.SizeNS,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ResizeBehavior = GridResizeBehavior.PreviousAndNext
            };
            SetRow(_splitter, 1);
            Children.Add(_splitter);

            // پنل دوم
            if (SecondPane != null)
            {
                SetRow(SecondPane, 2);
                Children.Add(SecondPane);
            }
        }
    }

    /// <summary>
    /// ریست کردن تقسیم‌بندی به حالت پیش‌فرض (نصف نصف)
    /// </summary>
    public void ResetSplit()
    {
        if (_isVertical)
        {
            if (ColumnDefinitions.Count >= 3)
            {
                ColumnDefinitions[0].Width = _defaultSplitSize;
                ColumnDefinitions[2].Width = _defaultSplitSize;
            }
        }
        else
        {
            if (RowDefinitions.Count >= 3)
            {
                RowDefinitions[0].Height = _defaultSplitSize;
                RowDefinitions[2].Height = _defaultSplitSize;
            }
        }
    }
}
