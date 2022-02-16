using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Spotisharp.Client.Services;

public static class FFmpegService
{

    private static Regex _timeRegex = new Regex(@"\w\w:\w\w:\w\w");
    private static string _args = "-threads 1 -i pipe: -q:a 0";

    public static async Task ConvertStreamAsync
    (
        Stream inputStream,
        string outputFile,
        IProgress<Tuple<TimeSpan, TimeSpan>> progress
    )
    {
        Process ffmpegProcess = RunProcess(_args + " \""+outputFile+"\"", stdIn: true, stdErr: true);

        if (progress != null)
        {
            TimeSpan duration = TimeSpan.Zero;
            TimeSpan position = TimeSpan.Zero;
            ffmpegProcess.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    TimeSpan tempDuration = GetDuration(e, _timeRegex);
                    TimeSpan tempPosition = GetPosition(e, _timeRegex);

                    if (tempDuration != TimeSpan.Zero)
                    {
                        duration = tempDuration;
                    }

                    if (tempPosition != TimeSpan.Zero)
                    {
                        position = tempPosition;
                    }

                    progress.Report(Tuple.Create(position, duration));
                }
            };
            ffmpegProcess.BeginErrorReadLine();
        }
        Stream ffInput = ffmpegProcess.StandardInput.BaseStream;
        inputStream.Seek(0, SeekOrigin.Begin);
        await inputStream.CopyToAsync(ffInput);
        ffInput.Close();
        ffmpegProcess.WaitForExit();
        ffmpegProcess.Close();

    }

    private static TimeSpan GetPosition(DataReceivedEventArgs e, Regex rgx)
    {
        if (e.Data!.Contains("size"))
        {
            Match match = rgx.Match(e.Data!);
            if (match.Success)
            {
                return TimeSpan.Parse(match.Value);
            }
        }
        return TimeSpan.Zero;
    }

    private static TimeSpan GetDuration(DataReceivedEventArgs e, Regex rgx)
    {
        if (e.Data!.Contains("Duration: N/A"))
        {
            return TimeSpan.Zero;
        }
        else if (e.Data!.Contains("Duration:"))
        {
            Match match = rgx.Match(e.Data!);
            if (match.Success)
            {
                return TimeSpan.Parse(match.Value);
            }
        }
        return TimeSpan.Zero;
    }

    private static Process RunProcess
    (
        string args,
        bool stdOut = false,
        bool stdIn = false,
        bool stdErr = false
    )
    {
        var proc = new Process()
        {
            StartInfo =
            {
                FileName = "ffmpeg",
                Arguments = args,
                RedirectStandardOutput = stdOut,
                RedirectStandardInput = stdIn,
                RedirectStandardError = stdErr,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        proc.Start();
        return proc;
    }

    public static bool IsFFmpegInstalled()
    {
        try
        {
            using (Process ffmpegProc = new Process())
            {
                ffmpegProc.StartInfo.FileName = "ffmpeg";
                ffmpegProc.StartInfo.RedirectStandardError = true;
                ffmpegProc.StartInfo.UseShellExecute = false;
                ffmpegProc.StartInfo.CreateNoWindow = true;
                ffmpegProc.Start();
            };
            return true;
        }
        catch (Win32Exception)
        {
            return false;
        }

    }
}
