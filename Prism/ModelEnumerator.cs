using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;

namespace Prism
{
    public static class EnumExtension
    {
        public static SevModelEnumerator SevModelEnumerator(this Model model)
        {
            return new SevModelEnumerator(model);
        }
    }

    public class SevModelEnumerator
    {
        public ArrayList selectedModelParts;
        public ArrayList selectedModelBolts;
        public Tekla.Structures.Model.Part myParts;
        public BoltGroup myBolts;
        public List<string> myMarks;
        public ModelObjectEnumerator moe;
        public DrawingHandler myDrawingHandler;
        public DrawingEnumerator drawingEnum;       

        public SevModelEnumerator(Model model)
        {
            selectedModelParts = new ArrayList();
            selectedModelBolts = new ArrayList();
            myDrawingHandler = new DrawingHandler();
            moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();            
            drawingEnum = myDrawingHandler.GetDrawings();
            myMarks = new List<string>();

            foreach (object myObject in moe)
            {
                myParts = myObject as Tekla.Structures.Model.Part;
                myBolts = myObject as BoltGroup;
                
                if (myParts != null)
                {
                    selectedModelParts.Add(myParts);
                    myMarks.Add(myParts.GetPartMark());
                }
                
                if (myBolts !=null)
                {
                    selectedModelBolts.Add(myBolts);
                }
            }
        }
    }
}
