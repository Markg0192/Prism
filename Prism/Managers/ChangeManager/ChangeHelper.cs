using Prism.CustomDialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace Prism.Managers.ChangeManager
{
    public static class ChangeHelper
    {
        private static List<SteelItemBase> _revisedItems;
        private static List<SteelItemBase> _omitItems;
        private static List<SteelItemBase> _addItems;
        private static string IssueNo;

        public static bool RunChangeManagement(Model model, string currentIssueNo, string fileLocation, string phaseNumber, PrismProjectData projectData,
            out List<SteelItemBase> revisedItems, out List<SteelItemBase> omitItems, out List<SteelItemBase> addItems, out string messageForEmail)
        {
            _revisedItems = new List<SteelItemBase>();
            _omitItems = new List<SteelItemBase>();
            _addItems = new List<SteelItemBase>();
            WriteNewXML(model, currentIssueNo, fileLocation, phaseNumber);
            if (currentIssueNo.Contains("01"))
            {
                revisedItems = null;
                omitItems = null;
                addItems = null;
                messageForEmail = "";
                return true;
            }
            else
            {
                string xmlPath = ReturnXmlPath(currentIssueNo, fileLocation, phaseNumber);

                var previousSelection = LoadFromXml(xmlPath);

                var oldFittingDictionary = previousSelection.FittingMarkCounts.ToDictionary(fmc => fmc.Mark, fmc => fmc.Count);

                Console.WriteLine("Fetching new steel info for comparison...");
                var currentSelection = TeklaHelper.GetSelectedSteelInfo(model);

                var newFittingDictionary = CreateFittingDictionary(currentSelection);

                var omittedMembers = TeklaHelper.CompareSteelLists(previousSelection.Assemblies, currentSelection, oldFittingDictionary, newFittingDictionary);

                FormChangeLists(currentSelection, omittedMembers);

                revisedItems = _revisedItems;
                omitItems = _omitItems;
                addItems = _addItems;

                return WriteMyChangeMessage(fileLocation, phaseNumber, projectData, out messageForEmail);



                //  Logging.LogProgress(model.GetProjectInfo().Name, currentSelection.Count(), differenceMessages.Count());
            }
        }

        private static string ReturnXmlPath(string issueNumber, string fileLocation, string phaseNumber)
        {
            // If the selectedItem contains "(new)", it implies this is a new issue, so we extract the numeric part only
            if (issueNumber.Contains("(new)"))
            {
                issueNumber = issueNumber.Substring(0, 2); // or other appropriate substring logic if "(new)" is not at the end
            }

            if (int.TryParse(issueNumber, out int issueNo))
            {
                IssueNo = $"{issueNo:D2}";
                issueNo--; // Decrease the issueNo by 1
            }


            return $"{fileLocation}\\Phase {phaseNumber} - Issue {issueNo:D2}.xml";
        }

        private static bool WriteMyChangeMessage(string fileLocation, string phaseNumber, PrismProjectData projectData, out string finalHtmlMessage)
        {

            string phaseAndIssue = $"\\Phase {phaseNumber} - Issue {IssueNo}.txt";
            string filePath = $"{fileLocation}{phaseAndIssue}"; // Provide the desired file path
            List<string> htmlMessages = new List<string>();

            if (_revisedItems.Count() != 0 || _omitItems.Count() != 0 || _addItems.Count() != 0)
            {
                _revisedItems = _revisedItems.GroupBy(p => p.PartMark).Select(g => g.First()).ToList();
                _omitItems = _omitItems.GroupBy(p => p.PartMark).Select(g => g.First()).ToList();
                _addItems = _addItems.GroupBy(p => p.PartMark).Select(g => g.First()).ToList();
                _revisedItems = _revisedItems.OrderBy(g => g.PartMark).ToList();
                _omitItems = _omitItems.OrderBy(g => g.PartMark).ToList();
                _addItems = _addItems.OrderBy(g => g.PartMark).ToList();

                var rtfMessages = new List<string>();
                List<string> basicMessage = new List<string>();


                rtfMessages.Add(@"{\rtf1\ansi The following changes have been detected:");
                basicMessage.Add("The following changes have been detected");

                if (_revisedItems.Count > 0)
                {
                    htmlMessages.Add("<strong><u>Revisions</u></strong><br>");
                    rtfMessages.Add(@"\par\b\ul Revisions\ulnone\b0\par");
                    basicMessage.Add("\n\nRevisions\n\n");
                }

                foreach (SteelItemBase sib in _revisedItems)
                {
                    rtfMessages.Add(@"\i\ul\fs16 " + sib.PartMark + @"\ul0\i0\par");
                    basicMessage.Add($"{sib.PartMark}\n");
                    htmlMessages.Add("<i><u>" + sib.PartMark + "</u></i><br>");

                    foreach (string assemblyChange in sib.ChangeMessages)
                    {
                        rtfMessages.Add(@"\bullet " + assemblyChange);
                        basicMessage.Add($"\n- {assemblyChange}");
                        htmlMessages.Add("• " + assemblyChange + "<br>");
                    }
                    rtfMessages.Add(@"\par");
                    basicMessage.Add("\n\n");
                    htmlMessages.Add("<br>");
                }

                if (_addItems.Count > 0)
                {
                    rtfMessages.Add(@"\par\b\ul Adds\ulnone\b0\par");
                    basicMessage.Add("\n\nAdds\n");
                    htmlMessages.Add("<strong><u>Adds</u></strong><br>");
                }

                foreach (SteelItemBase sib in _addItems)
                {
                    rtfMessages.Add(@"\i\ul\fs16 " + sib.PartMark + @"\ul0\i0\par");
                    basicMessage.Add($"\n{sib.PartMark}\n");
                    htmlMessages.Add("<i><u>" + sib.PartMark + "</u></i><br>");

                    foreach (string assemblyChange in sib.ChangeMessages)
                    {
                        rtfMessages.Add(@"\bullet " + assemblyChange);
                        basicMessage.Add($"\n- {assemblyChange}");
                        htmlMessages.Add("• " + assemblyChange + "<br>");
                    }
                    rtfMessages.Add(@"\par");
                    basicMessage.Add("\n");
                    htmlMessages.Add("<br>");
                }

                if (_omitItems.Count > 0)
                {
                    rtfMessages.Add(@"\par\b\ul Omits\ulnone\b0\par");
                    basicMessage.Add($"\nOmits\n");
                    htmlMessages.Add("<strong><u>Omits</u></strong><br>");
                }

                foreach (SteelItemBase sib in _omitItems)
                {
                    rtfMessages.Add(@"\i\ul\fs16 " + sib.PartMark + @"\ul0\i0\par");
                    basicMessage.Add($"\n{sib.PartMark}\n");
                    htmlMessages.Add("<i><u>" + sib.PartMark + "</u></i><br>");

                    foreach (string assemblyChange in sib.ChangeMessages)
                    {
                        rtfMessages.Add(@"\bullet " + assemblyChange);
                        basicMessage.Add($"\n- {assemblyChange}");
                        htmlMessages.Add("• " + assemblyChange + "<br>");
                    }
                    rtfMessages.Add(@"\par");
                    basicMessage.Add("\n");
                    htmlMessages.Add("<br>");
                }

                rtfMessages.Add(@"\i\ul\fs16 " + "To continue, please confirm these changes are acceptable." + @"\ul0\i0\par");

                var finalMessage = string.Join(@"\par", rtfMessages) + "}";
                var finalBasicMessage = string.Join("", basicMessage);

                var messageForm = new ChangeMessage();
                messageForm.SetMessage(finalMessage);
                messageForm.ShowDialog();
                int stopReportContinue = messageForm.ContinueProgram;
             
                finalHtmlMessage = string.Join("", htmlMessages);
                if (stopReportContinue == 0)
                {
                    return false;
                }
                if (stopReportContinue == 1)
                {
                    return true;
                }
                if (stopReportContinue == 2)
                {
                    string TemporaryLogInformation = projectData.ProjPath + "\\" + Constants.PrismPackageFolderName + "\\" + "Change Manager Logs";
                    if (!Directory.Exists(TemporaryLogInformation)) { Directory.CreateDirectory(TemporaryLogInformation); }

                    File.WriteAllText(TemporaryLogInformation + phaseAndIssue, finalBasicMessage);
                    return false;
                }

                return true;
            }
            else
            {
                File.WriteAllText(filePath, "No changes");
                MessageBox.Show("No changes detected", "All good", MessageBoxButtons.OK, MessageBoxIcon.Information);
                finalHtmlMessage = "";
                return false;
            }
        }

        private static void WriteNewXML(Model model, string issueNumber, string fileLocation, string phaseNumber)
        {
            var currentSelection = TeklaHelper.GetSelectedSteelInfo(model);

            var fittingDictionary = CreateFittingDictionary(currentSelection);

            var wrapper = new SerializationWrapper
            {
                Assemblies = currentSelection,
                FittingMarkCounts = fittingDictionary.Select(kvp => new FittingMarkCount { Mark = kvp.Key, Count = kvp.Value }).ToList()
            };

            string filetoWrite = fileLocation + "\\" + "Phase " + phaseNumber + " - Issue " + issueNumber.Substring(0, 2) + ".xml";
            SaveToXml(wrapper, filetoWrite);
        }


        private static void FormChangeLists(List<MyAssembly> myAssemblies, List<SteelItemBase> omittedAssemblies)
        {
            _omitItems.AddRange(omittedAssemblies);
            foreach (MyAssembly assembly in myAssemblies)
            {
                AddItemToList(assembly);
                foreach (MyFitting fitting in assembly.Fittings)
                {
                    AddItemToList(fitting);
                }
            }
        }

        private static void AddItemToList(SteelItemBase item)
        {
            if (item.Modification == Enum.ModificationType.Revise || item.Modification == Enum.ModificationType.AddRevise || item.Modification == Enum.ModificationType.OmitRevise)
            {
                _revisedItems.Add(item);
            }
            if (item.Modification == Enum.ModificationType.Omit || item.Modification == Enum.ModificationType.OmitRevise)
            {
                _omitItems.Add(item);
            }
            if (item.Modification == Enum.ModificationType.Add || item.Modification == Enum.ModificationType.AddRevise)
            {
                _addItems.Add(item);
            }
        }

        private static Dictionary<string, int> CreateFittingDictionary(List<MyAssembly> myAssemblyList)
        {
            Dictionary<string, int> fittingMarkCounts = new Dictionary<string, int>();

            foreach (var assembly in myAssemblyList)
            {
                foreach (var fitting in assembly.Fittings)
                {
                    var fittingMark = fitting.PartMark;

                    if (fittingMarkCounts.ContainsKey(fittingMark))
                    {
                        fittingMarkCounts[fittingMark] += 1;
                    }
                    else
                    {
                        fittingMarkCounts[fittingMark] = 1;
                    }
                }
            }
            return fittingMarkCounts;
        }

        private static SerializationWrapper LoadFromXml(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new SerializationWrapper();
            }

            var serializer = new XmlSerializer(typeof(SerializationWrapper));
            using (var reader = new StreamReader(filePath))
            {
                return (SerializationWrapper)serializer.Deserialize(reader);

            }
        }

        private static void SaveToXml(SerializationWrapper wrapper, string filePath)
        {
            var serializer = new XmlSerializer(typeof(SerializationWrapper));
            using (var writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, wrapper);
            }
        }

        public static void PopulateIssueNumbers(ref ComboBox cmb_IssueNo, ref TextBox txt_PhaseNo, string fileLocation)
        {
            cmb_IssueNo.Items.Clear(); // clear old items
            var phaseNumber = txt_PhaseNo.Text;

            if (string.IsNullOrWhiteSpace(phaseNumber) || string.IsNullOrWhiteSpace(fileLocation))
                return; // do nothing if either phase number or file location is empty

            var xmlFiles = Directory.GetFiles(fileLocation, $"*Phase {phaseNumber}*.xml");

            var issueNumbers = new List<int>();
            foreach (var file in xmlFiles)
            {
                var match = Regex.Match(Path.GetFileName(file), $"Phase {phaseNumber} - Issue (\\d+).xml");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out var issueNumber))
                    {
                        issueNumbers.Add(issueNumber);
                    }
                }
            }

            issueNumbers.Sort(); // sort issue numbers
            foreach (var issueNumber in issueNumbers)
            {
                cmb_IssueNo.Items.Add(issueNumber.ToString("D2"));
            }

            if (issueNumbers.Count > 0)
            {
                var nextIssueNumber = issueNumbers[issueNumbers.Count - 1] + 1;
                cmb_IssueNo.Items.Add($"{nextIssueNumber:D2} (new)"); // Add a suggestion for a new issue number
            }
            else // In case there are no issues yet for the provided phase
            {
                cmb_IssueNo.Items.Add("01 (new)");
            }

            if (cmb_IssueNo.Items.Count > 0)
                cmb_IssueNo.SelectedIndex = cmb_IssueNo.Items.Count - 1; // select the last item
        }
    }
}