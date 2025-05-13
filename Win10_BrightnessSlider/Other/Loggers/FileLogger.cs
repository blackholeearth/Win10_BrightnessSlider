using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Win10_BrightnessSlider
{
    /// <summary>
    /// some people doesnt give file read write permisson!!!  dont wanna use it
    /// </summary>
    public static class FileLogger
    {
        static object _lock = new object();

        public readonly static string LogName = "w10_BS.log.txt";
        public readonly static string AppConfigFolderName = "Win10_BrightnessSlider";
        public readonly static string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string LogPath {
            get {  return Path.Combine(localAppData,  AppConfigFolderName,   LogName);    }
        }  

        public static void Log(string msg, bool consoleWriteline = true, 
            [CallerMemberName] string caller = "", [CallerFilePath] string file = "" , [CallerLineNumber] int line = -1) 
        {

            try
            {
                lock (_lock)
                {
                    var fileName = Path.GetFileName(file);
                    File.AppendAllText(LogPath,
                        $"{ DateTime.UtcNow } ==>  Caller(File,Member,Line): {fileName}, {caller}, {line} \r\n" +
                        $"{ msg } \r\n\r\n" 
                        );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception @ filelogger: " + ex);
            }

            Console.WriteLine("filelogger: " + msg);
        }

    }
}
