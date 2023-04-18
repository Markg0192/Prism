using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures;

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
    }
}
