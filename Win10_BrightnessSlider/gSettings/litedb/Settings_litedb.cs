using System;
using System.IO;
using LiteDB;

namespace Win10_BrightnessSlider.Gui
{
    //public class SettingJson {    }


    public static class Settings_litedb
    {
        static string path_local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        //static string path_roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static Settings_litedb() 
        {
            var fulldir = System.IO.Path.GetDirectoryName(settingFilePath);
            System.IO.Directory.CreateDirectory(fulldir); // directories need to be created
        }
        static string settingFilePath => Path.Combine(path_local, "Win10_BrigtnessSlider", "settings_ldb.litedb");
        static string conSTR = $@"Filename={settingFilePath};Connection='Shared' ";

        public static void Update(Action<Settings> act)
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(conSTR ))
            {
                // Get a collection (or create, if doesn't exist)
                var col = db.GetCollection<Settings>("settings");

                var row = col.FindOne(x => x._id == 1);

                if (row is null)
                { 
                    row = new Settings();
                    act.Invoke(row);
                    col.Insert(row);
                }
                else
                {
                    act.Invoke(row);
                    col.Update(row);
                }

            }
        }


        public static Settings GetSettings()
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(conSTR))
            {
                // Get a collection (or create, if doesn't exist)
                var col = db.GetCollection<Settings>("settings") .FindOne(x => x._id == 1);
                return col ?? new Settings();
            }
        }


       


    }




}
