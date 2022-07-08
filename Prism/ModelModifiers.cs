using System;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Drawing.Automation;
using System.IO;
using System.Collections.Generic;
using Tekla.Structures;

namespace Prism
{
    /// <summary>
    /// The Model modifiers class is where all changes to the model take place.
    /// This is usually adding stamps to member and drawing user fields.
    /// </summary>
    public static class ModelModifiers
    {
       /* public static ModelModifiers(Model model)
        {
            ModelData = model.CreateSevModelData();
        }

        public static SevModelData ModelData;*/

        public static void ModifyAttributes(this SelectedObjects modelEnum, int stageNumber, PrismProjectData projectData)
        {
            foreach (Part part in modelEnum.SelectedModelParts)
            {
                part.SetUserProperty($"PRISM-{stageNumber}-NAME", projectData.Full);
                part.SetUserProperty($"PRISM-{stageNumber}-DATE", projectData.Date);
                if (stageNumber == 3)
                {
                    part.SetUserProperty("SEV-UDA-39", projectData.Date);
                }
                if (stageNumber == 7)
                {
                    part.SetUserProperty($"PRISM-{stageNumber}-NUMBER", part.GetPartMark());
                }
                part.Modify();
            }
        }

        public static void AddStartNumbers(this SelectedObjects modelEnum, string startNumber)
        {
            foreach (Part p in modelEnum.SelectedModelParts)
            {
                p.PartNumber.StartNumber = Convert.ToInt32(startNumber);
                p.AssemblyNumber.StartNumber = Convert.ToInt32(startNumber);
            }
        }

        public static void LockSelected(this SelectedObjects modelEnum)
        {
            foreach (Part part in modelEnum.SelectedModelParts)
            {
                part.SetUserProperty("OBJECT_LOCKED", 1);
            }
        }

        public static void MoveAndRenameOmittedMembers(this SelectedObjects modelEnum, Model model)
        {
            double distanceToMovePartsInZ = -100000;
            foreach (Part p in modelEnum.SelectedModelParts)
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
            }
            model.CommitChanges();
        }

        public static void PerformNumbering(this SelectedObjects modelEnum)
        {
            new MacroBuilder().Callback("acmd_partnumbers_selected", string.Empty, "main_frame").Run();
        }

        public static void CreateDrawings(this SelectedObjects modelEnum)
        {
            string sniWizardLocation = @"C:\Sev_Firm_2019i\Roles\SNI\system\SNI Drawing Wizard.dproc";
            FileInfo file = new FileInfo(sniWizardLocation);
            AutoDrawingRule rule = new AutoDrawingRule(file.FullName);
            AutoDrawingsStatusEnum status;
            List<Identifier> idList = new List<Identifier>();
            foreach (Part part in modelEnum.SelectedModelParts)
            {
                idList.Add(part.Identifier);
            }
            DrawingCreator.CreateDrawings(rule, idList, out status);
        }
    }
}