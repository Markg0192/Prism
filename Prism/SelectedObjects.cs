using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using static Prism.Enums;

namespace Prism
{
    /// <summary>
    /// The SelecedObjects class gets and stores model objects for our use elsewhere.
    /// </summary>
    public class SelectedObjects
    {
        private ModelObjectEnumerator Moe;
        // public DrawingHandler MyDrawingHandler; This will be needed when drawing functionaility is introduced

        public SelectedObjects(StageTypes stageType)
        {
            List<BoltGroup> SiteBolts = new List<BoltGroup>();
            List<BoltGroup> ShopBolts = new List<BoltGroup>();

            NumbersNotUpToDate = true;
            AssembliesList = new List<Assembly>();
            SelectedModelParts = new List<Part>();
            //MyDrawingHandler = new DrawingHandler();
            Moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
            MyMarks = new List<string>();

            foreach (object myObject in Moe) // This selects all items, if it is a part add to list, if it is a component get the objects within and add, do this twice to deal with components inside components.
            {
                if (myObject is BaseComponent myComponent)
                {
                    ModelObjectEnumerator children = myComponent.GetChildren();
                    foreach (object child in children)
                    {
                        if (child is BaseComponent componentChild)
                        {
                            ModelObjectEnumerator grandChildren = componentChild.GetChildren();
                            foreach (object grandChild in grandChildren)
                            {
                                ProcessObject(grandChild, stageType);
                            }
                        }
                        else ProcessObject(child, stageType);
                    }
                }
                else ProcessObject(myObject, stageType);
            }

            if (AssembliesList != null)
            {
                foreach (Assembly assembly in AssembliesList)
                {
                    List<BoltGroup> MyBolts = GetBoltsFromAssembly(assembly);
                    double weight = 0;
                    assembly.GetReportProperty(ModelUDA.Weight(), ref weight);
                    TotalWeight = TotalWeight + weight;
                    if (MyBolts != null)
                    {
                        foreach (BoltGroup bolts in MyBolts)
                        {
                            if (bolts.Bolt)
                            {
                                if (bolts.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP)
                                {
                                    ShopBolts.Add(bolts);
                                    return;
                                }
                                SiteBolts.Add(bolts);
                            }
                        }
                    }
                }
                TotalWeight = Math.Round(TotalWeight / 1000, 3);
            }
            GetCorrectModelSelection();
            AllBolts.Add(SiteBolts);
            AllBolts.Add(ShopBolts);
        }

        public double SmallestX = 100000000;
        public double SmallestY = 100000000;
        public double SmallestZ = 100000000;
        public double BiggestX = -100000000;
        public double BiggestY = -100000000;
        public double BiggestZ = -100000000;

        public double TotalWeight { get; set; }
        public bool NumbersNotUpToDate { get; set; }
        public List<Assembly> AssembliesList { get; set; }
        public List<List<BoltGroup>> AllBolts = new List<List<BoltGroup>>();
        public List<Part> SelectedModelParts { get; set; }
        public List<string> MyMarks { get; set; }

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
                CheckXYZSize(myPart);
                if (!Operation.IsNumberingUpToDate(myPart) && stageType == StageTypes.FAB)
                {
                    PrismWarnings.NumberingIsNotUpToDate();
                    NumbersNotUpToDate = false;
                    return;
                }
                SelectedModelParts.Add(myPart);

                MyMarks.Add(myPart.GetPartMark());

                if (myPart.GetAssembly() is Assembly assembly)
                {
                    Assembly matchingAssembly = null;
                    matchingAssembly = AssembliesList.Find(x => x.Identifier.ToString() == assembly.Identifier.ToString());
                    if (matchingAssembly == null)
                    {
                        AssembliesList.Add(assembly);
                    }
                }
            }
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

            foreach (ModelObject item in secondaries)
            {
                if (item is Part part)
                {
                    ModelObjectEnumerator bolts = part.GetBolts();

                    foreach (var setOfBolts in bolts)
                    {
                        BoltGroup bolt = setOfBolts as BoltGroup;
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
            return myBoltsList;
        }
    }
}