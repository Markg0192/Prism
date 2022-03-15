using System.Diagnostics;

namespace Prism
{
    public class EmailWriter
    {              
        private const string _mailNewLine = "%0D%0A";

        public void WriteFabEmail(string fabPrefix, int assemblyCount, int partCount, string issueNumber, string phaseNumber, string projNumber, string projName, string userName)
        {
            FormIssueEmail("Test@email.com", $"{fabPrefix} Fab Issue",
                                                                                    $"Hello,{_mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"This is the fab package Issue {issueNumber} for phase {phaseNumber} in {projNumber}, {projName}.{_mailNewLine}" +
                                                                                    $"Please issue this package to the works when possible.{_mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"This fab package contains the following;{_mailNewLine}" +
                                                                                    $"{assemblyCount} Assemblies.{_mailNewLine}" +
                                                                                    $"{partCount} Parts.{_mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"Regards,{_mailNewLine}{_mailNewLine}" +
                                                                                    $"{userName}");
        }

        public void WriteMatEmail(string fabPrefix, int partCount, string issueNumber, string phaseNumber, string projNumber, string projName, string userName, string orderType)
        {   
            string typeOfOrderText = "";
            if(orderType == "Order Material")
            {
                typeOfOrderText = "This is the material order";
            }
            if (orderType == "Add Material")
            {
                typeOfOrderText = "This is an additional material order";
            }
            if (orderType == "Omit Material")
            {
                typeOfOrderText = "This is an omit material order";
            }
            FormIssueEmail("purchasing@severifeld.com", $"{fabPrefix} Prelim Issue",
                                                                                    $"Hello,{_mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"{typeOfOrderText}, issue {issueNumber} for phase {phaseNumber} in {projNumber}, {projName}.{_mailNewLine}" +
                                                                                    $"Please order this material as soon as possible.{_mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"This material order contains the following;{_mailNewLine}" +           
                                                                                    $"{partCount} Parts.{_mailNewLine}" +
                                                                                    $"{_mailNewLine}" +
                                                                                    $"Regards,{_mailNewLine}{_mailNewLine}" +
                                                                                    $"{userName}");
        }

        private static void FormIssueEmail(string emailAddress, string subject, string body)
        {
            Process.Start("mailto:" + emailAddress + "?subject=" + subject + "&body=" + body);
        }
    }
}