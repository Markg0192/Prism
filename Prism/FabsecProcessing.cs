//using Org.BouncyCastle.Utilities;
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
            List<ModelObject> myFabsecs = selectedObjects.FabsecParts;
            if (myFabsecs.Count != 0)
            {
                bool skipMainFabsecProcessing = !CheckPrismShouldAddGreenToFabsecs(myFabsecs);
                if (!skipMainFabsecProcessing)
                {
                    myFabsecs.AddUniqueNumbering(model.GetProjectInfo());
                    if (!myFabsecs.AddGreenToCarcasses()) return false;
                    myFabsecs.SelectParts();
                    ModelModifiers.PerformNumbering();
                }
                myFabsecs.SavePrelimNumbers(startNumber, skipMainFabsecProcessing);
            }
            return true;
        }

        private static bool CheckPrismShouldAddGreenToFabsecs(List<ModelObject> myFabsecs)
        {
            foreach (Part p in myFabsecs)
            {
                string attribute = "";
                p.GetReportProperty(ModelUDA.CurrentStageName(2), ref attribute);
                if (attribute != "")
                {
                    return PrismWarnings.FabsecsGreenAlreadyOn();
                }
            }
            return true;
        }

        private static void SavePrelimNumbers(this List<ModelObject> modelObjects, string startNumber, bool skip)
        {
            foreach (Part p in modelObjects)
            {
                p.SetUserProperty(ModelUDA.FabsecStartNumber(), startNumber);
                if (!skip) p.SetUserProperty(ModelUDA.PrelimMark(), p.GetPartMark());
            }
        }

        public static bool CreateFabsecCarcasses(this SelectedObjects selectedObjects, Model model)
        {
            if (!CheckForCarcass(selectedObjects.FabsecParts)) return false;
            List<ModelObject> myCarcasses = selectedObjects.FabsecParts.CopyPGs(selectedObjects);
            if (!myCarcasses.AddGreenToCarcasses()) return false;
            selectedObjects.FabsecParts.ChangeNumberingFromPGToStandard();
            model.CommitChanges();
            myCarcasses.ForceCarcassNumbering();
            myCarcasses.CreateCarcassDrawings(selectedObjects, model); //Create carcass drawings from the members in the material grave
            ModifyAttribute(selectedObjects.FabsecParts, ModelUDA.FabsecCarcassInfo(), Constants.FabsecModelShaftIndicator);
            ModifyAttribute(myCarcasses, ModelUDA.FabsecCarcassInfo(), Constants.FabsecCarcassIndicator);
            return true;
        }

        private static void ChangeNumberingFromPGToStandard(this List<ModelObject> fabsecs)
        {
            foreach (Part p in fabsecs)
            {
                string startNumber = "";
                p.GetUserProperty(ModelUDA.FabsecStartNumber(), ref startNumber);
                p.AssemblyNumber.Prefix = "";
                p.AssemblyNumber.StartNumber = Convert.ToInt32(startNumber);
                p.PartNumber.Prefix = "A";
                p.PartNumber.StartNumber = Convert.ToInt32(startNumber);
                p.Modify();
            }
        }

        private static void ModifyAttribute(List<ModelObject> listOfParts, string attributeToModify, string content)
        {
            foreach (Part part in listOfParts)
            {
                part.SetUserProperty(attributeToModify, content);
                part.Modify();
            }
        }

        public static void ForceCarcassNumbering(this List<ModelObject> myCarcasses)
        {
            Tekla.Structures.Model.UI.ModelObjectSelector ms = new Tekla.Structures.Model.UI.ModelObjectSelector();

            //  ArrayList fullSelection = new ArrayList(myCarcasses);
            foreach (Part carcass in myCarcasses)
            {
                string carcassPrelim = "";
                carcass.GetReportProperty(ModelUDA.PrelimMark(), ref carcassPrelim);
                if (carcassPrelim != "")
                {
                    string[] splitCarcassNumber = carcassPrelim.Split('-');
                    ms.Select(new ArrayList { carcass });
                    /*       carcass.PartNumber.Prefix = splitCarcassNumber[0] + "-";
                           carcass.AssemblyNumber.Prefix = splitCarcassNumber[0] + "-";
                           carcass.AssemblyNumber.StartNumber = 1;
                           carcass.PartNumber.StartNumber = 1;
                           carcass.Modify();*/

                    //  fullSelection.Add(carcass);
                    PrismMacroBuilder.FabsecNumberForcer(splitCarcassNumber[0], splitCarcassNumber[1]);

                }
            }
            //   ms.Select(fullSelection);
            //   ModelModifiers.PerformNumbering();
        }

        public static List<Part> GetMyFabsecs(this SelectedObjects selectedObjects)
        {
            List<Part> fabsecList = new List<Part>();
            string pgString = "PG";

            foreach (Part part in selectedObjects.SelectedModelParts)
            {
                if (part.Profile.ProfileString.StartsWith(pgString))
                {
                    fabsecList.Add(part);
                }
            }
            return fabsecList;
        }

        public static List<Part> GetCarcassesFromSelected(Model model, SelectedObjects selectedObjects, out List<Part> originalFabsecs)
        {
            originalFabsecs = GetMyFabsecs(selectedObjects);
            List<Part> allFabsecs = GetAllFabsecs(model);
            List<Part> myCarcasses = new List<Part>();

            foreach (Part fabsec in originalFabsecs)
            {
                List<Part> mathcingMembers = allFabsecs.FindAll(x => x.GetPrelimMark() == fabsec.GetPrelimMark());
                Part myCarcass = mathcingMembers.FirstOrDefault(x => x.Identifier.GUID != fabsec.Identifier.GUID);
                myCarcasses.Add(myCarcass);
                selectedObjects.SelectedModelParts.Add(myCarcass);
                selectedObjects.MyMarks.Add(myCarcass.GetPartMark());
                selectedObjects.SelectedModelParts.Remove(fabsec);
                selectedObjects.MyMarks.Remove(fabsec.GetPartMark());
            }
            return myCarcasses;
        }

        public static ModelObject GetCarcassFromSelected(Model model, Part fabsec)
        {
            List<Part> allFabsecs = GetAllFabsecs(model);
            List<Part> mathcingMembers = allFabsecs.FindAll(x => x.GetPrelimMark() == fabsec.GetPrelimMark());
            Part myCarcass = mathcingMembers.FirstOrDefault(x => x.Identifier.GUID != fabsec.Identifier.GUID);
            return myCarcass;
        }

        public static bool AddCarcassToSelection(Model model, SelectedObjects selectedObjects, out List<Part> originalFabsecs, out List<Part> fabsecCarcasses)
        {
            if (!CheckCarcassHasBeenCreated(selectedObjects.FabsecParts))
            { originalFabsecs = null; fabsecCarcasses = null; return false; }
            if (ModelChecker.NotOrderedParts.Count != 0)
            {
                PrismWarnings.Warning = ModelChecker.NotOrderedParts.Count.ToString();
                PrismWarnings.HasNotBeenOrderedFabsec();
                originalFabsecs = null; fabsecCarcasses = null;
                return false;
            }
            fabsecCarcasses = GetCarcassesFromSelected(model, selectedObjects, out originalFabsecs);
            selectedObjects.SelectedModelParts.SelectParts();
            return true;
        }

        private static bool CheckCarcassHasBeenCreated(List<ModelObject> fabsecs)
        {
            foreach (Part fabsec in fabsecs)
            {
                string attribute = "";
                fabsec.GetUserProperty(ModelUDA.FabsecCarcassInfo(), ref attribute);
                string carcassOrdered = "";
                fabsec.GetUserProperty(ModelUDA.FabsecCarcassOrdered(), ref carcassOrdered);
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

        private static bool CheckForCarcass(List<ModelObject> fabsecs)
        {
            foreach (Part fabsec in fabsecs)
            {
                string attribute = "";
                fabsec.GetUserProperty(ModelUDA.FabsecCarcassInfo(), ref attribute);
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

        private static List<Part> GetAllFabsecs(Model model)
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

            List<Part> allFabsecs = new List<Part>();

            foreach (var item in moe)
            {
                Part fabsec = item as Part;
                if (fabsec != null)
                {
                    allFabsecs.Add(fabsec);
                }
            }
            return allFabsecs;
        }

        private static void AddUniqueNumbering(this List<ModelObject> fabsecList, ProjectInfo pInfo)
        {
            List<Part> myParts = fabsecList.OfType<Part>().ToList();
            var groupedParts = myParts.GroupBy(p => new { profile = p.Profile.ProfileString }); //Group fabsecs by their profile

            foreach (var gp in groupedParts)
            {
                int fabsecGroup = 0;
                string fabsecProfileString = "";

                fabsecProfileString = "PRISM PG" + gp.First().Profile.ProfileString;
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

                foreach (Part p in gp)
                {
                    pInfo.GetUserProperty($"PG{fabsecGroup}", ref currentLastNumber);
                    p.AssemblyNumber.Prefix = $"PG{fabsecGroup}-";
                    p.PartNumber.Prefix = $"PG{fabsecGroup}-";
                    p.PartNumber.StartNumber = 1;
                    p.AssemblyNumber.StartNumber = 1;

                    p.SetUserProperty(ModelUDA.FabsecUniqueNumber(), $"PG{fabsecGroup}-{currentLastNumber}"); //The user phase UDA is used to trick tekla into thinking each piece in unique.
                    currentLastNumber++;
                    pInfo.SetUserProperty($"PG{fabsecGroup}", currentLastNumber);
                }
            }
        }

        private static void MovePGs(this List<Part> fabsecList, SelectedObjects selectedObjects)
        {
            foreach (Part fabsec in fabsecList)
            {
                selectedObjects.SelectedModelParts.Remove(fabsec);
                Vector myVector = new Vector(0, 0, -moveDistance);
                Operation.MoveObject(fabsec, myVector);
                fabsec.Select();
            }
        }

        private static List<ModelObject> CopyPGs(this List<ModelObject> fabsecList, SelectedObjects selectedObjects)
        {
            List<ModelObject> fabsecCarcassList = new List<ModelObject>();
            foreach (Part fabsec in fabsecList)
            {
                Vector myVector = new Vector(0, 0, moveDistance);
                Part copiedFabsec = Operation.CopyObject(fabsec, myVector) as Part;
                fabsecCarcassList.Add(copiedFabsec);
                copiedFabsec.SetUserProperty(ModelUDA.FabsecUniqueNumber(), fabsec.StageString(ModelUDA.FabsecUniqueNumber()));
                copiedFabsec.SetUserProperty(ModelUDA.CurrentStageName(1), fabsec.StageString(ModelUDA.CurrentStageName(1))); //Set prism values and prelim on the new copied fabsec
                copiedFabsec.SetUserProperty(ModelUDA.CurrentStageDate(1), fabsec.StageString(ModelUDA.CurrentStageDate(1))); //All these values are unique in the model settings
                copiedFabsec.SetUserProperty(ModelUDA.CurrentStageName(2), fabsec.StageString(ModelUDA.CurrentStageName(2))); //This means they won't copy with the member naturally.
                copiedFabsec.SetUserProperty(ModelUDA.CurrentStageDate(2), fabsec.StageString(ModelUDA.CurrentStageDate(2)));
                copiedFabsec.SetUserProperty(ModelUDA.CurrentStageName(2), fabsec.StageString(ModelUDA.CurrentStageName(3)));
                copiedFabsec.SetUserProperty(ModelUDA.CurrentStageDate(2), fabsec.StageString(ModelUDA.CurrentStageDate(3)));
                copiedFabsec.SetUserProperty(ModelUDA.PrelimMark(), fabsec.GetPrelimMark());
                copiedFabsec.SetUserProperty(ModelUDA.FabsecEngRef(), fabsec.GetFabsecEngRef());
                selectedObjects.SelectedModelParts.Add(copiedFabsec);
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

        private static bool AddGreenToCarcasses(this List<ModelObject> fabsecCarcassList)
        {
            List<ModelObject> fabsecsFailedToAddLength = new List<ModelObject>();
            foreach (Beam carcass in fabsecCarcassList)
            {
                double lengthBeforeExtension = ModelModifiers.GetPartLength(carcass);
                carcass.StartPointOffset.Dx -= carcassGreen;
                carcass.EndPointOffset.Dx += carcassGreen;
                carcass.Modify();
                double lengthAfterExtension = ModelModifiers.GetPartLength(carcass);
                if (lengthAfterExtension != lengthBeforeExtension + carcassGreen * 2)
                {
                    fabsecsFailedToAddLength.Add(carcass);
                }
            }
            if(fabsecsFailedToAddLength.Count > 0)
            {
                PrismWarnings.AddedLengthToFabsecFailed(fabsecsFailedToAddLength.Count);
                return false;
            }

            return true;
        }

        public static void RemoveGreenFromFabsecs(this List<ModelObject> fabsecList)
        {
            foreach (Beam carcass in fabsecList)
            {
                double length = 0;
                ModelModifiers.GetPartLength(carcass);
                carcass.SetUserProperty(ModelUDA.FabsecOrderLength(), Math.Round(length, 0).ToString());

                carcass.StartPointOffset.Dx += carcassGreen;
                carcass.EndPointOffset.Dx -= carcassGreen;
                carcass.Modify();
            }
        }

        private static void CreateCarcassDrawings(this List<ModelObject> fabsecCarcassList, SelectedObjects selectedObjects, Model model)
        {
            FileInfo file = new FileInfo(FirmFolderLoc.FabsecCarcassDrawingWizard());
            AutoDrawingRule rule = new AutoDrawingRule(file.FullName);
            AutoDrawingsStatusEnum status;
            List<Identifier> idList = new List<Identifier>();
            foreach (Part part in fabsecCarcassList)
            {
                idList.Add(part.Identifier);
            }

            fabsecCarcassList.SelectParts();
            ModelModifiers.PerformNumbering();
            DrawingCreator.CreateDrawings(rule, idList, out status);
        }
    }
}