using System.Diagnostics;

namespace Prism
{
    public static class EmailWriter
    {              
        private const string _mailNewLine = "%0D%0A";

        public static void WriteFabEmail(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber, string siteDate)
        {
            FormIssueEmail("ni.fabissue@severfield.com", $"{fabPrefix} Fab Issue",
                                                                                    $"Hello,{_mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"This is the fab package Issue {issueNumber} for phase {phaseNumber} in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
                                                                                    $"Please issue this package to the works when possible.{_mailNewLine}" +
                                                                                    $"Site date is {SiteDateNote(siteDate)}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"This fab package contains the following;{_mailNewLine}" +
                                                                                    $"{objects.AssembliesList.Count} Assemblies.{_mailNewLine}" +
                                                                                    $"{objects.SelectedModelParts.Count} Parts.{_mailNewLine}" +
                                                                                    $"{objects.PartWeight} T. { _mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"Regards,{_mailNewLine}{_mailNewLine}" +
                                                                                    $"{projData.Full}");
        }

        public static void WriteBoltOrderEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate)
        {
            FormIssueEmail("purchasing@severfield.com", $"{fabPrefix} Bolt Order",
                                                                                    $"Hello,{_mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"This is the bolt order Issue {issueNumber} for phase {phaseNumber} in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
                                                                                    $"Please order these bolts when possible.{_mailNewLine}{_mailNewLine}" +
                                                                                    $"Site date is {SiteDateNote(siteDate)}" +
                                                                                    $"{_mailNewLine}{_mailNewLine}" +
                                                                                    $"Regards,{_mailNewLine}{_mailNewLine}" +
                                                                                    $"{projData.Full}");
        }

        public static void WriteMatEmail(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber, string orderType)
        {
            FormIssueEmail("purchasing@severfield.com", $"{fabPrefix} {IssueType(orderType)}",
                                                                                    $"Hello,{_mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"{OrderTypeText(orderType)}, issue {issueNumber}, for {PhaseOrVariation(projData.IsVariation)} {phaseNumber} in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
                                                                                    $"{RemoveOrAddText(orderType)} as soon as possible.{_mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"This material order contains the following;{_mailNewLine}" +
                                                                                    $"{objects.SelectedModelParts.Count} Parts.{_mailNewLine}" +
                                                                                    $"{objects.PartWeight} T. { _mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"Regards,{_mailNewLine}{_mailNewLine}" +
                                                                                    $"{projData.Full}");
        }

        private static void FormIssueEmail(string emailAddress, string subject, string body)
        {
            Process.Start("mailto:" + emailAddress + "?subject=" + subject + "&body=" + body);
        }

        private static string SiteDateNote(string siteDate)
        {
            if (siteDate.Length < 3)
            {
                return "unconfirmed";
            }
            else
            {
                return siteDate;
            }            
        }

        private static string PhaseOrVariation(bool isVariation)
        {
            if(isVariation)
            {
                return "variation";
            }
            return "phase";
        }

        private static string OrderTypeText(string orderType)
        {            
            if (orderType == "Order Material")
            {
                return "This is the material order";
            }
            if (orderType == "Add Material")
            {
                return "This is an additional material order";
            }
            if (orderType == "Omit Material")
            {
                return "This is an omit material order";
            }
            return "";
        }

        private static string RemoveOrAddText(string orderType)
        {
            if (orderType == "Order Material")
            {
                return "Please order this material";
            }
            if (orderType == "Add Material")
            {
                return "Please add this to the material order for this phase";
            }
            if (orderType == "Omit Material")
            {
                return "Please remove this from the material order of this phase";
            }
            return "";
        }

        private static string IssueType(string orderType)
        {
            if (orderType == "Order Material")
            {
                return "Prelim Issue";
            }
            if (orderType == "Add Material")
            {
                return "Additional Prelim Issue";
            }
            if (orderType == "Omit Material")
            {
                return "OMIT Issue";
            }
            return "";
        }
    }
}