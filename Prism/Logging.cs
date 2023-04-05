using System;
using System.Collections;
using System.Collections.Generic;
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
                CountTimesUsed(autoFixCount, totalObjects, buttonPress);
            }
        }

        private static void CountTimesUsed(int autoFixCount, int totalObjects, string buttonPress)
        {
            int newTimesUsed = 0;
            int newPartsUsed = 0;
            int newAutoFixed = 0;
            int newFabPack = 0;

            int addToFabPacks = buttonPress == "Fab Package" ? 1 : 0;

            using (StreamReader read = new StreamReader(@"\\sev-los-fs1\application data$\Prism\TotalUseLog.txt"))
            {
                string line1 = read.ReadLine();
                string timesUsedLine = read.ReadLine();
                string partsUsedLine = read.ReadLine();
                string autoFixLine = read.ReadLine();
                string fabPacksMade = read.ReadLine();

                string timesUsed = timesUsedLine.Split(':')[1].Trim();
                newTimesUsed = Convert.ToInt32(timesUsed) + 1;

                string partsUsed = partsUsedLine.Split(':')[1].Trim();
                newPartsUsed = Convert.ToInt32(partsUsed) + totalObjects;

                string autoFixed = autoFixLine.Split(':')[1].Trim();
                newAutoFixed = Convert.ToInt32(autoFixed) + autoFixCount;

                string fabPacks = fabPacksMade.Split(':')[1].Trim();
                newFabPack = Convert.ToInt32(fabPacks) + addToFabPacks;
            }

            using (StreamWriter log = new StreamWriter(@"\\sev-los-fs1\application data$\Prism\TotalUseLog.txt", false))
            {
                log.WriteLine("---------------------------This log was started on 04/04/23-------");
                log.WriteLine($"Times used: {newTimesUsed}");
                log.WriteLine($"Parts processed: {newPartsUsed}");
                log.WriteLine($"Auto-Fix count: {newAutoFixed}");
                log.WriteLine($"Fabrication packages created: {newFabPack}");
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

        public static void LoginFail()
        {
            if (Environment.UserName != "mark.gibson")
            {
                using (StreamWriter log = new StreamWriter(@"\\sev-los-fs1\application data$\Prism\LoginLog.txt", true))
                {
                    log.WriteLine("--------------------------------------------------------------------------------------------------");
                    log.WriteLine($"{DateTime.Now} - User: {Environment.UserName}");
                    log.WriteLine($"Log in failed");
                }
            }
        }

        public static void DebugLog(string debugText, string modelName)
        {
           // if (Environment.UserName != "mark.gibson")
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