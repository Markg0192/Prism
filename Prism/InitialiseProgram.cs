using System;
using System.Windows.Forms;

namespace Prism
{
    static class InitialiseProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

           // Application.Run(new PrismUI());
           Launcher.Launch(new PrismUI());
        }
    }
}