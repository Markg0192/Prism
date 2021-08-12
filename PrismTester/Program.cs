using System;
using System.IO;
using Prism;
using Tekla.Structures.Model;
using System.Windows.Forms;
using Tekla.Structures.Drawing;

namespace PrismTester
{
    class Program
    {
        

        public static void Main(string[] args)
        {
             Model model;
             ReportManager myReportManager;
             DrawingManager myDrawingManager;
             SevFolders myFolderManager;
             SevModelData modelData;
             SevModelEnumerator modelEnum;
             PreRunChecks myPreRunChecks;
             ModelModifiers myModelModifiers;



            model = new Model();
            myPreRunChecks = new PreRunChecks(model);
            modelData = model.SevModelData();
            myModelModifiers = new ModelModifiers(model);

            string phaseNumber = "100";
            string issueNumber = "01";
            string testPackageLocation = "SNI";

            modelEnum = model.SevModelEnumerator();
            myReportManager = new ReportManager(model, phaseNumber, issueNumber);
            myDrawingManager = new DrawingManager(model, phaseNumber, issueNumber);
            myFolderManager = new SevFolders(model, phaseNumber, issueNumber);
            bool packageSNI = true;
            bool packageSUK = false;

            if (packageSNI)
            {
                if (!myPreRunChecks.CheckDrawingsAreUpToDate(modelEnum.DrawingEnum))
                {
                    //this.Close();
                    return;
                }
                myFolderManager.CreateFolders();
                myReportManager.CreateReports(modelEnum.SelectedModelParts, modelEnum.SelectedModelBolts, testPackageLocation);
                //myDrawingManager.DummyPrintDrawings(modelEnum.MyDrawingHandler);
                myModelModifiers.MarkAsFabPackComplete(modelEnum);
                myFolderManager.RemoveUnusedFolders();
                myModelModifiers.LockSelected(modelEnum);
            }
            if (packageSUK)
            {
                MessageBox.Show("Sorry, this function has not been added yet, please try again later.");
            }
            DialogResult finishBox = MessageBox.Show($"Thanks {modelData.First}, your fab package is now complete", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (finishBox == DialogResult.OK)
            {
                //this.Close();
                return;
            }

           /* public void DummyPrintDrawings(DrawingHandler myDrawingHandler)
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
                                ModelEnum.MyDrawingHandler.PrintDrawing(currentDrawing, myPDF);
                            }
                        }
                    }
                }
            }*/
        }
    }
}
