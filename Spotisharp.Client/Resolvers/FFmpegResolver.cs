using System.ComponentModel;
using System.Diagnostics;

public static class FFmpegResolver
{
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
                ffmpegProc.WaitForExit();
            };
            return true;
        }
        catch (Win32Exception)
        {
            return false;
        }
        
    }
}