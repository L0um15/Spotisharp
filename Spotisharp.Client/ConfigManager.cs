using Spotisharp.Client.Models;
using System.Reflection;
using System.Text.Json;

public static class ConfigManager
{
    private static string _appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    
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
    
    public static bool Init()
    {
        Directory.CreateDirectory(_configFolder);
        if (File.Exists(_configFile))
        {
            string configFileContent = File.ReadAllText(_configFile);
            var userProperties = JsonSerializer.Deserialize<ConfigModel>(configFileContent);
            if(userProperties != null)
            {
                if (userProperties.VersionControl != _appVersion)
                {
                    userProperties.VersionControl = _appVersion;
                    WriteChanges();
                }
                _properties = userProperties;
                return true;
            }
            return false;
        }
        else
        {
            WriteChanges();
            return true;
        }
    }

    public static void WriteChanges()
    {
        string serializedJson = JsonSerializer.Serialize(_properties, new JsonSerializerOptions() { WriteIndented = true});
        File.WriteAllText(_configFile, serializedJson);
    }
}