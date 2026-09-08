using System.Windows;

namespace PrismNewInterface
{
	public partial class ExecutionClassWindow : Window
	{
		public int SelectedExecutionClass { get; private set; }

		public ExecutionClassWindow()
		{
			InitializeComponent();
		}

		private void ApplyButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedExecutionClass = ExecutionClassComboBox.SelectedIndex;
			DialogResult = true;
		}
	}
}