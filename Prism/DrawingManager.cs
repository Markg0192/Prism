using System;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using Tekla.Structures.DrawingInternal;
using Tekla.Structures;

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
            var drawingsBySelectedParts = new List<Drawing>();
            var drawingNos = Operation.GetDrawingsBySelectedParts();
            if (drawingsBySelectedParts.Count == 0)
            {
                RefreshDrawings();
                drawingNos = Operation.GetDrawingsBySelectedParts();
            }
            foreach (var no in drawingNos)
            {
                var id = new Identifier(no);
                var drawing = Operation.GetDrawing(id);
                drawing.Select();
                drawingsBySelectedParts.Add(drawing);

            }

            foreach (Drawing drawing in drawingsBySelectedParts)
            {
                if (drawing != null)
                {
                    PrismDrawing prismDrawing = new PrismDrawing(drawing, _modelEnum, _model);
                    if (prismDrawing.IsDrawingRequired)
                    {
                        PrismDrawingList.Add(prismDrawing);
                    }
                }
                drawingCheckNo++;
            }
        }

        public static bool RefreshDrawings()
        {
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];
            if (!File.Exists(dir + @"\modeling\OpenAndCloseDocumentManager.cs"))
            {
                var writer = new StreamWriter(dir + @"\modeling\OpenAndCloseDocumentManager.cs");
                var macro = "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                "#pragma reference \"Tekla.Macros.Wpf.Runtime\"" + Environment.NewLine +
                "#pragma reference \"Tekla.Macros.Runtime\"" + Environment.NewLine +
                "#pragma warning restore 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                "namespace UserMacros {" + Environment.NewLine +
                "public sealed class Macro {" + Environment.NewLine +
                "[Tekla.Macros.Runtime.MacroEntryPointAttribute()]" + Environment.NewLine +
                "public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime) {" +
                Environment.NewLine +
                "Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();" +
                Environment.NewLine +
                "wpf.InvokeCommand(\"CommandRepository\", \"Drawing.DrawingList\");" + Environment.NewLine +
                "wpf.View(\"DocumentManager.MainWindow\").As.Window.Close();}}}";
                writer.Write(macro);
                writer.Close();
            }
            return Tekla.Structures.Model.Operations.Operation.RunMacro("OpenAndCloseDocumentManager.cs");
        }

        public void PrintDrawings(ToolStripStatusLabel statusLabel)
        {
            int drawingProcessCounter = 1;
            foreach (PrismDrawing myDrawing in PrismDrawingList)
            {
                statusLabel.Text = "Starting to print";
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