using Spotisharp.Client.ColoredConsole;
using Spotisharp.Client.Models;
using System.Reflection;
using System.Text.Json;

public static class ConfigManager
{

    private static string _appVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
    
    private static ConfigModel _properties = new ConfigModel();
    public static ConfigModel Properties { get { return _properties; } set { _properties = value; } }

    private static readonly string _configFolder =
        Path.Join
        (
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "spotisharp"
        );

    private static readonly string _configFile = Path.Join(_configFolder, "config.json");
    private static readonly string _localConfigFile = Path.Join(
        Directory.GetCurrentDirectory(),
        "config.json"
    );

    private static bool Init(string configFilePath) {

        string configFileContent = File.ReadAllText(configFilePath);
        var userProperties = JsonSerializer.Deserialize<ConfigModel>(configFileContent);
        if(userProperties != null)
        {
            if (userProperties.VersionControl != _appVersion)
            {
                userProperties.VersionControl = _appVersion;
                WriteChanges(configFilePath);
            }
            _properties = userProperties;
            CConsole.WriteLine($"Using config file at {configFilePath}");
            return true;
        }
        return false;
    }

    public static bool Init()
    {
        Directory.CreateDirectory(_configFolder);

        if (File.Exists(_localConfigFile)) {
            return Init(_localConfigFile);
        }
        else if (File.Exists(_configFile)) {
            return Init(_configFile);
        }
        else
        {
            WriteChanges(_configFile);
            return true;
        }
    }

    public static void WriteChanges(string configFilePath)
    {
        string serializedJson = JsonSerializer.Serialize(_properties, new JsonSerializerOptions() { WriteIndented = true});
        File.WriteAllText(configFilePath, serializedJson);
    }

}