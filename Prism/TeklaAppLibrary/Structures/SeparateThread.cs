#define TRACE
using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Forms;

namespace Tekla.Structures
{
	public static class SeparateThread
	{
		public delegate void Action();

		public delegate T Action<T>();

		private static int timeoutMilliseconds = 10000;

		public static int TimeoutMilliseconds
		{
			get
			{
				return timeoutMilliseconds;
			}
			set
			{
				timeoutMilliseconds = value;
			}
		}

		public static void Execute(Action action)
		{
			Execute(timeoutMilliseconds, action);
		}

		public static void Execute(int timeoutMilliseconds, Action action)
		{
			Exception error = null;
			ManualResetEvent ev = new ManualResetEvent(initialState: false);
			try
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					try
					{
						action();
					}
					catch (RemotingException ex2)
					{
						Trace.WriteLine(ex2.ToString());
					}
					catch (Exception ex3)
					{
						error = ex3;
					}
					finally
					{
						try
						{
							ev.Set();
						}
						catch (ObjectDisposedException)
						{
						}
					}
				});
				if (!ev.WaitOne(timeoutMilliseconds, exitContext: true))
				{
					WaitingDialog waitingDialog = new WaitingDialog(delegate
					{
						try
						{
							return ev.WaitOne(0, exitContext: true);
						}
						catch (ObjectDisposedException)
						{
							return true;
						}
					});
					if (waitingDialog.ShowDialog() == DialogResult.Cancel)
					{
						throw new TimeoutException("Operation timed out.");
					}
				}
				else if (error != null)
				{
					throw new InvalidOperationException("Operation caused an error. See the inner exception for details.", error);
				}
			}
			finally
			{
				if (ev != null)
				{
					((IDisposable)ev).Dispose();
				}
			}
		}

		public static T Execute<T>(Action<T> action)
		{
			return Execute(timeoutMilliseconds, action);
		}

		public static T Execute<T>(int timeoutMilliseconds, Action<T> action)
		{
			T result = default(T);
			Exception error = null;
			ManualResetEvent ev = new ManualResetEvent(initialState: false);
			try
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					try
					{
						result = action();
					}
					catch (RemotingException ex2)
					{
						Trace.WriteLine(ex2.ToString());
						result = default(T);
					}
					catch (Exception ex3)
					{
						error = ex3;
					}
					finally
					{
						try
						{
							ev.Set();
						}
						catch (ObjectDisposedException)
						{
						}
					}
				});
				if (!ev.WaitOne(timeoutMilliseconds, exitContext: true))
				{
					WaitingDialog waitingDialog = new WaitingDialog(delegate
					{
						try
						{
							return ev.WaitOne(0, exitContext: true);
						}
						catch (ObjectDisposedException)
						{
							return true;
						}
					});
					if (waitingDialog.ShowDialog() == DialogResult.Cancel)
					{
						throw new TimeoutException("Operation timed out.");
					}
				}
				else if (error != null)
				{
					throw new InvalidOperationException("Operation caused an error. See the inner exception for details.", error);
				}
			}
			finally
			{
				if (ev != null)
				{
					((IDisposable)ev).Dispose();
				}
			}
			return result;
		}
	}
}
