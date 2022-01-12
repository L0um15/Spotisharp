using Spotisharp.Client.Enums;
using System.Runtime.InteropServices;

public static class CConsole
{

    #region DLLs
    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
    #endregion

    private static object _locker = new object();

    static CConsole()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Std output handle
            var iStdOut = GetStdHandle(-11);

            // Enable virtual terminal processing
            var enable = GetConsoleMode(iStdOut, out var outConsoleMode)
                         && SetConsoleMode(iStdOut, outConsoleMode | 0x0004);
        }
    }

    public static void Info(object message)
    {
        string coloredSpacing = "\u001b[48;2;63;212;147m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;24;224;166m" + message + "\u001b[0m";

        Console.WriteLine(coloredSpacing + ' ' + coloredMessage);
    }

    public static void Warn(object message)
    {
        string coloredSpacing = "\u001b[48;2;227;164;26m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;227;164;26m" + message + "\u001b[0m";

        Console.WriteLine(coloredSpacing + ' ' + coloredMessage);
    }

    public static void Error(object message)
    {
        string coloredSpacing = "\u001b[48;2;230;0;103m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;230;0;103m" + message + "\u001b[0m";

        Console.WriteLine(coloredSpacing + ' ' + coloredMessage);
    }

    public static void Debug(object message)
    {
        string coloredSpacing = "\u001b[48;2;255;171;238m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;255;171;238m" + message + "\u001b[0m";

        Console.WriteLine(coloredSpacing + ' ' + coloredMessage);
    }

    public static void Note(object message)
    {
        string coloredSpacing = "\u001b[48;2;16;171;240m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;16;171;240m" + message + "\u001b[0m";

        Console.WriteLine(coloredSpacing + ' ' + coloredMessage);
    }

    public static string ReadInput()
    {
        string coloredSpacing = "\u001b[48;2;255;203;165m" + " " + "\u001b[0m";
        Console.Write(coloredSpacing + ' ' + "\u001b[38;2;255;203;165m");
        string? input = Console.ReadLine();
        Console.Write("\u001b[0m");
        return input ?? string.Empty;
    }

    public static void Overwrite(object message, int positionY, CConsoleType cType = CConsoleType.Info)
    {
        string msg = message.ToString();
        string emptySpace = new string(' ', Console.BufferWidth - msg.Length - 2);

        lock (_locker)
        {
            Console.SetCursorPosition(0, positionY);
            switch (cType)
            {
                case CConsoleType.Info:
                    CConsole.Info(msg + emptySpace);
                    break;
                case CConsoleType.Warn:
                    CConsole.Warn(msg + emptySpace);
                    break;
                case CConsoleType.Error:
                    CConsole.Error(msg + emptySpace);
                    break;
                case CConsoleType.Debug:
                    CConsole.Debug(msg + emptySpace);
                    break;
                case CConsoleType.Note:
                    CConsole.Note(msg + emptySpace);
                    break;
            }
        }
    }

}
