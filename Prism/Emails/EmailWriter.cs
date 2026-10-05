using Microsoft.Office.Interop.Outlook;
using Tekla.Structures.Model;
using Attachment = Microsoft.Office.Interop.Outlook.Attachment;
using Application = Microsoft.Office.Interop.Outlook.Application;
using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Diagnostics;

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

		public static void ExecuteEmailAction(Action<Application> emailAction, Action<string, double> recordTiming = null)
		{
			Application outlookApp = null;

			try
			{
				Stopwatch timer = Stopwatch.StartNew();
				outlookApp = new Application();
				timer.Stop();
				recordTiming?.Invoke("EmailOutlookStart", timer.Elapsed.TotalMilliseconds);
				emailAction(outlookApp);
			}
			catch (System.Exception ex)
			{
				MessageBox.Show("Prism has failed to create an Email, the usual fix for this is to have Microsoft 365 re-installed.");
			}
			finally
			{
				if (outlookApp != null)
				{
					System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
					outlookApp = null;
				}

				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		public static void WritePreparedEmail(string emailFilePath)
		{
			ExecuteEmailAction(outlookApp =>
			{
				string[] emailContent = File.ReadAllLines(emailFilePath);

				if (emailContent.Length < 3)
				{
					throw new ArgumentException("The email file must contain at least three lines: subject, CC, and body.");
				}

				string emailSubject = emailContent[0];
				string emailCC = emailContent[1];
				string emailBody = string.Join(Environment.NewLine, emailContent.Skip(2));

				MailItem email = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

				email.Subject = emailSubject;
				email.Body = emailBody;
				email.To = _seversafeTeamEmail;
				email.CC = emailCC;

				email.Display();
			});
		}

		public static void WriteEpoEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string fabPath)
		{
			ExecuteEmailAction(outlookApp =>
			{
				MailItem email = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

				email.Subject = $"{fabPrefix} E.P.O. Order{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";

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
					$"{projData.Full}";

				string attachmentPath = $"{fabPath}.zip";
				Attachment attachment = email.Attachments.Add(attachmentPath);

				email.To = _seversafeTeamEmail;
				email.CC = FormCCString(false, projData.WebService, projData.ProjNumberAndGuid);

				email.Display();
			});
		}

		public static void CreateFabEmailText(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber,
			string siteDate, string fabPath)
		{
			double fittingsToAssemblyWeightRatio = Math.Round((objects.FittingWeight / objects.MainPartWeight) * 100, 1);
			double totalWeight = objects.MainPartWeight + objects.FittingWeight;

			string subject = $"{fabPrefix} Fab Issue{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";
			string ccString = FormCCString(false, projData.WebService, projData.ProjNumberAndGuid);

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

			string[] emailLines = new string[]
			{
				subject,
				ccString,
				bodyContent
			};

			string outputFilePath = Path.Combine(fabPath, "Fab Email Text.txt");
			File.WriteAllLines(outputFilePath, emailLines);
		}

		public static void WriteFabEmail(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber,
			string siteDate, string fabPath, bool zipFileCanBeAttached, string teklaVersion, Action<string, double> recordTiming = null)
		{
			Stopwatch timer = Stopwatch.StartNew();
			string emailBody = FabEmailBuilder.Build(projData, objects, issueNumber, phaseNumber, siteDate, zipFileCanBeAttached, teklaVersion);
			timer.Stop();
			recordTiming?.Invoke("EmailBuild", timer.Elapsed.TotalMilliseconds);

			ExecuteEmailAction(outlookApp =>
			{
				timer.Restart();
				MailItem email = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

				email.Subject = $"{fabPrefix} Fab Issue{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";
				email.BodyFormat = OlBodyFormat.olFormatHTML;
				email.HTMLBody = emailBody;

				if (zipFileCanBeAttached)
				{
					string attachmentPath = $"{fabPath}.zip";
					email.Attachments.Add(attachmentPath);
				}

				email.CC = FormCCString(false, projData.WebService, projData.ProjNumberAndGuid);
				timer.Stop();
				recordTiming?.Invoke("EmailPrepare", timer.Elapsed.TotalMilliseconds);

				timer.Restart();
				email.Display();
				timer.Stop();
				recordTiming?.Invoke("EmailUserWait", timer.Elapsed.TotalMilliseconds);
			}, recordTiming);
		}

		public static void WriteRevisedFabEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate,
			string fabPath, bool zipFileCanBeAttached, string messageForEmail)
		{
			ExecuteEmailAction(outlookApp =>
			{
				MailItem email = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

				email.Subject = $"{fabPrefix} Revised Fab Issue{AddVariationNoIfReqd(projData.IsVariation, projData.VariationNumber)}";
				email.BodyFormat = OlBodyFormat.olFormatHTML;

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

				email.CC = FormCCString(false, projData.WebService, projData.ProjNumberAndGuid);
				email.Display();
			});
		}

		private static string AddItems(List<SteelItemBase> steelItems, string type)
		{
			if (steelItems.Count == 0)
			{
				return "";
			}

			string myString = $"<b><u>{type}</u></b><br>";

			foreach (SteelItemBase item in steelItems)
			{
				myString += $"<br>{item.PartMark}<br>";

				foreach (string changeMessage in item.ChangeMessages)
				{
					myString += $"<li>{changeMessage}</li><br>";
				}
			}

			return myString;
		}

		public static void WriteBoltOrderEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string boltPath)
		{
			ExecuteEmailAction(outlookApp =>
			{
				MailItem email = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

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
			});
		}

		public static void WriteFabsecCarcassEmail(PrismProjectData projData, string fabPrefix, string issueNumber, string phaseNumber, string siteDate, string boltPath)
		{
			ExecuteEmailAction(outlookApp =>
			{
				MailItem email = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

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
				email.CC = _purchasingEmail;

				email.Display();
			});
		}

		public static void WriteMatEmail(PrismProjectData projData, SelectedObjects objects, string fabPrefix, string issueNumber, string phaseNumber,
			string orderType, string matPath, bool fabsecPresent, string siteDate)
		{
			ExecuteEmailAction(outlookApp =>
			{
				MailItem email = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

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
			});
		}

		public static void WriteHelpEmail(string version)
		{
			ExecuteEmailAction(outlookApp =>
			{
				MailItem email = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

				email.Subject = $"Request for Prism help - Version No. {version}";

				email.Body = $"Please enter your Prism request here, we will get back to you as soon as possible.{_mailNewLine}" +
					$"{_mailNewLine}" +
					$"Regards,{_mailNewLine}" +
					$"Prism development team.";

				email.To = marksEmail;
				email.CC = $"{johnsEmail}; {dansEmail}";

				email.Display();
			});
		}

		private static string WriteEpoLengths()
		{
			List<string> lines = new List<string>();

			if (SeversafeOrder.LinMeterRun1mSystem != 0)
				lines.Add($"1m Edge - {SeversafeOrder.LinMeterRun1mSystem}m.");

			if (SeversafeOrder.LinMeterRun1_8mSystem != 0)
				lines.Add($"1.8m edge - {SeversafeOrder.LinMeterRun1_8mSystem}m.");

			if (SeversafeOrder.LinMeterRun1mPhaseBreak != 0)
				lines.Add($"1m phase break - {SeversafeOrder.LinMeterRun1mPhaseBreak}m.");

			if (SeversafeOrder.LinMeterRun1_8mPhaseBreak != 0)
				lines.Add($"1m phase break - {SeversafeOrder.LinMeterRun1_8mPhaseBreak}m.");

			return string.Join(_mailNewLine, lines);
		}

		private static void FormIssueEmail(string emailAddress, string subject, string body)
		{
			System.Diagnostics.Process.Start("mailto:" + emailAddress + "?subject=" + subject + "&body=" + body);
		}

		private static string SiteDateNote(string siteDate)
		{
			return string.IsNullOrWhiteSpace(siteDate) ? "unconfirmed" : siteDate;
		}

		private static string IsPartOfAVariation(bool isVariation, string variationNumber)
		{
			return isVariation
				? $"This should be recorded as being required for Variation number {variationNumber}.{_mailNewLine}"
				: string.Empty;
		}

		private static string AddVariationNoIfReqd(bool isVariation, string variationNumber)
		{
			return isVariation ? $" {variationNumber}" : string.Empty;
		}

		private static string DateRequired(string orderType, string siteDate)
		{
			return orderType.Contains("Omit")
				? string.Empty
				: $"Date material required is {SiteDateNote(siteDate)}{_mailNewLine}{_mailNewLine}";
		}

		private static string DateRequiredNote(string note, string siteDate)
		{
			return string.IsNullOrWhiteSpace(siteDate)
				? string.Empty
				: $"{note} {siteDate}{_mailNewLine}{_mailNewLine}";
		}

		private static string OrderTypeText(string orderType)
		{
			switch (orderType)
			{
				case "Order Material":
					return "This is the material order";

				case "Add Material":
					return "This is an additional material order";

				case "Omit Material":
					return "This is an omit material order";

				case "Order Special Fittings":
					return "This is a special fitting order";

				case "Add Special Fittings":
					return "This is an additional fitting order";

				case "Omit Special Fittings":
					return "This is an omit fitting order";

				default:
					return "";
			}
		}

		private static string RemoveOrAddText(string orderType)
		{
			switch (orderType)
			{
				case string value when value.Contains("Order"):
					return "Please order this material";

				case string value when value.Contains("Add"):
					return "Please add this to the material order for this phase";

				case string value when value.Contains("Omit"):
					return "Please remove this from the material order of this phase";

				default:
					return "";
			}
		}

		private static string RemoveOrAddMaterial(string orderType)
		{
			switch (orderType)
			{
				case string value when value.Contains("Order"):
					return "The material to be ordered is as follows;";

				case string value when value.Contains("Add"):
					return "The material to be added is as follows;";

				case string value when value.Contains("Omit"):
					return "The material to be omitted is as follows;";

				default:
					return "";
			}
		}

		private static string IssueType(string orderType)
		{
			switch (orderType)
			{
				case string value when value.Contains("Order"):
					return "Prelim Issue";

				case string value when value.Contains("Add"):
					return "Additional Issue";

				case string value when value.Contains("Omit"):
					return "OMIT Issue";

				default:
					return "";
			}
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

			if (fabsecPresent)
			{
				AppendString(_fabsecEmail, fourth, out ccString);
			}

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