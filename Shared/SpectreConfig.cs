using Spectre.Console;

namespace Shared;

public static class SpectreConfig
{
    public static readonly Style Style = new(Color.Aquamarine1);
    public static readonly Color BorderColor = Color.Yellow;
    public static readonly BoxBorder BoxBorder = BoxBorder.Square;
    public static readonly TableBorder TableBorder = TableBorder.Square;
    public const int PageSize = 25;
    public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public static string MarkupGrey(this string str)
    {
        return $"[grey]{str}[/]";
    }
    
    public static string MarkupSecondary(this string str)
    {
        return $"[yellow]{str}[/]";
    }
    
    public static string MarkupPrimary(this string str)
    {
        return $"[aquamarine1_1]{str}[/]";
    }
    
    public static string MarkupError(this string str)
    {
        return $"[red3_1]{str}[/]";
    }
}