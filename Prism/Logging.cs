using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Tekla.Structures.Model;

namespace Prism
{
    public static class Logging
    {
        public static void CreateModelLog(PrismProjectData pData)
        {
            //this method checks the model data folder on our server for a folder named after the users current model, if it does not exist we create it
            bool logExists = false;
            foreach (string folder in Directory.GetDirectories(Constants.PrismDataLogLocation))
            {
                string check = Constants.ModelDataLogLocation(pData.ProjNumberAndName);
                if (folder == Constants.ModelDataLogLocation(pData.ProjNumberAndName))
                { logExists = true; }
            }
            if (!logExists)
            {
                Directory.CreateDirectory(Constants.ModelDataLogLocation(pData.ProjNumberAndName));
                // Set full control permissions on the folder
                DirectorySecurity directorySecurity = Directory.GetAccessControl(Constants.ModelDataLogLocation(pData.ProjNumberAndName));
                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                directorySecurity.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(Constants.ModelDataLogLocation(pData.ProjNumberAndName), directorySecurity);

                WriteToFile(Constants.ModelDataLogLocation(pData.ProjNumberAndName) + "\\Project Info.txt", pData.pInfo);
            }
        }

        public static void WriteToFile(string filePath, ProjectInfo pInfo)
        {
            int currentLastNumber = 0;
            pInfo.GetUserProperty(ModelUDA.LastUsedPrelim(), ref currentLastNumber);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"Next prelim to use: {currentLastNumber}");
                writer.WriteLine("Material orders processed: 0");
                writer.WriteLine("Fab packages created: 0");
                writer.Close();
            }
        }

        public static int GetLastUsedPrelim(string jobName)
        {            
            using (StreamReader read = new StreamReader(Constants.ModelDataLogLocation(jobName) + "\\Project Info.txt"))
            {
                string lastUsedPrelimLine = read.ReadLine();              

                string lastusedPrelim = lastUsedPrelimLine.Split(':')[1].Trim();
                return Convert.ToInt32(lastusedPrelim);
            }
        }

        public static void SetLastUsedPrelim(string jobName, int lastUsedPrelim)
        {
            string fileLocation = Constants.ModelDataLogLocation(jobName) + "\\Project Info.txt";
            using (StreamReader read = new StreamReader(fileLocation))
            {
                string lastUsedPrelimLine = read.ReadLine();
                string materialOrderProcessedLine = read.ReadLine();
                string fabPackagesMadeLine = read.ReadLine();

                string lastusedPrelim = lastUsedPrelimLine.Split(':')[0].Trim();
                read.Close();

                using (StreamWriter writer = new StreamWriter(fileLocation))
                {
                    writer.WriteLine($"Next prelim to use: {lastUsedPrelim}");
                    writer.WriteLine(materialOrderProcessedLine);
                    writer.WriteLine(fabPackagesMadeLine);
                    writer.Close();
                }
            }
        }

        public static void LogProgress(string modelName, string buttonPress, int autoFixCount, int totalObjects)
        {
            if (!Constants.IsSpecialPerson())
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
            if (!Constants.IsSpecialPerson())
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
            if (!Constants.IsSpecialPerson())
            {
                using (StreamWriter log = new StreamWriter(@"\\sev-los-fs1\application data$\Prism\LoginLog.txt", true))
                {
                    log.WriteLine("--------------------------------------------------------------------------------------------------");
                    log.WriteLine($"{DateTime.Now} - User: {Environment.UserName}");
                    log.WriteLine($"Log in failed");
                }
            }
        }

        public static void PartsModifiedAfterRun()
        {
            if (!Constants.IsSpecialPerson())
            {
                int newPartsUpdated = 0;
                string timesUsedLine = "";
                string partsUsedLine = "";
                string autoFixLine = "";
                string fabPacksMade = "";

                using (StreamReader read = new StreamReader(@"\\sev-los-fs1\application data$\Prism\TotalUseLog.txt"))
                {
                    string line1 = read.ReadLine();
                    timesUsedLine = read.ReadLine();
                    partsUsedLine = read.ReadLine();
                    autoFixLine = read.ReadLine();
                    fabPacksMade = read.ReadLine();
                    string partUpdated = read.ReadLine();

                    string partsUpdated = partUpdated.Split(':')[1].Trim();
                    newPartsUpdated = Convert.ToInt32(partsUpdated) + 1;
                }

                using (StreamWriter log = new StreamWriter(@"\\sev-los-fs1\application data$\Prism\TotalUseLog.txt", false))
                {
                    log.WriteLine("---------------------------This log was started on 04/04/23-------");
                    log.WriteLine(timesUsedLine);
                    log.WriteLine(partsUsedLine);
                    log.WriteLine(autoFixLine);
                    log.WriteLine(fabPacksMade);
                    log.WriteLine($"Parts updated after Prism: {newPartsUpdated}");
                }
            }
        }

        public static void DebugLog(string debugText, string modelName)
        {
            if (!Constants.IsSpecialPerson())
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