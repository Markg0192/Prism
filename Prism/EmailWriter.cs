using Microsoft.Office.Interop.Outlook;
using Tekla.Structures.Model;
using Attachment = Microsoft.Office.Interop.Outlook.Attachment;
using Application = Microsoft.Office.Interop.Outlook.Application;
using System.Collections.Generic;
using Prism.CustomDialogs;
using System;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using static QRCoder.PayloadGenerator;
using Newtonsoft.Json.Linq;

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

		public static void ExecuteEmailAction(Action<Application> emailAction)
		{
			Application outlookApp = null;

			try
			{
				// Create a new instance of the Outlook application
				outlookApp = new Application();

				// Perform the email action
				emailAction(outlookApp);
			}
			catch (System.Exception ex)
			{
				// Handle exceptions
				MessageBox.Show("Prism has failed to create an Email, the usual fix for this is to have Microsoft 365 re-installed.");
			}
			finally
			{
				// Ensure the Outlook application object is released
				if (outlookApp != null)
				{
					System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
					outlookApp = null;
				}

				// Force garbage collection
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		public static void WritePreparedEmail(string emailFilePath)
		{
			ExecuteEmailAction(outlookApp =>
			{
				// Read all lines from the provided text file
				string[] emailContent = File.ReadAllLines(emailFilePath);

				if (emailContent.Length < 3)
				{
					throw new ArgumentException("The email file must contain at least three lines: subject, CC, and body.");
				}

				// The first line is the subject
				string emailSubject = emailContent[0];

				// The second line is the CC string
				string emailCC = emailContent[1];

				// The remaining lines form the body
				string emailBody = string.Join(Environment.NewLine, emailContent.Skip(2));

				// Create a new MailItem
				MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
				email.Subject = emailSubject;
				email.Body = emailBody;

				// Set the recipient and CC fields
				email.To = _seversafeTeamEmail;
				email.CC = emailCC;

				// Display the email (or send it using email.Send())
				email.Display();
				//email.Send();
			});
		}

		public static void WriteEpoEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string fabPath)
		{
			ExecuteEmailAction(outlookApp =>
			{
				// Create a new MailItem
				MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
				email.Subject = $"{fabPrefix} E.P.O. Order{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";

				// Set the email body
				email.Body = $"Hello,{_mailNewLine}" +
							  $"{_mailNewLine}" +
							  $"This is the edge protection order for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
							  $"{IsPartOfAVariation(projData.IsVariation, projData.VariationNumber)}" +
							  $"Please make this order available.{_mailNewLine}" +
							  $"{_mailNewLine}" +
							  $"Site date is {SiteDateNote(siteDate)}{_mailNewLine}" +
							  $"{_mailNewLine}" +
							  $"This order consists of: {_mailNewLine}" +
							  $"{WriteEpoLengths()}{_mailNewLine}" +
							  $"Note, all length values have been rounded to nearest 0.5m.{_mailNewLine}" +
							  $"{_mailNewLine}" +
							  $"Regards,{_mailNewLine}{_mailNewLine}" +
							  $"{projData.Full}"; ;

				// Add an attachment
				string attachmentPath = $"{fabPath}.zip";
				Attachment attachment = email.Attachments.Add(attachmentPath);

				// Set the recipient and CC fields
				email.To = _seversafeTeamEmail;
				email.CC = FormCCString(false, projData.WebService, projData.ProjNumberAndGuid);

				// Display the email (or send it using email.Send())
				email.Display();
				//email.Send();
			});
		}

		public static void CreateFabEmailText(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string fabPath)
		{
			double fittingsToAssemblyWeightRatio = Math.Round((objects.FittingWeight / objects.MainPartWeight) * 100, 1);
			double totalWeight = objects.MainPartWeight + objects.FittingWeight;

			// Subject
			string subject = $"{fabPrefix} Fab Issue{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";

			// CC string (you can adjust this logic as needed)
			string ccString = FormCCString(false, projData.WebService, projData.ProjNumberAndGuid);

			// Body content
			string bodyContent = $"Hello,{_mailNewLine}" +
								 $"{_mailNewLine}" +
								 $"This is the fab package for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
								 $"{IsPartOfAVariation(projData.IsVariation, projData.VariationNumber)}" +
								 $"TEKLA MODEL NAME: {projData.ModelName}" + 
								 $"Please issue this package to the works when possible.{_mailNewLine}" +
								 $"{_mailNewLine}" +
								 $"Site date is {SiteDateNote(siteDate)}{_mailNewLine}" +
								 $"{_mailNewLine}" +
								 $"This fab package contains the following;{_mailNewLine}" +
								 $"{objects.GetMainParts().Count} Assemblies.{_mailNewLine}" +
								 $"{objects.PrismParts.Count} Parts.{_mailNewLine}" +
								 $"{_mailNewLine}" +
								 $"Total weight = {totalWeight}t. {_mailNewLine}" +
								 $"PLT/RSA weight = {objects.FittingWeight}t ({fittingsToAssemblyWeightRatio}% of total). {_mailNewLine}" +
								 $"{_mailNewLine}" +
								 $"Regards,{_mailNewLine}{_mailNewLine}" +
								 $"{projData.Full}";

			// Combine everything into an array of lines
			string[] emailLines = new string[]
			{
				subject,    // First line is the subject
				ccString,   // Second line is the CC string
				bodyContent // Rest is the email body
			};

			string outputFilePath = Path.Combine(fabPath, "Fab Email Text.txt");

			// Write the lines to the text file
			File.WriteAllLines(outputFilePath, emailLines);
		}

		public static void WriteFabEmail(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string fabPath, bool zipFileCanBeAttached)
		{
			double fittingsToAssemblyWeightRatio = 0;
			if (objects.MainPartWeight != 0)
			{
				fittingsToAssemblyWeightRatio = Math.Round((objects.FittingWeight / (objects.MainPartWeight + objects.FittingWeight)) * 100, 1);
			}
			else
			{
				// Decide what you want to do if MainPartWeight is zero
				fittingsToAssemblyWeightRatio = 100; //because then 100% is fittings (plate/angle)
			}

			double totalWeight = objects.MainPartWeight + objects.FittingWeight;
			ExecuteEmailAction(outlookApp =>
			{
				MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
				email.Subject = $"{fabPrefix} Fab Issue{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";

				email.Body = $"Hello,{_mailNewLine}" +
								$"{_mailNewLine}" +
								$"This is the fab package for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
								$"TEKLA MODEL NAME: {projData.ModelName}.{_mailNewLine}" +
								$"{_mailNewLine}" +
								$"{IsPartOfAVariation(projData.IsVariation, projData.VariationNumber)}" +						
								$"Please issue this package to the works when possible.{_mailNewLine}" +
								$"{_mailNewLine}" +
								$"Site date is {SiteDateNote(siteDate)}{_mailNewLine}" +
								$"{_mailNewLine}" +
								$"This fab package contains the following;{_mailNewLine}" +
								$"{objects.GetMainParts().Count} Assemblies.{_mailNewLine}" +
								$"{objects.PrismParts.Count} Parts.{_mailNewLine}" +
								$"{_mailNewLine}" +
								$"Total weight = {totalWeight}t. {_mailNewLine}" +
								$"PLT/RSA weight = {objects.FittingWeight}t ({fittingsToAssemblyWeightRatio}% of total). {_mailNewLine}" +
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
			});
		}

		public static void WriteRevisedFabEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string fabPath, bool zipFileCanBeAttached, string messageForEmail)
		{
			ExecuteEmailAction(outlookApp =>
			{
				MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
				email.Subject = $"{fabPrefix} Revised Fab Issue{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";

				// Set the body format to HTML
				email.BodyFormat = OlBodyFormat.olFormatHTML;

				// Construct the HTML body
				email.HTMLBody = $"<html><body>" +
								 $"Hello,<br><br>" +
								 $"This is the revised fab package for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.<br>" +
								 $"{IsPartOfAVariation(projData.IsVariation, projData.VariationNumber)}" +
								 $"Please issue this package to the works when possible.<br><br>" +
								 $"Site date is <u>{SiteDateNote(siteDate)}</u><br><br>" +
								 $"The changes to the package are as follows;<br><br>" +
								 messageForEmail +
								 $"<br>" +
								 $"Regards,<br><br>" +
								 $"{projData.Full}" +
								 $"</body></html>";

				if (zipFileCanBeAttached)
				{
					string attachmentPath = $"{fabPath}.zip";
					Attachment attachment = email.Attachments.Add(attachmentPath);
				}

				//email.To = "ni.fabissue@severfield.com";
				email.CC = FormCCString(false, projData.WebService, projData.ProjNumberAndGuid);
				email.Display();
				//email.Send();
			});
		}

		private static string AddItems(List<SteelItemBase> steelItems, string type)
		{
			if (steelItems.Count == 0) return "";
			string myString = $"<b><u>{type}</u></b><br>";
			foreach (SteelItemBase item in steelItems)
			{
				myString += $"<br>{item.PartMark}<br>";
				foreach (string changeMessage in item.ChangeMessages)
				{
					{
						myString += $"<li>{changeMessage}</li><br>";
					}
				}
			}

			return myString;
		}

		public static void WriteBoltOrderEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string boltPath)
		{
			ExecuteEmailAction(outlookApp =>
			{

				MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
				email.Subject = $"{fabPrefix} Bolt Order{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";

				email.Body = $"Hello,{_mailNewLine}" +
								$"{_mailNewLine}" +
								$"This is the bolt order for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
								$"{IsPartOfAVariation(projData.IsVariation, projData.VariationNumber)}" +
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
			});
		}

		public static void WriteFabsecCarcassEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string boltPath)
		{
			ExecuteEmailAction(outlookApp =>
			{
				MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
				email.Subject = $"{fabPrefix} Fabsec Carcass Order{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";

				email.Body = $"Hello,{_mailNewLine}" +
								$"{_mailNewLine}" +
								$"This is the fabsec carcass order for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
								$"{IsPartOfAVariation(projData.IsVariation, projData.VariationNumber)}" +
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
			});
		}

		public static void WriteMatEmail(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber, string orderType, string matPath, bool fabsecPresent, string siteDate)
		{
			ExecuteEmailAction(outlookApp =>
			{
				MailItem email = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
				email.Subject = $"{fabPrefix} {IssueType(orderType)}{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";

				email.Body = $"Hello,{_mailNewLine}" +
								$"{_mailNewLine}" +
								$"{OrderTypeText(orderType)} for phase {phaseNumber}, issue {issueNumber}, in {projData.ProjNumber}, {projData.ProjName}.{_mailNewLine}" +
								$"{IsPartOfAVariation(projData.IsVariation, projData.VariationNumber)}" +
								$"{RemoveOrAddText(orderType)} as soon as possible.{_mailNewLine}" +
								$"{_mailNewLine}" +
								$"{RemoveOrAddMaterial(orderType)}{_mailNewLine}" +
								$"{objects.PrismParts.Count} Parts.{_mailNewLine}" +
								$"{objects.MainPartWeight + objects.FittingWeight}t. {_mailNewLine}" +
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
			});
		}

		public static void WriteHelpEmail(string version)
		{
			ExecuteEmailAction(outlookApp =>
			{
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
			});
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

		private static string IsPartOfAVariation(bool isVariation, string variationNumber)
		{
			if (isVariation)
			{
				return $"This should be recorded as being required for Variation number {variationNumber}.{_mailNewLine}";
			}
			return "";
		}

		private static string AddVariationNoIfReqd(bool isVariation, string variationNumber)
		{
			if (isVariation)
			{
				return $" {variationNumber}";
			}
			return "";
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
				return $"Prelim Issue";
			}
			if (orderType.Contains("Add"))
			{
				return $"Additional Issue";
			}
			if (orderType.Contains("Omit"))
			{
				return $"OMIT Issue";
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