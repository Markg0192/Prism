#define DEBUG
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Windows.Forms;
using Tekla.Structures.Dialog;

namespace Tekla.Structures
{
	internal class MainWindow : IMainWindow, IWin32Window
	{
		public IntPtr Handle
		{
			get
			{
				if (MainWindow.Frame != null)
				{
					return MainWindow.Frame.get_Handle();
				}
				return IntPtr.Zero;
			}
		}

		public bool IsActive => MainWindow.Frame.get_Handle() == GetForegroundWindow();

		public bool IsMinimized => IsIconic(MainWindow.Frame.get_Handle());

		public void Activate()
		{
			SetForegroundWindow(MainWindow.Frame.get_Handle());
		}

		public void AttachChildForm(Form form)
		{
			if (form == null)
			{
				throw new ArgumentNullException("form");
			}
			try
			{
				MainWindow.Frame.AddExternalWindow(form.Name, form.Handle);
			}
			catch (RemotingException value)
			{
				Debug.WriteLine(value);
			}
		}

		public void DetachChildForm(Form form)
		{
			if (form == null)
			{
				throw new ArgumentNullException("form");
			}
			try
			{
				MainWindow.Frame.RemoveExternalWindow(form.Name, form.Handle);
			}
			catch (RemotingException value)
			{
				Debug.WriteLine(value);
			}
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsIconic(IntPtr windowHandle);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr windowHandle);
	}
}
