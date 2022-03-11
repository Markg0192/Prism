using System.Collections.Generic;
using System.IO;
using Tekla.Structures.Model;

namespace Prism
{
    /// <summary>
    /// The folder manager class is where all folders required for a package is created.
    /// The Create folders method creates all folders that may be required (even if they aren't) because of this we also have he remove 
    /// unused folders method which runs at the end of the process to clear out any unused folders.
    /// </summary>
    public class SevFolders
    {  
        private const string _assFolder = "ASS";
        private const string _fitFolder = "FIT";
        private const string _prtFolder = "PRT";
        private const string _dspFolder = "DSP";
        private const string _ncFolder = "NC";
        private const string _reportFolder = "Lists";
        private const string _ifcFolder = "IFC";
        private readonly string _fabFolder;
        private readonly string _matFolder;
        private readonly string _ifcPath;
        private List<string> _folderNames;    
        private readonly string _assPath;
        private readonly string _fitPath;
        private readonly string _prtPath;

        public SevFolders(Model model, string phaseNum, string issueNum)
        {  
            SevModelData modelData = model.CreateSevModelData();
            ModelPath = model.GetInfo().ModelPath;
            _fabFolder = $"{modelData.ProjNumber}-{phaseNum}-FAB-ISSUE{issueNum}";
            _matFolder = $"{modelData.ProjNumber}-{phaseNum}-Prelim-ISSUE{issueNum}";
            FabPath = Path.Combine(ModelPath, _fabFolder);
            MatPath = Path.Combine(ModelPath, _matFolder);
            _assPath = Path.Combine(FabPath, _assFolder);
            _fitPath = Path.Combine(FabPath, _fitFolder);
            _prtPath = Path.Combine(FabPath, _prtFolder);
            NcPath = Path.Combine(FabPath, _ncFolder);
            ReportPath = Path.Combine(FabPath, _reportFolder);
            DspPath = Path.Combine(FabPath, _dspFolder);
            _ifcPath = Path.Combine(FabPath, _ifcFolder);
            _folderNames = new List<string>
                {_assPath, _fitPath, _prtPath, DspPath, NcPath, ReportPath, _ifcPath};
        }

        public readonly string ModelPath;
        public readonly string FabPath;
        public readonly string MatPath;
        public readonly string NcPath;
        public readonly string ReportPath;
        public readonly string DspPath;

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
        
        public void CreateMatFolder()
        {
            Directory.CreateDirectory(MatPath);
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