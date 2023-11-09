using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Prism
{
    public class Launcher
    {
        #region public fields

        public bool RestartRequired;
        public bool CloseNow;

        #endregion

        #region private fields

        private static string _teklaVersion = string.Empty;
        private static string _configFile = string.Empty;
        private static List<TeklaInstall> _teklaInstalls;

        #endregion

        public Launcher()
        {
            // Find which versions of Tekla are installed
            // and check which of those are running
            FindTeklaVersions();

            // List running Tekla versions
            var list = new List<string>();
            foreach (var teklaInstall in _teklaInstalls.Where(teklaInstall => teklaInstall.Status))
            {
                _teklaVersion = teklaInstall.Version;
                list.Add(teklaInstall.Version);
            }

            // Check previously used version of Tekla
            var version = ReadTeklaVersion();

            // If the user has selected one of multiple versions
            // we add an underscore to the version name so we know
            // to just run this version
            var userSelectedVersion = version[0] == '_';

            // Check if this is the second run
            if (!userSelectedVersion)
            {
                // if not check if there are multiple versions running
                if (list.Count > 1)
                {
                    // Let user choose from multiple
                    var launcher = new LauncherForm(list);
                    var result = launcher.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        _teklaVersion = launcher.SelectedVersion;
                        userSelectedVersion = true;
                    }
                    else
                        CloseNow = true;
                }
            }
            else
            {
                // This is the second run so just run it
                version = version.Substring(1);
                _teklaVersion = version;
            }

            // Check if it matches current version
            RestartRequired = false;
            if (string.IsNullOrEmpty(_teklaVersion)) return;
            if (version != _teklaVersion)
            {
                // Versions do not match so we need to get
                // the config file for the running version
                GetConfigFile();
                if (string.IsNullOrEmpty(_configFile)) return;

                ReplaceConfigFile(_configFile);

                // Add the underscore so it runs right away on next run
                var txt = userSelectedVersion ? "_" + _teklaVersion : _teklaVersion;
                WriteToTeklaVersion(txt);

                RestartRequired = true;
            }
            else
            {
                // Write current version about to run on second attempt
                if (userSelectedVersion)
                    WriteToTeklaVersion(version);
            }
        }

        #region private classes

        /// <summary>
        /// Private class for storing Tekla install data
        /// </summary>
        private class TeklaInstall
        {
            public readonly string Version;
            public readonly string ConfigFile;
            public readonly bool Status;

            public TeklaInstall(string version, string configFile, bool status)
            {
                Version = version;
                ConfigFile = configFile;
                Status = status;
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Finds all installed versions of Tekla and checks if they are running
        /// </summary>
        private static void FindTeklaVersions()
        {
            // First get installed versions based on registry info
            try
            {
                _teklaInstalls = new List<TeklaInstall>();
                var localKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Trimble\Tekla Structures", false);
                var subKeyNames = localKey?.GetSubKeyNames();
                if (subKeyNames == null) return;
                foreach (var version in subKeyNames)
                {
                    var verKey = localKey?.OpenSubKey(version);
                    var setupKey = verKey?.OpenSubKey("setup");
                    if (setupKey == null) continue;
                    var mainDir = setupKey.GetValue("MainDir");
                    var exeFilepath = mainDir + version + $@"\nt\bin\TeklaStructures.exe";

                    if (!File.Exists(exeFilepath))
                    {
                        exeFilepath = mainDir + version + @"\bin\TeklaStructures.exe";
                    }

                    var status = ProgramIsRunning(exeFilepath);
                    var install = new TeklaInstall(version, exeFilepath + ".config", status);
                    _teklaInstalls.Add(install);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Checks if an executable file is running
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private static bool ProgramIsRunning(string fullPath)
        {
            var filePath = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileNameWithoutExtension(fullPath).ToLower();
            var pList = Process.GetProcessesByName(fileName);
            return pList.Any(program => program.MainModule.FileName.StartsWith(filePath, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Gets the config file for a specific version of Tekla
        /// </summary>
        private static void GetConfigFile()
        {
            foreach (var teklaInstall in _teklaInstalls)
            {
                if (teklaInstall.Version == _teklaVersion)
                {
                    _configFile = teklaInstall.ConfigFile;
                }
            }
        }

        /// <summary>
        /// Replaces the Polo config file from the path provided
        /// </summary>
        /// <param name="path"></param>
        private static void ReplaceConfigFile(string path)
        {
            if (File.Exists("Prism.exe.config")) File.Delete("Prism.exe.config");
            File.Copy(path, "Prism.exe.config");
        }

        /// <summary>
        /// Saves the string value to the Tekla Version file
        /// </summary>
        /// <param name="value"></param>
        private static void WriteToTeklaVersion(string value)
        {
            if (File.Exists("TeklaVersion.txt")) File.Delete("TeklaVersion.txt");
            using (var writer = new StreamWriter("TeklaVersion.txt"))
            {
                writer.WriteLine(value);
            }
        }

        /// <summary>
        /// Reads the version from the Tekla Version file
        /// </summary>
        /// <returns></returns>
        private static string ReadTeklaVersion()
        {
            string version;
            if (!File.Exists("TeklaVersion.txt"))
            {
                version = "2021.0";
            }
            else
            {
                using (var reader = new StreamReader("TeklaVersion.txt"))
                {
                    version = reader.ReadLine();
                    if (string.IsNullOrEmpty(version)) version = "1.0";
                }
            }

            return version;
        }

        #endregion
    }
}
