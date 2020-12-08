using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace SpotiSharp
{
    class Config
    {
        public static string ClientID { get; set; } = "";
        public static string ClientSecret { get; set; } = "";
        public static string FFmpegPath { get; set; } = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        public static string DownloadPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "SpotiSharp");
    }
    class ConfigurationHandler
    {
        string configfile = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "config.json");
        public ConfigurationHandler()
        {
            if (!File.Exists(configfile))
                Setup();
            else
                Initialize();
        }
        private void Setup()
        {
            Console.WriteLine("Config.json was not found. Creating new one.");
            generateConfigurationFile();
            Console.WriteLine("Config.json was created. Fill missing fields. Exiting.");
            Environment.Exit(0);
        }
        
        
        private void Initialize()
        {
            var jsonstream = JObject.Parse(File.ReadAllText(configfile));
            assignProperties(jsonstream);
            string configversion = jsonstream["SpotiSharp"]["ConfigVersion"].Value<string>();

            if(configversion != VersionChecker.Version)
            {
                Console.WriteLine("SpotiSharp was updated.\n" +
                    "Checking config compatiblility.\n" +
                    "SpotiSharp will close after task is done.");
                generateConfigurationFile();
                Environment.Exit(0);
            }

            if (Config.ClientID == "" || Config.ClientSecret == "")
            {
                Console.WriteLine("Config.json is missing required fields. Exiting.");
                Environment.Exit(0);
            }
        }

        private void generateConfigurationFile()
        {
            var configurationGroup = new Dictionary<string, Dictionary<string, object>>();
            configurationGroup.Add("Settings",new Dictionary<string, object>() {
                { "ConfigVersion", VersionChecker.Version },
                { nameof(Config.ClientID), Config.ClientID},
                { nameof(Config.ClientSecret), Config.ClientSecret },
                { nameof(Config.FFmpegPath), Config.FFmpegPath },
                { nameof(Config.DownloadPath), Config.DownloadPath }
            });
            using (StreamWriter streamWriter = File.CreateText(configfile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(streamWriter, configurationGroup );
            }
        }

        private void assignProperties(JObject jsonstream)
        {
            var properties = typeof(Config).GetProperties();
            foreach (var property in properties)
            {
                if (jsonstream["Settings"][property.Name] != null)
                    property.SetValue(null, jsonstream["Settings"][property.Name]);
            }
        }

    }
}
