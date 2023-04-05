using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Tekla.Structures.Drawing;
using Tekla.Structures.DrawingInternal;
using Tekla.Structures.Model;

//This class is temporarily not in use.
namespace Prism
{
    public class PrismDrawing
    {
        private string _notRequired = "Not Required";
        private Model _model;
        private SelectedObjects _selectedObjects;

        public PrismDrawing(Drawing currentDrawing, SelectedObjects selectedObjects, Model model, List<List<string>> dpmList)
        {
            _model = model;
            _selectedObjects = selectedObjects;
            DrawingRequired = true;
            GetPrismDrawing(currentDrawing);
            GetDPMFromDrawing(currentDrawing, model, dpmList);
        }

        public bool DrawingRequired { get; set; }
        public string RevMark { get; set; }
        public string PdfName { get; set; }
        public string DrawingFolderName { get; set; }
        public Drawing TeklaDrawing { get; set; }
        public string DpmPDFSaveName { get; set; }
        public string DpmFileName { get; set; }
        public string DpmPrinterSetting { get; set; }

        private void GetDPMFromDrawing(Drawing teklaDrawing, Model model, List<List<string>> dpmList)
        {
            string modelPath = model.GetInfo().ModelPath;

            string dpmName = GetDPMNameFromDrawing(dpmList, teklaDrawing.GetIdentifier().ToString());

            DpmPrinterSetting = modelPath + @"\attributes\" + "standard.PdfPrintOptions.xml";
            DpmPDFSaveName = $@"{DrawingFolderName}\" + $"{PdfName}";
            DpmFileName = modelPath + $@"\drawings\snapshots\{dpmName}.DPM";
        }

        private static string GetDPMNameFromDrawing(List<List<string>> dpmList, string drawingName)
        {
            List<string> nww = dpmList.Find(x => x[0] == drawingName);
            return nww[1];
        }

        private string GetDrawingRevision(Drawing currentDrawing)
        {
            if (currentDrawing.UpToDateStatus != DrawingUpToDateStatus.DrawingIsUpToDate)
            {
                return null;
            }
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

        private void GetPrismDrawing(Drawing currentDrawing)
        {
            if (currentDrawing.Title1 == _notRequired)
            {
                DrawingRequired = false;
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
            if (_selectedObjects.MyMarks != null)
            {
                if (_selectedObjects.MyMarks.Contains(drawingName))
                {
                    RevMark = GetDrawingRevision(currentDrawing);
                    PdfName = ($"{drawingName}-{RevMark}.pdf");
                    TeklaDrawing = currentDrawing;
                }
                else
                {
                    DrawingRequired = false;
                }
            }
        }
    }
}