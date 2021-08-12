using System;
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
        private const string assFolder = "ASS Folder";
        private const string fitFolder = "FIT Folder";
        private const string prtFolder = "PRT Folder";
        private const string dspFolder = "DSP";
        private const string ncFolder = "NC";
        private const string reportFolder = "Reports";
        private const string ifcFolder = "IFC";
        private readonly string fabFolder;
        private readonly string modelPath;
        public readonly string fabPath;
        private readonly string assPath;
        private readonly string fitPath;
        private readonly string prtPath;
        public readonly string ncPath;
        public readonly string reportPath;
        public readonly string dspPath;
        private readonly string ifcPath;
        private List<string> folderNames;

        public SevFolders(Model model, string phaseNum, string issueNum)
        {  
            SevModelData modelData = model.CreateSevModelData();
            modelPath = model.GetInfo().ModelPath;
            fabFolder = $"{modelData.ProjNumber}-{phaseNum}-FAB-ISSUE{issueNum}";
            fabPath = Path.Combine(modelPath, fabFolder);
            assPath = Path.Combine(fabPath, assFolder);
            fitPath = Path.Combine(fabPath, fitFolder);
            prtPath = Path.Combine(fabPath, prtFolder);
            ncPath = Path.Combine(fabPath, ncFolder);
            reportPath = Path.Combine(fabPath, reportFolder);
            dspPath = Path.Combine(fabPath, dspFolder);
            ifcPath = Path.Combine(fabPath, ifcFolder);
            folderNames = new List<string>
                {assPath, fitPath, prtPath, dspPath, ncPath, reportPath, ifcPath};
        }

        public void CreateFolders()
        {        
            foreach (string folder in folderNames)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    Console.WriteLine($"{folder} folder created");
                }
            }
        }     

        public void RemoveUnusedFolders()
        {
            if (Directory.Exists(fabPath))
            {
                foreach (string subdirectory in Directory.GetDirectories(fabPath))
                {
                    string[] file = Directory.GetFiles(subdirectory, "*.*");
                    if (file.Length == 0)

                        Directory.Delete(subdirectory);
                }
            }
        }      
    }
}

