using System;
using System.IO;

namespace Prism
{
    public static class Logging
    {
        public static void LogProgress(string modelName, string buttonPress, int autoFixCount, int totalObjects)
        {
            using (StreamWriter log = new StreamWriter(@"\\sev-los-fs1\application data$\Prism\Log.txt", true))
            {
                log.WriteLine("--------------------------------------------------------------------------------------------------");
                log.WriteLine($"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}");
                log.WriteLine($"Button press: {buttonPress} - Assemblies processed: {totalObjects} - Auto-Fix count: {autoFixCount}");
            }
        }
    }
}