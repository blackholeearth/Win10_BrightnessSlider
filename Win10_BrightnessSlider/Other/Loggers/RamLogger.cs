using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{ 
    public static class RamLogger
    {
        static int loggerMaxCap = 80;
        static int loggerTrimSize_TriggerCap =  loggerMaxCap-10   ;
        public static List<string> logger = new List<string>(loggerMaxCap);

        public static bool DenseMode = true;

        public static void Log(string msg, bool consoleWriteline = true,
            [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = -1)
        {
            var fileName = "";
            try
            {
                fileName = Path.GetFileName(file);
            }
            catch (Exception ex) { fileName = "GetFileName Failed.."; }

            string str1 =  $"[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff")}] [{fileName}, {caller}, {line}] :: {msg} \r\n";
            // str1 =  $"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss fff")} => (File,Member,Line): {fileName}, {caller}, {line} \r\n {msg} \r\n";
            logger.Add(  str1  );
            

            Console.WriteLine("filelogger: " + str1);

            if(logger.Count > loggerTrimSize_TriggerCap)
                logger.RemoveRange(0,20);
        }

        /// <summary>
        /// Get Log As String
        /// </summary>
        /// <returns></returns>
        public static  string ToString()
        {
            return string.Join(Environment.NewLine, RamLogger.logger);
        }


    }
}
