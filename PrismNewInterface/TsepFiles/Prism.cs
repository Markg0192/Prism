using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Tekla.Structures;
using TSM = Tekla.Structures.Model;

namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            string ApplicationName = "PrismNewInterface.exe";
            string xsdir = "";
            TSM.Model CurrentModel = new TSM.Model();

            //TeklaStructuresSettings.GetAdvancedOption("XS_DIR", ref xsdir);
            //string EnginePath = Path.Combine(xsdir, "nt\\bin\\applications\\tekla\\Model\\" + ApplicationName);
            TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsdir);
            string EnginePath = Path.Combine(xsdir, "Environments\\common\\extensions\\Prism\\" + ApplicationName);

            Process NewProcess = new Process();

            if (File.Exists(EnginePath))
            {
                NewProcess.StartInfo.FileName = EnginePath;

                try
                {
                    NewProcess.Start();
                    NewProcess.WaitForExit();
                }
                catch
                {
                    MessageBox.Show("Starting " + ApplicationName + " failed.");
                }
            }
            else
            {
                MessageBox.Show(ApplicationName + " not found.");
            }
        }
    }
}
