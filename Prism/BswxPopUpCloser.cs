using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Prism
{
	public class BswxPopUpCloser
	{
		private readonly TimeSpan _timeout;
		private readonly int _pollIntervalMs;

		public BswxPopUpCloser(TimeSpan timeout, int pollIntervalMs = 500)
		{
			_timeout = timeout;
			_pollIntervalMs = pollIntervalMs;
		}

		public bool Run()
		{
			var stopwatch = Stopwatch.StartNew();

			bool handledBimReview = false;
			bool handledExport = false;

			while (stopwatch.Elapsed < _timeout)
			{
				if (!handledBimReview && HandleBIMReviewPopUp())
				{
					handledBimReview = true;
				}

				if (!handledExport && HandleExportCompletePopUp())
				{
					handledExport = true;
				}

				// ✅ Only exit when BOTH have been handled
				if (handledBimReview && handledExport)
				{
					Console.WriteLine("✅ All target popups handled.");
					return true;
				}

				Thread.Sleep(_pollIntervalMs);
			}

			Console.WriteLine($"⏱ Timeout reached. BIMReview: {handledBimReview}, Export: {handledExport}");
			return false;
		}

		#region Win32 Imports
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);

		[DllImport("user32.dll")]
		private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		// ✅ Non-blocking
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		private const uint BM_CLICK = 0x00F5;
		private const uint WM_CLOSE = 0x0010;

		#endregion

		#region Core Logic

		private bool HandleBIMReviewPopUp()
		{
			IntPtr hWnd = FindWindow(null, "BIMReview");
			if (hWnd == IntPtr.Zero)
				return false;

			IntPtr button = FindControlByText(hWnd, "&No");
			if (button == IntPtr.Zero)
				return false;

			PostMessage(button, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
			Console.WriteLine("✅ Closed BIMReview popup via 'No'");
			return true;
		}

		private bool HandleExportCompletePopUp()
		{
			IntPtr mainWindow = GetTeklaMainWindowHandle();
			if (mainWindow == IntPtr.Zero)
				return false;

			GetWindowThreadProcessId(mainWindow, out uint teklaPid);

			bool handled = false;

			EnumWindows((hWnd, lParam) =>
			{
				if (!IsWindowVisible(hWnd))
					return true;

				GetWindowThreadProcessId(hWnd, out uint windowPid);
				if (windowPid != teklaPid)
					return true;

				StringBuilder className = new StringBuilder(256);
				GetClassName(hWnd, className, className.Capacity);

				if (!className.ToString().StartsWith("WindowsForms10.Window"))
					return true;

				StringBuilder text = new StringBuilder(256);
				GetWindowText(hWnd, text, text.Capacity);

				if (!string.IsNullOrEmpty(text.ToString()))
					return true;

				// ✅ Found target popup
				PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
				Console.WriteLine($"✅ Closed export popup: {hWnd}");

				handled = true;
				return false; // stop enumeration

			}, IntPtr.Zero);

			return handled;
		}

		private static IntPtr FindControlByText(IntPtr parent, string textToFind)
		{
			IntPtr result = IntPtr.Zero;

			EnumChildWindows(parent, (hWnd, lParam) =>
			{
				StringBuilder text = new StringBuilder(256);
				GetWindowText(hWnd, text, text.Capacity);

				if (text.ToString().Equals(textToFind, StringComparison.OrdinalIgnoreCase))
				{
					result = hWnd;
					return false;
				}

				return true;
			}, IntPtr.Zero);

			return result;
		}

		private static IntPtr GetTeklaMainWindowHandle()
		{
			IntPtr result = IntPtr.Zero;

			EnumWindows((hWnd, lParam) =>
			{
				StringBuilder text = new StringBuilder(256);
				GetWindowText(hWnd, text, text.Capacity);

				if (!string.IsNullOrEmpty(text.ToString()) &&
					text.ToString().StartsWith("Tekla Structures"))
				{
					result = hWnd;
					return false;
				}

				return true;
			}, IntPtr.Zero);

			return result;
		}

		#endregion
	}
}