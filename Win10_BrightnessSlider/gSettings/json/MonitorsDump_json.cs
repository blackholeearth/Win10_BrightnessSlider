using System;
using System.IO;
using Newtonsoft.Json;

namespace Win10_BrightnessSlider
{
    public static class MonitorsDump_json
    {
        static string path_local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        //static string path_roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static MonitorsDump_json()
        {
            var fulldir = Path.GetDirectoryName(settingFilePath);
            Directory.CreateDirectory(fulldir); // directories need to be created
        }
        public static string settingFilePath => Path.Combine(path_local, "Win10_BrigtnessSlider", "MonitorsDump.json");

        static void Update(Action<MonitorsDump> act)
        {
            var settingsPOCO = Get();
            act.Invoke(settingsPOCO);

            SaveTo_JsonFile(settingsPOCO);
        }

        
        static MonitorsDump Get()
        {
            if (!File.Exists(settingFilePath))
            {
                SaveTo_JsonFile(new MonitorsDump());
            }

            MonitorsDump settingsPOCO = From_JsonFile();
            return settingsPOCO;
        }


        public static void SaveTo_JsonFile(this MonitorsDump POCO) => File.WriteAllText(settingFilePath, POCO.ToJson());
        private static MonitorsDump From_JsonFile() => ToObject(File.ReadAllText(settingFilePath));

        /// <summary>
        ///cannot convert json to Object  -Error 
        /// </summary>
        public static MonitorsDump ToObject(string json) => JsonConvert.DeserializeObject<MonitorsDump>(json, Converter._Settings);
        public static string ToJson(this MonitorsDump self) => JsonConvert.SerializeObject(self, Converter._Settings);


    }



}
