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

    public const string Yellow = "yellow";
    public const string Fuchsia = "fuchsia";
    public const string Aquamarine = "aquamarine1_1";
    public const string Red = "red3_1";
    public const string Grey = "grey";
}