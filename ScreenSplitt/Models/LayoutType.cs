// FILE_ID: MOD-002 | FILE_PATH: ScreenSplitt/Models/LayoutType.cs
namespace ScreenSplitt.Models;

/// <summary>
/// نوع چیدمان صفحه
/// </summary>
public enum LayoutType
{
    /// <summary>
    /// تک پنجره کل صفحه
    /// </summary>
    SinglePane,
    
    /// <summary>
    /// دو قسمت عمودی (چپ و راست)
    /// </summary>
    SplitVertical,
    
    /// <summary>
    /// دو قسمت افقی (بالا و پایین)
    /// </summary>
    SplitHorizontal,
    
    /// <summary>
    /// چهار قسمت مساوی
    /// </summary>
    Grid4
}
