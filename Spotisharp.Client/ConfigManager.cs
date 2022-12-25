using CommandLine;
using Spotisharp.Client.ColoredConsole;
using Spotisharp.Client.Models;
using System.Reflection;
using System.Text.Json;

public static class ConfigManager
{
    private static string _appVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

    private static OptionsModel _options = new OptionsModel();

    public static OptionsModel Options { get { return _options; } set { _options = value; } }

    private static readonly string _configFolder =
        Path.Join
        (
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "spotisharp"
        );

    private static readonly string _configFile = Path.Join(_configFolder, "config.json");
    
    public static bool Init(string[] args)
    {
        OptionsModel? launchOptions = null;

        Parser.Default.ParseArguments<OptionsModel>(args)
            .WithParsed(options => launchOptions = options);

        if (launchOptions == null)
        {
            return false;
        }

        Directory.CreateDirectory(_configFolder);

        if (!File.Exists(_configFile))
        {
            SaveDefaultConfiguration(_configFile);
        }

        string configContents = File.ReadAllText(_configFile);
        ConfigModel? deserializedConfig = JsonSerializer.Deserialize<ConfigModel>(configContents);
        if (deserializedConfig == null)
        {
            return false;
        }

        if(deserializedConfig.VersionControl != _appVersion)
        {
            deserializedConfig.VersionControl = _appVersion;
            SaveConfiguration(deserializedConfig, _configFile);
        }

        Tuple<ConfigModel, OptionsModel> userConfiguration = FillOutConfigurations(deserializedConfig, launchOptions);

        if (userConfiguration.Item2.KeepOptions == true)
        {
            SaveConfiguration(userConfiguration.Item1, _configFile);
        }

        _options = userConfiguration.Item2;

        return true;
    }

    private static Tuple<ConfigModel, OptionsModel> FillOutConfigurations(ConfigModel configModel, OptionsModel optionsModel)
    {
        OptionsModel tempOptions = optionsModel;
        ConfigModel tempConfig = configModel;

        PropertyInfo[] properties = tempOptions.GetType().GetProperties();

        foreach (PropertyInfo property in properties)
        {
            object? optionsValue = property.GetValue(optionsModel, null);
            object? configValue = tempConfig.GetType().GetProperty(property.Name)?.GetValue(tempConfig, null);

            if (optionsValue == null)
            {
                property.SetValue(tempOptions, configValue, null);
            }
            else
            {
                PropertyInfo? configProperty = tempConfig.GetType().GetProperty(property.Name);
                if (configProperty != null)
                {
                    configProperty.SetValue(tempConfig, optionsValue);
                }
            }
        }
        return Tuple.Create(tempConfig, tempOptions);
    }

    public static void SaveConfiguration(ConfigModel configModel, string configPath)
    {
        string serializedConfig = JsonSerializer.Serialize(configModel, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(configPath, serializedConfig);
    }

    public static void SaveDefaultConfiguration(string configPath)
    {
        ConfigModel defaultConfiguration = new ConfigModel();
        SaveConfiguration(defaultConfiguration, configPath);
    }

}