using System;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Model;

namespace Prism
{
	public static class Logging
	{
		private const string _primaryMaterialButtonText = "Button: Primary material checks";
		private const string _secondaryMaterialButtonText = "Button: Secondary material checks and Fabsec Processing";
		private const string _materialOrderPackageButtonText = "Button: Compile material order package";
		private const string _primaryDetailButtonText = "Button: Primary detailing checks";
		private const string _secondaryDetailButtonText = "Button: Secondary detailing checks and Column Orientation marking";
		private const string _tertiaryDetailButtonText = "Button: Tertiary detailing checks and Drawing creation";
		private const string _fabricationPackageButtonText = "Button: Compile Fabrication package";
		private const string _autoFix = "AUTO-FIX ";

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
				$"{Enums.AdvancedSettingType.FabsecGreen.ToString()}{split}",
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

		/// <summary>
		/// Retrieves the "last used Prelim" value from the project info file stored in the model data folder.
		/// This method validates the response by reading the value repeatedly until three consistent answers are received.
		/// The extra validation is necessary to handle intermittent connectivity issues that can result in inconsistent values.
		/// </summary>
		/// <param name="jobName">The project number and GUID identifying the projects Model data folder.</param>
		/// <returns>The last used prelim value as an integer.</returns>
		/// <exception cref="Exception">Thrown if a consistent value is not retrieved after the maximum number of attempts.</exception>
		public static int? GetLastUsedPrelim(string jobName)
		{
			const int requiredMatches = 3;
			const int maxAttempts = 10;
			const int delayMilliseconds = 250;

			int? lastPrelimValue = null;
			int consistentCount = 0;
			int attempts = 0;

			while (attempts < maxAttempts)
			{
				attempts++;
				string line = WebService.ReadSpecificLine(Constants.PrismModelData, 1, Constants.ModelProjectInforLocation(jobName));

				// Ensure the line is not null or whitespace.
				if (string.IsNullOrWhiteSpace(line))
				{
					// Optionally log an error here.
					System.Threading.Thread.Sleep(delayMilliseconds);
					continue;
				}

				// Validate the expected format: "Next prelim to use: (number)"
				string[] parts = line.Split(new[] { ':' }, 2);
				if (parts.Length < 2)
				{
					// Error: Unexpected format.
					System.Threading.Thread.Sleep(delayMilliseconds);
					continue;
				}

				string prelimText = parts[1].Trim();
				if (!int.TryParse(prelimText, out int currentPrelim))
				{
					// Error: Parsing failed.
					System.Threading.Thread.Sleep(delayMilliseconds);
					continue;
				}

				// Check if the current reading matches the previous one.
				if (lastPrelimValue.HasValue && lastPrelimValue.Value == currentPrelim)
				{
					consistentCount++;
				}
				else
				{
					// Reset if the value is different.
					lastPrelimValue = currentPrelim;
					consistentCount = 1;
				}

				// Return if we have enough consistent readings.
				if (consistentCount >= requiredMatches)
				{
					return lastPrelimValue.Value;
				}

				// Wait before the next attempt to avoid hammering the web service.
				System.Threading.Thread.Sleep(delayMilliseconds);
			}

			return lastPrelimValue;
		}

		public static string GetAdvancedSetting(string jobName, Enums.AdvancedSettingType settingType)
		{
			string fullSettingLine = WebService.ReadSpecificLine(Constants.PrismModelData, (int)settingType, Constants.ModelProjectAdvancedSettingLocation(jobName));
			//string fullSettingLine = WebService.ReadSpecificLine(Constants.PrismModelData, (int)settingType, Constants.ModelProjectAdvancedSettingLocation(jobName));
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


		private static void WriteSettingLine(Enums.AdvancedSettingType settingType, string jobName)
		{
			WebService.WriteToSpecificLine(Constants.PrismModelData, (int)settingType, settingType.ToString() + ":split: ", Constants.ModelProjectAdvancedSettingLocation(jobName));
		}

		/// <summary>
		/// Here we set the last used Prelim, this writes to our model data log to store the last used number, ready for next time.
		/// We validate this by waiting a short delay and reading the line back, if the re-read line does not match what should have been written
		/// we wait and try again, we do this up to 10 times if we have to, to ensure the number is properly saved.
		/// </summary>
		/// <exception cref="Exception"></exception>
		public static bool SetLastUsedPrelim(string jobName, int lastUsedPrelim)
		{
			// Build the content to be written.
			string content = $"Next prelim to use: {lastUsedPrelim}";

			// Define constants for retry logic.
			const int maxAttempts = 10;
			const int delayMilliseconds = 250;
			int attempts = 0;
			bool isWritten = false;

			while (attempts < maxAttempts && !isWritten)
			{
				attempts++;

				// Write the content to the specified line.
				WebService.WriteToSpecificLine(Constants.PrismModelData, 1, content, Constants.ModelProjectInforLocation(jobName));

				// Wait a short period to allow the write operation to complete.
				System.Threading.Thread.Sleep(delayMilliseconds);

				// Read the line back from the file.
				string readBack = WebService.ReadSpecificLine(Constants.PrismModelData, 1, Constants.ModelProjectInforLocation(jobName));

				// Compare the written content with what was read (trim extra whitespace for safety).
				if (readBack.Trim() == content.Trim())
				{
					isWritten = true;
				}
			}

			// If after maxAttempts the content still doesn't match, the write has failed.
			if (!isWritten)
			{
				PrismWarnings.PrelimSaveFailure();
				return false;
			}

			return true;
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

		public static void AddToFabCompleteCount()
		{
			int linetoWriteTo = 5;
			string fabCompleteLine = WebService.ReadSpecificLine(Constants.PrismTotalUseLogLocation, linetoWriteTo, "");
			string fabCompleteCount = fabCompleteLine.Split(':')[1].Trim();
			int newFabCompleteCount = Convert.ToInt32(fabCompleteCount) + 1;
			string content = $"Fab packages created: {newFabCompleteCount}";
			WebService.WriteToSpecificLine(Constants.PrismTotalUseLogLocation, linetoWriteTo, content, "");
		}

		public static void LogProgress(string modelName, string buttonPress, int autoFixCount, int totalObjects)
		{
			//if (Environment.UserName != "mark.gibson")
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

				LogToDataBase(buttonPress, totalObjects, autoFixCount);
				CountTimesUsed(autoFixCount, totalObjects, buttonPress);
				UpdateUserUseCount(Environment.UserName);
			}
		}

		private static void LogToDataBase(string buttonPress, int totalObjects, int autoFixCount)
		{
			bool isAutoFix = autoFixCount > 0;
			string buttonPressText = DetermineButtonPress(buttonPress, false);

			string emailString = "@severfield.com";
			if (Environment.UserDomainName != "SFRPLC") { emailString = "@external.com"; }
			string userName = Environment.UserName;

			WebService.AddRecordToAppUseDatabase(userName, "Prism", buttonPressText, userName + emailString, totalObjects);
			if (isAutoFix)
			{
				buttonPressText = DetermineButtonPress(buttonPress, true);
				WebService.AddRecordToAppUseDatabase(userName, "Prism", buttonPressText, userName + emailString, autoFixCount);
			}
		}

		private static string DetermineButtonPress(string buttonPress, bool isAutoFix)
		{
			string autoFixText = isAutoFix ? _autoFix : "";

			switch (true)
			{
				case true when buttonPress == "Material 1":
					return InsertAutoFix(_primaryMaterialButtonText, autoFixText);

				case true when buttonPress.Contains("Material 2"):
					return InsertAutoFix(_secondaryMaterialButtonText, autoFixText);

				case true when buttonPress.Contains("Material 3"):
					return InsertAutoFix(_materialOrderPackageButtonText, autoFixText);

				case true when buttonPress.Contains("Detail 1"):
					return InsertAutoFix(_primaryDetailButtonText, autoFixText);

				case true when buttonPress.Contains("Detail 2"):
					return InsertAutoFix(_secondaryDetailButtonText, autoFixText);

				case true when buttonPress.Contains("Detail 3"):
					return InsertAutoFix(_tertiaryDetailButtonText, autoFixText);

				case true when buttonPress.Contains("FAB") || buttonPress.Contains("Rocket"):
					return InsertAutoFix(_fabricationPackageButtonText, autoFixText);

				default:
					return "ERROR";
			}
		}

		private static string InsertAutoFix(string originalText, string autoFixText)
		{
			// Replace "Button:" with "Button{autoFixText}:"
			return originalText.Replace("Button:", $"{autoFixText}Button:");
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
			// if (Environment.UserName == "mark. gibson")
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