using System;
using System.IO;
using Prism;
using Tekla.Structures.Model;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;

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
            SevFolders myFolderManager;
            SevModelData modelData;
            SevModelEnumerator modelEnum;
            PreRunChecks myPreRunChecks;
            ModelModifiers myModelModifiers;
            BswxExporter myBswxExporter = new BswxExporter();
            const string MailNewLine = "%0D%0A";

            model = new Model();
            myPreRunChecks = new PreRunChecks(model);

            modelData = model.CreateSevModelData();
            myModelModifiers = new ModelModifiers(model);

            watch.Stop();
            Console.WriteLine($"Stage 1.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            string phaseNumber = "100";
            string issueNumber = "01";
            string testPackageLocation = "SNI";

            modelEnum = model.CreateSevModelEnumerator();
            myFolderManager = new SevFolders(model, phaseNumber, issueNumber);
            myBswxExporter.ExportBSWX(modelEnum, myFolderManager, modelData, phaseNumber, issueNumber);

            watch.Stop();
            Console.WriteLine($"Stage 1.1 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            myReportManager = new ReportManager(model, phaseNumber, issueNumber);

            watch.Stop();
            Console.WriteLine($"Stage 1.2 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            myDrawingManager = new DrawingManager(model, phaseNumber, issueNumber, modelEnum);

            watch.Stop();
            Console.WriteLine($"Stage 1.3 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            bool packageSNI = true;
            bool packageSUK = false;

            watch.Stop();
            Console.WriteLine($"Stage 2.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            ToolStripStatusLabel DummyStrip = new ToolStripStatusLabel();
            myDrawingManager.CreateDrawingList();

            watch.Stop();
            Console.WriteLine($"Stage 3.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            if (packageSNI)
            {
                if (!myPreRunChecks.CheckDrawingsAreUpToDate(myDrawingManager.PrismDrawingList))
                {
                    return;
                }
                if (!myPreRunChecks.CheckNumberingIsUpToDate(modelEnum.SelectedModelParts))
                {
                    return;
                }

                watch.Stop();
                Console.WriteLine($"Stage 4.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
                watch.Restart();

                myFolderManager.CreateFolders();
                myReportManager.CreateReports(modelEnum.SelectedModelParts, modelEnum.SelectedModelBolts, testPackageLocation);

                watch.Stop();
                Console.WriteLine($"Stage 5.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
                watch.Restart();

                DummyPrintDrawings(modelEnum.MyDrawingHandler, myFolderManager, myDrawingManager, modelEnum);

                watch.Stop();
                Console.WriteLine($"Stage 6.0 complete, runtime: {watch.ElapsedMilliseconds} ms");
                watch.Restart();

                myModelModifiers.MarkAsFabPackComplete(modelEnum);
                myFolderManager.RemoveUnusedFolders();
                myModelModifiers.LockSelected(modelEnum);

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
                                                                                     $"{modelEnum.AssembliesList.Count} Assemblies.{MailNewLine}" +
                                                                                     $"{modelEnum.SelectedModelParts.Count} Parts.{MailNewLine}{MailNewLine}" +
                                                                                     $"Regards,{MailNewLine}{MailNewLine}" +
                                                                                     $"{modelData.Full}");

            DialogResult finishBox = MessageBox.Show($"Thanks {modelData.First}, your fab package is now complete", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (finishBox == DialogResult.OK)
            {
                return;
            }
        }

        public static void DummyPrintDrawings(DrawingHandler myDrawingHandler, SevFolders myFolderManager, DrawingManager myDrawingManager, SevModelEnumerator modelEnum)
        {
            Console.WriteLine($"You have {myDrawingManager.PrismDrawingList.Count} drawings to print");
            int printNumber = 1;
            foreach (PrismDrawing myDrawing in myDrawingManager.PrismDrawingList)
            {
                Console.WriteLine($"Printing drawing {printNumber} of {myDrawingManager.PrismDrawingList.Count}");
                modelEnum.MyDrawingHandler.IssueDrawing(myDrawing.TeklaDrawing);
                DPMPrinterAttributes myPDF = new DPMPrinterAttributes();
                myPDF.ColorMode = DotPrintColor.BlackAndWhite;
                myPDF.OpenFileWhenFinished = false;
                myPDF.Orientation = DotPrintOrientationType.Landscape;
                myPDF.OutputFileName = $"{myFolderManager.fabPath}/{myDrawing.DrawingFolderName}/{myDrawing.PdfName}";
                myPDF.OutputType = DotPrintOutputType.PDF;
                myPDF.PaperSize = DotPrintPaperSize.Auto;
             //   modelEnum.MyDrawingHandler.PrintDrawing(myDrawing.TeklaDrawing, myPDF);
                printNumber++;
            }
        }

        public static void FormIssueEmail(string emailAddress, string subject, string body)
        {
            Process.Start("mailto:" + emailAddress + "?subject=" + subject + "&body=" + body + "&Attachment=" + @"C:\Users\mark.gibson\Desktop\Pdf\Book1.pdf");
        }
    }
}