using Newtonsoft.Json;
using System;
using System.IO;

namespace Prism.Validation
{
	public static class VersionValidation
	{
		public static void ValidateAppVersion()
		{
			RunVersionValidation();
		}

		private static readonly string _noticeFilePath = "C:\\temp\\update_notice.txt";
			private const int _gracePeriodDays = 7;
		private static readonly int _latestVersionLine = 13; //this is the line number on DevServerLog for the path of the latest version log

		/// <summary>
		/// Version Validation uses the webservice to check for a version using a file in the logging locations.
		/// This should contain a version for our app, Major and Minor build umbers only (eg 5.1).
		/// If the version found == the version of the app, then continue.
		/// If the version found is newer than the version of the app, start a 7 day clock warning the user to update
		/// If the version found is older than the version of the app, update the text file to match the version of the app.
		/// </summary>
		private static void RunVersionValidation()
		{
			Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			string currentVersion = $"{version.Major}.{version.Minor}";

			try
			{
				string jsonResponse = WebService.ReadSpecificLine(_latestVersionLine, 1, "");
				var versionInfo = JsonConvert.DeserializeObject<VersionInfo>(jsonResponse);
				string logVersion = versionInfo.LatestVersion;

				int versionComparison = CompareVersions(currentVersion, logVersion);

				if (versionComparison > 0)
				{
					// Current version is higher than the latest version → Update the version file
					UpdateVersionFile(currentVersion, logVersion);
				}
				else if (versionComparison < 0)
				{
					// Current version is older → Trigger warning
					HandleOutdatedVersion(logVersion, currentVersion);
				}
				else
				{
					// Versions match → Clear notice if exists
					ClearFirstNoticeDate(currentVersion);
				}
			}
			catch (Exception)
			{
				throw; // Let the UI handle the error
			}
		}

		private static int CompareVersions(string current, string latest)
		{
			Version currentVer = new Version(current + ".0");  // Append ".0" to make it valid
			Version latestVer = new Version(latest + ".0");

			return currentVer.CompareTo(latestVer); // Returns -1, 0, or 1
		}

		private static void UpdateVersionFile(string newVersion, string logVersion)
		{
			var versionInfo = new VersionInfo { LatestVersion = newVersion };
			string json = JsonConvert.SerializeObject(versionInfo);
			WebService.WriteAppendStringToFile(14, $"\r---------------\rUser {Environment.UserName} has updated the log version from {logVersion} to {newVersion}", "");
			WebService.WriteToSpecificLine(13, 1, json, ""); // Assuming this writes to the same file
		}

		private static void HandleOutdatedVersion(string latestVersion, string currentVersion)
		{
			DateTime? firstNoticeDate = GetFirstNoticeDate();

			if (firstNoticeDate == null)
			{
				SaveFirstNoticeDate(DateTime.Now);
				WebService.WriteAppendStringToFile(14, $"\r---------------\rUser {Environment.UserName}, version is out of date, theirs {currentVersion}, latest = {latestVersion}, 7 days to update.", "");
				PrismWarnings.NotUsingLatestVersion(latestVersion, currentVersion);
			}
			else
			{
				double daysLeft = (DateTime.Now - firstNoticeDate.Value).TotalDays;

				if (daysLeft > _gracePeriodDays)
				{
					WebService.WriteAppendStringToFile(14, $"\r---------------\rUser {Environment.UserName}, version is out of date, theirs {currentVersion}, latest = {latestVersion}, 0 days to update.", "");
					PrismWarnings.NotUsingLatestVersionForceUpdate(latestVersion, currentVersion);
					Environment.Exit(0); // Force update
				}
				else
				{
					int daysRemaining = _gracePeriodDays - (int)Math.Floor(daysLeft);
					WebService.WriteAppendStringToFile(14, $"\r---------------\rUser {Environment.UserName}, version is out of date, theirs {currentVersion}, latest = {latestVersion}, {daysRemaining} days to update.", "");
					PrismWarnings.NotUsingLatestVersionReminderToUpdate(latestVersion, currentVersion, daysRemaining);
				}
			}
		}

		// Helper methods
		private static DateTime? GetFirstNoticeDate()
		{
			if (File.Exists(_noticeFilePath))
			{
				if (DateTime.TryParse(File.ReadAllText(_noticeFilePath), out DateTime date))
				{
					return date;
				}
			}
			return null;
		}

		private static void SaveFirstNoticeDate(DateTime date)
		{
			if (!Directory.Exists("C:\\temp\\")) Directory.CreateDirectory("C:\\temp");
			File.WriteAllText(_noticeFilePath, date.ToString("o")); // ISO 8601 format
		}

		private static void ClearFirstNoticeDate(string newVersion)
		{
			if (File.Exists(_noticeFilePath))
			{
				WebService.WriteAppendStringToFile(14, $"\r---------------\rUser {Environment.UserName} has updated to {newVersion}.", "");
				File.Delete(_noticeFilePath);
			}
		}

		// Model for deserializing JSON
		public class VersionInfo
		{
			public string LatestVersion { get; set; }
		}
	}
}