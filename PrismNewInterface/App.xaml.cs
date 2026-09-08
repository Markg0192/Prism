using PrismNewInterface.Services;
using PrismNewInterface.ViewModels;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace PrismNewInterface
{
	public partial class App : Application
	{
		private SplashWindow _splashWindow;
		private Thread _splashThread;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			ShowSplashScreen();

			try
			{
				Action<string> startupProgress = UpdateSplashStatus;

				PrismOperations prismOperations = new PrismOperations();

				MainWindow mainWindow = new MainWindow(prismOperations);

				MainWindow = mainWindow;

				if (!prismOperations.Initialise(startupProgress))
				{
					throw new InvalidOperationException(
						"Prism could not initialise or connect to the current Tekla model.");
				}

				MainViewModel mainViewModel = new MainViewModel(prismOperations);

				mainWindow.DataContext = mainViewModel;

				CloseSplashScreen();

				mainWindow.Show();

				_ = prismOperations.RunStartupLoggingAsync();
			}
			catch (Exception ex)
			{
				CloseSplashScreen();

				MessageBox.Show(ex.Message, "Prism", MessageBoxButton.OK, MessageBoxImage.Error);

				Shutdown();
			}
		}

		private void ShowSplashScreen()
		{
			ManualResetEventSlim splashReady =
				new ManualResetEventSlim(false);

			_splashThread = new Thread(() =>
			{
				_splashWindow = new SplashWindow();

				_splashWindow.Loaded += delegate
				{
					splashReady.Set();
				};

				_splashWindow.Show();

				Dispatcher.Run();
			});

			_splashThread.SetApartmentState(ApartmentState.STA);
			_splashThread.IsBackground = true;
			_splashThread.Start();

			splashReady.Wait();
		}

		private void UpdateSplashStatus(string message)
		{
			if (_splashWindow == null)
			{
				return;
			}

			_splashWindow.Dispatcher.BeginInvoke(
				new Action(() =>
				{
					_splashWindow.StatusText = message;
				}));
		}

		private void CloseSplashScreen()
		{
			if (_splashWindow == null)
			{
				return;
			}

			_splashWindow.Dispatcher.Invoke(() =>
			{
				_splashWindow.Close();

				Dispatcher.CurrentDispatcher.BeginInvokeShutdown(
					DispatcherPriority.Background);
			});

			_splashWindow = null;
		}
	}
}