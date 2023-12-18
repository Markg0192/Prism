using System;
using System.Collections;
using System.Collections.Generic;
//using System.EnterpriseServices.Internal;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using Tekla.Structures.Filtering;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using static Prism.Enums;
using ModelObject = Tekla.Structures.Model.ModelObject;
using Part = Tekla.Structures.Model.Part;

namespace Prism
{
    /// <summary>
    /// The SelecedObjects class gets and stores model objects for our use elsewhere.
    /// </summary>
    public class SelectedObjects
    {
        private ModelObjectEnumerator Moe;
        public DrawingHandler MyDrawingHandler;

        public SelectedObjects(StageTypes stageType)
        {
            NumbersUpToDate = true;
            AssembliesList = new List<Assembly>();
            SelectedModelParts = new List<Part>();
            LockedParts = new List<ModelObject>();
            SeversafeParts = new List<Part>();
            NonSeversafeParts = new List<Part>();
            MyDrawingHandler = new DrawingHandler();
            Moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
            MyMarks = new List<string>();

            ProcessModelObjects(stageType);

            PartWeight = Math.Round(PartWeight / 1000, 3);

            if (AssembliesList != null)
            {
                List<BoltGroup> SiteBolts = new List<BoltGroup>();
                List<BoltGroup> ShopBolts = new List<BoltGroup>();

                foreach (Assembly assembly in AssembliesList)
                {
                    List<BoltGroup> myBolts = GetBoltsFromAssembly(assembly);
                    if (myBolts != null)
                    {
                        SiteBolts.AddRange(myBolts.Where(b => b.Bolt && b.BoltType != BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP));
                        ShopBolts.AddRange(myBolts.Where(b => b.Bolt && b.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP));
                    }
                }

                AllBolts.Add(SiteBolts);
                AllBolts.Add(ShopBolts);
            }
        }

        private void ProcessModelObjects(StageTypes stageType)
        {
            foreach (object myObject in Moe)
            {
                if (!NumbersUpToDate) return;
                if (myObject is BaseComponent myComponent)
                {
                    ProcessChildren(myComponent, stageType);
                }
                else
                {
                    ProcessObject(myObject, stageType);
                }
            }
        }

        private void ProcessChildren(BaseComponent component, StageTypes stageType)
        {
            foreach (object child in component.GetChildren())
            {
                if (child is BaseComponent componentChild)
                {
                    foreach (object grandChild in componentChild.GetChildren())
                    {
                        ProcessObject(grandChild, stageType);
                    }
                }
                else
                {
                    ProcessObject(child, stageType);
                }
            }
        }

        public double SmallestX = 100000000;
        public double SmallestY = 100000000;
        public double SmallestZ = 100000000;
        public double BiggestX = -100000000;
        public double BiggestY = -100000000;
        public double BiggestZ = -100000000;

        //public double TotalWeight { get; set; }
        public double PartWeight { get; set; }
        public bool NumbersUpToDate { get; set; }
        public bool SeversafePresent = false;
        public List<Assembly> AssembliesList { get; set; }
        public List<List<BoltGroup>> AllBolts = new List<List<BoltGroup>>();
        public List<Part> SelectedModelParts { get; set; }
        public List<string> MyMarks { get; set; }
        public List<ModelObject> LockedParts { get; set; }
        public List<ModelObject> FabsecParts = new List<ModelObject>();
        public List<ModelObject> NonFabsecParts = new List<ModelObject>();
        public List<Part> SeversafeParts { get; set; }
        public List<Part> NonSeversafeParts { get; set; }
        public List<Part> OmittedParts = new List<Part>();

        private void CheckXYZSize(Part myPart)
        {
            if (myPart is Beam beam)
            {
                SmallestX = Math.Min(beam.EndPoint.X, Math.Min(beam.StartPoint.X, SmallestX));
                SmallestY = Math.Min(beam.EndPoint.Y, Math.Min(beam.StartPoint.Y, SmallestY));
                SmallestZ = Math.Min(beam.EndPoint.Z, Math.Min(beam.StartPoint.Z, SmallestZ));

                BiggestX = Math.Max(beam.EndPoint.X, Math.Max(beam.StartPoint.X, BiggestX));
                BiggestY = Math.Max(beam.EndPoint.Y, Math.Max(beam.StartPoint.Y, BiggestY));
                BiggestZ = Math.Max(beam.EndPoint.Z, Math.Max(beam.StartPoint.Z, BiggestZ));
            }
        }

        private void ProcessObject(object myObject, StageTypes stageType)
        {
            if (myObject is Part myPart)
            {
                if (IsValidPart(myPart))
                {
                    bool isSeversafe = IsSeversafePart(myPart);
                    if (stageType == StageTypes.FAB)
                    {
                        CheckXYZSize(myPart);
                        if (!isSeversafe && !Operation.IsNumberingUpToDate(myPart))
                        {
                            PrismWarnings.NumberingIsNotUpToDate();
                            NumbersUpToDate = false;
                            return;
                        }
                    }

                    SelectedModelParts.Add(myPart);

                    if (!isSeversafe && IsLocked(myPart)) LockedParts.Add(myPart);
                    if (isSeversafe)
                    {
                        SeversafeParts.Add(myPart); SeversafePresent = true;
                    }
                    else
                    {
                        NonSeversafeParts.Add(myPart);
                        if (myPart.Profile.ProfileString.StartsWith("PG"))
                        {
                            FabsecParts.Add(myPart);
                        }
                        else { NonFabsecParts.Add(myPart); }
                    } 

                    double weight = 0;
                    myPart.GetReportProperty(ModelUDA.Weight(), ref weight);
                    PartWeight = PartWeight + weight;

                    MyMarks.Add(myPart.GetPartMark());

                    if (myPart.GetAssembly() is Assembly assembly)
                    {
                        if (!AssembliesList.Any(x => x.Identifier.ToString() == assembly.Identifier.ToString()))
                        {
                            AssembliesList.Add(assembly);
                        }
                    }
                }
            }
        }

        private bool IsSeversafePart(Part myPart)
        {
            if (myPart.Name.Contains("SS-"))
            {
                return true;
            }
            return false;
        }

        private bool IsLocked(Part myPart)
        {
            string isLocked = "";
            myPart.GetReportProperty("OBJECT_LOCKED", ref isLocked);
            if (isLocked == "Yes")
            {
                return true;
            }
            return false;
        }

        private bool IsValidPart(Part p)
        {
            if (p.Name == "GROUT")
            {
                return false;
            }
            if (p.Profile.ProfileString.StartsWith("HEX"))
            {
                return false;
            }
            if (p.Profile.ProfileString.StartsWith("ROD"))
            {
                return false;
            }
            if (p.Name.StartsWith("HD"))
            {
                return false;
            }
            return true;
        }

        public void GetCorrectModelSelection()
        {
            ArrayList selectList = new ArrayList();
            foreach (Part part in SelectedModelParts)
            {
                selectList.Add(part);
            }
            Tekla.Structures.Model.UI.ModelObjectSelector ms = new Tekla.Structures.Model.UI.ModelObjectSelector();
            ms.Select(selectList);
        }

        private static List<BoltGroup> GetBoltsFromAssembly(Assembly assembly)
        {
            List<BoltGroup> myBoltsList = new List<BoltGroup>();
            ArrayList secondaries = assembly.GetSecondaries();
            secondaries.Add(assembly.GetMainPart());

            foreach (Tekla.Structures.Model.ModelObject item in secondaries)
            {
                if (item is Part part)
                {
                    ModelObjectEnumerator bolts = part.GetBolts();

                    foreach (var setOfBolts in bolts)
                    {
                        BoltGroup bolt = setOfBolts as BoltGroup;
                        if (bolt != null)
                        {
                            if (bolt.PartToBeBolted.Identifier.GUID == part.Identifier.GUID)
                            {
                                BoltGroup matchingBolt = null;
                                matchingBolt = myBoltsList.Find(x => x.Identifier.GUID == bolt.Identifier.GUID);
                                if (matchingBolt == null)
                                {
                                    myBoltsList.Add(bolt);
                                }
                            }
                        }
                    }
                }
            }
            return myBoltsList;
        }
    }
}