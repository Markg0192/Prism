using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.Win32;
using Prism;

namespace Prism
{
    public class Launcher
    {
        #region public fields

        private static bool CloseNow;

        #endregion

        #region private fields

        private static string _parentExePath;
        private static string _teklaVersion = string.Empty;
        private static string _configFile = string.Empty;
        private static List<TeklaInstall> _teklaInstalls;

        private static bool _restart;
        private static Assembly _assembly;

        #endregion

        public static void CheckTekla()
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
                //_teklaVersion = version;
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

                    launcher.Close();
                }
            }
            else
            {
                // This is the second run so just run it
                version = version.Substring(1);
                _teklaVersion = version;
            }

            // Get the config file for the running version
            _restart = false;
            if (string.IsNullOrEmpty(_teklaVersion)) return;
            GetConfigFile();
            if (string.IsNullOrEmpty(_configFile))
            {
                MessageBox.Show("Error: Cannot find config for Tekla " + _teklaVersion);
                return;
            }
            ReplaceConfigFile(_configFile);

            // Check if it matches current version
            if (version != _teklaVersion)
            {
                // Versions do not match so we need to
                // add the underscore so it runs right away on next run
                var txt = userSelectedVersion ? "_" + _teklaVersion : _teklaVersion;
                WriteToTeklaVersion(txt);

                _restart = true;
            }
            else
            {
                // Write current version about to run on second attempt
                if (userSelectedVersion)
                    WriteToTeklaVersion(version);
            }

            if (CloseNow)
            {
                foreach (var process in Process.GetProcessesByName(_assembly.GetName().Name))
                {
                    if (process.MainModule?.FileName != _parentExePath) continue;
                    process.Kill();
                    break;
                }
            }
        }

        public static bool Launch()
        {
            // Get parent assembly path
            _assembly = Assembly.GetCallingAssembly();
            _parentExePath = _assembly.Location;

            CheckTekla();

            var ok = true;
            if (_restart)
            {
                Process.Start(_parentExePath);
                ok = false;
            }

            return ok;
        }

        public static void Launch(Form runForm)
        {
            // Get parent assembly path
            _assembly = Assembly.GetCallingAssembly();
            _parentExePath = _assembly.Location;

            CheckTekla();

            if (!_restart)
                Application.Run(runForm);
            else
                Process.Start(_parentExePath);
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
                    var exeFilepath = mainDir + version + @"\nt\bin\TeklaStructures.exe";
                    if (!File.Exists(exeFilepath)) exeFilepath = mainDir + version + @"\bin\TeklaStructures.exe";
                    if (File.Exists(exeFilepath))
                    {
                        var status = ProgramIsRunning(exeFilepath);
                        var install = new TeklaInstall(version, exeFilepath + ".config", status);
                        _teklaInstalls.Add(install);
                    }
                    else
                    {
                        var install = new TeklaInstall(version, "Exe not found at '" + exeFilepath + "'", false);
                        _teklaInstalls.Add(install);
                    }
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
            var configPath = _parentExePath + ".config";
            if (File.Exists(configPath)) File.Delete(configPath);
            //File.Copy(path, configPath);

            using (var stream = File.OpenRead(path))
            using (var writeStream = File.OpenWrite(configPath))
            {
                // create a buffer to hold the bytes 
                var buffer = new byte[1024];
                int bytesRead;

                // while the read method returns bytes
                // keep writing them to the output stream
                while ((bytesRead = stream.Read(buffer, 0, 1024)) > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                }
            }
        }

        /// <summary>
        /// Saves the string value to the Tekla Version file
        /// </summary>
        /// <param name="value"></param>
        private static void WriteToTeklaVersion(string value)
        {
            var name = _assembly.GetName();
            var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Severfield\\" + name.Name + "\\");
            key?.SetValue("TeklaVersion", value);
        }

        /// <summary>
        /// Reads the version from the Tekla Version file
        /// </summary>
        /// <returns></returns>
        private static string ReadTeklaVersion()
        {
            var version = "2021.0";

            try
            {
                var name = _assembly.GetName();
                using (var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Severfield\\" + name.Name + "\\"))
                {
                    if (key != null)
                    {
                        var o = key.GetValue("TeklaVersion");
                        if (o != null)
                        {
                            version = o as string;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return version;
        }

        #endregion
    }
}
