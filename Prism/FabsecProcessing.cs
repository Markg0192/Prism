using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tekla.Structures;
using Tekla.Structures.Drawing.Automation;
using Tekla.Structures.Filtering;
using Tekla.Structures.Filtering.Categories;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Model = Tekla.Structures.Model.Model;
using Operation = Tekla.Structures.Model.Operations.Operation;

namespace Prism
{
    public static class FabsecProcessing
    {
        private static double moveDistance = 1000000;
        private static double carcassGreen = 100;

        public static bool PrepFabsecCarcassesForMaterialOrder(this SelectedObjects selectedObjects, Model model, string startNumber)
        {
            List<PrismPart> myFabsecs = selectedObjects.GetFabsecParts();
            if (myFabsecs.Count != 0)
            {
                bool skipMainFabsecProcessing = !CheckPrismShouldAddGreenToFabsecs(myFabsecs);
                if (!skipMainFabsecProcessing)
                {
                    myFabsecs.AddUniqueNumbering(model.GetProjectInfo());
                    if (!myFabsecs.AddGreenToCarcasses(model, Enums.StageTypes.PrelimPG)) return false;
                    myFabsecs.SelectParts();
                    if (!ModelModifiers.PerformNumbering()) return false;
                }
                myFabsecs.SavePrelimNumbers(startNumber, skipMainFabsecProcessing);
            }
            return true;
        }

        private static bool CheckPrismShouldAddGreenToFabsecs(List<PrismPart> myFabsecs)
        {
            foreach (PrismPart p in myFabsecs)
            {
                string attribute = "";
                p.Part.GetReportProperty(ModelUDA.CurrentStageName(2), ref attribute);
                if (attribute != "")
                {
                    return PrismWarnings.FabsecsGreenAlreadyOn();
                }
            }
            return true;
        }

        private static void SavePrelimNumbers(this List<PrismPart> modelObjects, string startNumber, bool skip)
        {
            foreach (PrismPart p in modelObjects)
            {
                p.Part.SetUserProperty(ModelUDA.FabsecStartNumber(), startNumber);
                if (!skip) p.Part.SetUserProperty(ModelUDA.PrelimMark(), p.Part.GetPartMark());
            }
        }

        public static bool CreateFabsecCarcasses(this SelectedObjects selectedObjects, Model model)
        {
            List<PrismPart> fabsecParts = selectedObjects.GetFabsecParts();
            if (!CheckForCarcass(fabsecParts)) return false;

            ModelModifiers.ResetWorkPlane(model);

            List<PrismPart> myCarcasses = selectedObjects.GetFabsecParts().CopyPGs(selectedObjects);

            RemoveComponentsFromCopiedFabsecs(myCarcasses);

            if (!myCarcasses.AddGreenToCarcasses(model, Enums.StageTypes.Unassigned)) return false;
            fabsecParts.ChangeNumberingFromPGToStandard();
            model.CommitChanges();
            myCarcasses.ForceCarcassNumbering(model);
            if (!myCarcasses.CreateCarcassDrawings(selectedObjects, model)) return false; //Create carcass drawings from the members in the material grave
            ModifyAttribute(fabsecParts, ModelUDA.FabsecCarcassInfo(), Constants.FabsecModelShaftIndicator);
            ModifyAttribute(myCarcasses, ModelUDA.FabsecCarcassInfo(), Constants.FabsecCarcassIndicator);
            return true;
        }

        private static void RemoveComponentsFromCopiedFabsecs(List<PrismPart> copiedFabsecs)
        {
            foreach (PrismPart fabsec in copiedFabsecs)
            {
                var children = fabsec.Part.GetChildren();
                foreach (ModelObject child in children)
                {
                    if (child is Fitting)
                    {
                       var father = child.GetFatherComponent(); 
                        if(father != null)
                        {
                            father.Delete();
                        }
                        else
                        {
                            child.Delete();
                        }
                    }      
                }               
            }
        }

        private static void ChangeNumberingFromPGToStandard(this List<PrismPart> fabsecs)
        {
            foreach (PrismPart p in fabsecs)
            {
                string startNumber = "";
                p.Part.GetUserProperty(ModelUDA.FabsecStartNumber(), ref startNumber);
                p.Part.AssemblyNumber.Prefix = "";
                p.Part.AssemblyNumber.StartNumber = Convert.ToInt32(startNumber);
                p.Part.PartNumber.Prefix = "A";
                p.Part.PartNumber.StartNumber = Convert.ToInt32(startNumber);
                p.Part.Modify();
            }
        }

        private static void ModifyAttribute(List<PrismPart> listOfParts, string attributeToModify, string content)
        {
            foreach (PrismPart part in listOfParts)
            {
                part.Part.SetUserProperty(attributeToModify, content);
                part.Part.Modify();
            }
        }

        public static void ForceCarcassNumbering(this List<PrismPart> myCarcasses, Model model)
        {
            Tekla.Structures.Model.UI.ModelObjectSelector ms = new Tekla.Structures.Model.UI.ModelObjectSelector();

            //  ArrayList fullSelection = new ArrayList(myCarcasses);
            foreach (PrismPart carcass in myCarcasses)
            {
                string carcassPrelim = "";
                carcass.Part.GetReportProperty(ModelUDA.PrelimMark(), ref carcassPrelim);
                if (carcassPrelim != "")
                {
                    string[] splitCarcassNumber = carcassPrelim.Split('-');
                    ms.Select(new ArrayList { carcass });
                    carcass.Part.PartNumber.Prefix = splitCarcassNumber[0] + "-";
                    carcass.Part.AssemblyNumber.Prefix = splitCarcassNumber[0] + "-";
                    carcass.Part.AssemblyNumber.StartNumber = 1;
                    carcass.Part.PartNumber.StartNumber = 1;
                    carcass.Part.Modify();
                    // model.CommitChanges();
                    //  fullSelection.Add(carcass);
                    PrismMacroBuilder.FabsecNumberForcer(splitCarcassNumber[0], splitCarcassNumber[1]);

                }
            }
        }

        public static List<PrismPart> GetCarcassesFromSelected(Model model, SelectedObjects selectedObjects, out List<PrismPart> originalFabsecs)
        {
            originalFabsecs = selectedObjects.GetFabsecParts();
            List<PrismPart> allFabsecs = GetAllFabsecs(model);
            List<PrismPart> myCarcasses = new List<PrismPart>();

            foreach (PrismPart fabsec in originalFabsecs)
            {
                List<PrismPart> mathcingMembers = allFabsecs.FindAll(x => x.Prelim == fabsec.Prelim);
                PrismPart myCarcass = mathcingMembers.FirstOrDefault(x => x.Guid != fabsec.Guid);
                myCarcasses.Add(myCarcass);
                selectedObjects.PrismParts.Add(myCarcass);
                selectedObjects.MyMarks.Add(myCarcass.Part.GetPartMark());
                selectedObjects.PrismParts.Remove(fabsec);
                selectedObjects.MyMarks.Remove(fabsec.Part.GetPartMark());
            }
            return myCarcasses;
        }

        public static PrismPart GetCarcassFromSelected(Model model, PrismPart fabsec)
        {
            List<PrismPart> allFabsecs = GetAllFabsecs(model);
            List<PrismPart> mathcingMembers = allFabsecs.FindAll(x => x.Prelim == fabsec.Prelim);
            PrismPart myCarcass = mathcingMembers.FirstOrDefault(x => x.Guid != fabsec.Guid);
            return myCarcass;
        }

        public static bool AddCarcassToSelection(Model model, SelectedObjects selectedObjects, out List<PrismPart> originalFabsecs, out List<PrismPart> fabsecCarcasses)
        {
            if (!CheckCarcassHasBeenCreated(selectedObjects.GetFabsecParts()))
            { originalFabsecs = null; fabsecCarcasses = null; return false; }
            if (ModelChecker.NotOrderedParts.Count != 0)
            {
                PrismWarnings.Warning = ModelChecker.NotOrderedParts.Count.ToString();
                PrismWarnings.HasNotBeenOrderedFabsec();
                originalFabsecs = null; fabsecCarcasses = null;
                return false;
            }
            fabsecCarcasses = GetCarcassesFromSelected(model, selectedObjects, out originalFabsecs);
            selectedObjects.PrismParts.SelectParts();
            return true;
        }

        private static bool CheckCarcassHasBeenCreated(List<PrismPart> fabsecs)
        {
            foreach (PrismPart fabsec in fabsecs)
            {
                string attribute = "";
                fabsec.Part.GetUserProperty(ModelUDA.FabsecCarcassInfo(), ref attribute);
                string carcassOrdered = "";
                fabsec.Part.GetUserProperty(ModelUDA.FabsecCarcassOrdered(), ref carcassOrdered);
                if (attribute != Constants.FabsecModelShaftIndicator && attribute != Constants.FabsecCarcassIndicator)
                {
                    PrismWarnings.FabsecSelectedDoesNotHaveCarcass();
                    return false;
                }
                if (attribute == Constants.FabsecCarcassIndicator)
                {
                    PrismWarnings.FabsecCarcassSelected();
                    return false;
                }
                if (carcassOrdered != "")
                {
                    PrismWarnings.FabsecCarcassAlreadyOrdered();
                    return false;
                }
            }

            return true;
        }

        private static bool CheckForCarcass(List<PrismPart> fabsecs)
        {
            foreach (PrismPart fabsec in fabsecs)
            {
                string attribute = "";
                fabsec.Part.GetUserProperty(ModelUDA.FabsecCarcassInfo(), ref attribute);
                if (ModelChecker.NotOrderedParts.Count != 0)
                {
                    PrismWarnings.Warning = ModelChecker.NotOrderedParts.Count.ToString();
                    PrismWarnings.HasNotBeenOrderedFabsec();
                    return false;
                }
                if (attribute == Constants.FabsecModelShaftIndicator)
                {
                    PrismWarnings.FabsecAlreadyHasCarcass();
                    return false;
                }
                if (attribute == Constants.FabsecCarcassIndicator)
                {
                    PrismWarnings.FabsecCarcassSelected();
                    return false;
                }
            }

            return true;
        }

        private static List<PrismPart> GetAllFabsecs(Model model)
        {
            PartFilterExpressions.Profile profile = new PartFilterExpressions.Profile();
            ObjectFilterExpressions.Type objectType = new ObjectFilterExpressions.Type();

            TeklaStructuresDatabaseTypeEnum partType = TeklaStructuresDatabaseTypeEnum.PART;
            NumericConstantFilterExpression part = new NumericConstantFilterExpression(partType);
            BinaryFilterExpression objectFilter = new BinaryFilterExpression(objectType, NumericOperatorType.IS_EQUAL, part);

            StringConstantFilterExpression PG = new StringConstantFilterExpression("PG");
            BinaryFilterExpression filterByProfile = new BinaryFilterExpression(profile, StringOperatorType.CONTAINS, PG);

            BinaryFilterExpressionCollection expressionCollection = new BinaryFilterExpressionCollection();
            expressionCollection.Add(new BinaryFilterExpressionItem(objectFilter, BinaryFilterOperatorType.BOOLEAN_AND));
            expressionCollection.Add(new BinaryFilterExpressionItem(filterByProfile, BinaryFilterOperatorType.BOOLEAN_AND));
            ModelObjectEnumerator moe = model.GetModelObjectSelector().GetObjectsByFilter(expressionCollection);

            List<PrismPart> allFabsecs = new List<PrismPart>();

            foreach (var item in moe)
            {
                Part fabsec = item as Part;
                if (fabsec != null)
                {
                    allFabsecs.Add(new PrismPart(fabsec));
                }
            }
            return allFabsecs;
        }

        private static void AddUniqueNumbering(this List<PrismPart> fabsecList, ProjectInfo pInfo)
        {
            var groupedParts = fabsecList.GroupBy(p => new { profile = p.Part.Profile.ProfileString }); //Group fabsecs by their profile

            foreach (var gp in groupedParts)
            {
                int fabsecGroup = 0;
                string fabsecProfileString = "";

                fabsecProfileString = "PRISM PG" + gp.First().Part.Profile.ProfileString;
                pInfo.GetUserProperty(fabsecProfileString, ref fabsecGroup); //Look for the value of a project UDA made from the fabsec profile string
                if (fabsecGroup == 0) //if fabsec group == 0 then this is the first time we have encountered this profile
                {
                    pInfo.GetUserProperty(ModelUDA.NextFabsecPrefixNumber(), ref fabsecGroup); //This UDA is used to identify the PG prefix ie PG"1"
                    if (fabsecGroup == 0) // If this is zero then we are ecnountering the first PG in this model
                    {
                        fabsecGroup = 1;//so we set this to 1, PG"1"
                    }
                    pInfo.SetUserProperty(fabsecProfileString, fabsecGroup); //Set the UDA named after the profile with the fabsec group for future use
                    pInfo.SetUserProperty("PG" + fabsecGroup, 1); //Create a new UDA, named after the PG type and set its value to 1, this is the current last number used
                    pInfo.SetUserProperty(ModelUDA.NextFabsecPrefixNumber(), fabsecGroup + 1); //Set this to current group plus 1 so the next group of PGs to come along will get PG"2"
                }
                else //if fabsec group is not 0 then all the above has happened to this profile before and we can pick up from there below.
                {
                    Console.WriteLine("Last number read" + fabsecGroup);
                }

                int currentLastNumber = 0;

                foreach (PrismPart p in gp)
                {
                    pInfo.GetUserProperty($"PG{fabsecGroup}", ref currentLastNumber);
                    p.Part.AssemblyNumber.Prefix = $"PG{fabsecGroup}-";
                    p.Part.PartNumber.Prefix = $"PG{fabsecGroup}-";
                    p.Part.PartNumber.StartNumber = 1;
                    p.Part.AssemblyNumber.StartNumber = 1;

                    p.Part.SetUserProperty(ModelUDA.FabsecUniqueNumber(), $"PG{fabsecGroup}-{currentLastNumber}"); //The user phase UDA is used to trick tekla into thinking each piece in unique.
                    currentLastNumber++;
                    pInfo.SetUserProperty($"PG{fabsecGroup}", currentLastNumber);
                }
            }
        }

        private static void MovePGs(this List<PrismPart> fabsecList, SelectedObjects selectedObjects)
        {
            foreach (PrismPart fabsec in fabsecList)
            {
                selectedObjects.PrismParts.Remove(fabsec);
                Vector myVector = new Vector(0, 0, -moveDistance);
                Operation.MoveObject(fabsec.Part, myVector);
                fabsec.Part.Select();
            }
        }

        private static List<PrismPart> CopyPGs(this List<PrismPart> fabsecList, SelectedObjects selectedObjects)
        {
            List<PrismPart> fabsecCarcassList = new List<PrismPart>();
            foreach (PrismPart fabsec in fabsecList)
            {
                Vector myVector = new Vector(0, 0, moveDistance);
                PrismPart copiedFabsec = new PrismPart(Operation.CopyObject(fabsec.Part, myVector));
                fabsecCarcassList.Add(copiedFabsec);
                copiedFabsec.Part.SetUserProperty(ModelUDA.FabsecUniqueNumber(), fabsec.Part.StageString(ModelUDA.FabsecUniqueNumber()));
                copiedFabsec.Part.SetUserProperty(ModelUDA.CurrentStageName(1), fabsec.Part.StageString(ModelUDA.CurrentStageName(1))); //Set prism values and prelim on the new copied fabsec
                copiedFabsec.Part.SetUserProperty(ModelUDA.CurrentStageDate(1), fabsec.Part.StageString(ModelUDA.CurrentStageDate(1))); //All these values are unique in the model settings
                copiedFabsec.Part.SetUserProperty(ModelUDA.CurrentStageName(2), fabsec.Part.StageString(ModelUDA.CurrentStageName(2))); //This means they won't copy with the member naturally.
                copiedFabsec.Part.SetUserProperty(ModelUDA.CurrentStageDate(2), fabsec.Part.StageString(ModelUDA.CurrentStageDate(2)));
                copiedFabsec.Part.SetUserProperty(ModelUDA.CurrentStageName(2), fabsec.Part.StageString(ModelUDA.CurrentStageName(3)));
                copiedFabsec.Part.SetUserProperty(ModelUDA.CurrentStageDate(2), fabsec.Part.StageString(ModelUDA.CurrentStageDate(3)));
                copiedFabsec.Part.SetUserProperty(ModelUDA.PrelimMark(), fabsec.Part.GetPrelimMark());
                copiedFabsec.Part.SetUserProperty(ModelUDA.FabsecEngRef(), fabsec.Part.GetFabsecEngRef());
                selectedObjects.PrismParts.Add(copiedFabsec);
            }
            return fabsecCarcassList;
        }

        private static string StageString(this Part part, string stageType)
        {
            string stageString = "";
            part.GetUserProperty(stageType, ref stageString);
            return stageString;
        }

        private static void ReMarkModelFabsecs(this List<Part> fabsecList)
        {
            foreach (Part fabsec in fabsecList)
            {
                fabsec.AssemblyNumber.Prefix = GdomValues.AssemblyPrefix;
                fabsec.PartNumber.Prefix = GdomValues.AssemblyPartPrefix;
                fabsec.Class = GdomValues.FabsecClass;
                fabsec.Modify();
            }
        }

        private static bool AddGreenToCarcasses(this List<PrismPart> fabsecCarcassList, Model model, Enums.StageTypes stageType)
        {
            double tolerance = 10;
            List<ModelObject> fabsecsFailedToAddLength = new List<ModelObject>();
            foreach (PrismPart prismPart in fabsecCarcassList)
            {
                Beam carcass = prismPart.Part as Beam;
                double lengthBeforeExtension = ModelModifiers.GetPartLength(carcass);
                carcass.StartPointOffset.Dx -= carcassGreen;
                carcass.EndPointOffset.Dx += carcassGreen;
                carcass.Modify();
                double lengthAfterExtension = ModelModifiers.GetPartLength(carcass);

                double val = Math.Abs(lengthAfterExtension - lengthBeforeExtension - (carcassGreen * 2));

                if (val > tolerance)
                {
                    fabsecsFailedToAddLength.Add(carcass);
                }
            }
            if (fabsecsFailedToAddLength.Count > 0)
            {
                PrismWarnings.AddedLengthToFabsecFailed(fabsecsFailedToAddLength.Count);
                if (!PrismWarnings.ContinueAnyway())
                {
                    if (stageType == Enums.StageTypes.PrelimPG)
                    {
                        foreach (PrismPart pPart in fabsecCarcassList)
                        {
                            Beam carcass = pPart.Part as Beam;
                            carcass.StartPointOffset.Dx += carcassGreen;
                            carcass.EndPointOffset.Dx -= carcassGreen;
                            carcass.Modify();
                        }
                    }
                    else
                    {
                        foreach (PrismPart carcass in fabsecCarcassList)
                        {
                            carcass.Part.Delete();
                        }
                    }
                    model.CommitChanges();
                    return false;
                }
                return true;
            }

            return true;
        }

        public static void RemoveGreenFromFabsecs(this List<PrismPart> fabsecList)
        {
            foreach (PrismPart carcass in fabsecList)
            {
                Beam b = carcass.Part as Beam;
                double length = ModelModifiers.GetPartLength(b);
                carcass.Part.SetUserProperty(ModelUDA.FabsecOrderLength(), Math.Round(length, 0).ToString());

                b.StartPointOffset.Dx += carcassGreen;
                b.EndPointOffset.Dx -= carcassGreen;
                b.Modify();
            }
        }

        private static bool CreateCarcassDrawings(this List<PrismPart> fabsecCarcassList, SelectedObjects selectedObjects, Model model)
        {
            FileInfo file = new FileInfo(FirmFolderLoc.FabsecCarcassDrawingWizard());
            AutoDrawingRule rule = new AutoDrawingRule(file.FullName);
            AutoDrawingsStatusEnum status;
            List<Identifier> idList = new List<Identifier>();
            foreach (PrismPart part in fabsecCarcassList)
            {
                idList.Add(part.Part.Identifier);
            }

            fabsecCarcassList.SelectParts();
            if (!ModelModifiers.PerformNumbering()) return false;
            DrawingCreator.CreateDrawings(rule, idList, out status);
            return true;
        }
    }
}