using Spotisharp.Client.Enums;
using Swan;
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
            WriteToFile(message, CConsoleType.Info);
        }
    }

    public static void Warn(object message)
    {
        string coloredSpacing = "\u001b[48;2;227;164;26m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;227;164;26m" + message + "\u001b[0m";
        Console.WriteLine(coloredSpacing + ' ' + coloredMessage);
        WriteToFile(message, CConsoleType.Warn);
    }

    public static void Error(object message)
    {
        string coloredSpacing = "\u001b[48;2;230;0;103m" + " " + "\u001b[0m";
        string coloredMessage = "\u001b[38;2;230;0;103m" + message + "\u001b[0m";
        Console.WriteLine(coloredSpacing + ' ' + coloredMessage);
        WriteToFile(message, CConsoleType.Error);
    }

    public static void Debug(object message)
    {
        WriteToFile(message, CConsoleType.Debug);
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
            string emptySpace = string.Empty;

            int clearLineLength = Console.BufferWidth - msg.Length - 2;

            if (clearLineLength > 0)
            {
                emptySpace = new string(' ', clearLineLength);
            }

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
                    default:
                        break;
                }
            }
        }
    }

    private static void WriteToFile(object message, CConsoleType cType)
    {
        _writer.WriteLine
        (
            string.Format
            (
                "[{0}][{1}]: {2}",
                DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
                cType.ToString(),
                message
            )
        );
        _writer.Flush();
    }
}
