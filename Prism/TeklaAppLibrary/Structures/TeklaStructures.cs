using System;
using System.Windows.Forms;

namespace Tekla.Structures
{
	public static class TeklaStructures
	{
		private static readonly CommonTasks TeklaStructuresCommonTasks;

		private static readonly Connection TeklaStructuresConnection;

		private static readonly MainWindow TeklaStructuresMainWindow;

		private static readonly Registry TeklaStructuresRegistry;

		public static ICommonTasks CommonTasks => TeklaStructuresCommonTasks;

		public static Configuration Configuration => TeklaStructuresConnection.Model.Configuration;

		public static IConnection Connection => TeklaStructuresConnection;

		public static IDrawing Drawing => TeklaStructuresConnection.Drawing;

		public static IEnvironment Environment => TeklaStructuresConnection.Model;

		public static bool IsRunning => TeklaStructuresMainWindow.Handle != IntPtr.Zero;

		public static IMainWindow MainWindow => TeklaStructuresMainWindow;

		public static IModel Model => TeklaStructuresConnection.Model;

		public static IRegistry Registry => TeklaStructuresRegistry;

		public static string Version => TeklaStructuresConnection.Model.Version;

		public static event EventHandler Closed
		{
			add
			{
				TeklaStructuresConnection.Model.ApplicationClosed += value;
			}
			remove
			{
				TeklaStructuresConnection.Model.ApplicationClosed -= value;
			}
		}

		static TeklaStructures()
		{
			TeklaStructuresCommonTasks = new CommonTasks();
			TeklaStructuresConnection = new Connection();
			TeklaStructuresMainWindow = new MainWindow();
			TeklaStructuresRegistry = new Registry();
			Application.ApplicationExit += delegate
			{
				Disconnect();
			};
		}

		public static bool Connect()
		{
			return TeklaStructuresConnection.Connect();
		}

		public static void Disconnect()
		{
			TeklaStructuresConnection.Disconnect();
		}

		public static void ExecuteScript(string script)
		{
			new MacroBuilder(script).Run(TeklaStructuresConnection);
		}
	}
}
