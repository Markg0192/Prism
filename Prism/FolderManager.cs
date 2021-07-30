using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using System.Windows.Forms;

namespace Prism
{    
    public static class FolderExtension
    {
        public static SevFolders SevFolders(this Model model, string phaseNum, string issueNum)
        {
            return new SevFolders(model, phaseNum, issueNum);
        }
    }

    public class SevFolders
    {  
        public const string AssFolder = "ASS Folder";
        public const string FitFolder = "FIT Folder";
        public const string PrtFolder = "PRT Folder";
        public const string DspFolder = "DSP";
        public const string NCFolder = "NC";
        public const string ReportFolder = "Reports";
        public const string IFCFolder = "IFC";
        public readonly string FabFolder;
        public readonly string ModelPath;
        public readonly string FabPath;
        public readonly string AssPath;
        public readonly string FitPath;
        public readonly string PrtPath;
        public readonly string NCPath;
        public readonly string ReportPath;
        public readonly string DspPath;
        public readonly string IFCPath;
        public List<string> folderNames;
        public SevFolders(Model model, string phaseNum, string issueNum)
        {  
            SevModelData modelData = model.SevModelData();
            ModelPath = model.GetInfo().ModelPath;
            FabFolder = $"{modelData.projNumber}-{phaseNum}-FAB-ISSUE{issueNum}";
            FabPath = Path.Combine(ModelPath, FabFolder);
            AssPath = Path.Combine(FabPath, AssFolder);
            FitPath = Path.Combine(FabPath, FitFolder);
            PrtPath = Path.Combine(FabPath, PrtFolder);
            NCPath = Path.Combine(FabPath, NCFolder);
            ReportPath = Path.Combine(FabPath, ReportFolder);
            DspPath = Path.Combine(FabPath, DspFolder);
            IFCPath = Path.Combine(FabPath, IFCFolder);

            folderNames = new List<string>
                {AssPath, FitPath, PrtPath, DspPath, NCPath, ReportPath, IFCPath};
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
            // var possibleFolders = PossibleFolderNames();

            // foreach(string folder in myFoldernames)
            // if its empty delete it
        }
    }
}
