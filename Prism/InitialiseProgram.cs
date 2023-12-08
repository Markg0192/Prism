using System;
using System.Diagnostics;
using System.Windows.Forms;
using TeklaLauncher;

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

           //   Application.Run(new PrismUI());
            Launcher.Launch(new PrismUI());

            bool internalUser = System.Environment.UserDomainName == "SFRPLC";
            bool validExternalUser = false;

            /*  Application.EnableVisualStyles();
              Application.SetCompatibleTextRenderingDefault(false);

              if (internalUser)
              {
                  LicService licence = new LicService("Prism", 30, 30);
                  licence.LicenceChecked += LicenceChecked;
                  if (licence.CheckLicence())
                  {
                      validExternalUser = true;
                  }
              }

              if (internalUser || validExternalUser)
              {*/
            /*  var launcher = new Launcher();
              if (launcher.CloseNow) return;
              if (!launcher.RestartRequired)
                  Application.Run(new PrismUI());
              else
                  Process.Start("Prism.exe");*/



            //  }
            //  else { Application.Exit(); return; }
        }

        private static void LicenceChecked(bool valid)
        {
            if (!valid)
            {
                MessageBox.Show("Licence Check Failed.");
                Application.Exit();
            }
        }

        private static bool IsSpecialPerson()
        {
            return Environment.UserName == "mark.gibson";
        }
    }

}
