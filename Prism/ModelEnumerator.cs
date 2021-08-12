using System.Collections;
using System.Collections.Generic;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;

namespace Prism
{
    /// <summary>
    /// The model Enumerator class is the central area for all model enumeration, outputting model parts, bolts, drawings etc.
    /// </summary>
    public static class EnumExtension
    {
        public static SevModelEnumerator SevModelEnumerator(this Model model)
        {
            return new SevModelEnumerator();
        }
    }
    public class SevModelEnumerator
    {
        public ArrayList SelectedModelParts;
        public ArrayList SelectedModelBolts;
        public Tekla.Structures.Model.Part MyPart;
        public List<string> MyMarks;
        private ModelObjectEnumerator Moe;
        public DrawingHandler MyDrawingHandler;
        public DrawingEnumerator DrawingEnum;
        public int Ndrawings;

        public SevModelEnumerator()
        {           
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
                }
                BoltGroup MyBolts = myObject as BoltGroup;                 
                if (MyBolts !=null)
                {
                    SelectedModelBolts.Add(MyBolts);
                }
            }
        }
    }
}
