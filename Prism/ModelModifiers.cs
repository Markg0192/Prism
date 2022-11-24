using System;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Drawing.Automation;
using System.IO;
using System.Collections.Generic;
using Tekla.Structures;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Tekla.Structures.Model.UI;
//using System.Threading;

namespace Prism
{
    /// <summary>
    /// The Model modifiers class is where all changes to the model take place.
    /// This is usually adding stamps to member and drawing user fields.
    /// </summary>
    public static class ModelModifiers
    {
        public static void VariationCheck(string phaseNumber, SelectedObjects myObjects, PrismProjectData projData)
        {
            if (phaseNumber.Contains("V") || phaseNumber.Contains("v"))
            {
                if (PrismWarnings.IsVariation())
                {
                    SetVariationAttribute(phaseNumber, myObjects);
                    projData.IsVariation = true;
                }                
            }
            else { projData.IsVariation = false; }
        }

        private static void SetVariationAttribute(string phaseNumber, SelectedObjects myObjects)
        {
            foreach (Part p in myObjects.SelectedModelParts)
            {
                string firstVNo = "";
                string secondVNo = "";
                p.GetUserProperty(ModelUDA.FirstVariationNumber(), ref firstVNo);
                if (firstVNo == "")
                {
                    p.SetUserProperty(ModelUDA.FirstVariationNumber(), phaseNumber);
                }
                else
                {
                    p.GetUserProperty(ModelUDA.SecondVariationNumber(), ref secondVNo);
                    if (secondVNo == "")
                    {
                        p.SetUserProperty(ModelUDA.SecondVariationNumber(), phaseNumber);
                    }
                    else
                    {
                        p.SetUserProperty(ModelUDA.FirstVariationNumber(), phaseNumber);
                    }
                }
            }
        }

        public static void ModifyAttributes(this List<Part> selectedObjects, int stageNumber, PrismProjectData projectData)
        {
            foreach (Part part in selectedObjects)
            {
                part.SetUserProperty(ModelUDA.CurrentStageName(stageNumber), projectData.Full);
                part.SetUserProperty(ModelUDA.CurrentStageDate(stageNumber), projectData.Date);
                if (stageNumber == 7)
                {
                    part.SetUserProperty(ModelUDA.PartMarkAtFab(), part.GetPartMark());
                }
                part.Modify();
            }
        }

        public static void StampBoltUDA(List<BoltGroup> allBolts, string name, string date)
        {
            foreach (BoltGroup bolts in allBolts)
            {
                bolts.SetUserProperty(ModelUDA.BoltOrderedBy(), name);
                bolts.SetUserProperty(ModelUDA.BoltOrderedDate(), date);
            }
        }

        public static void StampPartFabUDA(List<Part> selectedModelParts, string phaseNumber, string issueNumber)
        {
            foreach (Part part in selectedModelParts)
            {
                part.SetUserProperty(ModelUDA.FabStampUDA(), ModelUDA.FabStamp(phaseNumber, issueNumber));
                part.Modify();
            }
        }

        public static void AddStartNumbers(this SelectedObjects selectedObjects, string startNumber)
        {
            foreach (Part p in selectedObjects.SelectedModelParts)
            {
                p.PartNumber.StartNumber = Convert.ToInt32(startNumber);
                p.AssemblyNumber.StartNumber = Convert.ToInt32(startNumber);
            }
        }

        public static void AddPrelimMarksOldMethod(this SelectedObjects selectedObjects, ProjectInfo pInfo)
        {
            //This method adds prelim marks 'the old fashioned way' it rationalises members by profile, grade and length and adds numbers based on phase.
            //We have moved to numbering each piece individually but keeping this method incase we change our mind again.
            var allParts = selectedObjects.SelectedModelParts.Cast<Part>().ToList();
            var groupedParts = allParts.GroupBy(p => new { profile = p.Profile.ProfileString, length = GetPartLength(p), material = p.Material.MaterialString });

            foreach (var gp in groupedParts)
            {
                int currentLastNumber = 0;
                string prismLastNumberAttributeName = "";

                foreach (Part p in gp)
                {
                    if (p.GetPrelimMark().Length < 1)
                    {
                        prismLastNumberAttributeName = "PRISM" + p.AssemblyNumber.StartNumber;
                        pInfo.GetUserProperty(prismLastNumberAttributeName, ref currentLastNumber);
                        if (currentLastNumber == 0)
                        {
                            Console.WriteLine("Failed to read last number");
                            currentLastNumber = 1;
                            pInfo.SetUserProperty(prismLastNumberAttributeName, currentLastNumber);
                        }
                        else
                        {
                            Console.WriteLine("Last number read" + currentLastNumber);
                        }
                        p.SetUserProperty(ModelUDA.PrelimMark(), (currentLastNumber + p.AssemblyNumber.StartNumber - 1).ToString());
                    }
                }
                currentLastNumber++;
                pInfo.SetUserProperty(prismLastNumberAttributeName, currentLastNumber);
            }
        }

        public static void AddPrelimMarks(this SelectedObjects selectedObjects, ProjectInfo pInfo)
        {
            //This method adds prelim marks 'the old fashioned way' it rationalises members by profile, grade and length and adds numbers based on phase.
            //We have moved to numbering each piece individually but keeping this method incase we change our mind again.
            int currentLastNumber = 0;
            pInfo.GetUserProperty(ModelUDA.LastUsedPrelim(), ref currentLastNumber);

            foreach (Part p in selectedObjects.SelectedModelParts)
            {
                if (p.GetPrelimMark().Length == 0)
                {
                    if (currentLastNumber == 0)
                    {
                        Console.WriteLine("Failed to read last number");
                        currentLastNumber = 1;
                        pInfo.SetUserProperty(ModelUDA.LastUsedPrelim(), currentLastNumber);
                    }
                    else
                    {
                        Console.WriteLine("Last number read" + currentLastNumber);
                    }
                    p.SetUserProperty(ModelUDA.PrelimMark(), currentLastNumber.ToString());
                }
                currentLastNumber++;
            }
            pInfo.SetUserProperty(ModelUDA.LastUsedPrelim(), currentLastNumber);
        }

        public static void ClearPrelimMarking(ProjectInfo pInfo, string resetNumber)
        {
            pInfo.SetUserProperty(ModelUDA.LastUsedPrelim(), Convert.ToInt32(resetNumber));
        }

        public static double GetPartLength(Part myPart)
        {
            ArrayList points = myPart.GetCenterLine(true);
            Point start = points[0] as Point;
            Point end = points[1] as Point;
            double Length = Distance.PointToPoint(end, start);
            return Length;
        }

        public static void LockSelected(this SelectedObjects selectedObjects)
        {
            foreach (Part part in selectedObjects.SelectedModelParts)
            {
                part.SetUserProperty(ModelUDA.ObjectLock(), 1);
            }
        }

        public static void MoveAndRenameOmittedMembers(this SelectedObjects selectedObjects)
        {
            double distanceToMovePartsInZ = -100000;
            foreach (Part p in selectedObjects.SelectedModelParts)
            {
                p.Name = "OMIT";
                p.AssemblyNumber.Prefix = "OMIT";
                p.PartNumber.Prefix = "OMIT";
                string test = p.Class;
                p.Class = "6";
                p.GetPhase(out Phase currentPhase);
                Phase myPhase = new Phase((Convert.ToInt32(currentPhase.PhaseNumber) + 1000000), $"Phase {currentPhase.PhaseNumber} OMIT", "", 0);
                myPhase.Insert();
                p.SetPhase(myPhase);
                p.Modify();
                Vector myVector = new Vector(0, 0, distanceToMovePartsInZ);
                Operation.MoveObject(p, myVector);
                p.Select();
            }
        }

        public static void PerformNumbering()
        {
            new MacroBuilder().Callback("acmd_partnumbers_selected", string.Empty, "main_frame").Run();
        }

        public static void CreateDrawings(this SelectedObjects selectedObjects)
        {
            FileInfo file = new FileInfo(FirmFolderLoc.DrawingWizard());
            AutoDrawingRule rule = new AutoDrawingRule(file.FullName);
            AutoDrawingsStatusEnum status;
            List<Identifier> idList = new List<Identifier>();
            foreach (Part part in selectedObjects.SelectedModelParts)
            {
                idList.Add(part.Identifier);
            }
            DrawingCreator.CreateDrawings(rule, idList, out status);
        }

        public static void SelectParts(this List<Part> partsToBeSelected)
        {
            ArrayList selectList = new ArrayList();
            foreach (Part part in partsToBeSelected)
            {
                selectList.Add(part);
            }
            Tekla.Structures.Model.UI.ModelObjectSelector ms = new Tekla.Structures.Model.UI.ModelObjectSelector();
            ms.Select(selectList);
            foreach (Part part in partsToBeSelected)
            {
                part.Modify();
            }
        }

        public static string GetPrelimMark(this Part p)
        {
            string prelim = "";
            p.GetUserProperty(ModelUDA.PrelimMark(), ref prelim);
            return prelim;
        }

        public static void HideOrRestoreTekla(int hideOrRestore)
        {
            //if hideOrRestore = 7 then minimise tekla
            //if hideOrRestore = 9 then restore tekla
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.MainWindowTitle.ToUpper().Contains("TEKLA"))
                {
                    ShowWindow(process.MainWindowHandle, hideOrRestore);
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void RemoveLog(string folderPath)
        {
            List<string> fileTypes = new List<string>();
            if (Directory.Exists(folderPath))
            {
                foreach (string subFile in Directory.GetFiles(folderPath))
                {
                    fileTypes.Add(subFile.Substring(subFile.Length - 3));
                }
                if (fileTypes.Where(x => x.Contains("bswx")).Count() == fileTypes.Where(x => x.Contains("Log")).Count())
                {
                    DeleteLog(folderPath);
                }
                else
                {
                    // Thread.Sleep(1000);
                    RemoveLog(folderPath);
                }
            }
        }

        private static void DeleteLog(string folderPath)
        {
            foreach (string subFile in Directory.GetFiles(folderPath))
            {
                if (subFile.Substring(subFile.Length - 3) == "txt")
                {
                    File.Delete(subFile);
                }
            }
        }

        public static void RedrawViews()
        {
            var selectedView = ViewHandler.GetAllViews();

            while (selectedView.MoveNext())
            {
                ViewHandler.RedrawView(selectedView.Current);
            }
        }

        public static void SetPartsRed(List<ModelObject> myParts)
        {
            ModelObjectVisualization.SetTransparencyForAll(TemporaryTransparency.SEMITRANSPARENT);
            ModelObjectVisualization.SetTemporaryStateForAll(new Color(0.5, 0.5, 0.5));
            ModelObjectVisualization.SetTemporaryState(myParts, new Color(1, 0, 0));
        }
    }
}