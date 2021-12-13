using System;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Prism
{
    /// <summary>
    /// The Drawing Manager class takes care of all drawing related tasks.
    /// This includes updating status label with current drawing print status and Printing drawings
    /// </summary>
    public class DrawingManager
    {
        private SevModelEnumerator _modelEnum;
        private SevFolders _folders;
        private const string _notRequired = "Not Required";
        private Model _model;

        public DrawingManager(Model model, string phaseNum, string issueNum, SevModelEnumerator modelEnum)
        {
            _modelEnum = modelEnum;
            _folders = model.CreateSevFolders(phaseNum, issueNum);
            this._model = model;
        }

        public List<PrismDrawing> PrismDrawingList = new List<PrismDrawing>();    

        public void CreateDrawingList(ToolStripStatusLabel statusLabel)
        {
            int drawingCheckNo = 1;
            DrawingEnumerator drawingsList = _modelEnum.MyDrawingHandler.GetDrawings();
            while (drawingsList.MoveNext())
            {
                statusLabel.Text = $"Checking drawing {drawingCheckNo} of {drawingsList.GetSize()}";
                Console.WriteLine($"Checking drawing {drawingCheckNo} of {drawingsList.GetSize()}");
                Drawing currentDrawing = drawingsList.Current as Drawing;
                if (currentDrawing != null)
                {
                    PrismDrawing drawing = new PrismDrawing(currentDrawing, _modelEnum, _model);
                    if (drawing.IsDrawingRequired)
                    {
                        PrismDrawingList.Add(drawing);
                    }
                }
                drawingCheckNo++;
            }
        }

        public void PrintDrawings(ToolStripStatusLabel statusLabel)
        {
            int drawingProcessCounter = 1;
            foreach (PrismDrawing myDrawing in PrismDrawingList)
            {
                _modelEnum.MyDrawingHandler.IssueDrawing(myDrawing.TeklaDrawing);
                DPMPrinterAttributes myPDF = new DPMPrinterAttributes();
                myPDF.ColorMode = DotPrintColor.BlackAndWhite;
                myPDF.OpenFileWhenFinished = false;
                myPDF.Orientation = DotPrintOrientationType.Landscape;
                myPDF.OutputFileName = $"{_folders.fabPath}/{myDrawing.DrawingFolderName}/{myDrawing.PdfName}";
                myPDF.OutputType = DotPrintOutputType.PDF;
                myPDF.PaperSize = DotPrintPaperSize.Auto;
                statusLabel.Text = $"Printing drawing number {drawingProcessCounter} of {PrismDrawingList.Count}";
                _modelEnum.MyDrawingHandler.PrintDrawing(myDrawing.TeklaDrawing, myPDF);
                drawingProcessCounter++;
            }
        }
    }
}