using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Configuration;
using System.Threading;

namespace Win10_BrightnessSlider
{


    public static class Settings_json
    {
        static string path_local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        //static string path_roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static Settings_json()
        {
            var fulldir = Path.GetDirectoryName(settingFilePath);
            Directory.CreateDirectory(fulldir); // directories need to be created
        }
        public static string settingFilePath => Path.Combine(path_local, "Win10_BrigtnessSlider", "settings.json");

        public static void Update(Action<Settings> act)
        {
            var settingsPOCO = Get();
            act.Invoke(settingsPOCO);

            SaveTo_JsonFile(settingsPOCO);
        }
        public static Settings Get()
        {
            if (!File.Exists(settingFilePath))
            {
                SaveTo_JsonFile(new Settings());
            }

            Settings settingsPOCO = From_JsonFile();
            return settingsPOCO;
        }


        public static void SaveTo_JsonFile(this Settings settingsPOCO) => File.WriteAllText(settingFilePath, settingsPOCO.ToJson());
        private static Settings From_JsonFile() => ToObject(File.ReadAllText(settingFilePath));
        
        public static Settings ToObject(string json) => JsonConvert.DeserializeObject<Settings>(json, Converter._Settings);
        public static string ToJson(this Settings self) => JsonConvert.SerializeObject(self, Converter._Settings);


    }



    //---------json helpers
    internal static class Converter
    {
        public static readonly JsonSerializerSettings _Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }



}
