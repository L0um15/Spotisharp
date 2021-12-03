using System.Reflection;

namespace Spotisharp.Client.Models;

public class ConfigModel
{ 
    public bool IsFirstTime { get; set; } = true;

    public string VersionControl { get; set; } = 
        Assembly.GetExecutingAssembly().GetName().Version.ToString();

    public string MusicDirectory { get; set; } =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Spotisharp");

    public void EnsureDirsExist()
    {
        Directory.CreateDirectory(MusicDirectory);
    }
}
