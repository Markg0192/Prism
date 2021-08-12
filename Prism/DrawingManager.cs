using System;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using System.Windows.Forms;

namespace Prism
{
    /// <summary>
    /// The Drawing Manager class takes care of all drawing related tasks.
    /// This includes updating status label with current drawing print status and Printing drawings
    /// </summary>
    public class DrawingManager
    {
        private SevModelEnumerator ModelEnum;
        private SevFolders Folders;
        private const string NotRequired = "Not Required";
        private Model model;

        public DrawingManager(Model model, string phaseNum, string issueNum)
        {
            ModelEnum = model.CreateSevModelEnumerator();
            Folders = model.CreateSevFolders(phaseNum, issueNum);
            this.model = model;
        }

        private void UpdateStatusLabel(ToolStripStatusLabel statusLabel, int currentNumber, int totalNumber)
        {
            statusLabel.Text = $"Processing Drawing {currentNumber} of {totalNumber}";
        }

        private string GetDrawingRevison(Drawing currentDrawing)
        {
            AssemblyDrawing assDraw = currentDrawing as AssemblyDrawing;
            SinglePartDrawing singDraw = currentDrawing as SinglePartDrawing;
            Tekla.Structures.Identifier drawingID = null;
            string revMark = string.Empty;
            if (assDraw != null)
            {
                drawingID = assDraw.AssemblyIdentifier;
                Assembly myAssembly = model.SelectModelObject(drawingID) as Assembly;
                myAssembly.GetReportProperty("DRAWING.REVISION.MARK", ref revMark);
            }
            if (singDraw != null)
            {
                drawingID = singDraw.PartIdentifier;
                Tekla.Structures.Model.Part myPart = model.SelectModelObject(drawingID) as Tekla.Structures.Model.Part;
                myPart.GetReportProperty("DRAWING.REVISION.MARK", ref revMark);
            }
            return revMark;
        }

        public void PrintDrawings(DrawingHandler myDrawingHandler, ToolStripStatusLabel statusLabel)
        {            
            int drawingProcessCounter = 0;
            DrawingEnumerator drawingsList = myDrawingHandler.GetDrawings();
            while (drawingsList.MoveNext())
            {
                Drawing currentDrawing = drawingsList.Current as Drawing;
                if (currentDrawing != null)
                {
                    if (currentDrawing.Title1 == NotRequired)
                    {
                        currentDrawing.Delete();
                    }
                    string[] Mark = currentDrawing.Mark.Split(new char[] { '[', '.', ']' });
                    string drawingName = "";
                    foreach (string s in Mark)
                    {
                        drawingName = drawingName + s;
                    }
                    if (ModelEnum.MyMarks != null)
                    {
                        if (ModelEnum.MyMarks.Contains(drawingName))
                        {                            
                            string revMark = GetDrawingRevison(currentDrawing);
                            drawingProcessCounter++;
                            ModelEnum.MyDrawingHandler.IssueDrawing(currentDrawing);
                            string PDFname = ($"{drawingName}-{revMark}.pdf");
                            DPMPrinterAttributes myPDF = new DPMPrinterAttributes();
                            myPDF.ColorMode = DotPrintColor.BlackAndWhite;
                            myPDF.OpenFileWhenFinished = false;
                            myPDF.Orientation = DotPrintOrientationType.Landscape;
                            myPDF.OutputFileName = $"{Folders.fabPath}/{currentDrawing.Title1}/{PDFname}";
                            myPDF.OutputType = DotPrintOutputType.PDF;
                            myPDF.PaperSize = DotPrintPaperSize.Auto;
                            UpdateStatusLabel(statusLabel, drawingProcessCounter, ModelEnum.MyMarks.Count);
                            ModelEnum.MyDrawingHandler.PrintDrawing(currentDrawing, myPDF);
                        }
                    }
                }
            }
        }
    }
}
