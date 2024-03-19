using Prism.CustomDialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Tekla.Structures.Model;

namespace Prism.Managers.ChangeManager
{
    public static class ChangeHelper
    {
        private static List<SteelItemBase> _revisedItems;
        private static List<SteelItemBase> _omitItems;
        private static List<SteelItemBase> _addItems;
        private static string IssueNo;

        public static bool RunChangeManagement(Model model, string currentIssueNo, string fileLocation, string phaseNumber, PrismProjectData projectData, SelectedObjects selectedObjects,
          ToolStrip toolStrip, ToolStripStatusLabel label, out List<SteelItemBase> revisedItems, out List<SteelItemBase> omitItems, out List<SteelItemBase> addItems, out string messageForEmail)
        {
            _revisedItems = new List<SteelItemBase>();
            _omitItems = new List<SteelItemBase>();
            _addItems = new List<SteelItemBase>();
            WriteNewXML(model, currentIssueNo, fileLocation, phaseNumber, selectedObjects, toolStrip, label);
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
                ReturnXmlPath(currentIssueNo, fileLocation, phaseNumber, out string previousIssuePath, out string currentIssuePath);

                Console.WriteLine("Fetching new steel info for comparison...");

                //  List<MyAssembly> newAssemblyList = new List<MyAssembly>();

                // Ensure newAssemblyList is a ConcurrentBag to safely add items in parallel
                ConcurrentBag<MyAssembly> newAssemblyList = new ConcurrentBag<MyAssembly>();

                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                int processedCount = 0;

                Parallel.ForEach(selectedObjects.AssembliesList, parallelOptions, ass =>
                {
                    MyAssembly myAssembly = CheckForAndGetExistingData(previousIssuePath + "\\", ass, model);
                    //  FormChangeLists(myAssembly); // Ensure this operation is thread-safe if uncommented
                    newAssemblyList.Add(myAssembly);

                    int currentCount = Interlocked.Increment(ref processedCount);

                    // Throttle UI updates to avoid overwhelming the UI thread
                    //   if (currentCount % 5 == 0 || currentCount == totalCount)
                    toolStrip.Invoke(new System.Action(() =>
                    {
                        label.Text = $"Comparing Objects: {currentCount} of {selectedObjects.AssembliesList.Count}";
                    }));
                });


                /*      foreach (Assembly ass in selectedObjects.AssembliesList)
                      {
                          MyAssembly myAssembly = CheckForAndGetExistingData(previousIssuePath + "\\", ass, model);
                       //   FormChangeLists(myAssembly);
                          newAssemblyList.Add(myAssembly);
                      }*/

                Dictionary<string, int> oldFittingCounter = LoadDictionaryFromXml(previousIssuePath + "\\Fitting Count.xml");
                Dictionary<string, int> newFittingCount = LoadDictionaryFromXml(currentIssuePath + "\\Fitting Count.xml");
                Dictionary<string, int> oldAssemblyCounter = LoadDictionaryFromXml(previousIssuePath + "\\Assembly Count.xml");
                Dictionary<string, int> newAssemblyCounter = LoadDictionaryFromXml(currentIssuePath + "\\Assembly Count.xml");

                List<MyFitting> fittingCompairsonResult = TeklaHelper.IdentifyDifferencesInLists(oldFittingCounter, newFittingCount);
                (List<string> messages, List<MyAssembly> omittedAssemblies) = TeklaHelper.IdentifyDifferencesInLists(oldAssemblyCounter, newAssemblyCounter, newAssemblyList.ToList(), fittingCompairsonResult);

                FormChangeLists(newAssemblyList, fittingCompairsonResult, omittedAssemblies);

                //  TeklaHelper.GetSelectedSteelInfo(model, fileLocation, selectedObjects);
                //
                //   var newFittingDictionary = CreateFittingDictionary(currentSelection);
                //
                //   var omittedMembers = TeklaHelper.CompareSteelLists(previousSelection.Assemblies, currentSelection, oldFittingDictionary, newFittingDictionary);

                revisedItems = _revisedItems;
                omitItems = _omitItems;
                addItems = _addItems;

                return WriteMyChangeMessage(fileLocation, phaseNumber, projectData, out messageForEmail);
                //  Logging.LogProgress(model.GetProjectInfo().Name, currentSelection.Count(), differenceMessages.Count());
            }
        }

        private static Dictionary<string, int> DeserializeOldCounter(string fileToDeserialize)
        {
            return LoadDictionaryFromXml(fileToDeserialize);
        }

        private static MyAssembly CheckForAndGetExistingData(string filePath, Assembly myAssembly, Model model)
        {
            //  we need to check this method works, it should return the list of xmls and do a basic comparison of assemlies.
            //    var xmlFiles = Directory.GetFiles(filePath, "*.xml");
            // Use LINQ to find the matching XML file

            MyAssembly newAssembly = MyAssembly.CreateMyAssembly(myAssembly, model);

            var matchingFile = Directory
                .EnumerateFiles(filePath, "*.xml") // Get all XML files in the directory
                .Select(file => new FileInfo(file)) // Convert file paths to FileInfo objects for easier manipulation
                .FirstOrDefault(fileInfo => fileInfo.Name.Equals(myAssembly.GetMainPart().Identifier.GUID + ".xml", StringComparison.OrdinalIgnoreCase)); // Find the first file that matches the assembly GUID

            if (matchingFile != null)
            {
                // A matching file is found, load and return the XML document
                TeklaHelper.CompareSimilarItems(newAssembly, matchingFile.FullName);
            }
            else
            {
                // No matching XML found meaning this part is new

            }
            return newAssembly;
        }


        private static void ReturnXmlPath(string issueNumber, string fileLocation, string phaseNumber, out string previousIssuePath, out string currentIssuePath)
        {
            // If the selectedItem contains "(new)", it implies this is a new issue, so we extract the numeric part only
            if (issueNumber.Contains("(new)"))
            {
                issueNumber = issueNumber.Substring(0, 2); // or other appropriate substring logic if "(new)" is not at the end
            }

            if (int.TryParse(issueNumber, out int issueNo))
            {
                IssueNo = $"{issueNo:D2}";

            }

            currentIssuePath = $"{fileLocation}\\Phase {phaseNumber} - Issue {issueNo:D2}";
            issueNo--; // Decrease the issueNo by 1
            previousIssuePath = $"{fileLocation}\\Phase {phaseNumber} - Issue {issueNo:D2}";
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

        private static void WriteNewXML(Model model, string issueNumber, string fileLocation, string phaseNumber, SelectedObjects selectedObjects, ToolStrip toolStrip, ToolStripStatusLabel label)
        {
            string filetoWrite = fileLocation + "\\" + "Phase " + phaseNumber + " - Issue " + issueNumber.Substring(0, 2);

            Directory.CreateDirectory(filetoWrite);

            TeklaHelper.CreateAssemblyXmls(model, filetoWrite, selectedObjects, toolStrip, label);
        }

        private static void FormChangeLists(MyAssembly myAssembly)
        {
            //   _omitItems.Add(omittedAssembly);

            AddItemToList(myAssembly);
            foreach (MyFitting fitting in myAssembly.Fittings)
            {
                AddItemToList(fitting);
            }

        }

        private static void FormChangeLists(ConcurrentBag<MyAssembly> myAssemblies, List<MyFitting> comparisonResult, List<MyAssembly> omittedAssemblies)//, SteelItemBase omittedAssembly)
        {
            //   _omitItems.Add(omittedAssembly);
            foreach (MyAssembly myAssembly in myAssemblies)
            {
                AddItemToList(myAssembly);
                foreach (MyFitting fitting in myAssembly.Fittings)
                {
                    AddItemToList(fitting);
                }
            }
            foreach (MyFitting f in comparisonResult)
            {
                AddItemToList(f);
            }
            foreach (MyAssembly f in omittedAssemblies)
            {
                AddItemToList(f);
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

        /*   private static SerializationWrapper LoadFromXml(string filePath)
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
           }*/

        private static Dictionary<string, int> LoadDictionaryFromXml(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, int>();
            }

            var serializer = new XmlSerializer(typeof(KeyValueListWrapper));
            using (var reader = new StreamReader(filePath))
            {
                var result = (KeyValueListWrapper)serializer.Deserialize(reader);
                // Convert List<KeyValueItem> to Dictionary<string, int>
                return result.Items.ToDictionary(item => item.Key, item => item.Value);
            }
        }

        /* private static Dictionary<string, int> LoadDictionaryFromXml(string filePath)
         {
             if (!File.Exists(filePath))
             {
                 return new Dictionary<string, int>();
             }

             var serializer = new XmlSerializer(typeof(SerializationWrapper));
             using (var reader = new StreamReader(filePath))
             {
                 return (Dictionary<string, int>)serializer.Deserialize(reader);

             }
         }*/

        /*  private static void SaveToXml(SerializationWrapper wrapper, string filePath)
          {
              var serializer = new XmlSerializer(typeof(SerializationWrapper));
              using (var writer = new StreamWriter(filePath))
              {
                  serializer.Serialize(writer, wrapper);
              }
          }*/

        public static void PopulateIssueNumbers(ref ComboBox cmb_IssueNo, ref TextBox txt_PhaseNo, string fileLocation)
        {
            cmb_IssueNo.Items.Clear(); // clear old items
            var phaseNumber = txt_PhaseNo.Text;

            if (string.IsNullOrWhiteSpace(phaseNumber) || string.IsNullOrWhiteSpace(fileLocation))
                return; // do nothing if either phase number or file location is empty

            if(!Directory.Exists(fileLocation))
            {
                Directory.CreateDirectory(fileLocation);
            }
            var phaseDirectories = Directory.GetDirectories(fileLocation, $"*Phase {phaseNumber}*");

            var issueNumbers = new List<int>();
            foreach (var directory in phaseDirectories)
            {
                var match = Regex.Match(Path.GetFileName(directory), $"Phase {phaseNumber} - Issue (\\d+)");
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