using Tekla.Structures.Model;
using System.Collections.Generic;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace Prism
{
    public static class IFCExporter
    {
        public static async void ExportIndividualIFC(this SelectedObjects selectedObjects, string myFolder, string vaultContractNumber)
        {
            await Task.Run(() => RunIFCExport(selectedObjects.AssembliesList, myFolder, vaultContractNumber));

            selectedObjects.SelectedModelParts.SelectParts();
        }

        private static void RunIFCExport(List<Assembly> assemblyList, string localFolder, string vaultContractNumber)
        {
            // string myFolder = $@"\\sfrplc.local\\public\\DrawingVault\\TestContracts\\{vaultContractNumber}\\IFC";
            string myFolder = localFolder;
            foreach (Assembly assembly in assemblyList)
            {
                ExportIFC(assembly, myFolder);
            }
            CleanFolder(myFolder);
        }

        private static void ExportIFC(Assembly assembly, string myFolder)
        {
            assembly.SelectAssembly();
            ComponentInput componentInput = new ComponentInput();
            componentInput.AddOneInputPosition(new Tekla.Structures.Geometry3d.Point(0.0, 0.0, 0.0));
            Component component = new Component(componentInput)
            {
                Name = "ExportIFC",
                Number = -100000
            };
            component.LoadAttributesFromFile("-SEV-IFC");
            component.SetAttribute("OutputFile", $"{myFolder}/{assembly.Identifier}");
            component.Insert();
          //  File.Delete($"{myFolder}/{assembly.Identifier}.log");
        }

        static void CleanFolder(string folderPath)
        {
            // Get all .log files in the folder
            string[] logFiles = Directory.GetFiles(folderPath + "\\", "*.log");

            foreach (string file in logFiles)
            {
                File.Delete(file);
            }
        }
    }
}
