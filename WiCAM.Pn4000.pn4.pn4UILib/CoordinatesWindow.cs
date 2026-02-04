using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Screen;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public partial class CoordinatesWindow : Window, IScreenAddOn, IComponentConnector
{
	private const int WmMouseactivate = 33;

	private const int MaNoactivate = 3;

	private bool _isIn;

	private readonly IMainWindowDataProvider _mainWindow;

	private readonly ActivateProgramPart _activateProgramPart;

	private double _oldOpacity = 0.5;

	private readonly IConfigProvider _configProvider;

	private readonly IScreen2D _screen2D;

	public CoordinatesWindow(IConfigProvider configProvider, IScreen2D screen2D, IMainWindowDataProvider mainWindow, ActivateProgramPart activateProgramPart)
	{
		_configProvider = configProvider;
		_screen2D = screen2D;
		_mainWindow = mainWindow;
		_activateProgramPart = activateProgramPart;
		InitializeComponent();
	}

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		(PresentationSource.FromVisual(this) as HwndSource).AddHook(WndProc);
	}

	private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (msg == 33)
		{
			handled = true;
			return new IntPtr(3);
		}
		return IntPtr.Zero;
	}

	protected override void OnActivated(EventArgs e)
	{
		if (!DesignerProperties.GetIsInDesignMode(this))
		{
			base.Owner.Activate();
		}
	}

	public void CalculateLocation()
	{
		if (_isIn)
		{
			return;
		}
		_isIn = true;
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (!_screen2D.IsActive || _mainWindow.Is2DAlternativeScreen() || !generalUserSettingsConfig.GadgetCoordinates || _mainWindow.Active_screen_layout != 0 || !_activateProgramPart.ModuleName.ToUpper().Contains("2D"))
		{
			base.Visibility = Visibility.Collapsed;
			_isIn = false;
			return;
		}
		if (!base.IsVisible)
		{
			Show();
		}
		Tuple<int, int, int, int> cornersAtScreen = _screen2D.GetCornersAtScreen();
		PresentationSource presentationSource = PresentationSource.FromVisual(_mainWindow as Window);
		if (presentationSource != null)
		{
			base.Left = (double)cornersAtScreen.Item3 / presentationSource.CompositionTarget.TransformToDevice.M11 - base.Width;
			base.Top = (double)cornersAtScreen.Item4 / presentationSource.CompositionTarget.TransformToDevice.M22 - base.Height;
			_isIn = false;
		}
	}

	internal void Set(string v)
	{
		if (base.Visibility != Visibility.Collapsed)
		{
			text.Text = v;
		}
	}

	private void Window_MouseEnter(object sender, MouseEventArgs e)
	{
		_oldOpacity = base.Opacity;
		base.Opacity = 1.0;
		close.Visibility = Visibility.Visible;
	}

	private void Window_MouseLeave(object sender, MouseEventArgs e)
	{
		base.Opacity = _oldOpacity;
		close.Visibility = Visibility.Collapsed;
	}

	private void close_Click(object sender, RoutedEventArgs e)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		generalUserSettingsConfig.GadgetCoordinates = false;
		_configProvider.Push(generalUserSettingsConfig);
		_configProvider.Save<GeneralUserSettingsConfig>();
		base.Visibility = Visibility.Collapsed;
	}
}
