namespace Prism
{
    public static class FirmFolderLoc //both are set to use 2021 firm folder (so the latest reports/wizards, unsure if this works in 2019i
    {
        private static string firmFolder2021Location = @"C:\Sev_Firm_2021\";

        public static string DrawingWizard() //This needs to be made into a group wide one.
        {
            return $@"{firmFolder2021Location}Roles\SNI\system\SNI Drawing Wizard.dproc";
        }

        public static string ReportTemplates()
        {
            //return $@"{firmFolder2021Location}Reports";
            return "C:\\Users\\mark.gibson\\Desktop\\Project Documents\\Prism\\Test Reports";
        }

        public static string FabsecCarcassDrawingWizard()
        {
            return $@"{firmFolder2021Location}system\SEV Fabsec Carcass Drawing Wizard.dproc";
        }
    }
}