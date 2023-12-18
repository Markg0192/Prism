using Microsoft.Office.Interop.Outlook;
using Tekla.Structures.Model;
using Attachment = Microsoft.Office.Interop.Outlook.Attachment;
using Application = Microsoft.Office.Interop.Outlook.Application;

namespace Prism
{
    public static class EmailWriter
    {
        private const string _mailNewLine = "\r";
        private const string _purchasingEmail = "Purchasing@severfield.com";
        private const string _fabsecTeamEmail = "FabsecOffice.Dalton@severfield.com";
        private const string _seversafeTeamEmail = "seversafe.orders@severfield.com";
        private static string[] _fabsecEmail = new string[] { _fabsecTeamEmail };
        private const string marksEmail = "mark.gibson@severfield.com";
        private const string dansEmail = "dan.thompson@severfield.com";
        private const string johnsEmail = "john.gradwell@severfield.com";
        private static readonly string ApplicationName = "Your Application Name";

        public static void WriteEpoEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string fabPath)
        {
            Application outlookApp = new Application();

            MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
            email.Subject = $"{fabPrefix} E.P.O. Order";

            email.Body = $"Hello,{_mailNewLine}" +
                            $"{_mailNewLine}" +
                            $"This is the edge protection order for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
                            $"Please make this order available.{_mailNewLine}" +
                            $"{_mailNewLine}" +
                            $"Site date is {SiteDateNote(siteDate)}{_mailNewLine}" +
                            $"{_mailNewLine}" +
                            $"This order consists of: {_mailNewLine}" +
                            $"{WriteEpoLengths()}{_mailNewLine}" +
                            $"Note, all length values have been rounded to nearest 0.5m.{_mailNewLine}" +
                            $"{_mailNewLine}" +
                            $"Regards,{_mailNewLine}{_mailNewLine}" +
                            $"{projData.Full}";

            string attachmentPath = $"{fabPath}.zip";
            Attachment attachment = email.Attachments.Add(attachmentPath);

            email.To = _seversafeTeamEmail;
            email.CC = FormCCString(false, projData.WebService, projData.ProjNumberAndGuid);
            email.Display();
            //email.Send();

            // Release the Outlook application object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
            outlookApp = null;
        }

        public static void WriteFabEmail(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string fabPath, bool zipFileCanBeAttached)
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

            if (zipFileCanBeAttached)
            {
                string attachmentPath = $"{fabPath}.zip";
                Attachment attachment = email.Attachments.Add(attachmentPath);
            }

            //email.To = "ni.fabissue@severfield.com";
            email.CC = FormCCString(false, projData.WebService, projData.ProjNumberAndGuid);
            email.Display();
            //email.Send();

            // Release the Outlook application object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
            outlookApp = null;
        }

        public static void WriteBoltOrderEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string boltPath)
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
            email.CC = FormCCString(false, projData.WebService, projData.ProjNumberAndGuid);
            email.Display();
            //email.Send();

            // Release the Outlook application object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
            outlookApp = null;
        }

        public static void WriteFabsecCarcassEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string boltPath)
        {
            Application outlookApp = new Application();

            MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
            email.Subject = $"{fabPrefix} Fabsec Carcass Order";

            email.Body = $"Hello,{_mailNewLine}" +
                            $"{_mailNewLine}" +
                            $"This is the fabsec carcass order for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
                            $"Please process these carcasses when possible.{_mailNewLine}{_mailNewLine}" +
                            $"{DateRequiredNote("These carcasses are required for fab", siteDate)}" +
                            $"Regards,{_mailNewLine}{_mailNewLine}" +
                            $"{projData.Full}";

            string attachmentPath = $"{boltPath}.zip";
            Attachment attachment = email.Attachments.Add(attachmentPath);

            email.To = _fabsecTeamEmail;
            email.CC = _purchasingEmail; //FormCCString(true, projData.WebService, projData.ProjNumberAndGuid);
            email.Display();
            //email.Send();

            // Release the Outlook application object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
            outlookApp = null;
        }


        /*  public static async void TestNewEmail()
          {
              var requestBody = new SendMailPostRequestBody
              {
                  Message = new Message
                  {
                      Subject = "Meet for lunch?",
                      Body = new ItemBody
                      {
                          ContentType = BodyType.Text,
                          Content = "The new cafeteria is open.",
                      },
                      ToRecipients = new List<Microsoft.Graph.Models.Recipient>
          {
              new Microsoft.Graph.Models.Recipient
              {
                  EmailAddress = new EmailAddress
                  {
                      Address = "mark.gibson@severfield.com",
                  },
              },
          },
                      CcRecipients = new List<Microsoft.Graph.Models.Recipient>
          {
              new Microsoft.Graph.Models.Recipient
              {
                  EmailAddress = new EmailAddress
                  {
                      Address = "mark.gibson@severfield.com",
                  },
              },
          },
                  },
                  SaveToSentItems = false,
              };

              // To initialize your graphClient, see https://learn.microsoft.com/en-us/graph/sdks/create-client?from=snippets&tabs=csharp
              await graphClient.Me.SendMail.PostAsync(requestBody);
          }*/

        public static void WriteMatEmail(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber, string orderType, string matPath, bool fabsecPresent, string siteDate)
        {
            Application outlookApp = new Application();

            MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
            email.Subject = $"{fabPrefix} {IssueType(orderType)}";

            email.Body = $"Hello,{_mailNewLine}" +
                            $"{_mailNewLine}" +
                            $"{OrderTypeText(orderType)} for {PhaseOrVariation(projData.IsVariation)} {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
                            $"{RemoveOrAddText(orderType)} as soon as possible.{_mailNewLine}" +
                            $"{_mailNewLine}" +
                            $"{RemoveOrAddMaterial(orderType)}{_mailNewLine}" +
                            $"{objects.SelectedModelParts.Count} Parts.{_mailNewLine}" +
                            $"{objects.PartWeight} T. {_mailNewLine}" +
                            $"{_mailNewLine}" +
                            $"{DateRequired(orderType, siteDate)}" +
                            $"Regards,{_mailNewLine}{_mailNewLine}" +
                            $"{projData.Full}";

            string attachmentPath = $"{matPath}.zip";
            Attachment attachment = email.Attachments.Add(attachmentPath);

            email.To = _purchasingEmail;
            email.CC = FormCCString(fabsecPresent, projData.WebService, projData.ProjNumberAndGuid);
            email.Display();
            //email.Send();

            // Release the Outlook application object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
            outlookApp = null;
        }

        public static void WriteHelpEmail(string version)
        {
            Application outlookApp = new Application();

            MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
            email.Subject = $"Request for Prism help - Version No. {version}";

            email.Body = $"Please enter your Prism request here, we will get back to you as soon as possible.{_mailNewLine}" +
                            $"{_mailNewLine}" +
                            $"Regards,{_mailNewLine}" +
                            $"Prism development team.";

            email.To = marksEmail;
            email.CC = $"{johnsEmail}; {dansEmail}";
            email.Display();
            //email.Send();

            // Release the Outlook application object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
            outlookApp = null;
        }

        private static string ProjNumberAndGuid(ProjectInfo pInfo)
        {
            return pInfo.ProjectNumber + pInfo.GUID;
        }

        private static string WriteEpoLengths()
        {
            string textToReturn = "";
            if (SeversafeOrder.LinMeterRun1mSystem != 0)
            {
                textToReturn = textToReturn + "1m Edge - " + SeversafeOrder.LinMeterRun1mSystem + "m.";
            }
            if (SeversafeOrder.LinMeterRun1_8mSystem != 0)
            {
                textToReturn = textToReturn + _mailNewLine + "1.8m edge - " + SeversafeOrder.LinMeterRun1_8mSystem + "m.";
            }
            if (SeversafeOrder.LinMeterRun1mPhaseBreak != 0)
            {
                textToReturn = textToReturn + _mailNewLine + "1m phase break - " + SeversafeOrder.LinMeterRun1mPhaseBreak + "m.";
            }
            if (SeversafeOrder.LinMeterRun1_8mPhaseBreak != 0)
            {
                textToReturn = textToReturn + _mailNewLine + "1m phase break - " + SeversafeOrder.LinMeterRun1_8mPhaseBreak + "m.";
            }

            return textToReturn;
        }

        private static void FormIssueEmail(string emailAddress, string subject, string body)
        {
            System.Diagnostics.Process.Start("mailto:" + emailAddress + "?subject=" + subject + "&body=" + body);
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

        private static string DateRequired(string orderType, string siteDate)
        {
            if (orderType.Contains("Omit"))
            {
                return "";
            }
            return $"Date material required is {SiteDateNote(siteDate)}{_mailNewLine}{_mailNewLine}";
        }

        private static string DateRequiredNote(string note, string siteDate)
        {
            if (siteDate == "")
            {
                return "";
            }
            return $"{note} {siteDate}{_mailNewLine}{_mailNewLine}";
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

        private static string RemoveOrAddMaterial(string orderType)
        {
            if (orderType.Contains("Order"))
            {
                return $"The material to be ordered is as follows;";
            }
            if (orderType.Contains("Add"))
            {
                return "The material to be added is as follows;";
            }
            if (orderType.Contains("Omit"))
            {
                return "The material to be omitted is as follows;";
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

        private static string FormCCString(bool fabsecPresent, ExternalService.WebService1 service, string jobName)
        {
            string projectManager = WebService.ReadSpecificLine(Constants.PrismModelData, 17, Constants.ModelProjectInforLocation(jobName));
            string doManager = WebService.ReadSpecificLine(Constants.PrismModelData, 18, Constants.ModelProjectInforLocation(jobName));
            string documentControl = WebService.ReadSpecificLine(Constants.PrismModelData, 19, Constants.ModelProjectInforLocation(jobName));
            string others = WebService.ReadSpecificLine(Constants.PrismModelData, 20, Constants.ModelProjectInforLocation(jobName));

            string[] pString = projectManager.Split(' ');
            string[] doManagerString = doManager.Split(' ');
            string[] docControlString = documentControl.Split(' ');
            string[] othersString = others.Split(' ');

            AppendString(pString, "", out string first);
            AppendString(doManagerString, first, out string second);
            AppendString(docControlString, second, out string third);
            AppendString(othersString, third, out string fourth);
            string ccString = fourth;
            if (fabsecPresent) { AppendString(_fabsecEmail, fourth, out ccString); }

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