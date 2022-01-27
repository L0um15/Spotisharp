using Spotisharp.Client.Enums;

namespace Spotisharp.Client.ColoredConsole;

public readonly struct CConsoleColors
{
    public static string Reset => "\u001b[0m";

    public static string GreenFg => "\u001b[38;2;24;224;166m";
    public static string OrangeFg => "\u001b[38;2;227;164;26m";
    public static string RedFg => "\u001b[38;2;230;0;103m";
    public static string GrayFg => "\u001b[38;2;71;59;92m";

    public static string GreenBg => "\u001b[48;2;24;224;166m";
    public static string OrangeBg => "\u001b[48;2;227;164;26m";
    public static string RedBg => "\u001b[48;2;230;0;103m";
    public static string GrayBg => "\u001b[48;2;71;59;92m";

    public static string GetForeground(CConsoleType cType)
    {
        switch (cType)
        {
            case CConsoleType.Info:
                return GreenFg;
            case CConsoleType.Warn:
                return OrangeFg;
            case CConsoleType.Error:
                return RedFg;
            default:
                return string.Empty;
        }
    }

    public static string GetBackground(CConsoleType cType)
    {
        switch (cType)
        {
            case CConsoleType.Info:
                return GreenBg;
            case CConsoleType.Warn:
                return OrangeBg;
            case CConsoleType.Error:
                return RedBg;
            default:
                return string.Empty;
        }
    }
}
