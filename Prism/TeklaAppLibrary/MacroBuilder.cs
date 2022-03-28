#define DEBUG
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tekla.Structures
{
	public sealed class MacroBuilder
	{
		private const string FileNameFormat = "macro_{0:00}.cs";

		private const int MaxTempFiles = 32;

		private static readonly Random Random = new Random();

		private static int tempFileIndex = -1;

		private readonly StringBuilder macro;

		public MacroBuilder()
		{
			macro = new StringBuilder();
		}

		public MacroBuilder(string script)
		{
			macro = new StringBuilder(script);
		}

		public MacroBuilder Activate(string dialog, string field)
		{
			return AppendMethodCall("Activate", dialog, field);
		}

		public MacroBuilder Callback(string callback)
		{
			return Callback(callback, string.Empty);
		}

		public MacroBuilder Callback(string callback, string parameter)
		{
			return Callback(callback, parameter, "main_frame");
		}

		public MacroBuilder Callback(string callback, string parameter, string frame)
		{
			return AppendMethodCall("Callback", callback, parameter, frame);
		}

		public MacroBuilder CheckValue(string name, int value)
		{
			return AppendMethodCall("CheckValue", name, value);
		}

		public MacroBuilder CommandEnd()
		{
			return AppendMethodCall("CommandEnd");
		}

		public MacroBuilder CommandStart(string command, string parameter, string frame)
		{
			return AppendMethodCall("CommandStart", command, parameter, frame);
		}

		public MacroBuilder FileSelection(params string[] items)
		{
			macro.Append("akit.FileSelection(");
			for (int i = 0; i < items.Length; i++)
			{
				if (i > 0)
				{
					macro.Append(", ");
				}
				macro.Append('"').Append(items[i]).Append('"');
			}
			macro.AppendLine(");");
			return this;
		}

		public MacroBuilder ListSelect(string dialog, string field, params string[] items)
		{
			macro.AppendFormat("akit.ListSelect(\"{0}\", \"{1}\"", dialog, field);
			for (int i = 0; i < items.Length; i++)
			{
				macro.Append(", ").Append('"').Append(items[i])
					.Append('"');
			}
			macro.AppendLine(");");
			return this;
		}

		public MacroBuilder ModalDialog(int value)
		{
			return AppendMethodCall("ModalDialog", value);
		}

		public MacroBuilder MouseDown(string frame, string subframe, int x, int y, int modifier)
		{
			return AppendMethodCall("MouseDown", frame, subframe, x, y, modifier);
		}

		public MacroBuilder MouseUp(string frame, string subframe, int x, int y, int modifier)
		{
			return AppendMethodCall("MouseUp", frame, subframe, x, y, modifier);
		}

		public MacroBuilder PushButton(string button, string frame)
		{
			return AppendMethodCall("PushButton", button, frame);
		}

		public void Run()
		{
			Run(TeklaStructures.Connection);
		}

		public void Run(IConnection connection)
		{
			try
			{
				string macroFileName = GetMacroFileName();
				File.WriteAllText(Path.Combine(TeklaStructures.Environment.MacrosFolder, macroFileName), "namespace Tekla.Technology.Akit.UserScript {public class Script {public static void Run(Tekla.Technology.Akit.IScript akit) {" + macro.ToString() + "}}}");
				connection.RunMacro("..\\" + macroFileName);
			}
			catch (IOException value)
			{
				Debug.WriteLine(value);
			}
		}

		public MacroBuilder TabChange(string dialog, string field, string item)
		{
			return AppendMethodCall("TabChange", dialog, field, item);
		}

		public MacroBuilder TableSelect(string dialog, string field, params int[] items)
		{
			macro.AppendFormat("akit.TableSelect(\"{0}\", \"{1}\"", dialog, field);
			for (int i = 0; i < items.Length; i++)
			{
				macro.Append(", ").Append(items[i]);
			}
			macro.AppendLine(");");
			return this;
		}

		public MacroBuilder TableValueChange(string dialog, string table, string field, string value)
		{
			macro.AppendFormat("akit.TableValueChange(\"{0}\",\"{1}\",\"{2}\",\"{3}\")", dialog, table, field, value);
			macro.AppendLine(";");
			return this;
		}

		public override string ToString()
		{
			return macro.ToString();
		}

		public MacroBuilder TreeSelect(string dialog, string field, string rowstring)
		{
			return AppendMethodCall("TreeSelect", dialog, field, rowstring);
		}

		public MacroBuilder ValueChange(string dialog, string field, string data)
		{
			return AppendMethodCall("ValueChange", dialog, field, data);
		}

		private static string GetMacroFileName()
		{
			lock (Random)
			{
				if (tempFileIndex < 0)
				{
					tempFileIndex = Random.Next(0, 32);
				}
				else
				{
					tempFileIndex = (tempFileIndex + 1) % 32;
				}
				return $"macro_{tempFileIndex:00}.cs";
			}
		}

		private MacroBuilder AppendMethodCall(string method, params object[] arguments)
		{
			macro.Append("akit.").Append(method).Append('(');
			for (int i = 0; i < arguments.Length; i++)
			{
				if (i > 0)
				{
					macro.Append(", ");
				}
				if (arguments[i] is string)
				{
					macro.Append('"').Append(arguments[i]).Append('"');
				}
				else
				{
					macro.Append(arguments[i]);
				}
			}
			macro.AppendLine(");");
			return this;
		}
	}
}
