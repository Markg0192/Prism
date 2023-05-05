using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static void SelectDrawings(int startAfter, int numberToSelect)
        {
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];
            //if (!File.Exists(dir + $@"\modeling\{Constants.DrawingOperation}"))
            {
                var writer = new StreamWriter(dir + $@"\modeling\PrismDrawingSelect.cs");
                var macro =
                            "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                            "#pragma reference \"Tekla.Macros.Wpf.Runtime\"" + Environment.NewLine +
                            "#pragma reference \"Tekla.Macros.Runtime\"" + Environment.NewLine +
                            "#pragma warning restore 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                            Environment.NewLine +
                            "namespace UserMacros" + Environment.NewLine +
                                "{" + Environment.NewLine +
                                    "public sealed class Macro" + Environment.NewLine +
                                   "{" + Environment.NewLine +
                                        "[Tekla.Macros.Runtime.MacroEntryPointAttribute()]" + Environment.NewLine +
                                        "public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime) {" + Environment.NewLine +
                                          Environment.NewLine +
                                            "Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();" + Environment.NewLine +
                                            $"wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_DataGridControl\").As.DataGrid.NewSelection.WithRange({startAfter}, {numberToSelect}).Invoke();" + Environment.NewLine +
                                        "}" + Environment.NewLine +
                                    "}" + Environment.NewLine +
                                "}";

                writer.Write(macro);
                writer.Close();
            }
        }

        public static void PrintSelectedDrawings(string fileToPrintTo)
        {
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];
            //if (!File.Exists(dir + $@"\modeling\{Constants.DrawingOperation}"))
            {
                var writer = new StreamWriter(dir + $@"\modeling\PrismDrawingPrint.cs");
                var macro =
                            "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                            "#pragma reference \"Tekla.Macros.Wpf.Runtime\"" + Environment.NewLine +
                            "#pragma reference \"Tekla.Macros.Runtime\"" + Environment.NewLine +
                            "#pragma warning restore 1633 // Unrecognized #pragma directive" + Environment.NewLine +

                            "namespace UserMacros" + Environment.NewLine +
                                "{" + Environment.NewLine +
                                    "public sealed class Macro" + Environment.NewLine +
                                    "{" + Environment.NewLine +
                                        "[Tekla.Macros.Runtime.MacroEntryPointAttribute()]" + Environment.NewLine +
                                       " public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)" + Environment.NewLine +
                                       " {" + Environment.NewLine +
                                            "Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();" + Environment.NewLine +
                                            "wpf.InvokeCommand(\"CommandRepository\", \"Common.PrintDrawings\");" + Environment.NewLine +
                                            "wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_LoadSaveCombo\").As.Selector.Select(0);" + Environment.NewLine +
                                            "wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_SettingsTabControl\", \"AID_PDFPD_ParentStackPanel\", \"AID_PDFPD_FileLocationPanel\", \"AID_PDFPD_FileLocation\").As.TextBox.SetText" + $"(\".{fileToPrintTo}\");" + Environment.NewLine +
                                            "wpf.View(\"DPMPrinterFeature.DPMPrinterViewWindow\").Find(\"AID_PDFPD_PrintButton\").As.Button.Invoke();" + Environment.NewLine +
                                        "}" + Environment.NewLine +
                                    "}" + Environment.NewLine +
                                "}";

                writer.Write(macro);
                writer.Close();
            }
        }

        public static void IssueDrawings()
        {
            var macrodir = "";
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref macrodir);
            var dir = macrodir.Split(';')[0];
            if (!File.Exists(dir + $@"\modeling\{Constants.IssueDrawings}"))
            {
                var writer = new StreamWriter(dir + $@"\modeling\{Constants.IssueDrawings}");
                var macro =
                            "#pragma warning disable 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                            "#pragma reference \"Tekla.Macros.Wpf.Runtime\"" + Environment.NewLine +
                            "#pragma reference \"Tekla.Macros.Runtime\"" + Environment.NewLine +
                            "#pragma warning restore 1633 // Unrecognized #pragma directive" + Environment.NewLine +
                             Environment.NewLine +
                            "namespace UserMacros" + Environment.NewLine +
                            "    {" + Environment.NewLine +
                            "        public sealed class Macro" + Environment.NewLine +
                            "       {" + Environment.NewLine +
                            "            [Tekla.Macros.Runtime.MacroEntryPointAttribute()]" + Environment.NewLine +
                            "           public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)" + Environment.NewLine +
                            "           {" + Environment.NewLine +
                            "                Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();" + Environment.NewLine +
                            "                wpf.InvokeCommand(\"CommandRepository\", \"Drawing.DrawingList\");" + Environment.NewLine +
                            "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_CategoryList\").As.Selector.DoSelection.With(\"albl_All_documents\").Invoke();" + Environment.NewLine +
                            "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ShowAllDocuments\").As.Button.Invoke();" + Environment.NewLine +
                            "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonSelectDrawings\").As.Button.Invoke();" + Environment.NewLine +
                            "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonUnissue\").As.Button.Invoke();" + Environment.NewLine +
                            "                wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_ButtonIssue\").As.Button.Invoke();" + Environment.NewLine +
                            "            }" + Environment.NewLine +
                            "       }" + Environment.NewLine +
                            "   }";

                writer.Write(macro);
                writer.Close();
            }

            Operation.RunMacro(Constants.IssueDrawings);

            while (Operation.IsMacroRunning()) // Wait until macro for selecting drawings in the document manager is complete before moving on
            {
                System.Threading.Tasks.Task.Delay(10);
            }
        }
    }
}
