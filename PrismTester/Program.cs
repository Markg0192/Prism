using System;
using System.IO;
using Prism;
using Tekla.Structures.Model;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using static Prism.Enums;

namespace PrismTester
{
    class Program
    {
        public static void Main(string[] args)
        {

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            Model model;
            ReportManager myReportManager;
            DrawingManager myDrawingManager;
            FolderManager myFolderManager;
            PrismProjectData modelData;
            SelectedObjects selectedObjects;


       
            const string MailNewLine = "%0D%0A";

            model = new Model();


       //    modelData = new PrismProjectData(model.GetProjectInfo());


            watch.Stop();
            Console.WriteLine($"Stage 1.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            string phaseNumber = "10000";
            string issueNumber = "01";
            string testPackageLocation = "SNI";



            selectedObjects = new SelectedObjects(stageTypes.Unassigned);


       //     selectedObjects.ModifyAttributes(3, modelData);

            selectedObjects.GetCorrectModelSelection();


            selectedObjects.AddPrelimMarks(model.GetProjectInfo());
            // myModelModifiers.MoveAndRenameOmittedMembers(selectedObjects, model);


            /* myPreRunChecks.RunStage4Checks(selectedObjects);


             selectedObjects.CheckExecutionField(selectedObjects);
             myPreRunChecks.CheckNameAndClassAlignment(selectedObjects);*/

            if (!selectedObjects.NumbersNotUpToDate)
            {
                return;
            }
         //   myFolderManager = new FolderManager(model.GetProjectInfo(), model.GetInfo().ModelPath, phaseNumber, issueNumber);
            //myBswxExporter.ExportBSWX(selectedObjects, myFolderManager.DspPath, modelData, phaseNumber, issueNumber, "");

            watch.Stop();
            Console.WriteLine($"Stage 1.1 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();



            watch.Stop();
            Console.WriteLine($"Stage 1.2 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

          //  myDrawingManager = new DrawingManager(model, phaseNumber, issueNumber, selectedObjects);

            watch.Stop();
            Console.WriteLine($"Stage 1.3 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            bool packageSNI = true;
            bool packageSUK = false;

            watch.Stop();
            Console.WriteLine($"Stage 2.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            ToolStripStatusLabel DummyStrip = new ToolStripStatusLabel();
           // bool allDrawingsUpToDate = myDrawingManager.CreateDrawingList();
          //  if(!allDrawingsUpToDate)
            {
                const string notUpToDateMessage = "Some drawings are not up to date, please update and try again";
                const string notUpToDateTitle = "Drawings not up to date";
                MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            watch.Stop();
            Console.WriteLine($"Stage 3.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            if (packageSNI)
            {
                /*if (!myPreRunChecks.CheckDrawingsAreUpToDate(myDrawingManager.PrismDrawingList))
                {
                    return;
                }
                if (!myPreRunChecks.CheckNumberingIsUpToDate(selectedObjects.SelectedModelParts))
                {
                    return;
                }*/

                watch.Stop();
                Console.WriteLine($"Stage 4.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
                watch.Restart();

                myFolderManager.CreateFabFolders();
                myReportManager.CreateFabReports(selectedObjects.SelectedModelParts, selectedObjects.AllBolts, testPackageLocation);

                watch.Stop();
                Console.WriteLine($"Stage 5.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
                watch.Restart();

                //DummyPrintDrawings(selectedObjects.MyDrawingHandler, myFolderManager, myDrawingManager, selectedObjects);

                watch.Stop();
                Console.WriteLine($"Stage 6.0 complete, runtime: {watch.ElapsedMilliseconds} ms");


                watch.Stop();
                Console.WriteLine($"Stage 7.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
                watch.Restart();

            }
            if (packageSUK)
            {
                MessageBox.Show("Sorry, this function has not been added yet, please try again later.");
            }

            Console.WriteLine("This is the end of the program, press enter to close");
            Console.ReadLine();

            FormIssueEmail("Test@email.com", "Test Subject", $"Hello,{MailNewLine}{MailNewLine}" +
                                                                                     $"This is the fab package Issue {issueNumber} for phase {phaseNumber} in {modelData.ProjNumber}, {modelData.ProjName}.{MailNewLine}" +
                                                                                     $"Please issue this package to the works when possible.{MailNewLine}{MailNewLine}" +
                                                                                     $"This fab package contains the following;{MailNewLine}" +
                                                                                     $"{selectedObjects.AssembliesList.Count} Assemblies.{MailNewLine}" +
                                                                                     $"{selectedObjects.SelectedModelParts.Count} Parts.{MailNewLine}{MailNewLine}" +
                                                                                     $"Regards,{MailNewLine}{MailNewLine}" +
                                                                                     $"{modelData.Full}");

            DialogResult finishBox = MessageBox.Show($"Thanks {modelData.First}, your fab package is now complete", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (finishBox == DialogResult.OK)
            {
                return;
            }
        }

        public static void DummyPrintDrawings(DrawingHandler myDrawingHandler, FolderManager myFolderManager, DrawingManager myDrawingManager, SelectedObjects selectedObjects)
        {
            Console.WriteLine($"You have {myDrawingManager.PrismDrawingList.Count} drawings to print");
            int printNumber = 1;

            DPMPrinterAttributes myPDF = new DPMPrinterAttributes();
            myPDF.ColorMode = DotPrintColor.BlackAndWhite;
            myPDF.OpenFileWhenFinished = false;
            myPDF.Orientation = DotPrintOrientationType.Landscape;
            myPDF.OutputType = DotPrintOutputType.PDF;
            myPDF.PaperSize = DotPrintPaperSize.Auto;

            List<Drawing> myDrawings = new List<Drawing>();

            foreach (PrismDrawing myDrawing in myDrawingManager.PrismDrawingList)
            {
                if(!myDrawing.DrawingRequired)
                {
                    continue;
                }
                myDrawings.Add(myDrawing.TeklaDrawing);
                Console.WriteLine($"Printing drawing {printNumber} of {myDrawingManager.PrismDrawingList.Count}");
                //   selectedObjects.MyDrawingHandler.IssueDrawing(myDrawing.TeklaDrawing);
                myPDF.OutputFileName = $"{myFolderManager.FabPath}/{myDrawing.DrawingFolderName}/{myDrawing.PdfName}";
                //selectedObjects.MyDrawingHandler.PrintDrawing(myDrawing.TeklaDrawing, myPDF);
                printNumber++;
            }
            // selectedObjects.MyDrawingHandler.PrintDrawings(myDrawings, myPDF);               

        }

        public static void FormIssueEmail(string emailAddress, string subject, string body)
        {
            Process.Start("mailto:" + emailAddress + "?subject=" + subject + "&body=" + body + "&Attachment=" + @"C:\Users\mark.gibson\Desktop\Pdf\Book1.pdf");
        }
    }
}