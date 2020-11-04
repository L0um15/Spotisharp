using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpotiSharp
{
    class ConfigurationHandler
    {
        public string CLIENTID { get; set; }
        public string SECRETID { get; set; }

        string configfile = "config.json";
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
            using (StreamWriter streamWriter = File.CreateText(configfile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(streamWriter, new { CLIENTID = "", SECRETID = "" });
            }
            Console.WriteLine("Config.json was created. Fill missing fields. Exiting.");
            Environment.Exit(0);
        }
        private void Initialize()
        {
            JObject jObject = JObject.Parse(File.ReadAllText(configfile));
            CLIENTID = jObject.SelectToken("CLIENTID").Value<string>();
            SECRETID = jObject.SelectToken("SECRETID").Value<string>();
            if(CLIENTID == "" || SECRETID == "")
            {
                Console.WriteLine("Config.json has empty fields. Exiting.");
                Environment.Exit(0);
            }
        }

    }
}
