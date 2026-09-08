using System.Collections.Generic;
using System.Windows;

namespace PrismNewInterface
{
	public partial class InformationWindow : Window
	{
		public InformationWindow(string informationTitle, string informationText) : this(informationTitle, informationText, null)
		{

		}

		public InformationWindow(string informationTitle, string informationText, IList<InformationSection> sections)
		{
			InitializeComponent();

			InformationTitleTextBlock.Text = informationTitle;
			InformationTextBlock.Text = informationText;
			InformationSectionsItemsControl.ItemsSource = sections;

			Title = "Prism - " + informationTitle;
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}

	public sealed class InformationSection
	{
		public string Title { get; set; }
		public string Summary { get; set; }
		public string Details { get; set; }
	}
}