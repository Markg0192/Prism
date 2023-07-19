using System.Diagnostics;
using Microsoft.Office.Interop.Outlook;
using Tekla.Structures.Model;
using Attachment = Microsoft.Office.Interop.Outlook.Attachment;

namespace Prism
{
    public static class EmailWriter
    {
        private const string _mailNewLine = "\r";
        private const string _purchasingEmail = "Purchasing@severfield.com";
        private const string _fabsecTeamEmail = "FabsecOffice.Dalton@severfield.com";
        private static string[] _fabsecEmail = new string[] { _fabsecTeamEmail };
        private const string marksEmail = "mark.gibson@severfield.com";

        public static void WriteFabEmail(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string fabPath)
        {
            if (!Constants.IsSpecialPerson())
            {
                Application outlookApp = new Application();

                MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
                email.Subject = $"{fabPrefix} Fab Issue";

                email.Body = $"Hello,{_mailNewLine}" +
                                $"{_mailNewLine}" +
                                $"This is the fab package for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
                                $"Please issue this package to the works when possible.{_mailNewLine}" +
                                $"{_mailNewLine}" +
                                $"Site date is {SiteDateNote(siteDate)}{_mailNewLine}" +
                                $"{_mailNewLine}" +
                                $"This fab package contains the following;{_mailNewLine}" +
                                $"{objects.AssembliesList.Count} Assemblies.{_mailNewLine}" +
                                $"{objects.SelectedModelParts.Count} Parts.{_mailNewLine}" +
                                $"{objects.PartWeight} T. {_mailNewLine}" +
                                $"{_mailNewLine}" +
                                $"Regards,{_mailNewLine}{_mailNewLine}" +
                                $"{projData.Full}";

                string attachmentPath = $"{fabPath}.zip";
                Attachment attachment = email.Attachments.Add(attachmentPath);

                //email.To = "ni.fabissue@severfield.com";
                email.CC = FormCCString(projData.pInfo, false);
                email.Display();
                //email.Send();

                // Release the Outlook application object
                System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
                outlookApp = null;
            }
        }

        public static void WriteBoltOrderEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string boltPath)
        {
            if (!Constants.IsSpecialPerson())
            {
                Application outlookApp = new Application();

                MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
                email.Subject = $"{fabPrefix} Bolt Order";

                email.Body = $"Hello,{_mailNewLine}" +
                                $"{_mailNewLine}" +
                                $"This is the bolt order for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
                                $"Please order these bolts when possible.{_mailNewLine}{_mailNewLine}" +
                                $"Site date is {SiteDateNote(siteDate)}" +
                                $"{_mailNewLine}{_mailNewLine}" +
                                $"Regards,{_mailNewLine}{_mailNewLine}" +
                                $"{projData.Full}";

                string attachmentPath = $"{boltPath}.zip";
                Attachment attachment = email.Attachments.Add(attachmentPath);

                email.To = _purchasingEmail;
                email.CC = FormCCString(projData.pInfo, false);
                email.Display();
                //email.Send();

                // Release the Outlook application object
                System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
                outlookApp = null;
            }
        }

        public static void WriteMatEmail(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber, string orderType, string matPath, bool fabsecPresent)
        {
          //  if (!Constants.IsSpecialPerson())
            {
                Application outlookApp = new Application();

                MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
                email.Subject = $"{fabPrefix} {IssueType(orderType)}";

                email.Body = $"Hello,{_mailNewLine}" +
                                $"{_mailNewLine}" +
                                $"{OrderTypeText(orderType)} for {PhaseOrVariation(projData.IsVariation)} {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
                                $"{RemoveOrAddText(orderType)} as soon as possible.{_mailNewLine}" +
                                $"{_mailNewLine}" +
                                $"This material order contains the following;{_mailNewLine}" +
                                $"{objects.SelectedModelParts.Count} Parts.{_mailNewLine}" +
                                $"{objects.PartWeight} T. {_mailNewLine}" +
                                $"{_mailNewLine}" +
                                $"Regards,{_mailNewLine}{_mailNewLine}" +
                                $"{projData.Full}";

                string attachmentPath = $"{matPath}.zip";
                Attachment attachment = email.Attachments.Add(attachmentPath);

                email.To = _purchasingEmail;
                email.CC = FormCCString(projData.pInfo, fabsecPresent);
                email.Display();
                //email.Send();

                // Release the Outlook application object
                System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
                outlookApp = null;
            }
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
            if (isVariation)
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
            if (orderType == "Order Special Fittings")
            {
                return "This is a special fitting order";
            }
            if (orderType == "Add Special Fittings")
            {
                return "This is an additional fitting order";
            }
            if (orderType == "Omit Special Fittings")
            {
                return "This is an omit special fitting order";
            }
            return "";
        }

        private static string RemoveOrAddText(string orderType)
        {
            if (orderType.Contains("Order"))
            {
                return "Please order this material";
            }
            if (orderType.Contains("Add"))
            {
                return "Please add this to the material order for this phase";
            }
            if (orderType.Contains("Omit"))
            {
                return "Please remove this from the material order of this phase";
            }
            return "";
        }

        private static string IssueType(string orderType)
        {
            if (orderType.Contains("Order"))
            {
                return "Prelim Issue";
            }
            if (orderType.Contains("Add"))
            {
                return "Additional Issue";
            }
            if (orderType.Contains("Omit"))
            {
                return "OMIT Issue";
            }
            return "";
        }

        private static string FormCCString(ProjectInfo pInfo, bool fabsecPresent)
        {
            string projectManager = "";
            pInfo.GetUserProperty("PrismPM", ref projectManager);
            string doManager = "";
            pInfo.GetUserProperty("PrismDOM", ref doManager);
            string documentControl = "";
            pInfo.GetUserProperty("PrismDOC", ref documentControl);
            string others = "";
            pInfo.GetUserProperty("PrismOTHERS", ref others);

            string[] pString = projectManager.Split(' ');
            string[] doManagerString = doManager.Split(' ');
            string[] docControlString = documentControl.Split(' ');
            string[] othersString = others.Split(' ');

            AppendString(pString, "", out string first);
            AppendString(doManagerString, first, out string second);
            AppendString(docControlString, second, out string third);
            AppendString(othersString, third, out string fourth);
            string ccString = fourth;
            if(fabsecPresent) { AppendString(_fabsecEmail, fourth, out ccString); }

            return ccString;
        }

        private static void AppendString(string[] input, string ccString, out string newCCstring)
        {
            foreach (string st in input)
            {
                if (st != "")
                {
                    ccString = ccString + st + "; ";
                }

            }
            newCCstring = ccString;
        }
    }
}