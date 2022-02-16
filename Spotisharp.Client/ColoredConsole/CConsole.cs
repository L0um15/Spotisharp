using Spotisharp.Client.Enums;
using System.Runtime.InteropServices;

namespace Spotisharp.Client.ColoredConsole;

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

    private static readonly object _locker = new object();
    private static readonly StreamWriter _writer;

    static CConsole()
    {
        RenderApplicationLogo();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Std output handle
            int STD_OUTPUT_HANDLE = -11;
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);

            // Enable virtual terminal processing
            uint ENABLE_ECHO_INPUT = 0x0004;
            var enable = GetConsoleMode(iStdOut, out var outConsoleMode)
                         && SetConsoleMode(iStdOut, outConsoleMode | ENABLE_ECHO_INPUT);
        }

        // Enable file logging
        Directory.CreateDirectory("./logs");
        _writer = new StreamWriter(File.OpenWrite($"./logs/log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt"));

        // Write all exceptions to log file
        AppDomain.CurrentDomain.UnhandledException += (sender, ex) =>
        {
            Exception exception = (Exception)ex.ExceptionObject;
            string exceptionMessage = string.Format
            (
                "Application crashed!\n\n" +
                "--- Message ---\n" +
                "{0}\n\n" +
                "--- Helplink ---\n" +
                "{1}\n\n" +
                "--- Source ---\n" +
                "{2}\n\n" +
                "--- StackTrace ---\n" +
                "{3}\n\n" +
                "--- TargetSide ---\n" +
                "{4}\n\n",
                exception.Message,
                exception.HelpLink,
                exception.Source,
                exception.StackTrace!,
                exception.TargetSite
            );
            WriteLine(exceptionMessage, CConsoleType.Error);
        };
    }

    public static void WriteLine
    (
        string message,
        CConsoleType cType = CConsoleType.Info,
        bool writeToFile = true,
        bool trimMessage = true
    )
    {
        ReadOnlySpan<char> slicedMessage 
            = trimMessage ? SliceExcessChars(message.AsSpan(), cType) : message.AsSpan();
        string fgColor = CConsoleColors.GetForeground(cType);
        string bgColor = CConsoleColors.GetBackground(cType);
        if (cType != CConsoleType.Debug)
        {
            Console.WriteLine
            (
                bgColor + ' ' +
                CConsoleColors.Reset + ' ' +
                fgColor + slicedMessage.ToString() +
                CConsoleColors.Reset
            );
        }
        if (writeToFile)
        {
            DateTime date = DateTime.Now;
            _writer.WriteLine
            (
                string.Format
                (
                    "[{0}] [{1}]: {2}",
                    date.ToString("yyyy.MM.dd HH:mm:ss"),
                    cType.ToString(),
                    message
                )
            );
            _writer.Flush();
        }
    }

    public static void Overwrite
    (
        string message,
        int positionY,
        CConsoleType cType = CConsoleType.Info,
        bool writeToFile = true
    )
    {
        ReadOnlySpan<char> slicedMessage = SliceExcessChars(message.AsSpan(), cType);
        int bufferWidth = Console.BufferWidth;
        int fixedWidth = bufferWidth - slicedMessage.Length - 2;

        lock (_locker)
        {
            Console.SetCursorPosition(0, positionY);
            WriteLine(message + new string(' ', fixedWidth > 0 ? fixedWidth : 1), cType, writeToFile);
        }
    }

    public static string ReadLine()
    {
        string prefix = 
            CConsoleColors.GrayBg + ' ' + CConsoleColors.Reset + ' ' +
            CConsoleColors.RedFg;
        Console.Write(prefix);
        string? input = Console.ReadLine();
        Console.Write(CConsoleColors.Reset);
        return input ?? string.Empty;
    }

    private static ReadOnlySpan<char> SliceExcessChars(ReadOnlySpan<char> messageSpan, CConsoleType cType)
    {
        if (cType != CConsoleType.Error && cType != CConsoleType.Debug)
        {
            int maxWidth = Console.BufferWidth - 4;
            return messageSpan.Length <= maxWidth ? messageSpan : messageSpan.Slice(0, maxWidth);
        }
        return messageSpan;
    }

    private static void RenderApplicationLogo()
    {
        string[] logo =
        {
            "        ▓        ▓",
            "        ▓        ▓      ▄▄",
            "  ▄▄▄███▓████████▓▄▄▓▓▀▀▀",
            " ████████████████▓█████▄▄",
            "     ▄▄▄▓▀▀▀     ▓ ▀▀█████",
            "▄████████████████▓▄▄",
            "  ▀█▀▀▀▀▓▀▀▀▀▀▀▀██████▄",
            "      ▄▄▓▄▄▄▄▄   ▓  ▀▀█▀",
            "   █████▓█▀██████▓█▄",
            "        ▓        ▓▀██",
            "        ▓        ▓",
            "        ▓        ▓",
            "        ▀        ▀"
        };
        string prefix = CConsoleColors.GrayBg + ' ' + CConsoleColors.Reset + ' ';
        for (int i = 0; i < logo.Length; i++)
        {
            Console.Write(prefix);
            for (int j = 0; j < logo[i].Length; j++)
            {
                char c = logo[i][j];
                switch (c)
                {
                    case '▀':
                    case '▓':
                    case '▄':
                        Console.Write(CConsoleColors.GrayFg + c + CConsoleColors.Reset);
                        break;
                    default:
                        Console.Write(CConsoleColors.RedFg + c + CConsoleColors.Reset);
                        break;
                }
            }
            Console.Write('\n');
        }
    }
}
