using System;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Drawing;
using Tekla.Structures.DrawingInternal;
using Tekla.Structures.Model;

namespace Prism
{

    public static class Logging
    {
        public static void UpdateUserUseCount(string userName)
        {
            Dictionary<string, int> userCounts = new Dictionary<string, int>();

            // Read existing log
            foreach (string line in WebService.ReadAllLinesIntoArray(Constants.PrismUserUserLogLocation, ""))
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
            List<string> newContent = new List<string>{"---------------------------This log was started on 21/08/23-------"};
            foreach (var entry in userCounts)
            {
                newContent.Add($"{entry.Key}-{entry.Value}");
            }

            // Write updated log using your methods
            WebService.WriteAllLinesWithArray(Constants.PrismUserUserLogLocation, newContent.ToArray(), "");
        }

        public static void CreateModelLog(PrismProjectData pData)
        {
            //this method checks the model data folder on our server for a folder named after the users current model, if it does not exist we create it
            bool newLogExists = false;
            bool oldLogExists = false;
            var test = WebService.GetDirectories(Constants.PrismDataLogLocation, "");
            foreach (string folder in WebService.GetDirectories(Constants.PrismDataLogLocation, ""))
            {
                if (WebService.FileExists(folder, 8, pData.ProjNumberAndGuid))
                {
                    newLogExists = true;
                    break;
                }
            }
            if (!newLogExists)
            {
                foreach (string folder in WebService.GetDirectories(Constants.PrismDataLogLocation, ""))
                {
                    if (WebService.FileExists(folder, 8, pData.ProjNumberAndName))
                    {
                        oldLogExists = true;
                        break;
                    }
                }
                if (oldLogExists)
                {
                    CreateNewLogUsingOldLog(pData);
                }
            }

            if (!newLogExists && !oldLogExists)
            {
                WebService.CreateNewDirectory(Constants.PrismModelData, pData.ProjNumberAndGuid);

                WriteFirstDataLog(Constants.PrismModelData, Constants.ModelProjectInforLocation(pData.ProjNumberAndGuid), pData.pInfo);

                PrismWarnings.FirstTimeInTheModel();
            }
        }

        private static void CreateNewLogUsingOldLog(PrismProjectData pData)
        {
            WebService.CreateNewDirectory(Constants.PrismModelData, pData.ProjNumberAndGuid);

            string[] array = WebService.ReadAllLinesIntoArray(Constants.PrismDataLogLocation, Constants.ModelProjectInforLocation(pData.ProjNumberAndName));

            // Create a new array with additional slots for 4 more strings
            string[] newArray = new string[array.Length + 4];

            // Copy the original elements to the new array
            Array.Copy(array, newArray, array.Length);

            // Set the last four elements to blank strings
            for (int i = array.Length; i < newArray.Length; i++)
            {
                newArray[i] = ""; // Assign a blank string
            }

            // Now newArray contains the original data plus four blank strings at the end, we now use these for project controllers.

            WebService.WriteAllLinesWithArray(Constants.PrismDataLogLocation, newArray, Constants.ModelProjectInforLocation(pData.ProjNumberAndGuid));

            string[] oldLogOverwrite = new string[] { $"File overwritten, now exists as {pData.ProjNumberAndGuid}\r", "Old info before copy:\r" };

            // Concatenate oldLogOverwrite and array
            string[] combinedArray = oldLogOverwrite.Concat(array).ToArray();

            WebService.WriteAllLinesWithArray(Constants.PrismDataLogLocation, combinedArray, Constants.ModelProjectInforLocation(pData.ProjNumberAndName));
        }

        public static void WriteFirstDataLog(int filePathLine, string additonalString, ProjectInfo pInfo)
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
                "********",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                ""
            };

            WebService.WriteAllLinesWithArray(filePathLine, content, additonalString);
        }

        public static int GetLastUsedPrelim(string jobName)
        {
            string lastUsedPrelimLine = WebService.ReadSpecificLine(Constants.PrismModelData, 1, Constants.ModelProjectInforLocation(jobName));
            string lastusedPrelim = lastUsedPrelimLine.Split(':')[1].Trim();
            return Convert.ToInt32(lastusedPrelim);
        }

        public static void SetLastUsedPrelim(string jobName, int lastUsedPrelim)
        {
            string content = $"Next prelim to use: {lastUsedPrelim}";
            WebService.WriteToSpecificLine(Constants.PrismModelData, 1, content, Constants.ModelProjectInforLocation(jobName));
        }

        public static void UpdateFrozenDrawingCount(string jobName, int frozenDrawings, int unFrozenDrawings)
        {
            string frozenDrawingLine = WebService.ReadSpecificLine(Constants.PrismModelData, 4, Constants.ModelProjectInforLocation(jobName));
            string frozenDrawingCount1 = frozenDrawingLine.Split('=')[1].Trim();
            string frozenDrawingCount2 = (Convert.ToInt32(frozenDrawingCount1.Split(',')[0].Trim()) + frozenDrawings).ToString();

            string unFrozenDrawingCount = (Convert.ToInt32(frozenDrawingLine.Split('=')[2].Trim()) + unFrozenDrawings).ToString();

            string content = $"Frozen drawing count: Frozen = {frozenDrawingCount2}, Un-Frozen = {unFrozenDrawingCount}";
            WebService.WriteToSpecificLine(Constants.PrismModelData, 4, content, Constants.ModelProjectInforLocation(jobName));
        }

        public static void AddToMaterialOrderProcessedCount(string jobName)
        {
            int linetoWriteTo = 2;
            string materialProcessedLine = WebService.ReadSpecificLine(Constants.PrismModelData, linetoWriteTo, jobName);
            string materialProcessedCount = materialProcessedLine.Split(':')[1].Trim();
            int newMaterialProcessedCount = Convert.ToInt32(materialProcessedCount) + 1;
            string content = $"Material orders processed: {newMaterialProcessedCount}";
            WebService.WriteToSpecificLine(Constants.PrismModelData, linetoWriteTo, content, jobName);
        }

        public static void AddToFabCompleteCount(string jobName)
        {
            int linetoWriteTo = 3;
            string fabCompleteLine = WebService.ReadSpecificLine(Constants.PrismModelData, linetoWriteTo, jobName);
            string fabCompleteCount = fabCompleteLine.Split(':')[1].Trim();
            int newFabCompleteCount = Convert.ToInt32(fabCompleteCount) + 1;
            string content = $"Fab packages created: {newFabCompleteCount}";
            WebService.WriteToSpecificLine(Constants.PrismModelData, linetoWriteTo, content, jobName);
        }

        public static void LogProgress(string modelName, string buttonPress, int autoFixCount, int totalObjects)
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

                WebService.WriteAppendStringsToFile(Constants.PrismLogLocation, content, "");
                CountTimesUsed(autoFixCount, totalObjects, buttonPress);
                UpdateUserUseCount(Environment.UserName);
            }
        }

        private static void CountTimesUsed(int autoFixCount, int totalObjects, string buttonPress)
        {
            int addToFabPacks = buttonPress == "Fab Package" ? 1 : 0;

            string[] content = WebService.ReadAllLinesIntoArray(Constants.PrismTotalUseLogLocation, "");

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

            if (newTimesUsed % 1000 == 0)
            {
                string[] content2 = new string[]
               {
                    "--------------------------------------------------------------------------------------------------",
                    $"{DateTime.Now} - User: {Environment.UserName}",
                    $"Times used = {newTimesUsed}"
               };

                WebService.WriteAppendStringsToFile(Constants.PrismLogLocation, content2, "");
                PrismWarnings.BigTimeUsage(newTimesUsed);
            }

            WebService.WriteAllLinesWithArray(Constants.PrismTotalUseLogLocation, newContent, "");
        }

        private static int SplitStringAndAddToNumber(string stringToSplit, int numberToAdd)
        {
            string splitString = stringToSplit.Split(':')[1].Trim();
            return Convert.ToInt32(splitString) + numberToAdd;
        }

        public static void LoginMessage(string modelName, string message)
        {
            if (Environment.UserName != "mark. gibson")
            {
                string[] content = new string[]
                {
                    "--------------------------------------------------------------------------------------------------",
                    $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}",
                    $"{message}"
                };

                WebService.WriteAppendStringsToFile(Constants.PrismLoginLogLocation, content, "");
            }
        }

        public static void DebugLog(string debugText, string modelName)
        {
            if (Environment.UserName == "mark. gibson")
            {
                string[] content = new string[]
                {
                    "--------------------------------------------------------------------------------------------------",
                    $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}",
                    $"{debugText}"
                };

                WebService.WriteAppendStringsToFile(Constants.PrismDebugLogLoction, content, "");
            }
        }

        public static void UnAssignedDrawings(string modelName, List<Drawing> drawingsList)
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

            WebService.WriteAppendStringsToFile(Constants.PrismUnassignedDrawingsLocation, content, "");

        }
    }
}