using System;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using System.Windows.Forms;

namespace Prism
{
    public class DrawingManager
    {
        public SevModelEnumerator ModelEnum;
        public SevFolders Folders;       
        public const string notRequired = "Not Required";
        public string[] mark;
        public DrawingManager(Model model, string phaseNum, string issueNum)
        {
            ModelEnum = model.SevModelEnumerator();
            Folders = model.SevFolders(phaseNum, issueNum);
        }        
        public void UpdateStatusLabel (ToolStripStatusLabel StatusLabel,  int currentNumber, int TotalNumber)
        {        
          StatusLabel.Text = $"Processing Drawing {currentNumber} of {TotalNumber}";           
        }         
        public void PrintDrawings(DrawingEnumerator drawingsList, ToolStripStatusLabel StatusLabel)
        {
            int weldNumberCounter = 0;
            string revMark = string.Empty;
            foreach (var item in ModelEnum.moe)
            {
                var p = item as Tekla.Structures.Model.Part;
                if (p != null)
                {                    
                    p.GetReportProperty("DRAWING.REVISON.MARK", ref revMark);
                }
            }
            foreach (Drawing currentDrawing in drawingsList)
            {          
                if (currentDrawing != null)
                {
                    if (currentDrawing.Title1 == notRequired)
                    {
                        currentDrawing.Delete();
                    }
                    mark = currentDrawing.Mark.Split(new char[] { '[', '.', ']' });
                    string drawingName = "";
                    foreach (string s in mark) drawingName = drawingName + s;                   
                    int totalDrawings = ModelEnum.myMarks.Count;

                    if (ModelEnum.myMarks != null)
                    {
                        if (ModelEnum.myMarks.Contains(drawingName))
                        {     
                            weldNumberCounter++;       
                            ModelEnum.myDrawingHandler.IssueDrawing(currentDrawing);
                            string PDFname = ($"{drawingName}-{revMark}.pdf");
                            DPMPrinterAttributes MyPDF = new DPMPrinterAttributes();
                            MyPDF.ColorMode = DotPrintColor.BlackAndWhite;
                            MyPDF.OpenFileWhenFinished = false;
                            MyPDF.Orientation = DotPrintOrientationType.Landscape;
                            MyPDF.OutputFileName = $"{Folders.FabPath}/{currentDrawing.Title1}/{PDFname}";
                            MyPDF.OutputType = DotPrintOutputType.PDF;
                            MyPDF.PaperSize = DotPrintPaperSize.Auto;
                            UpdateStatusLabel(StatusLabel, weldNumberCounter, ModelEnum.myMarks.Count);
                            ModelEnum.myDrawingHandler.PrintDrawing(currentDrawing, MyPDF);                         
                        }                                            
                    }
                }
            }
        }           
        public void DummyPrintDrawings(DrawingEnumerator drawingsList)
        {
            int weldNumberCounter = 0;
            string revMark = string.Empty;
            foreach (var item in ModelEnum.moe)
            {
                var p = item as Tekla.Structures.Model.Part;
                if (p != null)
                {
                   p.GetReportProperty("DRAWING.REVISON.MARK", ref revMark);
                }
            }
            foreach (Drawing currentDrawing in drawingsList)
            {          
                if (currentDrawing != null)
                {
                    if (currentDrawing.Title1 == notRequired)
                    {
                        currentDrawing.Delete();
                    }
                    mark = currentDrawing.Mark.Split(new char[] { '[', '.', ']' });
                    string drawingName = "";
                    foreach (string s in mark) drawingName = drawingName + s;                   
                    int totalDrawings = ModelEnum.myMarks.Count;

                    if (ModelEnum.myMarks != null)
                    {
                        if (ModelEnum.myMarks.Contains(drawingName))
                        {     
                            weldNumberCounter++;       
                            ModelEnum.myDrawingHandler.IssueDrawing(currentDrawing);
                            string PDFname = ($"{drawingName}-{revMark}.pdf");
                            DPMPrinterAttributes MyPDF = new DPMPrinterAttributes();
                            MyPDF.ColorMode = DotPrintColor.BlackAndWhite;
                            MyPDF.OpenFileWhenFinished = false;
                            MyPDF.Orientation = DotPrintOrientationType.Landscape;
                            MyPDF.OutputFileName = $"{Folders.FabPath}/{currentDrawing.Title1}/{PDFname}";
                            MyPDF.OutputType = DotPrintOutputType.PDF;
                            MyPDF.PaperSize = DotPrintPaperSize.Auto;
                            Console.WriteLine(PDFname);
                            ModelEnum.myDrawingHandler.PrintDrawing(currentDrawing, MyPDF);                         
                        }                                            
                    }
                }
            }
        }
    }
}
