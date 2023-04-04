using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tekla.Structures;
using Tekla.Structures.Drawing.Automation;
using Tekla.Structures.Filtering;
using Tekla.Structures.Filtering.Categories;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;

namespace Prism
{
    public static class FabsecProcessing
    {
        private static double moveDistance = 100000;
        private static double carcassGreen = 100;

        public static void ProcessFabsecs(this SelectedObjects selectedObjects, Model model, PrismProjectData projectData)
        {
            List<Part> myFabsecCarcasses = GetMyFabsecs(selectedObjects); //Run through the selection and single out Fabsecs
            if (myFabsecCarcasses.Count() != 0) //Keep going if there are fabsecs present
            {
                myFabsecCarcasses.MovePGs(selectedObjects); // Move the model fabsecs to the "Material grave, deep below the model space" and remove them from selected objects
                myFabsecCarcasses.AddUniqueNumbering(model.GetProjectInfo());//Number each part uniquely with a profile specific prefix.
                myFabsecCarcasses.AddGreenToCarcasses(); //Add "green" to carcasses, currently 100mm each side.
                myFabsecCarcasses.SelectParts(); //select carcasses only for numbering
                ModelModifiers.PerformNumbering(); //Perform a numbering
                myFabsecCarcasses.SavePrelimNumbers(); //save prelim marks on carcasses (should look like PG1-1, PG1-2, PG2-1, PG2-2 etc...)

                List<Part> myFabsecs = myFabsecCarcasses.CopyPGs(selectedObjects); //Copy carcasses back into model space and add these to selected objects
                myFabsecs.RemoveGreenFromFabsecs(); // remove green from model space fabsecs
                myFabsecs.ReMarkModelFabsecs(); // remove unique prefixing from model members and return to local phase numbering
                myFabsecCarcasses.CreateCarcassDrawings(selectedObjects, model); //Create carcass drawings from the members in the material grave
                myFabsecCarcasses.ModifyAttributes(2, projectData);
            }
        }

        private static void SavePrelimNumbers(this List<Part> fabsecCarcassList)
        {
            foreach (Part carcass in fabsecCarcassList)
            {
                carcass.SetUserProperty(ModelUDA.PrelimMark(), carcass.GetPartMark());
            }
        }

        public static List<Part> GetMyFabsecs(this SelectedObjects selectedObjects)
        {
            List<Part> fabsecList = new List<Part>();
            string pgString = "PG";
            /*if(Environment.UserName == "mark.gibson")
            {
                pgString = "XXX";
            }*/
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

        private static bool PGsArePresent(List<Part> partsList)
        {
            foreach (Part part in partsList)
            {
                string sectionSize = part.Profile.ProfileString.Substring(0, 2);
                if (sectionSize == "PG") return true;
            }
            return false;
        }

        public static bool AddCarcassToSelection(Model model, SelectedObjects selectedObjects, out List<Part> originalFabsecs)
        {
            if (PGsArePresent(selectedObjects.SelectedModelParts))
            {
                GetCarcassesFromSelected(model, selectedObjects, out originalFabsecs);
                selectedObjects.SelectedModelParts.SelectParts();
                return true;
            }
            originalFabsecs = null;
            return false;
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

        private static void AddUniqueNumbering(this List<Part> fabsecList, ProjectInfo pInfo)
        {
            var groupedParts = fabsecList.GroupBy(p => new { profile = p.Profile.ProfileString }); //Group fabsecs by their profile

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

        private static List<Part> CopyPGs(this List<Part> fabsecList, SelectedObjects selectedObjects)
        {
            List<Part> fabsecCarcassList = new List<Part>();
            foreach (Part fabsec in fabsecList)
            {
                Vector myVector = new Vector(0, 0, moveDistance);
                Part copiedFabsec = Operation.CopyObject(fabsec, myVector) as Part;
                fabsecCarcassList.Add(copiedFabsec);
                copiedFabsec.SetUserProperty(ModelUDA.CurrentStageName(1), fabsec.StageString(ModelUDA.CurrentStageName(1))); //Set prism values and prelim on the new copied fabsec
                copiedFabsec.SetUserProperty(ModelUDA.CurrentStageDate(1), fabsec.StageString(ModelUDA.CurrentStageDate(1))); //All these values are unique in the model settings
                copiedFabsec.SetUserProperty(ModelUDA.CurrentStageName(2), fabsec.StageString(ModelUDA.CurrentStageName(2))); //This means they won't copy with the member naturally.
                copiedFabsec.SetUserProperty(ModelUDA.CurrentStageDate(2), fabsec.StageString(ModelUDA.CurrentStageDate(2)));
                copiedFabsec.SetUserProperty(ModelUDA.PrelimMark(), fabsec.GetPrelimMark());
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

        private static void AddGreenToCarcasses(this List<Part> fabsecCarcassList)
        {
            foreach (Beam carcass in fabsecCarcassList)
            {
                carcass.StartPointOffset.Dx -= carcassGreen;
                carcass.EndPointOffset.Dx += carcassGreen;
                carcass.Modify();
            }
        }

        private static void RemoveGreenFromFabsecs(this List<Part> fabsecList)
        {
            foreach (Beam carcass in fabsecList)
            {
                carcass.StartPointOffset.Dx += carcassGreen;
                carcass.EndPointOffset.Dx -= carcassGreen;
                carcass.Modify();
            }
        }

        private static void CreateCarcassDrawings(this List<Part> fabsecCarcassList, SelectedObjects selectedObjects, Model model)
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
            selectedObjects.SelectedModelParts.SelectParts();
        }

    }
}