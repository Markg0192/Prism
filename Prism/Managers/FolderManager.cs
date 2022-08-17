using System.Collections.Generic;
using System.IO;

namespace Prism
{
    /// <summary>
    /// The folder manager class is where all folders required for a package is created.
    /// The Create folders method creates all folders that may be required (even if they aren't) because of this we also have he remove 
    /// unused folders method which runs at the end of the process to clear out any unused folders.
    /// </summary>
    public class FolderManager
    {
        private const string _fabsecCarcasses = "Carcass Drawings";
        private const string _assFolder = "ASS";
        private const string _fitFolder = "FIT";
        private const string _prtFolder = "PRT";
        private const string _dspFolder = "DSP";
        private const string _ncFolder = "NC";
        private const string _reportFolder = "Lists";
        private const string _ifcFolder = "IFC";
        private List<string> _folderNames;    

        public FolderManager(PrismProjectData projectData, string phaseNum, string issueNum)
        {
            string fabFolder = $"{projectData.ProjNumber}-{phaseNum}-FAB-ISSUE{issueNum}";
            string matFolder = $"{projectData.ProjNumber}-{phaseNum}-PRELIM-ISSUE{issueNum}";
            string boltFolder = $"{projectData.ProjNumber}-{phaseNum}-BOLT-ISSUE{issueNum}";
            FabPath = Path.Combine(projectData.ProjPath, fabFolder);
            MatPath = Path.Combine(projectData.ProjPath, matFolder);
            BoltPath = Path.Combine(projectData.ProjPath, boltFolder);
            string assPath = Path.Combine(FabPath, _assFolder);
            string fitPath = Path.Combine(FabPath, _fitFolder);
            string prtPath = Path.Combine(FabPath, _prtFolder);
            FabsecCarcassPath = Path.Combine(MatPath, _fabsecCarcasses);
            NcPath = Path.Combine(FabPath, _ncFolder);
            ReportPath = Path.Combine(FabPath, _reportFolder);
            DspPath = Path.Combine(FabPath, _dspFolder);
            string ifcPath = Path.Combine(FabPath, _ifcFolder);
            _folderNames = new List<string>
                {assPath, fitPath, prtPath, DspPath, NcPath, ReportPath, ifcPath};
        }

        public readonly string FabPath;
        public readonly string MatPath;
        public readonly string BoltPath;
        public readonly string NcPath;
        public readonly string ReportPath;
        public readonly string DspPath;
        private readonly string FabsecCarcassPath;

        public void CreateFabFolders()
        {        
            foreach (string folder in _folderNames)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }    
        
        public void CreateMatFolder(bool fabsecsPresent)
        {
            Directory.CreateDirectory(MatPath);
            if(fabsecsPresent)
            {
                Directory.CreateDirectory(FabsecCarcassPath);
            }
        }

        public void CreateBoltFolder()
        {
            Directory.CreateDirectory(BoltPath);
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

                        Directory.Delete(subdirectory);
                }

                foreach (string subFile in Directory.GetFiles(ReportPath))
                {
                    if (subFile.Substring(subFile.Length - 3) == "dpm")
                    {
                           File.Delete(subFile);
                    }
                }
            }
        }      
    }
}