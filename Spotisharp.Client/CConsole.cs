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
    private static readonly StreamWriter _writer;

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

        // Enable file logging
        Directory.CreateDirectory("./logs");
        _writer = new StreamWriter(File.OpenWrite($"./logs/log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt"));

        // Write all exceptions to log file
        AppDomain.CurrentDomain.UnhandledException += (sender, ex) =>
        {
            Exception exception = (Exception) ex.ExceptionObject;
            CConsole.Error(exception);
        };
    }

    public static void Info(object message, bool writeToFile = true)
    {
        string coloredSpacing = "\u001b[48;2;63;212;147m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;24;224;166m" + message + "\u001b[0m";

        Console.WriteLine(coloredSpacing + ' ' + coloredMessage);
        if (writeToFile)
        {
            _writer.WriteLine
            (
                string.Format
                (
                    "[{0}][INFO]: {1}",
                    DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
                    message
                )
            );
            _writer.Flush();
        }
    }

    public static void Warn(object message)
    {
        string coloredSpacing = "\u001b[48;2;227;164;26m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;227;164;26m" + message + "\u001b[0m";

        Console.WriteLine(coloredSpacing + ' ' + coloredMessage);
        _writer.WriteLine
        (
            string.Format
            (
                "[{0}][INFO]: {1}",
                DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
                message
            )
        );
        _writer.Flush();
    }

    public static void Error(object message)
    {
        string coloredSpacing = "\u001b[48;2;230;0;103m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;230;0;103m" + message + "\u001b[0m";

        Console.WriteLine(coloredSpacing + ' ' + coloredMessage);
        _writer.WriteLine
        (
            string.Format
            (
                "[{0}][INFO]: {1}",
                DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
                message
            )
        );
        _writer.Flush();
    }

    public static void Debug(object message)
    {
        _writer.WriteLine
        (
            string.Format
            (
                "[{0}][DEBUG]: {1}",
                DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
                message
            )
        );
        _writer.Flush();
    }

    public static void Note(object message)
    {
        string coloredSpacing = "\u001b[48;2;255;171;238m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;255;171;238m" + message + "\u001b[0m";

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

    public static void Overwrite
    (
        object message, 
        int positionY, 
        CConsoleType cType = CConsoleType.Info, 
        bool writeToFile = true
    )
    {
        string? msg = message.ToString();
        if(msg != null)
        {
            string emptySpace = new string(' ', Console.BufferWidth - msg.Length - 2);
            lock (_locker)
            {
                Console.SetCursorPosition(0, positionY);
                switch (cType)
                {
                    case CConsoleType.Info:
                        CConsole.Info(msg + emptySpace, writeToFile);
                        break;
                    case CConsoleType.Warn:
                        CConsole.Warn(msg + emptySpace);
                        break;
                    case CConsoleType.Error:
                        CConsole.Error(msg + emptySpace);
                        break;
                    case CConsoleType.Note:
                        CConsole.Note(msg + emptySpace);
                        break;
                    default:
                        break;
                }
            }
        }
    }

}
