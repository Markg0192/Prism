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
            string phaseNumber = "100";
            string issueNumber = "01";
            Model model = new Model();
            ReportManager myReportManager = new ReportManager(model, phaseNumber, issueNumber);
            DrawingManager myDrawingManager = new DrawingManager(model, phaseNumber, issueNumber);
            SevFolders myFolderManager = new SevFolders(model, phaseNumber, issueNumber);
            SevModelData modelData = model.SevModelData();
            SevModelEnumerator modelEnum = model.SevModelEnumerator();
            PreRunChecks myPreRunChecks = new PreRunChecks(model);
             
            if (model.GetConnectionStatus())
            {
                Console.WriteLine($"Hello {modelData.First}, your connection to {modelData.projNumber}-{modelData.projName} was successful");
            }
            else
            {
                Console.WriteLine("Model Connection Failure");
            }

            myPreRunChecks.CheckDrawingsAreUpToDate(modelEnum.drawingEnum);
            myFolderManager.CreateFolders();
            //myReportManager.CreateReports(modelEnum.selectedModelParts, modelEnum.selectedModelBolts);

            //myDrawingManager.PrintDrawings(modelEnum.drawingEnum);              
         
            Console.WriteLine("Press enter to close");
            Console.ReadLine();
        }
    }
}
