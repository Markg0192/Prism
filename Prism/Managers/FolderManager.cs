using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Tekla.Structures.Model;

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
        private const string _shaftFolder = "SHA";
        private const string _ncFolder = "NC";
        private const string _reportFolder = "Lists";
        private const string _ifcFolder = "IFC";
        private List<string> _folderNames;

        public FolderManager(PrismProjectData projectData, string phaseNum, string issueNum)
        {
            ProjectLocation = projectData.ProjPath;
            FabFolder = $"{projectData.ProjNumber}-{phaseNum}-FAB-ISSUE{issueNum}";
            MatFolder = $"{projectData.ProjNumber}-{phaseNum}-PRELIM-ISSUE{issueNum}";
            EpoFolder = $"{projectData.ProjNumber}-{phaseNum}-EPO-ISSUE{issueNum}";
            string boltFolder = $"{projectData.ProjNumber}-{phaseNum}-BOLT-ISSUE{issueNum}";
            FabPath = Path.Combine(projectData.ProjPath, FabFolder);
            MatPath = Path.Combine(projectData.ProjPath, MatFolder);
            EpoPath = Path.Combine(projectData.ProjPath, EpoFolder);
            BoltPath = Path.Combine(projectData.ProjPath, boltFolder);
            string assPath = Path.Combine(FabPath, _assFolder);
            string fitPath = Path.Combine(FabPath, _fitFolder);
            string prtPath = Path.Combine(FabPath, _prtFolder);
            FabsecCarcassPath = Path.Combine(MatPath, _fabsecCarcasses);
            SpecialFittingPath = Path.Combine(MatPath, _specialFittings);
            IfcPath = Path.Combine(FabPath, _ifcFolder);
            NcPath = Path.Combine(FabPath, _ncFolder);
            ReportPath = Path.Combine(FabPath, _reportFolder);
            DspPath = Path.Combine(FabPath, _dspFolder);
            ShaftPath = Path.Combine(FabPath, _shaftFolder);
            _folderNames = new List<string>
                {assPath, fitPath, prtPath, DspPath, NcPath, ReportPath, ShaftPath, IfcPath};
            DrawingVaultFolders = new List<string>
            { _assFolder, _prtFolder, _fitFolder, _shaftFolder, _ifcFolder};
        }

        public readonly string FabFolder;
        public readonly string MatFolder;
        public readonly string EpoFolder;
        public readonly string FabPath;
        public readonly string MatPath;
        public readonly string EpoPath;
        public readonly string BoltPath;
        public readonly string NcPath;
        public readonly string ReportPath;
        public readonly string DspPath;
        public readonly string ShaftPath;
        public readonly string FabsecCarcassPath;
        public readonly string SpecialFittingPath;
        public readonly string IfcPath;
        private string ProjectLocation;
        private List<string> DrawingVaultFolders = new List<string>();

        public bool CreateFabFolders()
        {
            if (!CheckForExistingFolder(FabPath)) return false;

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
            if (!CheckForExistingFolder(MatPath)) return false;

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

        public bool CreateBoltFolder()
        {
            if (!CheckForExistingFolder(BoltPath)) return false;

            Directory.CreateDirectory(BoltPath);
            return true;
        }

        public bool CreateEpoFolder()
        {
            if (!CheckForExistingFolder(EpoPath)) return false;

            Directory.CreateDirectory(EpoPath);
            return true;
        }

        private bool CheckForExistingFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                PrismWarnings.FolderAlreadyExists(folderPath);
                return false;
            }
            var zip = Directory.GetFiles(ProjectLocation, "*.zip");
            if (zip.Contains($"{folderPath}.zip"))
            {
                PrismWarnings.FolderAlreadyExists($"{folderPath}.zip");
                return false;
            }
            return true;
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

        public void ZipFolder(string folderPath)
        {
            if (Directory.Exists($"{folderPath}.zip"))
            {
                File.Delete($"{folderPath}.zip");
            }
            ZipFile.CreateFromDirectory(folderPath, $"{folderPath}.zip");
        }
    }
}