using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

using Tomlyn;

namespace Win10_BrightnessSlider
{
     
    //----------
    public static class Settings_toml
    {
        static string path_local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        //static string path_roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static Settings_toml()
        {
            var fulldir = Path.GetDirectoryName(settingFilePath);
            Directory.CreateDirectory(fulldir); // directories need to be created
        }
        static string settingFilePath => Path.Combine(path_local, "Win10_BrigtnessSlider", "settings.toml");

        public static void Update(Action<Settings> act)
        {
            var settingsPOCO = GetSettings();
            act.Invoke(settingsPOCO);
            SaveTo_TomlFile(settingsPOCO);
        }
        public static Settings GetSettings()
        {
            if (!File.Exists(settingFilePath)) 
            {
                SaveTo_TomlFile(new Settings());
            }
            
            Settings settingsPOCO = From_TomlFile();
            return settingsPOCO;
        }


        public static void SaveTo_TomlFile(this Settings settingsPOCO)
        {
            var tomlSTR = Toml.FromModel(settingsPOCO );
            File.WriteAllText(settingFilePath, tomlSTR);
        }
        private static Settings From_TomlFile()
        {
            var tomlSTR = File.ReadAllText(settingFilePath);
            return Toml.ToModel<Settings>(tomlSTR);
        }

        public static Settings ToObject(string toml) => Toml.ToModel<Settings>(toml);
        public static string ToToml(this Settings self) => Toml.FromModel(self );


    }




}
