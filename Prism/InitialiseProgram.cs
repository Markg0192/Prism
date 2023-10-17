using SeverfieldLicenceService;
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
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PrismUI());*/

            bool internalUser = System.Environment.UserDomainName == "SFPLC";
            bool validExternalUser = false;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!internalUser)
            {
                LicService licence = new LicService("Prism", 30, 30);
                licence.LicenceChecked += LicenceChecked;
                if (licence.CheckLicence())
                {
                    validExternalUser = true;
                }
            }

            if (internalUser || validExternalUser)
            {
                Application.Run(new PrismUI());
            }
            else { Application.Exit(); return; }
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
