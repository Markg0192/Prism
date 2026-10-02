using PrismNewInterface.Models;
using PrismNewInterface.Services;
using PrismNewInterface.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ValidationResult = PrismNewInterface.Models.ValidationResult;

namespace PrismNewInterface
{
	public partial class MainWindow : Window
	{
		private readonly PrismOperations _prismOperations;
		private static readonly Regex _numbersOnly = new Regex("^[0-9]+$");

		public MainWindow(PrismOperations prismOperations)
		{
			if (prismOperations == null)
			{
				throw new ArgumentNullException(nameof(prismOperations));
			}

			_prismOperations = prismOperations;

			InitializeComponent();

			DataContextChanged += MainWindow_DataContextChanged;
		}

		private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			MainViewModel viewModel = e.NewValue as MainViewModel;

			if (viewModel == null)
			{
				return;
			}

			viewModel.MaterialOrder.ValidationResults.CollectionChanged += MaterialValidationResults_CollectionChanged;
			viewModel.Drawing.ValidationResults.CollectionChanged += DrawingValidationResults_CollectionChanged;
			viewModel.FabPack.ValidationResults.CollectionChanged += FabPackValidationResults_CollectionChanged;

			BuildValidationColumns(MaterialValidationDataGrid, viewModel.MaterialOrder.ValidationResults);

			BuildValidationColumns(DrawingValidationDataGrid, viewModel.Drawing.ValidationResults);

			BuildValidationColumns(FabPackValidationDataGrid, viewModel.FabPack.ValidationResults);
		}

		private void NumbersOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !_numbersOnly.IsMatch(e.Text);
		}

		private void MaterialValidationResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			MainViewModel viewModel = DataContext as MainViewModel;

			if (viewModel == null)
			{
				return;
			}

			BuildValidationColumns(MaterialValidationDataGrid, viewModel.MaterialOrder.ValidationResults);
		}

		private void DrawingValidationResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			MainViewModel viewModel = DataContext as MainViewModel;

			if (viewModel == null)
			{
				return;
			}

			BuildValidationColumns(DrawingValidationDataGrid, viewModel.Drawing.ValidationResults);
		}

		private void FabPackValidationResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			MainViewModel viewModel = DataContext as MainViewModel;

			if (viewModel == null)
			{
				return;
			}

			BuildValidationColumns(FabPackValidationDataGrid, viewModel.FabPack.ValidationResults);
		}

		private void BuildValidationColumns(DataGrid dataGrid, IList<ValidationResult> results)
		{
			dataGrid.Columns.Clear();

			dataGrid.Columns.Add(new DataGridTextColumn
			{
				Header = "Part Mark",
				Binding = new Binding("PartMark"),
				Width = new DataGridLength(1, DataGridLengthUnitType.Star)
			});

			if (results == null || results.Count == 0)
			{
				return;
			}

			ValidationContext context;

			if (dataGrid == MaterialValidationDataGrid)
			{
				context = ValidationContext.Material;
			}
			else if (dataGrid == DrawingValidationDataGrid)
			{
				context = ValidationContext.Drawing;
			}
			else
			{
				context = ValidationContext.FabPack;
			}

			ValidationResult firstResult = results[0];

			for (int i = 0; i < firstResult.Checks.Count; i++)
			{
				ValidationCheckResult check = firstResult.Checks[i];

				bool hasFailures = results.Any(result => result.Checks.Count > i && !result.Checks[i].Passed);

				object header = check.CheckName;

				if (check.CanAutoComplete && hasFailures)
				{
					header = CreateAutoCompleteHeader(check.CheckName, check.AutoCompleteType, context);
				}

				dataGrid.Columns.Add(new DataGridTextColumn
				{
					Header = header,
					Binding = new Binding("Checks[" + i + "].Result"),
					Width = new DataGridLength(1, DataGridLengthUnitType.Star)
				});
			}
		}

		private void MaterialRequiredByCalendarButton_Click(
	object sender,
	RoutedEventArgs e)
		{
			MaterialRequiredByPopup.IsOpen = true;
		}

		private void MaterialRequiredByCalendar_SelectedDatesChanged(
			object sender,
			SelectionChangedEventArgs e)
		{
			if (!MaterialRequiredByCalendar.SelectedDate.HasValue)
			{
				return;
			}

			MaterialRequiredByTextBox.Text =
				MaterialRequiredByCalendar.SelectedDate.Value.ToString("dd/MM/yyyy");

			MaterialRequiredByPopup.IsOpen = false;
		}

		private void FabSiteDateCalendarButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			FabSiteDatePopup.IsOpen = true;
		}

		private void FabSiteDateCalendar_SelectedDatesChanged(
			object sender,
			SelectionChangedEventArgs e)
		{
			if (!FabSiteDateCalendar.SelectedDate.HasValue)
			{
				return;
			}

			FabSiteDateTextBox.Text =
				FabSiteDateCalendar.SelectedDate.Value.ToString("dd/MM/yyyy");

			FabSiteDatePopup.IsOpen = false;
		}

		private object CreateAutoCompleteHeader(string checkName, AutoCompleteType autoCompleteType, ValidationContext context)
		{
			Grid grid = new Grid
			{
				HorizontalAlignment = HorizontalAlignment.Stretch
			};

			grid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1, GridUnitType.Star)
			});

			grid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});

			TextBlock text = new TextBlock
			{
				Text = checkName,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Left,
				TextTrimming = TextTrimming.CharacterEllipsis
			};

			Button button = new Button
			{
				Content = "⚡",
				Tag = new AutoCompleteButtonTag
				{
					AutoCompleteType = autoCompleteType,
					ValidationContext = context
				},
				Style = (Style)FindResource("AutoCompleteButtonStyle"),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center,
				ToolTip = "Auto-Complete this check"
			};

			Grid.SetColumn(text, 0);
			Grid.SetColumn(button, 1);

			button.Click += AutoCompleteButton_Click;

			grid.Children.Add(text);
			grid.Children.Add(button);

			return grid;
		}

		private async void AutoCompleteButton_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;

			if (button == null || !(button.Tag is AutoCompleteButtonTag tag))
			{
				return;
			}

			MainViewModel viewModel = DataContext as MainViewModel;

			if (viewModel == null)
			{
				return;
			}

			int? executionClass = null;

			if (tag.AutoCompleteType == AutoCompleteType.ExecutionClass)
			{
				ExecutionClassWindow window = new ExecutionClassWindow
				{
					Owner = this
				};

				if (window.ShowDialog() != true)
				{
					return;
				}

				executionClass = window.SelectedExecutionClass;
			}

			string autoCompleteName = GetAutoCompleteDisplayName(tag.AutoCompleteType);

			button.IsEnabled = false;
			button.Content = "…";
			button.ToolTip = "Applying Auto-Complete...";

			viewModel.SetProgress(15, "Auto-completing " + autoCompleteName + "...");

			OperationResult result = await _prismOperations.AutoCompleteAsync(tag.AutoCompleteType, executionClass);

			if (!result.Success)
			{
				button.IsEnabled = true;
				button.Content = "⚡";
				button.ToolTip = "Auto-Complete this check";

				viewModel.SetProgress(100, "Auto-Complete failed.");

				MessageBox.Show(result.Message, "Auto-Complete", MessageBoxButton.OK, MessageBoxImage.Warning);

				return;
			}

			viewModel.SetProgress(85, "Restoring original selection...");

			_prismOperations.ReselectOriginalParts();

			viewModel.SetProgress(100, autoCompleteName + " Auto-Complete applied.");

			button.Content = "✓";
			button.ToolTip = "Auto-Complete applied - re-run the check to verify";

			if (tag.ValidationContext == ValidationContext.Material)
			{
				viewModel.MaterialOrder.AutoCompleteApplied();
			}
			else if (tag.ValidationContext == ValidationContext.Drawing)
			{
				viewModel.Drawing.AutoCompleteApplied();
			}
		}

		private async void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				await Task.Yield();

				SettingsWindow settingsWindow = new SettingsWindow(_prismOperations);

				settingsWindow.Owner = this;

				Mouse.OverrideCursor = null;

				settingsWindow.ShowDialog();
			}
			catch
			{
				Mouse.OverrideCursor = null;
				throw;
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!ReferenceEquals(e.Source, MainTabControl))
			{
				return;
			}

			Dispatcher.BeginInvoke(new Action(() =>
			{
				MoveTabIndicator(true);
			}), DispatcherPriority.Loaded);
		}

		private void MoveTabIndicator(bool animate)
		{
			if (MainTabControl == null || SlidingTabIndicator == null || TabIndicatorCanvas == null)
			{
				return;
			}

			TabItem selectedTab = MainTabControl.SelectedItem as TabItem;

			if (selectedTab == null || !selectedTab.IsLoaded)
			{
				return;
			}

			Point tabPosition = selectedTab.TranslatePoint(new Point(0, 0), TabIndicatorCanvas);

			double targetLeft = tabPosition.X + 10;
			double targetTop = tabPosition.Y + selectedTab.ActualHeight - 3;
			double targetWidth = Math.Max(0, selectedTab.ActualWidth - 20);

			Canvas.SetTop(SlidingTabIndicator, targetTop);

			if (!animate || SlidingTabIndicator.Width == 0)
			{
				SlidingTabIndicator.BeginAnimation(Canvas.LeftProperty, null);
				SlidingTabIndicator.BeginAnimation(WidthProperty, null);

				Canvas.SetLeft(SlidingTabIndicator, targetLeft);
				SlidingTabIndicator.Width = targetWidth;

				return;
			}

			DoubleAnimation positionAnimation = new DoubleAnimation
			{
				To = targetLeft,
				Duration = TimeSpan.FromMilliseconds(220),
				EasingFunction = new CubicEase
				{
					EasingMode = EasingMode.EaseOut
				}
			};

			DoubleAnimation widthAnimation = new DoubleAnimation
			{
				To = targetWidth,
				Duration = TimeSpan.FromMilliseconds(220),
				EasingFunction = new CubicEase
				{
					EasingMode = EasingMode.EaseOut
				}
			};

			SlidingTabIndicator.BeginAnimation(Canvas.LeftProperty, positionAnimation);
			SlidingTabIndicator.BeginAnimation(WidthProperty, widthAnimation);
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				MoveTabIndicator(false);
			}), DispatcherPriority.Loaded);
		}

		private void PrepareFabsecsButton_Click(object sender, RoutedEventArgs e)
		{
			PrepareFabsecsWindow prepareFabsecsWindow =
				new PrepareFabsecsWindow(_prismOperations);

			prepareFabsecsWindow.Owner = this;
			prepareFabsecsWindow.ShowDialog();
		}

		private void StartNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !_numbersOnly.IsMatch(e.Text);
		}

		private void PrepareSpecialFittingsButton_Click(object sender, RoutedEventArgs e)
		{
			PrepareSpecialFittingsWindow window = new PrepareSpecialFittingsWindow(_prismOperations);

			window.Owner = this;
			window.ShowDialog();
		}

		private void MaterialCheckInformationButton_Click(object sender, RoutedEventArgs e)
		{
			PrismInformationService.ShowMaterialCheck(this);
		}

		private void PrepareFabsecsInformationButton_Click(object sender, RoutedEventArgs e)
		{
			PrismInformationService.ShowPrepareFabsecs(this);
		}

		private void CreateOrderInformationButton_Click(object sender, RoutedEventArgs e)
		{
			PrismInformationService.ShowCreateMaterialOrder(this);
		}

		private void PrepareSpecialFittingsInformationButton_Click(object sender, RoutedEventArgs e)
		{
			PrismInformationService.ShowPrepareSpecialFittings(this);
		}

		private void DrawingCheckInformationButton_Click(object sender, RoutedEventArgs e)
		{
			PrismInformationService.ShowDrawingCheck(this);
		}

		private void DetailOrientationHolesInformationButton_Click(object sender, RoutedEventArgs e)
		{
			PrismInformationService.ShowDetailOrientationHoles(this);
		}

		private void CreateDrawingsInformationButton_Click(object sender, RoutedEventArgs e)
		{
			PrismInformationService.ShowCreateDrawings(this);
		}

		private void FabPackCheckInformationButton_Click(object sender, RoutedEventArgs e)
		{
			PrismInformationService.ShowFabPackCheck(this);
		}

		private void CreateFabPackInformationButton_Click(object sender, RoutedEventArgs e)
		{
			PrismInformationService.ShowCreateFabPack(this);
		}

		public enum ValidationContext
		{
			Material,
			Drawing,
			FabPack
		}

		private string GetAutoCompleteDisplayName(AutoCompleteType autoCompleteType)
		{
			switch (autoCompleteType)
			{
				case AutoCompleteType.NameAndClass:
					return "Name and Class";

				case AutoCompleteType.ExecutionClass:
					return "Execution Class";

				case AutoCompleteType.Orientation:
					return "Orientation";

				case AutoCompleteType.SecondaryNumberingMismatch:
					return "Secondary Numbering";

				case AutoCompleteType.SecondaryPhasingMismatch:
					return "Secondary Phasing";

				default:
					return "selected check";
			}
		}

		private sealed class AutoCompleteButtonTag
		{
			public AutoCompleteType AutoCompleteType { get; set; }

			public ValidationContext ValidationContext { get; set; }
		}

		private void ValidationDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DataGrid dataGrid = sender as DataGrid;

			if (dataGrid == null)
			{
				return;
			}

			ValidationResult row = dataGrid.SelectedItem as ValidationResult;

			if (row == null || string.IsNullOrWhiteSpace(row.Guid))
			{
				return;
			}

			_prismOperations.SelectAndZoomToPart(row.Guid);
		}
	}
}