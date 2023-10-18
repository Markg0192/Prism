using MarksWebService;
using Prism.ExternalService;
using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Services.Description;
using Tekla.Structures.Drawing;
using Tekla.Structures.DrawingInternal;
using Tekla.Structures.Model;

namespace Prism
{

    public static class Logging
    {
        public static void UpdateUserUseCount(string userName, ExternalService.WebService1 service)
        {
            Dictionary<string, int> userCounts = new Dictionary<string, int>();

            // Read existing log
            foreach (string line in service.ReadAllLinesIntoArray(Constants.PrismUserUserLogLocation, ""))
            {
                if (!line.StartsWith("------log started"))
                {
                    string[] parts = line.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int count))
                    {
                        userCounts[parts[0]] = count;
                    }
                }
            }

            // Update user count
            if (userCounts.ContainsKey(userName))
            {
                userCounts[userName]++;
            }
            else
            {
                userCounts[userName] = 1;
            }


            // Prepare data for writing
            List<string> newContent = new List<string>();
            newContent.Add("---------------------------This log was started on 21/08/23-------");
            foreach (var entry in userCounts)
            {
                newContent.Add($"{entry.Key}-{entry.Value}");
            }

            // Write updated log using your methods
            service.WriteAllLinesWithArray(Constants.PrismUserUserLogLocation, newContent.ToArray(), "");
        }

        public static void CreateModelLog(PrismProjectData pData, ExternalService.WebService1 service)
        {
            //this method checks the model data folder on our server for a folder named after the users current model, if it does not exist we create it
            bool logExists = false;

            foreach (string folder in service.GetDirectories(Constants.PrismDataLogLocation, ""))
            {
                if(service.FileExists(folder, 8, pData.ProjNumberAndGuid))
                {
                    logExists = true;
                    continue;
                }
            }
            if (!logExists)
            { 
                service.CreateNewDirectory(Constants.PrismModelData, pData.ProjNumberAndGuid);
  
                WriteFirstDataLog(Constants.PrismModelData, Constants.ModelProjectInforLocation(pData.ProjNumberAndGuid), pData.pInfo, service);

                PrismWarnings.FirstTimeInTheModel();
            }
        }

        public static void WriteFirstDataLog(int filePathLine, string additonalString, ProjectInfo pInfo, ExternalService.WebService1 service)
        {
            int currentLastNumber = 0;
            pInfo.GetUserProperty(ModelUDA.LastUsedPrelim(), ref currentLastNumber);

            string[] content = new string[]
            {
                $"Next prelim to use: {currentLastNumber}",
                "Material orders processed: 0",
                "Fab packages created: 0",
                "Frozen drawing count: Frozen = 0, Un - Frozen = 0",
                "",
                "------------Uniclass Codes----------------",
                "---Filter------------Code----------------Title",
                "UniClass-Beam*****Ss_20_20_75_35*****Steel beam systems",
                "UniClass-Column****Ss_20_30_75_35*****Steel column systems",
                "UniClass-Heavy*****Ss_20_10_75_35*****Heavy steel framing systems",
                "UniClass-Light******Ss_20_10_75_45*****Light steel framing systems",
                "********",
                "********",
                "********",
                "********"
            };

            service.WriteAllLinesWithArray(filePathLine, content, additonalString);
        }

        public static int GetLastUsedPrelim(string jobName, ExternalService.WebService1 service)
        {
            string lastUsedPrelimLine = service.ReadSpecificLine(Constants.PrismModelData, 1, Constants.ModelProjectInforLocation(jobName));
            string lastusedPrelim = lastUsedPrelimLine.Split(':')[1].Trim();
            return Convert.ToInt32(lastusedPrelim);
        }

        public static void SetLastUsedPrelim(string jobName, int lastUsedPrelim, ExternalService.WebService1 service)
        {
            string content = $"Next prelim to use: {lastUsedPrelim}";
            service.WriteToSpecificLine(Constants.PrismModelData, 1, content, Constants.ModelProjectInforLocation(jobName));
        }

        public static void UpdateFrozenDrawingCount(string jobName, int frozenDrawings, int unFrozenDrawings, ExternalService.WebService1 service)
        {
            string frozenDrawingLine = service.ReadSpecificLine(Constants.PrismModelData, 4, jobName);
            string frozenDrawingCount1 = frozenDrawingLine.Split('=')[1].Trim();
            string frozenDrawingCount2 = (Convert.ToInt32(frozenDrawingCount1.Split(',')[0].Trim()) + frozenDrawings).ToString();

            string unFrozenDrawingCount = (Convert.ToInt32(frozenDrawingLine.Split('=')[2].Trim()) + unFrozenDrawings).ToString();

            string content = $"Frozen drawing count: Frozen = {frozenDrawingCount2}, Un-Frozen = {unFrozenDrawingCount}";
            service.WriteToSpecificLine(Constants.PrismModelData, 4, content, jobName);
        }

        public static void AddToMaterialOrderProcessedCount(string jobName, WebService1 service)
        {
            int linetoWriteTo = 2;
            string materialProcessedLine = service.ReadSpecificLine(Constants.PrismModelData, linetoWriteTo, jobName);
            string materialProcessedCount = materialProcessedLine.Split(':')[1].Trim();
            int newMaterialProcessedCount = Convert.ToInt32(materialProcessedCount) + 1;
            string content = $"Material orders processed: {newMaterialProcessedCount}";
            service.WriteToSpecificLine(Constants.PrismModelData, linetoWriteTo, content, jobName);
        }

        public static void AddToFabCompleteCount(string jobName, WebService1 service)
        {
            int linetoWriteTo = 3;
            string fabCompleteLine = service.ReadSpecificLine(Constants.PrismModelData, linetoWriteTo, jobName);
            string fabCompleteCount = fabCompleteLine.Split(':')[1].Trim();
            int newFabCompleteCount = Convert.ToInt32(fabCompleteCount) + 1;
            string content = $"Fab packages created: {newFabCompleteCount}";
            service.WriteToSpecificLine(Constants.PrismModelData, linetoWriteTo, content, jobName);
        }

        public static void LogProgress(string modelName, string buttonPress, int autoFixCount, int totalObjects, ExternalService.WebService1 service)
        {
            if (Environment.UserName != "mark.gibson")
            {
                bool isPrelimReset = buttonPress.StartsWith("PRELIM RESET");
                string textType1 = isPrelimReset ? "Number before reset:" : "Assemblies processed:";
                string textType2 = isPrelimReset ? "Number after reset:" : "Auto-Fix count:";

                string[] content = new string[]
                {
                    "--------------------------------------------------------------------------------------------------",
                    $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}",
                    $"Button press: {buttonPress} - {textType1} {totalObjects} - {textType2} {autoFixCount}"
                };

                service.WriteAppendStringsToFile(Constants.PrismLogLocation, content, "");
                CountTimesUsed(autoFixCount, totalObjects, buttonPress, service);
                UpdateUserUseCount(Environment.UserName, service);
            }
        }

        private static void CountTimesUsed(int autoFixCount, int totalObjects, string buttonPress, ExternalService.WebService1 service)
        {
            int addToFabPacks = buttonPress == "Fab Package" ? 1 : 0;

            string[] content = service.ReadAllLinesIntoArray(Constants.PrismTotalUseLogLocation, "");

            string timesUsedLine = content[1];
            string partsUsedLine = content[2];
            string autoFixLine = content[3];
            string fabPacksMade = content[4];

            int newTimesUsed = SplitStringAndAddToNumber(timesUsedLine, 1);
            int newPartsUsed = SplitStringAndAddToNumber(partsUsedLine, totalObjects);
            int newAutoFixed = SplitStringAndAddToNumber(autoFixLine, autoFixCount);
            int newFabPack = SplitStringAndAddToNumber(fabPacksMade, addToFabPacks);

            string[] newContent = new string[]
            {
                "---------------------------This log was started on 04/04/23-------",
                $"Times used: {newTimesUsed}",
                $"Parts processed: {newPartsUsed}",
                $"Auto-Fix count: {newAutoFixed}",
                $"Fabrication packages created: {newFabPack}"
            };

            service.WriteAllLinesWithArray(Constants.PrismTotalUseLogLocation, newContent, "");

            if (newTimesUsed % 1000 == 0)
            {
                PrismWarnings.BigTimeUsage(newTimesUsed);
            }
        }

        private static int SplitStringAndAddToNumber(string stringToSplit, int numberToAdd)
        {
            string splitString = stringToSplit.Split(':')[1].Trim();
            return Convert.ToInt32(splitString) + numberToAdd;
        }

        public static void LoginMessage(string modelName, ExternalService.WebService1 service, string message)
        {
            if (Environment.UserName != "mrk.gibson")
            {
                string[] content = new string[]
                {
                    "--------------------------------------------------------------------------------------------------",
                    $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}",
                    $"{message}"
                };

                service.WriteAppendStringsToFile(Constants.PrismLoginLogLocation, content, "");
            }
        }

        public static void DebugLog(string debugText, string modelName, ExternalService.WebService1 service)
        {
            if (Environment.UserName == "ark.gibson")
            {
                string[] content = new string[]
                {
                    "--------------------------------------------------------------------------------------------------",
                    $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}",
                    $"{debugText}"
                };

                service.WriteAppendStringsToFile(Constants.PrismDebugLogLoction, content, "");
            }
        }

        public static void UnAssignedDrawings(string modelName, List<Drawing> drawingsList, ExternalService.WebService1 service)
        {
            List<string> contentList = new List<string>
            {
                "--------------------------------------------------------------------------------------------------",
                $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}"
            };

            foreach (Drawing drawing in drawingsList)
            {
                contentList.Add($"Drawing ID No: {drawing.GetIdentifier()}");
            }

            string[] content = contentList.ToArray();

            service.WriteAppendStringsToFile(Constants.PrismUnassignedDrawingsLocation, content, "");

        }
    }
}