using System;
using System.Windows.Forms;

namespace Tekla.Structures
{
	public static class TeklaStructures
	{
		private static readonly CommonTasks TeklaStructuresCommonTasks;

		private static readonly Connection TeklaStructuresConnection;

		private static readonly Registry TeklaStructuresRegistry;

		public static ICommonTasks CommonTasks => TeklaStructuresCommonTasks;

		public static Configuration Configuration => TeklaStructuresConnection.Model.Configuration;

		public static IConnection Connection => TeklaStructuresConnection;
		public static IEnvironment Environment => TeklaStructuresConnection.Model;

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
