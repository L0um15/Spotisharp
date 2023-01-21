using CommandLine;
using System.Reflection;

namespace Spotisharp.Client.Models;

public class ConfigModel
{

    public bool CheckUpdates { get; set; } = true; 

    public bool ExplicitContents { get; set; } = true;

    public int WorkersCount { get; set; } = 2;

    public string OutputDir { get; set; } =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Spotisharp");

    public string VersionControl { get; set; } =
        Assembly.GetExecutingAssembly().GetName().Version!.ToString();

}
