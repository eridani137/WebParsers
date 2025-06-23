using System.Text;

namespace Shared;

public static class SpectreExtensions
{
    public static string Highlight(this string input, string highlightColor, string textColor)
    {
        var result = new List<string>();

        var split = input.Split(' ');
        foreach (var str in split)
        {
            switch (true)
            {
                case true when int.TryParse(str, out var intValue):
                    result.Add($"[{highlightColor}]{intValue}[/]");
                    break;

                case true when double.TryParse(str, out var doubleValue):
                    result.Add($"[{highlightColor}]{doubleValue}[/]");
                    break;
                
                case true when bool.TryParse(str, out var boolValue):
                    result.Add($"[{highlightColor}]{boolValue}[/]");
                    break;

                default:
                    result.Add($"[{textColor}]{str}[/]");
                    break;
            }
        }

        return string.Join(" ", result);
    }

    public static string MarkupFuchsia(this string str)
    {
        return $"[{SpectreConfig.Fuchsia}]{str}[/]";
    }
    
    public static string MarkupGrey(this string str)
    {
        return $"[{SpectreConfig.Grey}]{str}[/]";
    }
    
    public static string MarkupYellow(this string str)
    {
        return $"[{SpectreConfig.Yellow}]{str}[/]";
    }
    
    public static string MarkupAqua(this string str)
    {
        return $"[{SpectreConfig.Aquamarine}]{str}[/]";
    }
    
    public static string MarkupRed(this string str)
    {
        return $"[{SpectreConfig.Red}]{str}[/]";
    }
}