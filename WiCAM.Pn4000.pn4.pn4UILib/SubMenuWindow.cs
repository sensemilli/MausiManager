using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using pncommon.WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.pn4.Contracts;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.uicontrols.Buttons;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public partial class SubMenuWindow : Window, IComponentConnector
{
	private const int GwlStyle = -16;

	private const int WsSysmenu = 524288;

	private Dictionary<object, int> _connection;

	private Dictionary<int, PnRibbonButton> _connectionAntyRibbon;

	public bool FirstShow = true;

	private MFileExpert _lastExpert;

	private MFileExpert _lastExpertRibbon;

	private bool _oldHideToolTips;

	private bool _oldSubMenuBigIcons;

	private bool _oldSubMenuHideText;

	private readonly Color[] _pncolor = new Color[64]
	{
		Color.FromArgb(byte.MaxValue, 3, 3, 3),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 0),
		Color.FromArgb(byte.MaxValue, 0, byte.MaxValue, 0),
		Color.FromArgb(byte.MaxValue, 70, 140, 140),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 240),
		Color.FromArgb(byte.MaxValue, 0, byte.MaxValue, 240),
		Color.FromArgb(byte.MaxValue, 0, 0, 0),
		Color.FromArgb(byte.MaxValue, 10, 160, 250),
		Color.FromArgb(byte.MaxValue, 250, 140, 150),
		Color.FromArgb(byte.MaxValue, 200, 200, 200),
		Color.FromArgb(byte.MaxValue, 170, 170, 170),
		Color.FromArgb(byte.MaxValue, 160, 160, 160),
		Color.FromArgb(byte.MaxValue, 140, 140, 140),
		Color.FromArgb(byte.MaxValue, 120, 120, 120),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, 150, 150),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, 100, 100),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 0),
		Color.FromArgb(byte.MaxValue, 200, 0, 0),
		Color.FromArgb(byte.MaxValue, 150, 0, 0),
		Color.FromArgb(byte.MaxValue, 150, byte.MaxValue, 150),
		Color.FromArgb(byte.MaxValue, 100, byte.MaxValue, 100),
		Color.FromArgb(byte.MaxValue, 0, byte.MaxValue, 0),
		Color.FromArgb(byte.MaxValue, 0, 200, 0),
		Color.FromArgb(byte.MaxValue, 0, 170, 0),
		Color.FromArgb(byte.MaxValue, 150, 150, byte.MaxValue),
		Color.FromArgb(byte.MaxValue, 100, 100, byte.MaxValue),
		Color.FromArgb(byte.MaxValue, 0, 0, byte.MaxValue),
		Color.FromArgb(byte.MaxValue, 0, 0, 200),
		Color.FromArgb(byte.MaxValue, 0, 0, 150),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 150),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 100),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0),
		Color.FromArgb(byte.MaxValue, 200, 200, 0),
		Color.FromArgb(byte.MaxValue, 180, 180, 0),
		Color.FromArgb(byte.MaxValue, 120, 120, 120),
		Color.FromArgb(byte.MaxValue, 60, 80, 100),
		Color.FromArgb(byte.MaxValue, 250, 170, 120),
		Color.FromArgb(byte.MaxValue, 0, 0, 100),
		Color.FromArgb(byte.MaxValue, 0, 0, 0),
		Color.FromArgb(byte.MaxValue, 60, 80, 100),
		Color.FromArgb(byte.MaxValue, 60, 80, 100),
		Color.FromArgb(byte.MaxValue, 200, 200, 200),
		Color.FromArgb(byte.MaxValue, 100, 100, 100),
		Color.FromArgb(byte.MaxValue, 0, byte.MaxValue, 240),
		Color.FromArgb(byte.MaxValue, 240, 240, 240),
		Color.FromArgb(byte.MaxValue, 0, byte.MaxValue, 240),
		Color.FromArgb(byte.MaxValue, 120, 50, 90),
		Color.FromArgb(byte.MaxValue, 130, 60, 110),
		Color.FromArgb(byte.MaxValue, 140, 70, 130),
		Color.FromArgb(byte.MaxValue, 180, 180, 180),
		Color.FromArgb(byte.MaxValue, 250, 250, 250),
		Color.FromArgb(byte.MaxValue, 220, 220, 220),
		Color.FromArgb(byte.MaxValue, 200, 200, 200),
		Color.FromArgb(byte.MaxValue, 150, 150, 150),
		Color.FromArgb(byte.MaxValue, 120, 120, 120),
		Color.FromArgb(byte.MaxValue, 90, 90, 90),
		Color.FromArgb(byte.MaxValue, 60, 80, 100),
		Color.FromArgb(byte.MaxValue, 150, 150, 150),
		Color.FromArgb(byte.MaxValue, 0, 0, 0),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 160),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 0),
		Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0)
	};

	private readonly IPnIconsService _pnIconsService;

	private readonly IConfigProvider _configProvider;

	private readonly IPnToolTipService _pnToolTipService;

	private readonly ISubMenuConnector _subMenuConnector;

	private readonly QuickLunchManager _quickLunchManager;

	public nint Handle => new WindowInteropHelper(this).Handle;

	public SubMenuWindow(IPnIconsService pnIconsService, IConfigProvider configProvider, IPnToolTipService pnToolTipService, ISubMenuConnector subMenuConnector, QuickLunchManager quickLunchManager)
	{
		_pnIconsService = pnIconsService;
		_configProvider = configProvider;
		_pnToolTipService = pnToolTipService;
		_subMenuConnector = subMenuConnector;
		_quickLunchManager = quickLunchManager;
		_subMenuConnector.OnSetSubMenu += SetSubMenu;
		_subMenuConnector.OnShowWithLocationCheck += ShowWithLocationCheck;
		_subMenuConnector.OnHide += base.Hide;
		InitializeComponent();
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int GetWindowLong(nint hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

	private void CallPn4000Command(PnCommand cmd)
	{
		cmd.Group = Convert.ToInt32(cmd.Command);
		_subMenuConnector.CallPnCommandSubmenuEdition(cmd);
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		nint handle = new WindowInteropHelper(this).Handle;
		SetWindowLong(handle, -16, GetWindowLong(handle, -16) & -524289);
		(PresentationSource.FromVisual(this) as HwndSource).AddHook(WndProc);
	}

	private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (hwnd != Handle)
		{
			return IntPtr.Zero;
		}
		if (msg == 522)
		{
			int delta = WindowEvents.GET_WHEEL_DELTA_WPARAM(wParam);
			if (_quickLunchManager.IsQuickLunchMenuVisible())
			{
				_quickLunchManager.MoueWheelToMenu(delta);
			}
			else
			{
				_subMenuConnector.ToKernelMouseWheel(delta);
			}
			handled = true;
			return IntPtr.Zero;
		}
		return IntPtr.Zero;
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
	}

	private void SubToolbar_Loaded(object sender, RoutedEventArgs e)
	{
		ToolBar toolBar = sender as ToolBar;
		if (toolBar.Template.FindName("OverflowGrid", toolBar) is FrameworkElement frameworkElement)
		{
			frameworkElement.Visibility = Visibility.Collapsed;
		}
	}

	private void SetSubMenu(IMFileExpert expert)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		bool subMenuBigIcons = generalUserSettingsConfig.SubMenuBigIcons;
		bool subMenuHideText = generalUserSettingsConfig.SubMenuHideText;
		bool hideToolTips = generalUserSettingsConfig.HideToolTips;
		if (_oldSubMenuBigIcons == subMenuBigIcons && _oldSubMenuHideText == subMenuHideText && _oldHideToolTips == hideToolTips && CheckForColorUpdateOnly(expert))
		{
			return;
		}
		_oldSubMenuBigIcons = subMenuBigIcons;
		_oldSubMenuHideText = subMenuHideText;
		_oldHideToolTips = hideToolTips;
		foreach (UIElement item in (IEnumerable)SubToolbar.Items)
		{
			_pnIconsService.UnregisterIconData(item);
		}
		SubToolbar.Items.Clear();
		_connection = new Dictionary<object, int>();
		int num = 1;
		double num2 = -1.0;
		foreach (MFileData line in (expert as MFileExpert).lines)
		{
			if (line.PnText == string.Empty)
			{
				SubToolbar.Items.Add(new Separator());
			}
			else
			{
				Button button = new Button();
				Image image = new Image();
				image.BeginInit();
				if (subMenuBigIcons)
				{
					image.Source = _pnIconsService.GetBigIcon(line.PnCommand);
					image.Height = 32.0;
					image.Width = 32.0;
				}
				else
				{
					image.Source = _pnIconsService.GetSmallIcon(line.PnCommand);
					image.Height = 16.0;
					image.Width = 16.0;
				}
				image.EndInit();
				StackPanel stackPanel = new StackPanel();
				TextBlock textBlock = new TextBlock();
				if (subMenuHideText)
				{
					textBlock.Text = "";
					button.Width = 40.0;
					button.HorizontalContentAlignment = HorizontalAlignment.Center;
				}
				else
				{
					textBlock.Text = line.PnText;
					button.HorizontalContentAlignment = HorizontalAlignment.Left;
				}
				textBlock.VerticalAlignment = VerticalAlignment.Center;
				textBlock.Padding = new Thickness(2.0);
				stackPanel.Children.Add(image);
				stackPanel.Children.Add(textBlock);
				stackPanel.Orientation = Orientation.Horizontal;
				stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
				stackPanel.Measure(new Size(double.MaxValue, double.MaxValue));
				if (stackPanel.DesiredSize.Width > num2)
				{
					num2 = stackPanel.DesiredSize.Width;
				}
				button.Content = stackPanel;
				button.Style = (Style)button.TryFindResource("SubMenuButtonStyle");
				if (!hideToolTips)
				{
					_pnToolTipService.SetTooltip(button, new PnCommand(1, line.PnCommand, line.PnText, line.PnTextId, line.PnTooltipId));
				}
				if (line.PnColorIndex != 0)
				{
					button.Background = new SolidColorBrush(_pncolor[line.PnColorIndex]);
				}
				button.Click += btn_Click;
				SubToolbar.Items.Add(button);
				_connection.Add(button, num);
			}
			num++;
		}
		if (subMenuHideText)
		{
			return;
		}
		foreach (object item2 in (IEnumerable)SubToolbar.Items)
		{
			if (item2 is Button)
			{
				((Button)item2).Width = num2 + 6.0;
			}
		}
	}

	private bool CheckForColorUpdateOnly(IMFileExpert expert)
	{
		if (_lastExpert == null)
		{
			CopyExpert(expert);
			return false;
		}
		if (_lastExpert.lines.Count != (expert as MFileExpert).lines.Count)
		{
			CopyExpert(expert);
			return false;
		}
		int num = 0;
		foreach (MFileData line in (expert as MFileExpert).lines)
		{
			MFileData mFileData = _lastExpert.lines[num];
			if (mFileData.PnGroup != line.PnGroup || mFileData.PnCommand != line.PnCommand || mFileData.PnText != line.PnText)
			{
				CopyExpert(expert);
				return false;
			}
			num++;
		}
		num = 0;
		foreach (MFileData line2 in (expert as MFileExpert).lines)
		{
			if (line2.PnText != string.Empty)
			{
				Button button = (Button)SubToolbar.Items[num];
				if (line2.PnColorIndex != 0)
				{
					button.Background = new SolidColorBrush(_pncolor[line2.PnColorIndex]);
				}
				else
				{
					button.Background = new SolidColorBrush(Colors.Transparent);
				}
			}
			num++;
		}
		CopyExpert(expert);
		return true;
	}

	private void CopyExpert(IMFileExpert expert)
	{
		_lastExpert = new MFileExpert();
		foreach (MFileData line in (expert as MFileExpert).lines)
		{
			_lastExpert.lines.Add(line);
		}
	}

	private void btn_Click(object sender, RoutedEventArgs e)
	{
		ClickSubMenu(sender);
	}

	private void ClickSubMenu(object sender)
	{
		if (_connection == null)
		{
			MessageBox.Show("SubMenu Error 1");
		}
		else if (sender == null)
		{
			MessageBox.Show("SubMenu Error  2");
		}
		else if (_connection.ContainsKey(sender))
		{
			try
			{
				CallPn4000Command(new PnCommand(25, _connection[sender].ToString(), "sub", 0, 0));
			}
			catch (Exception ex)
			{
				MessageBox.Show("SubMenu Error 4:" + ex.Message);
			}
		}
	}

	internal void ShowWithLocationCheck()
	{
		if (base.IsVisible)
		{
			return;
		}
		if (FirstShow)
		{
			GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
			FirstShow = false;
			if (generalUserSettingsConfig.SubWindowPosX < 0 || generalUserSettingsConfig.SubWindowPosY < 0 || (double)generalUserSettingsConfig.SubWindowPosY > SystemParameters.PrimaryScreenHeight)
			{
				base.Top = 170.0;
				base.Left = (int)SystemParameters.PrimaryScreenWidth - 200;
			}
			else
			{
				base.Left = generalUserSettingsConfig.SubWindowPosX;
				base.Top = generalUserSettingsConfig.SubWindowPosY;
			}
		}
		Show();
	}
}
