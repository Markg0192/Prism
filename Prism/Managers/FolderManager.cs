using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Prism
{
    /// <summary>
    /// The folder manager class is where all folders required for a package is created.
    /// The Create folders method creates all folders that may be required (even if they aren't) because of this we also have he remove 
    /// unused folders method which runs at the end of the process to clear out any unused folders.
    /// </summary>
    public class FolderManager
    {
        private const string _fabsecCarcasses = "PGC";
        private const string _specialFittings = "SPC";
        private const string _assFolder = "ASS";
        private const string _fitFolder = "FIT";
        private const string _prtFolder = "PRT";
        private const string _dspFolder = "DSP";
        private const string _wldFolder = "WLD";
        private const string _shaftFolder = "SHA";
        private const string _ncFolder = "NC";
        private const string _reportFolder = "Lists";
        private const string _ifcFolder = "IFC";
        private List<string> _folderNames;

        public FolderManager(PrismProjectData projectData, string phaseNum, string issueNum)
        {
            PrismFileLocaton = projectData.ProjPath + "\\" + Constants.PrismPackageFolderName;
            QrCodePath = PrismFileLocaton + "\\" + "QR Codes";
            if(!Directory.Exists(PrismFileLocaton)) { Directory.CreateDirectory(PrismFileLocaton); }
            if(!Directory.Exists(QrCodePath)) { Directory.CreateDirectory(QrCodePath); }
            FabFolder = $"{projectData.ProjNumber}-{phaseNum}-FAB-ISSUE{issueNum}";
            MatFolder = $"{projectData.ProjNumber}-{phaseNum}-PRELIM-ISSUE{issueNum}";
            EpoFolder = $"{projectData.ProjNumber}-{phaseNum}-EPO-ISSUE{issueNum}";
            FabsecCarcassFolder = $"{projectData.ProjNumber}-{phaseNum}-FABSEC-ISSUE{issueNum}";
            string boltFolder = $"{projectData.ProjNumber}-{phaseNum}-BOLT-ISSUE{issueNum}";
            FabPath = Path.Combine(PrismFileLocaton, FabFolder);
            MatPath = Path.Combine(PrismFileLocaton, MatFolder);
            EpoPath = Path.Combine(PrismFileLocaton, EpoFolder);
            BoltPath = Path.Combine(PrismFileLocaton, boltFolder);
            CarcassOrderPath = Path.Combine(PrismFileLocaton, FabsecCarcassFolder);
            string assPath = Path.Combine(FabPath, _assFolder);
            string fitPath = Path.Combine(FabPath, _fitFolder);
            string prtPath = Path.Combine(FabPath, _prtFolder);
            FabsecCarcassPath = Path.Combine(CarcassOrderPath, _fabsecCarcasses);
            SpecialFittingPath = Path.Combine(MatPath, _specialFittings);
            IfcPath = Path.Combine(FabPath, _ifcFolder);
            NcPath = Path.Combine(FabPath, _ncFolder);
            ReportPath = Path.Combine(FabPath, _reportFolder);
            DspPath = Path.Combine(FabPath, _dspFolder);
            WldPath = Path.Combine(FabPath, _wldFolder);

            ShaftPath = Path.Combine(FabPath, _shaftFolder);
            _folderNames = new List<string>
                {assPath, fitPath, prtPath, DspPath, NcPath, ReportPath, ShaftPath, IfcPath, WldPath};
            DrawingVaultFolders = new List<string>
            { _assFolder, _prtFolder, _fitFolder, _shaftFolder, _ifcFolder};
        }

        public string FabFolder { get; set; }
        public string MatFolder {get;set;}
        public string EpoFolder {get;set;}
        public string FabsecCarcassFolder { get;set;}
        public string QrCodePath { get; set; }
        public string FabPath {get;set;}
        public string MatPath {get;set;}
        public string EpoPath {get;set;}
        public string BoltPath {get;set;}
        public string CarcassOrderPath { get; set;}
        public string NcPath {get;set;}
        public string ReportPath {get;set;}
        public string DspPath {get;set;}
        public string WldPath {get;set;}
        public string ShaftPath {get;set;}
        public string FabsecCarcassPath {get;set;}
        public string SpecialFittingPath {get;set;}
        public string IfcPath {get;set;}
        private string ProjectLocation;
        private string PrismFileLocaton { get; set; }

        private List<string> DrawingVaultFolders = new List<string>();

        public bool CreateFabFolders()
        {
            if (!CheckAndDeleteFolder(FabPath)) return false;

            foreach (string folder in _folderNames)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            return true;
        }

        public bool CreateDrawingVaultFolders(string vaultFolder)
        {
            foreach (string folder in DrawingVaultFolders)
            {
                if (!Directory.Exists($"{vaultFolder}/{folder}"))
                {
                    Directory.CreateDirectory($"{vaultFolder}/{folder}");
                }
            }
            return true;
        }

        public bool CreateMatFolder(bool fabsecsPresent, bool specialFittingsPresent = false)
        {
            if (!CheckAndDeleteFolder(MatPath)) return false;

            Directory.CreateDirectory(MatPath);
            if (fabsecsPresent)
            {
                Directory.CreateDirectory(FabsecCarcassPath);
            }
            if(specialFittingsPresent)
            {
                Directory.CreateDirectory(SpecialFittingPath);
            }
            return true;
        }

        public bool CreateFabsecCarcassFolder()
        {
            if (!CheckAndDeleteFolder(CarcassOrderPath)) return false;

            Directory.CreateDirectory(CarcassOrderPath);
            Directory.CreateDirectory(CarcassOrderPath + "\\PGC");
            return true;
        }

        public bool CreateBoltFolder()
        {
            if (!CheckAndDeleteFolder(BoltPath)) return false;

            Directory.CreateDirectory(BoltPath);
            return true;
        }

        public bool CreateEpoFolder()
        {
            if (!CheckAndDeleteFolder(EpoPath)) return false;

            Directory.CreateDirectory(EpoPath);
            return true;
        }

        private bool CheckForExistingFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                if(PrismWarnings.FolderAlreadyExists(folderPath))
                {
                   Directory.Delete(folderPath, true);
                    return true;
                }
                return false;
            }
            var zip = Directory.GetFiles(PrismFileLocaton, "*.zip");
            if (zip.Contains($"{folderPath}.zip"))
            {
                if(PrismWarnings.FolderAlreadyExists($"{folderPath}.zip"))
                {
                    File.Delete($"{folderPath}.zip");
                }
                return false;
            }
            return true;
        }

        private bool CheckAndDeleteFolder(string folderPath)
        {
            bool folderExists = Directory.Exists(folderPath);
            string zipFilePath = $"{folderPath}.zip";
            bool zipExists = File.Exists(zipFilePath);

            // If neither the folder nor the zip file exists, return true indicating no conflicts
            if (!folderExists && !zipExists)
            {
                return true;
            }

            // If either the folder or the zip file exists, ask the user for confirmation to delete
            if (folderExists || zipExists)
            {
                // Update the message to indicate both or either one exists
                string message = folderExists && zipExists ?
                                 $"Both the folder '{folderPath}' and its zipped version exist." :
                                 folderExists ?
                                 $"The folder '{folderPath}' exists." :
                                 $"The zipped version of the folder '{folderPath}' exists.";

                // Add this to your existing confirmation method or modify it to handle this case
                if (PrismWarnings.FolderAlreadyExists(message))
                {
                    try
                    {
                        // Delete the folder if it exists
                        if (folderExists)
                        {
                            Directory.Delete(folderPath, true); // true to delete recursively
                        }

                        // Delete the zip file if it exists
                        if (zipExists)
                        {
                            File.Delete(zipFilePath);
                        }

                        return true; // Indicate that the deletion was successful
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions, such as permission issues or IO errors
                        Console.WriteLine($"Error during deletion: {ex.Message}");
                        return false; // Indicate that there was an issue with deletion
                    }
                }
            }

            return false; // User chose not to delete, or there was an issue
        }

        //This method is not used yet, it will be required when drawing printing is enabled.
        public void RemoveUnusedFolders()
        {
            if (Directory.Exists(FabPath))
            {
                foreach (string subdirectory in Directory.GetDirectories(FabPath))
                {
                    string[] file = Directory.GetFiles(subdirectory, "*.*");
                    if (file.Length == 0)
                    {
                        Directory.Delete(subdirectory);
                    }
                }

                foreach (string subFile in Directory.GetFiles(ReportPath))
                {
                    if (subFile.Substring(subFile.Length - 3) == "dpm")
                    {
                        File.Delete(subFile);
                    }
                }
                ModelModifiers.RemoveLog(DspPath);
            }
        }

        public bool ZipFolder(string folderPath)
        {
            int bytesAllowedOnAttachment = 40000000; //approx 39500KB
            string zipFilePath = $"{folderPath}.zip";
            if (Directory.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
            ZipFile.CreateFromDirectory(folderPath, zipFilePath);

            FileInfo zipFileInfo = new FileInfo(zipFilePath);
            long zipFolderSizeInBytes = zipFileInfo.Length;
            return zipFolderSizeInBytes < bytesAllowedOnAttachment;
        }
    }
}