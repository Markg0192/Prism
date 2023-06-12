using System;
using System.IO;
using Tekla.Structures;
using Tekla.Structures.Model.Operations;

namespace Prism
{
    public static class PrismMacroBuilder
    {
        public static void RefreshDrawings()
        {
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];
            if (!File.Exists(dir + $@"\modeling\{Constants.RefreshDrawingsMacro}"))
            {
                var writer = new StreamWriter(dir + $@"\modeling\{Constants.RefreshDrawingsMacro}");
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
        }

        public static bool DrawingOperations()
        {
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];
            if (!File.Exists(dir + $@"\modeling\{Constants.DrawingOperation}"))
            {
                var writer = new StreamWriter(dir + $@"\modeling\{Constants.DrawingOperation}");
                var macro =
                            "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                            "#pragma reference \"Tekla.Macros.Wpf.Runtime\"" + Environment.NewLine +
                            "#pragma reference \"Tekla.Macros.Runtime\"" + Environment.NewLine +
                            "#pragma warning restore 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                             "" + Environment.NewLine +
                            "namespace UserMacros" + Environment.NewLine +
                                "{" + Environment.NewLine +
                                    "public sealed class Macro" + Environment.NewLine +
                                   "{" + Environment.NewLine +
                                        "[Tekla.Macros.Runtime.MacroEntryPointAttribute()]" + Environment.NewLine +
                                       "public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)" + Environment.NewLine +
                                       "{" + Environment.NewLine +
                                           " Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();" + Environment.NewLine +
                                           " wpf.InvokeCommand(\"CommandRepository\", \"Drawing.DrawingList\");" + Environment.NewLine +
                                           " wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_CategoryList\").As.Selector.DoSelection.With(\"albl_All_documents\").Invoke();" + Environment.NewLine +
                                           " wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ShowAllDocuments\").As.Button.Invoke();" + Environment.NewLine +
                                           " wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonSelectDrawings\").As.Button.Invoke();" + Environment.NewLine +
                                        "}" + Environment.NewLine +
                                   "}" + Environment.NewLine +
                               "}";

                writer.Write(macro);
                writer.Close();
                return true;
            }
            return false;
        }

        public static void PrintSelectedASSDrawings(string fileToPrintTo, int drawingCount)
        {
            string printLocation = $".\\\\{fileToPrintTo}";
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];

            var writer = new StreamWriter(dir + $@"\modeling\{Constants.DrawingPrinterMacro}");
            var macro =
                        "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                        "#pragma reference \"Tekla.Macros.Wpf.Runtime\"" + Environment.NewLine +
                        "#pragma reference \"Tekla.Macros.Runtime\"" + Environment.NewLine +
                        "#pragma warning restore 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                        Environment.NewLine +
                        "namespace UserMacros" + Environment.NewLine +
                        "    {" + Environment.NewLine +
                        "        public sealed class Macro" + Environment.NewLine +
                        "        {" + Environment.NewLine +
                        "            [Tekla.Macros.Runtime.MacroEntryPointAttribute()]" + Environment.NewLine +
                        "            public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)" + Environment.NewLine +
                        "            {" + Environment.NewLine +
                        "                Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();" + Environment.NewLine +

                        "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ShowAllDocuments\").As.Button.Invoke();" + Environment.NewLine +
                        "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonSelectDrawings\").As.Button.Invoke();" + Environment.NewLine +
                        "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_CategoryList\").As.Selector.DoSelection.With(\"albl_Assembly_drawings\").Invoke();" + Environment.NewLine +
                       $"                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_DataGridControl\").As.DataGrid.NewSelection.WithRange(0, {drawingCount}).Invoke();" + Environment.NewLine +
                        "                wpf.InvokeCommand(\"CommandRepository\", \"Common.PrintDrawings\");" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_LoadSaveCombo\").As.Selector.Select(0);" + Environment.NewLine +

                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_FileLocationPanel\", \"AID_PDFPD_FileLocation\").As.TextBox.SetText(\"" + printLocation + "\\\\ASS\");" + Environment.NewLine +
                       
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_PrintTargetStackPanel\", \"AID_PDFPD_PDFRadio\").As.ToggleButton.State.SetChecked();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_OrientationPanel\", \"AID_PDFPD_Orientation\").As.Selector.Select(0);" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_CenterDrawingOnPaper\").As.ToggleButton.State.SetChecked();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_ColorPanel\", \"AID_PDFPD_Color\").As.Selector.Select(1);" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_OutputToSingleFile\").As.ToggleButton.State.SetUnchecked();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_OpenFileWhenFinished\").As.ToggleButton.State.SetUnchecked();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_OpenFolderWhenFinished\").As.ToggleButton.State.SetUnchecked();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_IncludeRevision\").As.ToggleButton.State.SetChecked();" + Environment.NewLine +

                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_PrintButton\").As.Button.Invoke();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").As.Window.Close();" + Environment.NewLine +

                        "            }" + Environment.NewLine +
                        "        }" + Environment.NewLine +
                        "    }";
            writer.Write(macro);
            writer.Close();

            Operation.RunMacro(Constants.DrawingPrinterMacro);

            while (Operation.IsMacroRunning()) // Wait until macro for selecting drawings in the document manager is complete before moving on
            {
                System.Threading.Tasks.Task.Delay(10);
            }
        }

        public static void IssueAndLockStampOn()
        {
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];

            var writer = new StreamWriter(dir + $@"\modeling\{Constants.IssueDrawings}");
            var macro =
                        "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                        "#pragma reference \"Tekla.Macros.Wpf.Runtime\"" + Environment.NewLine +
                        "#pragma reference \"Tekla.Macros.Runtime\"" + Environment.NewLine +
                        "#pragma warning restore 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                         Environment.NewLine +
                        "namespace UserMacros" + Environment.NewLine +
                        " {" + Environment.NewLine +
                        "     public sealed class Macro" + Environment.NewLine +
                        "     {" + Environment.NewLine +
                        "          [Tekla.Macros.Runtime.MacroEntryPointAttribute()]" + Environment.NewLine +
                        "         public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)" + Environment.NewLine +
                        "         {" + Environment.NewLine +
                        "              Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();" + Environment.NewLine +
                        "              wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonIssue\").As.Button.Invoke();" + Environment.NewLine +
                        "              wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonLockOn\").As.Button.Invoke();" + Environment.NewLine +
                        "          }" + Environment.NewLine +
                        "     }" + Environment.NewLine +
                        " }";

            writer.Write(macro);
            writer.Close();

            Operation.RunMacro(Constants.IssueDrawings);
        }

        public static void IssueAndLockStampOff()
        {
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];

            var writer = new StreamWriter(dir + $@"\modeling\{Constants.IssueDrawings}");
            var macro =
                        "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                        "#pragma reference \"Tekla.Macros.Wpf.Runtime\"" + Environment.NewLine +
                        "#pragma reference \"Tekla.Macros.Runtime\"" + Environment.NewLine +
                        "#pragma warning restore 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                         Environment.NewLine +
                        "namespace UserMacros" + Environment.NewLine +
                        " {" + Environment.NewLine +
                        "     public sealed class Macro" + Environment.NewLine +
                        "     {" + Environment.NewLine +
                        "          [Tekla.Macros.Runtime.MacroEntryPointAttribute()]" + Environment.NewLine +
                        "         public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)" + Environment.NewLine +
                        "         {" + Environment.NewLine +
                        "              Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();" + Environment.NewLine +
                        "              wpf.InvokeCommand(\"CommandRepository\", \"Drawing.DrawingList\");" + Environment.NewLine +
                        "              wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_CategoryList\").As.Selector.DoSelection.With(\"albl_All_documents\").Invoke();" + Environment.NewLine +
                        "              wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ShowAllDocuments\").As.Button.Invoke();" + Environment.NewLine +
                        "              wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonSelectDrawings\").As.Button.Invoke();" + Environment.NewLine +
                        "              wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonLockOff\").As.Button.Invoke();" + Environment.NewLine +
                        "              wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonUnissue\").As.Button.Invoke();" + Environment.NewLine +           
                        "          }" + Environment.NewLine +
                        "     }" + Environment.NewLine +
                        " }";

            writer.Write(macro);
            writer.Close();

            Operation.RunMacro(Constants.IssueDrawings);
        }

        public static void PrintSelectedDrawings(string fileToPrintTo, string folderName, int startPoint, int drawingCount)
        {
            string printLocation = $".\\\\{fileToPrintTo}";
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];

            var writer = new StreamWriter(dir + $@"\modeling\{Constants.DrawingPrinterMacro}");
            var macro =
                        "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                        "#pragma reference \"Tekla.Macros.Wpf.Runtime\"" + Environment.NewLine +
                        "#pragma reference \"Tekla.Macros.Runtime\"" + Environment.NewLine +
                        "#pragma warning restore 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                        Environment.NewLine +
                        "namespace UserMacros" + Environment.NewLine +
                        "    {" + Environment.NewLine +
                        "        public sealed class Macro" + Environment.NewLine +
                        "        {" + Environment.NewLine +
                        "            [Tekla.Macros.Runtime.MacroEntryPointAttribute()]" + Environment.NewLine +
                        "            public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)" + Environment.NewLine +
                        "            {" + Environment.NewLine +
                        "                Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();" + Environment.NewLine +


                        "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_CategoryList\").As.Selector.DoSelection.With(\"albl_single_part_drawings\").Invoke();" + Environment.NewLine +

                        "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_DataGridControl\", \"AID_DocMgr_Mark\").As.Button.Invoke();" + Environment.NewLine +
                        "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_DataGridControl\", \"AID_DocMgr_Title1\").As.Button.Invoke();" + Environment.NewLine +
                       $"                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_DataGridControl\").As.DataGrid.NewSelection.WithRange({startPoint}, {drawingCount}).Invoke();" + Environment.NewLine +

                        "                wpf.InvokeCommand(\"CommandRepository\", \"Common.PrintDrawings\");" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_LoadSaveCombo\").As.Selector.Select(0);" + Environment.NewLine +

                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_FileLocationPanel\", \"AID_PDFPD_FileLocation\").As.TextBox.SetText(\"" + printLocation + "\\" + folderName + "\");" + Environment.NewLine +

                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_PrintTargetStackPanel\", \"AID_PDFPD_PDFRadio\").As.ToggleButton.State.SetChecked();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_OrientationPanel\", \"AID_PDFPD_Orientation\").As.Selector.Select(0);" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_CenterDrawingOnPaper\").As.ToggleButton.State.SetChecked();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_ColorPanel\", \"AID_PDFPD_Color\").As.Selector.Select(1);" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_OutputToSingleFile\").As.ToggleButton.State.SetUnchecked();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_OpenFileWhenFinished\").As.ToggleButton.State.SetUnchecked();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_OpenFolderWhenFinished\").As.ToggleButton.State.SetUnchecked();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_IncludeRevision\").As.ToggleButton.State.SetChecked();" + Environment.NewLine +

                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_PrintButton\").As.Button.Invoke();" + Environment.NewLine +
                        "                wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").As.Window.Close();" + Environment.NewLine +
                        "            }" + Environment.NewLine +
                        "        }" + Environment.NewLine +
                        "    }";
            writer.Write(macro);
            writer.Close();

            Operation.RunMacro(Constants.DrawingPrinterMacro);

            while (Operation.IsMacroRunning()) // Wait until macro for selecting drawings in the document manager is complete before moving on
            {
                System.Threading.Tasks.Task.Delay(10);
            }
        }
    }
}