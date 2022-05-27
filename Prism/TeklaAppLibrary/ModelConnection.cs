using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tekla.Structures
{
    using TSModelOperation = Tekla.Structures.Model.Operations.Operation;
    using TSModelConnection = Tekla.Structures.Model.Model;
    using TSLocalization = Tekla.Structures.Dialog.Localization;

    internal class ModelConnection
    {
        /// <summary>
        /// Model connection object.
        /// </summary>
        private readonly TSModelConnection connection = new TSModelConnection();

        /// <summary>
        /// Environment path separators.
        /// </summary>
        private static readonly char[] PathSeparators = new[] { ';' };

        /// <summary>
        /// Localization object.
        /// </summary>
        private TSLocalization localization;

        /// <summary>
        /// Gets the macros folder. If there are several paths, the first will be returned.
        /// <seealso cref="MacrosFolders">The MacrosFolders returns enumerable collection of all macro paths.</seealso>
        /// </summary>
        /// <value>
        /// Macros folder path.
        /// </value>
        public string MacrosFolder
        {
            get
            {
                return this.MacrosFolders.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the macros folders.
        /// </summary>
        /// <value>
        /// Enumerable collection of paths.
        /// </value>
        public IEnumerable<string> MacrosFolders
        {
            get
            {
                var macroDirectory = this["XS_MACRO_DIRECTORY"];
                var fixedMacroDirectory = macroDirectory.Replace(@"\\", @"\");
                return fixedMacroDirectory.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the object is active.
        /// </summary>
        /// <value>
        /// Indicates whether the object is active.
        /// </value>
        public bool IsActive
        {
            get
            {
                return this.connection != null && this.connection.GetConnectionStatus();
            }
        }

        /// <summary>Gets an environment variable.</summary>
        /// <param name="variableName">Variable name.</param>
        /// <value>Environment variable value.</value>
        /// <returns>The System.String.</returns>
        public string this[string variableName]
        {
            get
            {
                var value = string.Empty;
                if (
                    SeparateThread.Execute<bool>(
                        delegate { return TeklaStructuresSettings.GetAdvancedOption(variableName, ref value); }))
                {
                    return value;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the application language.
        /// </summary>
        /// <value>
        /// Application language.
        /// </value>
        public string Language
        {
            get
            {
                return this["XS_LANGUAGE"];
            }
        }

        /// <summary>
        /// Gets the application localization source.
        /// </summary>
        /// <value>
        /// Application localization source.
        /// </value>
        public TSLocalization Localization
        {
            get
            {
                if (this.IsActive && this.localization == null)
                {
                    this.localization = new TSLocalization();

                    try
                    {
                        switch (this.Language)
                        {
                            case "ENGLISH":
                                this.localization.Language = "enu";
                                break;

                            case "DUTCH":
                                this.localization.Language = "nld";
                                break;

                            case "FRENCH":
                                this.localization.Language = "fra";
                                break;

                            case "GERMAN":
                                this.localization.Language = "deu";
                                break;

                            case "ITALIAN":
                                this.localization.Language = "ita";
                                break;

                            case "SPANISH":
                                this.localization.Language = "esp";
                                break;

                            case "JAPANESE":
                                this.localization.Language = "jpn";
                                break;

                            case "CHINESE SIMPLIFIED":
                                this.localization.Language = "chs";
                                break;

                            case "CHINESE TRADITIONAL":
                                this.localization.Language = "cht";
                                break;

                            case "CZECH":
                                this.localization.Language = "csy";
                                break;

                            case "PORTUGUESE BRAZILIAN":
                                this.localization.Language = "ptb";
                                break;

                            case "HUNGARIAN":
                                this.localization.Language = "hun";
                                break;

                            case "POLISH":
                                this.localization.Language = "plk";
                                break;

                            case "RUSSIAN":
                                this.localization.Language = "rus";
                                break;

                            default:
                                this.localization.Language = "enu";
                                break;
                        }

                        this.localization.LoadFile(TSLocalization.DefaultLocalizationFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }

                return this.localization;
            }
        }
    }
}
