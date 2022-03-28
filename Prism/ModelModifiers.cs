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
    public class ModelModifiers
    {
        public ModelModifiers(Model model)
        {
            ModelData = model.CreateSevModelData();
        }

        public SevModelData ModelData;

        public void ModifyAttributes(SevModelEnumerator modelEnum, int stageNumber)
        {
            foreach (Part part in modelEnum.SelectedModelParts)
            {
                part.SetUserProperty($"PRISM-{stageNumber}-NAME", ModelData.Full);
                part.SetUserProperty($"PRISM-{stageNumber}-DATE", ModelData.Date);
                if (stageNumber == 3)
                {
                    part.SetUserProperty("SEV-UDA-39", ModelData.Date);
                }
                if (stageNumber == 7)
                {
                    part.SetUserProperty($"PRISM-{stageNumber}-NUMBER", part.GetPartMark());
                }
                part.Modify();
            }
        }

        public void AddStartNumbers(SevModelEnumerator modelEnum, string startNumber)
        {
            foreach (Part p in modelEnum.SelectedModelParts)
            {
                p.PartNumber.StartNumber = Convert.ToInt32(startNumber);
                p.AssemblyNumber.StartNumber = Convert.ToInt32(startNumber);
            }
        }

        public void LockSelected(SevModelEnumerator modelEnum)
        {
            foreach (Part part in modelEnum.SelectedModelParts)
            {
                part.SetUserProperty("OBJECT_LOCKED", 1);
            }
        }

        public void MoveAndRenameOmittedMembers(SevModelEnumerator modelEnum, Model model)
        {
            int distanceToMovePartsInZ = -100000;
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

        public void NumberModel(SevModelEnumerator modelEnum)
        {
           //TeklaStructures.Connect();
           // TeklaStructures.CommonTasks.PerformNumbering(true);
        }
        public void PerformNumbering()
        {
            new MacroBuilder().Callback("acmd_partnumbers_selected", string.Empty, "main_frame").Run();
        }

        public void CreateDrawings(SevModelEnumerator modelEnum)
        {
            FileInfo file = new FileInfo(@"C:\Sev_Firm_2019i\Roles\SNI\system\SNI Drawing Wizard.dproc");
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