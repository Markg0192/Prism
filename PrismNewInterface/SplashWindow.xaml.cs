using System.Windows;

namespace PrismNewInterface
{
	public partial class SplashWindow : Window
	{
		public SplashWindow()
		{
			InitializeComponent();

			StatusText = "Starting Prism...";
			DataContext = this;
		}

		public static readonly DependencyProperty StatusTextProperty =
			DependencyProperty.Register(
				nameof(StatusText),
				typeof(string),
				typeof(SplashWindow),
				new PropertyMetadata(string.Empty));

		public string StatusText
		{
			get { return (string)GetValue(StatusTextProperty); }
			set { SetValue(StatusTextProperty, value); }
		}
	}
}