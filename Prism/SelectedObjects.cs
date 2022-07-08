using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;

namespace Prism
{
    /// <summary>
    /// The SelecedObjects class is the central area for all model enumeration, outputting model parts, bolts, drawings etc.
    /// </summary>
    public class SelectedObjects
    {
        private ModelObjectEnumerator Moe;
        // public DrawingHandler MyDrawingHandler; This will be needed when drawing functionaility is introduced

        public SelectedObjects(string stageType)
        {
            NumbersNotUpToDate = true;
            AssembliesList = new List<Assembly>();
            SelectedModelParts = new List<Part>();
            SelectedModelBolts = new List<BoltGroup>();
            //MyDrawingHandler = new DrawingHandler();
            Moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
            MyMarks = new List<string>();

            foreach (object myObject in Moe)
            {
                Part myPart = myObject as Part;
                if (myPart != null)
                {
                    if (!Operation.IsNumberingUpToDate(myPart) && stageType == "FAB")
                    {
                        const string notUpToDateMessage = "Your member numbering is not up to date, please update and try again";
                        const string notUpToDateTitle = "Numbers not up to date";
                        MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        NumbersNotUpToDate = false;
                        return;
                    }
                    SelectedModelParts.Add(myPart);

                    MyMarks.Add(myPart.GetPartMark());

                    Assembly assembly = myPart.GetAssembly();
                    if (assembly != null)
                    {
                        Assembly matchingAssembly = null;
                        matchingAssembly = AssembliesList.Find(x => x.Identifier.ToString() == assembly.Identifier.ToString());
                        if (matchingAssembly == null) AssembliesList.Add(assembly);
                    }
                }
            }

            if (AssembliesList != null)
            {
                foreach (Assembly assembly in AssembliesList)
                {
                    List<BoltGroup> MyBolts = GetBoltsFromAssembly(assembly);
                    double weight = 0;
                    assembly.GetReportProperty("WEIGHT", ref weight);
                    totalWeight = totalWeight + weight;
                    if (MyBolts != null)
                    {
                        foreach (BoltGroup bolts in MyBolts)
                        {
                            SelectedModelBolts.Add(bolts);
                        }
                    }
                }
                totalWeight = Math.Round(totalWeight / 1000, 3);
            }
        }

        public double totalWeight { get; set; }
        public bool NumbersNotUpToDate { get; set; }
        public List<Assembly> AssembliesList { get; set; }
        public List<BoltGroup> SelectedModelBolts { get; set; }
        public List<Part> SelectedModelParts { get; set; }
        public List<string> MyMarks { get; set; }

        private static List<BoltGroup> GetBoltsFromAssembly(Assembly assembly)
        {
            List<BoltGroup> myBoltsList = new List<BoltGroup>();
            ArrayList secondaries = assembly.GetSecondaries();
            secondaries.Add(assembly.GetMainPart());

            foreach (ModelObject item in secondaries)
            {
                Part part = item as Part;
                if (part == null)
                {
                    continue;
                }
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
            return myBoltsList;
        }
    }
}