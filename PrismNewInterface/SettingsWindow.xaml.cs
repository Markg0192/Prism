using PrismNewInterface.Models;
using PrismNewInterface.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PrismNewInterface
{
	public partial class SettingsWindow : Window, INotifyPropertyChanged
	{
		private readonly PrismOperations _prismOperations;

		private string _projectManagement;
		private string _drawingOfficeManager;
		private string _documentControl;
		private string _otherProjectUsers;

		private string _prelimPrefix;
		private string _fabPackType;
		private string _fabsecGreen;

		private string _materialDirectory;
		private string _carcassDirectory;
		private string _boltDirectory;
		private string _seversafeDirectory;
		private string _fabPackDirectory;
		private string _variationDirectory;

		private string _prismVersion;

		public event PropertyChangedEventHandler PropertyChanged;

		public ObservableCollection<ClassificationCodeRow> ClassificationCodes { get; private set; }

		public string ProjectManagement
		{
			get { return _projectManagement; }
			set { SetProperty(ref _projectManagement, value); }
		}

		public string DrawingOfficeManager
		{
			get { return _drawingOfficeManager; }
			set { SetProperty(ref _drawingOfficeManager, value); }
		}

		public string DocumentControl
		{
			get { return _documentControl; }
			set { SetProperty(ref _documentControl, value); }
		}

		public string OtherProjectUsers
		{
			get { return _otherProjectUsers; }
			set { SetProperty(ref _otherProjectUsers, value); }
		}

		public string PrelimPrefix
		{
			get { return _prelimPrefix; }
			set { SetProperty(ref _prelimPrefix, value); }
		}

		public string FabPackType
		{
			get { return _fabPackType; }
			set { SetProperty(ref _fabPackType, value); }
		}

		public string FabsecGreen
		{
			get { return _fabsecGreen; }
			set { SetProperty(ref _fabsecGreen, value); }
		}

		public string MaterialDirectory
		{
			get { return _materialDirectory; }
			set { SetProperty(ref _materialDirectory, value); }
		}

		public string CarcassDirectory
		{
			get { return _carcassDirectory; }
			set { SetProperty(ref _carcassDirectory, value); }
		}

		public string BoltDirectory
		{
			get { return _boltDirectory; }
			set { SetProperty(ref _boltDirectory, value); }
		}

		public string SeversafeDirectory
		{
			get { return _seversafeDirectory; }
			set { SetProperty(ref _seversafeDirectory, value); }
		}

		public string FabPackDirectory
		{
			get { return _fabPackDirectory; }
			set { SetProperty(ref _fabPackDirectory, value); }
		}

		public string VariationDirectory
		{
			get { return _variationDirectory; }
			set { SetProperty(ref _variationDirectory, value); }
		}

		public string PrismVersion
		{
			get { return _prismVersion; }
			private set { SetProperty(ref _prismVersion, value); }
		}

		public SettingsWindow(PrismOperations prismOperations)
		{
			if (prismOperations == null)
			{
				throw new ArgumentNullException(nameof(prismOperations));
			}

			_prismOperations = prismOperations;

			ClassificationCodes = new ObservableCollection<ClassificationCodeRow>();

			InitializeComponent();

			DataContext = this;

			Loaded += SettingsWindow_Loaded;

			LoadSettings();
		}

		private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				MoveSettingsTabIndicator(false);
			}), DispatcherPriority.Render);
		}

		private void SettingsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!ReferenceEquals(e.Source, SettingsTabControl))
			{
				return;
			}

			Dispatcher.BeginInvoke(new Action(() =>
			{
				MoveSettingsTabIndicator(true);
			}), DispatcherPriority.Render);
		}

		private void MoveSettingsTabIndicator(bool animate)
		{
			if (SettingsTabControl == null || SettingsSlidingTabIndicator == null || SettingsTabIndicatorCanvas == null)
			{
				return;
			}

			TabItem selectedTab = SettingsTabControl.SelectedItem as TabItem;

			if (selectedTab == null || !selectedTab.IsLoaded)
			{
				return;
			}

			Point tabPosition = selectedTab.TranslatePoint(new Point(0, 0), SettingsTabIndicatorCanvas);

			double targetLeft = tabPosition.X + 10;
			double targetTop = tabPosition.Y + selectedTab.ActualHeight - 3;
			double targetWidth = Math.Max(0, selectedTab.ActualWidth - 20);

			Canvas.SetTop(SettingsSlidingTabIndicator, targetTop);

			double currentLeft = Canvas.GetLeft(SettingsSlidingTabIndicator);

			if (double.IsNaN(currentLeft))
			{
				currentLeft = targetLeft;
			}

			double currentWidth = SettingsSlidingTabIndicator.ActualWidth;

			if (currentWidth <= 0)
			{
				currentWidth = targetWidth;
			}

			if (!animate || SettingsSlidingTabIndicator.Width <= 0)
			{
				SettingsSlidingTabIndicator.BeginAnimation(Canvas.LeftProperty, null);
				SettingsSlidingTabIndicator.BeginAnimation(WidthProperty, null);

				Canvas.SetLeft(SettingsSlidingTabIndicator, targetLeft);
				SettingsSlidingTabIndicator.Width = targetWidth;

				return;
			}

			CubicEase easing = new CubicEase
			{
				EasingMode = EasingMode.EaseInOut
			};

			Canvas.SetLeft(SettingsSlidingTabIndicator, targetLeft);
			SettingsSlidingTabIndicator.Width = targetWidth;

			DoubleAnimation leftAnimation = new DoubleAnimation
			{
				From = currentLeft,
				To = targetLeft,
				Duration = TimeSpan.FromMilliseconds(280),
				EasingFunction = easing,
				FillBehavior = FillBehavior.Stop
			};

			DoubleAnimation widthAnimation = new DoubleAnimation
			{
				From = currentWidth,
				To = targetWidth,
				Duration = TimeSpan.FromMilliseconds(280),
				EasingFunction = easing,
				FillBehavior = FillBehavior.Stop
			};

			SettingsSlidingTabIndicator.BeginAnimation(Canvas.LeftProperty, leftAnimation);
			SettingsSlidingTabIndicator.BeginAnimation(WidthProperty, widthAnimation);
		}

		private void LoadSettings()
		{
			LoadClassificationCodes();
			LoadProjectUsers();
			LoadCurrentPrelimStartPoint();
			LoadAdvancedSettings();
			LoadPackageDirectories();

			PrismVersion = _prismOperations.GetPrismVersion();
		}

		private void LoadClassificationCodes()
		{
			ClassificationCodes.Clear();

			foreach (ClassificationCodeSetting setting in _prismOperations.GetClassificationCodes())
			{
				ClassificationCodes.Add(
					new ClassificationCodeRow
					{
						SelectionFilter = setting.SelectionFilter,
						Code = setting.Code,
						Title = setting.Title
					});
			}

			while (ClassificationCodes.Count < 8)
			{
				ClassificationCodes.Add(new ClassificationCodeRow());
			}
		}

		private void LoadProjectUsers()
		{
			ProjectUserSettings settings = _prismOperations.GetProjectUserSettings();

			ProjectManagement = settings.ProjectManagement;
			DrawingOfficeManager = settings.DrawingOfficeManager;
			DocumentControl = settings.DocumentControl;
			OtherProjectUsers = settings.Others;
		}

		private void LoadCurrentPrelimStartPoint()
		{
			int? currentStartPoint = _prismOperations.GetCurrentPrelimStartPoint();

			CurrentPrelimStartPointText.Text = currentStartPoint.HasValue
				? currentStartPoint.Value.ToString()
				: "Error";
		}

		private void LoadAdvancedSettings()
		{
			AdvancedPrismSettings settings = _prismOperations.GetAdvancedSettings();

			PrelimPrefix = settings.PrelimPrefix;
			FabPackType = settings.FabPackType;
			FabsecGreen = settings.FabsecGreen;
		}

		private void LoadPackageDirectories()
		{
			PackageDirectorySettings settings = _prismOperations.GetPackageDirectorySettings();

			MaterialDirectory = settings.Material;
			CarcassDirectory = settings.Carcasses;
			BoltDirectory = settings.Bolts;
			SeversafeDirectory = settings.Seversafe;
			FabPackDirectory = settings.FabPack;
			VariationDirectory = settings.Variation;
		}

		private void RefreshPrelimButton_Click(object sender, RoutedEventArgs e)
		{
			LoadCurrentPrelimStartPoint();
		}

		private async void ApplyAllSettingsButton_Click(object sender, RoutedEventArgs e)
		{
			ApplySettingsButton.IsEnabled = false;
			SettingsStatusText.Text = "Applying settings...";
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				// Give WPF a chance to repaint the status text and cursor
				// before the synchronous server operations begin.
				await Task.Yield();

				List<string> failedSections = new List<string>();

				ClassificationCodeSetting[] classificationCodeSettings = ClassificationCodes
					.Take(8)
					.Select(row => new ClassificationCodeSetting(row.SelectionFilter, row.Code, row.Title))
					.ToArray();

				if (!_prismOperations.SaveClassificationCodes(classificationCodeSettings))
				{
					failedSections.Add("Classification Codes");
				}

				ProjectUserSettings projectUserSettings = new ProjectUserSettings
				{
					ProjectManagement = ProjectManagement,
					DrawingOfficeManager = DrawingOfficeManager,
					DocumentControl = DocumentControl,
					Others = OtherProjectUsers
				};

				if (!_prismOperations.SaveProjectUserSettings(projectUserSettings))
				{
					failedSections.Add("Project Users");
				}

				AdvancedPrismSettings advancedSettings = new AdvancedPrismSettings
				{
					PrelimPrefix = PrelimPrefix,
					FabPackType = FabPackType,
					FabsecGreen = string.IsNullOrWhiteSpace(FabsecGreen) ? "100" : FabsecGreen
				};

				if (!_prismOperations.SaveAdvancedSettings(advancedSettings))
				{
					failedSections.Add("Advanced Settings");
				}
				else
				{
					FabsecGreen = advancedSettings.FabsecGreen;
				}

				PackageDirectorySettings packageDirectorySettings = new PackageDirectorySettings
				{
					Material = MaterialDirectory,
					Carcasses = CarcassDirectory,
					Bolts = BoltDirectory,
					Seversafe = SeversafeDirectory,
					FabPack = FabPackDirectory,
					Variation = VariationDirectory
				};

				if (!_prismOperations.SavePackageDirectorySettings(packageDirectorySettings))
				{
					failedSections.Add("Package Directories");
				}

				if (failedSections.Count == 0)
				{
					SettingsStatusText.Text = "Settings applied.";
					return;
				}

				SettingsStatusText.Text = "Some settings could not be applied.";

				MessageBox.Show(
					"The following settings could not be saved:\n\n" +
					string.Join("\n", failedSections.Select(section => "• " + section)),
					"Prism Settings",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
			}
			catch (Exception ex)
			{
				SettingsStatusText.Text = "Settings could not be applied.";

				MessageBox.Show(
					"Prism could not save the settings.\n\n" + ex.Message,
					"Prism Settings",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
			finally
			{
				Mouse.OverrideCursor = null;
				ApplySettingsButton.IsEnabled = true;
			}
		}

		private void ClassificationCodesInfoButton_Click(object sender, RoutedEventArgs e)
		{
			List<InformationSection> sections = new List<InformationSection>
			{
				new InformationSection
				{
					Title = "Selection Filter",
					Summary = "Controls which members are included.",
					Details = "This is the name of the Tekla selection filter that Prism uses to determine which members are included in this classification row."
				},

				new InformationSection
				{
					Title = "Code",
					Summary = "The UniClass code applied to the member.",
					Details = "The UniClass code entered here is applied to SEV-UDA-130 on each member that matches the selection filter."
				},

				new InformationSection
				{
					Title = "Title",
					Summary = "The UniClass description applied to the member.",
					Details = "The text entered here is applied to SEV-UDA-131 on each member that matches the selection filter."
				},

				new InformationSection
				{
					Title = "When are these applied?",
					Summary = "Classification data is applied during material ordering.",
					Details = "Prism applies the configured UniClass Code and Title to matching members when those members are being ordered for material."
				}
			};

			InformationWindow informationWindow = new InformationWindow(
				"Classification Codes",
				"These settings control how Prism assigns UniClass classification information to members during the material ordering process.",
				sections);

			informationWindow.Owner = this;
			informationWindow.ShowDialog();
		}

		private void SetNewPrelimStartPointButton_Click(object sender, RoutedEventArgs e)
		{
			int newStartPoint;

			if (!int.TryParse(NewPrelimStartPointTextBox.Text, out newStartPoint) || newStartPoint < 0)
			{
				MessageBox.Show(
					"Enter a valid non-negative preliminary start number.",
					"Prism Settings",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			int? currentStartPoint = _prismOperations.GetCurrentPrelimStartPoint();

			if (!currentStartPoint.HasValue)
			{
				MessageBox.Show(
					"Prism could not retrieve the current preliminary start point.",
					"Prism Settings",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			MessageBoxResult confirmation = MessageBox.Show(
				"The current preliminary start point is " + currentStartPoint.Value + ".\n\n" +
				"This will reset it to " + newStartPoint + ". Continue?",
				"Confirm Preliminary Reset",
				MessageBoxButton.YesNo,
				MessageBoxImage.Warning,
				MessageBoxResult.No);

			if (confirmation != MessageBoxResult.Yes)
			{
				return;
			}

			if (!_prismOperations.SetPrelimStartPoint(newStartPoint))
			{
				MessageBox.Show(
					"The preliminary start point could not be updated.",
					"Prism Settings",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			LoadCurrentPrelimStartPoint();

			NewPrelimStartPointTextBox.Clear();

			MessageBox.Show(
				"The preliminary start point has been changed from " +
				currentStartPoint.Value + " to " + newStartPoint + ".",
				"Prism Settings",
				MessageBoxButton.OK,
				MessageBoxImage.Information);
		}

		private void ContactSupportButton_Click(object sender, RoutedEventArgs e)
		{
			_prismOperations.WriteHelpEmail();
		}

		private void SeverfieldWebsiteButton_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://www.severfield.com");
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void SetProperty(ref string field, string value, [CallerMemberName] string propertyName = null)
		{
			if (field == value)
			{
				return;
			}

			field = value;

			OnPropertyChanged(propertyName);
		}

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public sealed class ClassificationCodeRow
	{
		public string SelectionFilter { get; set; }
		public string Code { get; set; }
		public string Title { get; set; }

		public ClassificationCodeRow()
		{
			SelectionFilter = string.Empty;
			Code = string.Empty;
			Title = string.Empty;
		}
	}
}