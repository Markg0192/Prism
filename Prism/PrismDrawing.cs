using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;

namespace Prism
{
    public class PrismDrawing
    {
        private string _notRequired = "Not Required";
        private Model _model;
        private SevModelEnumerator _modelEnum;

        public PrismDrawing(Drawing currentDrawing, SevModelEnumerator modelEnum, Model model)
        {
            _model = model;
            _modelEnum = modelEnum;
            GetPrismDrawing(currentDrawing);
        }

        public bool IsDrawingRequired = true;
        public string RevMark { get; set; }
        public string PdfName { get; set; }
        public string DrawingFolderName { get; set; }
        public Drawing TeklaDrawing { get; set; }


        private string GetDrawingRevison(Drawing currentDrawing)
        {
            AssemblyDrawing assDraw = currentDrawing as AssemblyDrawing;
            SinglePartDrawing singDraw = currentDrawing as SinglePartDrawing;
            Tekla.Structures.Identifier drawingID = null;
            string revMark = string.Empty;
            if (assDraw != null)
            {
                drawingID = assDraw.AssemblyIdentifier;
                Assembly myAssembly = _model.SelectModelObject(drawingID) as Assembly;
                myAssembly.GetReportProperty("DRAWING.REVISION.MARK", ref revMark);
            }
            if (singDraw != null)
            {
                drawingID = singDraw.PartIdentifier;
                Tekla.Structures.Model.Part myPart = _model.SelectModelObject(drawingID) as Tekla.Structures.Model.Part;
                myPart.GetReportProperty("DRAWING.REVISION.MARK", ref revMark);
            }
            return revMark;
        }

        public void GetPrismDrawing(Drawing currentDrawing)
        {
            if (currentDrawing.Title1 == _notRequired)
            {
                currentDrawing.Delete();
                return;
            }
            string[] drawingTitle1 = currentDrawing.Title1.Split(new char[] { ' ' });
            DrawingFolderName = drawingTitle1[0];
            string[] mark = currentDrawing.Mark.Split(new char[] { '[', '.', ']' });
            string drawingName = "";
            foreach (string s in mark)
            {
                drawingName = drawingName + s;
            }
            if (_modelEnum.MyMarks != null)
            {
                if (_modelEnum.MyMarks.Contains(drawingName))
                {
                    RevMark = GetDrawingRevison(currentDrawing);
                    PdfName = ($"{drawingName}-{RevMark}.pdf");
                    TeklaDrawing = currentDrawing;
                }
                else
                {
                    IsDrawingRequired = false;
                }
            }
        }
    }
}
