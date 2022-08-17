namespace Prism
{
    public static class FirmFolderLoc //both are set to use 2021 firm folder (so the latest reports/wizards, unsure if this works in 2019i
    {
        public static string DrawingWizard() //This needs to be made into a group wide one.
        {
            return @"C:\Sev_Firm_2021\Roles\SNI\system\SNI Drawing Wizard.dproc";
        }

        public static string ReportTemplates()
        {
            return "C:/Sev_Firm_2021/Reports";
        }
    }
}
