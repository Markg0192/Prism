using System.Collections;
using System.Collections.Generic;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;

namespace Prism
{
    /// <summary>
    /// The model Enumerator class is the central area for all model enumeration, outputting model parts, bolts, drawings etc.
    /// </summary>
    public class SevModelEnumerator
    {
        public ArrayList SelectedModelParts;
        public ArrayList SelectedModelBolts;
        private List<Assembly> assembliesList;
        public Tekla.Structures.Model.Part MyPart;
        public List<string> MyMarks;
        private ModelObjectEnumerator Moe;
        public DrawingHandler MyDrawingHandler;
        public DrawingEnumerator DrawingEnum;
        public int Ndrawings;

        public SevModelEnumerator()
        {
            assembliesList = new List<Assembly>();
            SelectedModelParts = new ArrayList();
            SelectedModelBolts = new ArrayList();
            MyDrawingHandler = new DrawingHandler();
            Moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
            DrawingEnum = MyDrawingHandler.GetDrawings();
            Ndrawings = DrawingEnum.GetSize();
            MyMarks = new List<string>();

            foreach (object myObject in Moe)
            {
                MyPart = myObject as Tekla.Structures.Model.Part;
                if (MyPart != null)
                {
                    SelectedModelParts.Add(MyPart);
                    MyMarks.Add(MyPart.GetPartMark());

                    Assembly assembly = MyPart.GetAssembly();
                    if (assembly != null)
                    {
                        Assembly matchingAssembly = null;
                        matchingAssembly = assembliesList.Find(x => x.Identifier.ToString() == assembly.Identifier.ToString());
                        if (matchingAssembly == null) assembliesList.Add(assembly);
                    }
                }
            }

            if (assembliesList != null)
            {
                foreach (Assembly assembly in assembliesList)
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

        public static List<BoltGroup> GetBoltsFromAssembly(Assembly assembly)
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
