using System;
using System.IO;

namespace Prism
{
    public static class Logging
    {
        public static void LogProgress(string modelName, string buttonPress, int autoFixCount, int totalObjects)
        {
            if (Environment.UserName != "mark.gibson")
            {
                using (StreamWriter log = new StreamWriter(@"\\sev-los-fs1\application data$\Prism\Log.txt", true))
                {
                    log.WriteLine("--------------------------------------------------------------------------------------------------");
                    log.WriteLine($"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}");
                    log.WriteLine($"Button press: {buttonPress} - Assemblies processed: {totalObjects} - Auto-Fix count: {autoFixCount}");
                }
            }
        }

        public static void Login(string modelName)
        {
            if (Environment.UserName != "mark.gibson")
            {
                using (StreamWriter log = new StreamWriter(@"\\sev-los-fs1\application data$\Prism\LoginLog.txt", true))
                {
                    log.WriteLine("--------------------------------------------------------------------------------------------------");
                    log.WriteLine($"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}");
                    log.WriteLine($"Log in succesful");
                }
            }
        }

        public static void DebugLog(string debugText, string modelName)
        {
            if (Environment.UserName != "mark.gibson")
            {
                using (StreamWriter log = new StreamWriter(@"\\sev-los-fs1\application data$\Prism\DebugLog.txt", true))
                {
                    log.WriteLine("--------------------------------------------------------------------------------------------------");
                    log.WriteLine($"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}");
                    log.WriteLine($"{debugText}");
                }
            }
        }
    }
}