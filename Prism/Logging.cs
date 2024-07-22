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
            List<string> newContent = new List<string> { "---------------------------This log was started on 21/08/23-------" };
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

                WebService.CreateNewDirectory(Constants.PrismModelData, pData.ProjNumberAndGuid + "\\FAB XMLs");

                WriteFirstDataLog(Constants.PrismModelData, Constants.ModelProjectInforLocation(pData.ProjNumberAndGuid), pData.pInfo);

                WriteAdvancedSettings(Constants.PrismModelData, Constants.ModelProjectAdvancedSettingLocation(pData.ProjNumberAndGuid));
                
                PrismWarnings.FirstTimeInTheModel();
            }
        }

        public static void WriteAdvancedSettings(int filePathLine, string additonalString)
        {
            string split = ":split: ";
            string[] content = new string[]
            {
                $"{Enums.AdvancedSettingType.PrelimPrefix.ToString()}{split} ",
                $"{Enums.AdvancedSettingType.FabPackType.ToString()}{split} By Phase",
                $"{Enums.AdvancedSettingType.DirectoryMaterial.ToString()}{split} ",
                $"{Enums.AdvancedSettingType.DirectoryCarcasses.ToString()}{split} ",
                $"{Enums.AdvancedSettingType.DirectoryBolts.ToString()}{split} ",
                $"{Enums.AdvancedSettingType.DirectorySeversafe.ToString()}{split} ",
                $"{Enums.AdvancedSettingType.DirectoryFabPack.ToString()}{split} ",
                $"{Enums.AdvancedSettingType.DirectoryVariation.ToString()}{split} ",
                $"",
                $"",
                $"",
                $"",
                $"",
                $"",
                $"",
                $"",
                $"",
                $"",
                $""
            };

            WebService.WriteAllLinesWithArray(filePathLine, content, additonalString);
        }

        private static void CreateNewLogUsingOldLog(PrismProjectData pData)
        {
            WebService.CreateNewDirectory(Constants.PrismModelData, pData.ProjNumberAndGuid);

            string[] array = WebService.ReadAllLinesIntoArray(Constants.PrismDataLogLocation, Constants.ModelProjectInforLocation(pData.ProjNumberAndName));

            string[] newArray = UpdateToNewLayout(array);

            WebService.WriteAllLinesWithArray(Constants.PrismDataLogLocation, newArray, Constants.ModelProjectInforLocation(pData.ProjNumberAndGuid));

            string[] oldLogOverwrite = new string[] { $"File overwritten, now exists as {pData.ProjNumberAndGuid}\r", "Old info before copy:\r" };

            // Concatenate oldLogOverwrite and array
            string[] combinedArray = oldLogOverwrite.Concat(array).ToArray();

            WebService.WriteAllLinesWithArray(Constants.PrismDataLogLocation, combinedArray, Constants.ModelProjectInforLocation(pData.ProjNumberAndName));

            WebService.CreateNewDirectory(Constants.PrismModelData, pData.ProjNumberAndGuid + "\\FAB XMLs");
        }

        private static string[] UpdateToNewLayout(string[] array)
        {
            if (array.Length < 20)
            {
                // Resize the array to length 15
                Array.Resize(ref array, 22);
            }

            if (array.Length > 5 && array[5] != "------------Uniclass Codes----------------")
            {
                array[4] = "";
                array[5] = "------------Uniclass Codes----------------";
                array[6] = "---Filter------------Code----------------Title";
                array[7] = "UniClass-Beam*****Ss_20_20_75_35*****Steel beam systems";
                array[8] = "UniClass-Column****Ss_20_30_75_35*****Steel column systems";
                array[9] = "UniClass-Heavy*****Ss_20_10_75_35*****Heavy steel framing systems";
                array[10] = "UniClass-Light******Ss_20_10_75_45*****Light steel framing systems";
                for (int i = 11; i < 15; i++)
                {
                    array[i] = "********";
                }

                for (int i = 15; i < array.Length; i++)
                {
                    array[i] = "";
                }
            }
            return array;
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

        public static string GetAdvancedSetting(string jobName, Enums.AdvancedSettingType settingType)
        {
            string fullSettingLine = WebService.ReadSpecificLine(Constants.PrismModelData, (int)settingType, Constants.ModelProjectAdvancedSettingLocation(jobName));

            if (string.IsNullOrEmpty(fullSettingLine))
            {
                WriteSettingLine(settingType, jobName);
                fullSettingLine = WebService.ReadSpecificLine(Constants.PrismModelData, (int)settingType, Constants.ModelProjectAdvancedSettingLocation(jobName));
            }

            string lastusedPrelim = ExtractSettingValue(fullSettingLine);
            return lastusedPrelim;
        }

        private static string ExtractSettingValue(string fullSettingLine)
        {
            const string splitMarker = ":split:";

            if (fullSettingLine.Contains(splitMarker))
            {
                string[] setting = fullSettingLine.Split(new string[] { splitMarker }, StringSplitOptions.None);
                return setting.Length > 1 ? setting[1].Trim() : string.Empty;
            }
            else
            {
                int firstColonIndex = fullSettingLine.IndexOf(':');
                return firstColonIndex != -1 ? fullSettingLine.Substring(firstColonIndex + 1).Trim() : string.Empty;
            }
        }


        private static void WriteSettingLine(Enums.AdvancedSettingType settingType,string jobName)
        {
            WebService.WriteToSpecificLine(Constants.PrismModelData, (int)settingType, settingType.ToString() + ":split: ", Constants.ModelProjectAdvancedSettingLocation(jobName));
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
            if (Environment.UserName != "mark.gibson")
            {
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string[] content = new string[]
                {                    
                    "--------------------------------------------------------------------------------------------------",
                    $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}",
                    $"Version {version} - {message}"
                };

                WebService.WriteAppendStringsToFile(Constants.PrismLoginLogLocation, content, "");
            }
        }

        public static void NCFailed(string modelName, string phaseNumber, string issueNumber, string teklaVersion, string ncLocation, int totalNcRequired, int totalNcCreated)
        {
            if (Environment.UserName != "mark.gibson")
            {
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string[] content = new string[]
                {
                    "--------------------------------------------------------------------------------------------------",
                    $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}",
                    $"Tekla version {teklaVersion}",
                    $"Phase - {phaseNumber} Issue - {issueNumber}",
                    $"NC Failed - Prism {version}",
                    $"NC Location - {ncLocation}",
                    $"NC Required  {totalNcRequired} - Nc Created {totalNcCreated}"
                };

                WebService.WriteAppendStringsToFile(11, content, "");
            }
        }

        public static void NCCreated(string modelName, string phaseNumber, string issueNumber, string teklaVersion, string ncLocation, int totalNcRequired, int totalNcCreated)
        {
            if (Environment.UserName != "mark.gibson")
            {
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string[] content = new string[]
                {
                    "--------------------------------------------------------------------------------------------------",
                    $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}",
                    $"Tekla version {teklaVersion}",
                    $"Phase - {phaseNumber} Issue - {issueNumber}",
                    $"NC Created - Prism {version}",
                    $"NC Location - {ncLocation}",
                    $"NC Required  {totalNcRequired} - Nc Created {totalNcCreated}"
                };

                WebService.WriteAppendStringsToFile(10, content, "");
            }
        }

        public static void ExceptionError(string modelName, string teklaVersion, string message, string stackTrace)
        {
           // if (Environment.UserName != "mark.gibson")
            {
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string[] content = new string[]
                {
                    "",
                    "---------------------------------------------------------------------------------------------------------",
                    $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName} - Tekla version: {teklaVersion}",
                    $"Tekla version {teklaVersion}",
                    $"NC Created - Prism {version}",
                    "-----------------------------------",
                    message,
                    "-----------------------------------",
                    stackTrace
                };

                WebService.WriteAppendStringsToFile(12, content, "");
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

        public static void UnAssignedDrawings(string modelName, DrawingManager dm)
        {
            List<string> contentList = new List<string>
            {
                "--------------------------------------------------------------------------------------------------",
                $"{DateTime.Now} - User: {Environment.UserName} - Model: {modelName}"
            };

            foreach (PrismDrawing drawing in dm.GetDrawingFolder(Enums.DrawingFolder.Default))
            {
                contentList.Add($"Drawing ID No: {drawing.DrawingPartName}");
                contentList.Add($"Drawing type: {drawing.DrawingType}");
            }

            int assCount = dm.GetDrawingFolder(Enums.DrawingFolder.ASS).Count;
            if (assCount != 0) contentList.Add($"ASS Drawings Found: {assCount}");
            int fitCount = dm.GetDrawingFolder(Enums.DrawingFolder.FIT).Count;
            if (fitCount != 0) contentList.Add($"FIT Drawings Found: {fitCount}");
            int prtCount = dm.GetDrawingFolder(Enums.DrawingFolder.PRT).Count;
            if (prtCount != 0) contentList.Add($"PRT Drawings Found: {prtCount}");
            int shaCount = dm.GetDrawingFolder(Enums.DrawingFolder.SHA).Count;
            if (shaCount != 0) contentList.Add($"SHA Drawings Found: {shaCount}");
            int pgcCount = dm.GetDrawingFolder(Enums.DrawingFolder.PGC).Count;
            if (pgcCount != 0) contentList.Add($"PGC Drawings Found: {pgcCount}");
            int wldCount = dm.GetDrawingFolder(Enums.DrawingFolder.WLD).Count;
            if (wldCount != 0) contentList.Add($"WLD Drawings Found: {wldCount}");

            string[] content = contentList.ToArray();

            WebService.WriteAppendStringsToFile(Constants.PrismUnassignedDrawingsLocation, content, "");

        }
    }
}