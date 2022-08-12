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

namespace Prism
{
    /// <summary>
    /// The Model modifiers class is where all changes to the model take place.
    /// This is usually adding stamps to member and drawing user fields.
    /// </summary>
    public static class ModelModifiers
    {
        public static void ModifyAttributes(this SelectedObjects selectedObjects, int stageNumber, PrismProjectData projectData)
        {
            foreach (Part part in selectedObjects.SelectedModelParts)
            {
                part.SetUserProperty(ModelUDA.CurrentStageName(stageNumber), projectData.Full);
                part.SetUserProperty(ModelUDA.CurrentStageDate(stageNumber), projectData.Date);
                if (stageNumber == 7)
                {
                    part.SetUserProperty(ModelUDA.CurrentStageNumber(stageNumber), part.GetPartMark());
                }
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

        public static void AddPrelimMarks(this SelectedObjects selectedObjects, ProjectInfo pInfo)
        {
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

        private static double GetPartLength(Part myPart)
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

        public static void SelectParts(this List<Part> partsToBeSelected, Model model)
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
    }
}