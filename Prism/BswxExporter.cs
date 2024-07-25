using System;
using System.Threading;
using System.Collections;
using System.Windows.Automation;
using static Prism.Enums;
using Tekla.Structures.Model;
using Task = System.Threading.Tasks.Task;

namespace Prism
{
    public static class BswxExporter
    {
        public static void ExportBSWX(this SelectedObjects selectedObjects, string myFolder, PrismProjectData modelData, string phaseNumber,
            string issueNumber, StageTypes stageType)
        {
            //To run the bswx exporter we need to give it an input, this input can be an ArrayList, only 1 part is required, the exporter will then create a bswx of all parts selected in the model
            ArrayList myInputList = new ArrayList
            {  selectedObjects.PrismParts[0].Part};
            RunBswxExport(myInputList, myFolder, modelData, phaseNumber, issueNumber, stageType);
        }

        private static bool RunBswxExport(ArrayList inputList, string myFolder, PrismProjectData modelData, string phaseNumber,
            string issueNumber, StageTypes stageType)
        {
            ModelModifiers.HideOrRestoreTekla(7);
            Component bimRevExp = new Component();
            bimRevExp.Name = "BIMREVIEW Export";
            bimRevExp.Number = -100000;
            ComponentInput myInputs = new ComponentInput();
            myInputs.AddInputObjects(inputList);

            string typeString;
            if (stageType == StageTypes.Prelim1 || stageType == StageTypes.Prelim2 || stageType == StageTypes.Prelim3)
            {
                typeString = stageType.ToString().Substring(0, (stageType.ToString().Length - 1)).ToUpper();
            }
            else
            {
                typeString = stageType.ToString();
            }
            if (stageType == StageTypes.PrelimPG)
            {
                typeString = "PG-Prelim";
            }

            bimRevExp.SetComponentInput(myInputs);
            bimRevExp.LoadAttributesFromFile(ModelUDA.BSWXAttributeName(stageType));
            bimRevExp.SetAttribute("output_file_path", $@"{myFolder}\{modelData.ProjNumber}-{phaseNumber}-{typeString}-ISSUE{issueNumber}.bswx");
            bimRevExp.SetAttribute("selected_parts_only", 1);

            // Start ActionPopUps in a new thread before calling Insert
            //Task.Run(() => ActionPopUps());

            bimRevExp.Insert();

            ModelModifiers.HideOrRestoreTekla(9);
            return true;
        }

        private static void ActionPopUps()
        {
            AutomationElement mainWindow = AutomationElement.RootElement;

            // First pop-up with "Yes" and "No" buttons
            bool firstPopupHandled = false;
            for (int i = 0; i < 30; i++) // Retry for up to 30 times
            {
                try
                {
                    AutomationElementCollection popUps = mainWindow.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

                    foreach (AutomationElement popUp in popUps)
                    {
                        try
                        {
                            AutomationElement noButton = popUp.FindFirst(TreeScope.Descendants, new AndCondition(
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                                new PropertyCondition(AutomationElement.NameProperty, "No")));

                            if (noButton != null)
                            {
                                // Click the "No" button
                                InvokePattern invokePattern = noButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                invokePattern?.Invoke();
                                firstPopupHandled = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception while processing first popup: {ex.Message}");
                        }
                    }

                    if (firstPopupHandled)
                    {
                        Console.WriteLine("First pop-up handled successfully.");
                        break;
                    }

                    Thread.Sleep(1000); // Delay before the next check
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while searching for first popup: {ex.Message}");
                }
            }

            // Second pop-up with "OK" button
          /*  bool secondPopupHandled = false;
            for (int i = 0; i < 30; i++) // Retry for up to 30 times
            {
                try
                {
                    AutomationElementCollection popUps = mainWindow.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

                    foreach (AutomationElement popUp in popUps)
                    {
                        try
                        {
                            AutomationElement okButton = popUp.FindFirst(TreeScope.Descendants, new AndCondition(
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                                new PropertyCondition(AutomationElement.NameProperty, "OK")));

                            if (okButton != null)
                            {
                                // Click the "OK" button
                                InvokePattern invokePattern = okButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                invokePattern?.Invoke();
                                secondPopupHandled = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception while processing second popup: {ex.Message}");
                        }
                    }

                    if (secondPopupHandled)
                    {
                        Console.WriteLine("Second pop-up handled successfully.");
                        break;
                    }

                    Thread.Sleep(1000); // Delay before the next check
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while searching for second popup: {ex.Message}");
                }
            }

            if (!secondPopupHandled)
            {
                Console.WriteLine("Second pop-up with 'OK' button not found within the time limit.");
                // Optionally, you can throw an exception or handle it differently
                // throw new Exception("Second pop-up with 'OK' button not found.");
            }*/
        }

    }
}