using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class DrawingControl
	{
		private const int WmSetRedraw = 11;

		public static void ResumeDrawing(Control parent)
		{
			SendMessage(parent.Handle, 11, windowParameter: true, 0);
			parent.Refresh();
		}

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr windowHandle, int windowMessage, bool windowParameter, int parameterLong);

		public static void SuspendDrawing(Control parent)
		{
			SendMessage(parent.Handle, 11, windowParameter: false, 0);
		}
	}
}
