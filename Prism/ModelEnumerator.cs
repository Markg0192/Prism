using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;

namespace Prism
{
    /// <summary>
    /// The model Enumerator class is the central area for all model enumeration, outputting model parts, bolts, drawings etc.
    /// </summary>
    public class SevModelEnumerator
    {
        public ArrayList SelectedModelParts;
        public ArrayList SelectedModelBolts;
        public List<Assembly> AssembliesList;
        public Tekla.Structures.Model.Part MyPart;
        public List<string> MyMarks;
        private ModelObjectEnumerator Moe;
       // public DrawingHandler MyDrawingHandler; This will be needed when drawing functionaility is introduced
        public bool NumbersNotUpToDate = true;

        public SevModelEnumerator(string stageType)
        {
            AssembliesList = new List<Assembly>();
            SelectedModelParts = new ArrayList();
            SelectedModelBolts = new ArrayList();
           //MyDrawingHandler = new DrawingHandler();
            Moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
            MyMarks = new List<string>();

            foreach (object myObject in Moe)
            {
                MyPart = myObject as Tekla.Structures.Model.Part;
                if (MyPart != null)
                {
                    if (!Operation.IsNumberingUpToDate(MyPart) && stageType == "FAB")
                    {
                        const string notUpToDateMessage = "Your member numbering is not up to date, please update and try again";
                        const string notUpToDateTitle = "Numbers not up to date";
                        MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        NumbersNotUpToDate = false;
                        return;
                    }
                    SelectedModelParts.Add(MyPart);
                    MyMarks.Add(MyPart.GetPartMark());

                    Assembly assembly = MyPart.GetAssembly();
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

                    if (MyBolts != null)
                    {
                        foreach (BoltGroup bolts in MyBolts)
                        {
                            SelectedModelBolts.Add(bolts);
                        }
                    }
                }
            }
        }

        private static List<BoltGroup> GetBoltsFromAssembly(Assembly assembly)
        {
            List<BoltGroup> myBoltsList = new List<BoltGroup>();
            ArrayList secondaries = assembly.GetSecondaries();
            secondaries.Add(assembly.GetMainPart());

            foreach (Tekla.Structures.Model.ModelObject item in secondaries)
            {
                Tekla.Structures.Model.Part part = item as Tekla.Structures.Model.Part;
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