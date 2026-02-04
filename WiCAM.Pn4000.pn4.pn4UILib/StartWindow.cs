using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Options;
using WiCAM.Pn4000.Contracts.Configuration;
using WiCAM.Pn4000.Contracts.Telemetry;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public partial class StartWindow : Window, IComponentConnector
{
	private readonly ITelemetrySource _telemetrySource;

	private readonly IOptions<StartupOptions> _options;

	public StartWindow(ITelemetrySource telemetrySource, IOptions<StartupOptions> options)
	{
		_telemetrySource = telemetrySource;
		_options = options;
	}

	public void Initialize()
	{
		using (_telemetrySource.StartActivity("StartWindow - Initialize"))
		{
			StartupOptions value = _options.Value;
			if (value != null && !value.Minimized && !value.FastStart && value.ShowStartWindow)
			{
				InitializeComponent();
				string text = Environment.ExpandEnvironmentVariables("%PNDRIVE%\\u\\pn\\bin\\start.jpg");
				if (File.Exists(text))
				{
					BitmapImage bitmapImage = new BitmapImage();
					bitmapImage.BeginInit();
					bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapImage.UriSource = new Uri(text);
					bitmapImage.EndInit();
					base.Width = bitmapImage.Width;
					base.Height = bitmapImage.Height;
					base.Background = new ImageBrush(bitmapImage);
					base.ShowActivated = true;
					Show();
				}
			}
		}
	}

	private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		Close();
	}
}
