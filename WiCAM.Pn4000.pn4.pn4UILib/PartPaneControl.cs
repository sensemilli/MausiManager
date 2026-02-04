using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.pn4Services.CADGEO;
using WiCAM.Pn4000.Screen;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public partial class PartPaneControl : System.Windows.Controls.UserControl, IComponentConnector, IStyleConnector
{
	public class NestingPart : INotifyPropertyChanged
	{
		private int _color;

		private PN4000_2D_Database _db;

		private PnImageSource _imageSource;

		private string _label1;

		private SolidColorBrush _label1Color;

		private string _label2;

		private string _label3;

		private int _lastMaxHeight;

		private int _lastMaxWidth;

		private float _lastScale;

		private string _name;

		public int ImgHeight;

		public int ImgWidth;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
				NotifyPropertyChanged("Name");
			}
		}

		public string Label1
		{
			get
			{
				return _label1;
			}
			set
			{
				_label1 = value;
				NotifyPropertyChanged("Label1");
			}
		}

		public string Label2
		{
			get
			{
				return _label2;
			}
			set
			{
				_label2 = value;
				NotifyPropertyChanged("Label2");
			}
		}

		public string Label3
		{
			get
			{
				return _label3;
			}
			set
			{
				_label3 = value;
				NotifyPropertyChanged("Label3");
			}
		}

		public ImageSource Image { get; set; }

		public List<string> ToolTipText { get; set; }

		public SolidColorBrush Label1Color
		{
			get
			{
				return _label1Color;
			}
			set
			{
				_label1Color = value;
				NotifyPropertyChanged("Label1Color");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public NestingPart(string path, int color, float scale, int maxWidth, int maxHeight, int screenWidth, int screenHeight, IConfigProvider configProvider, ILogCenterService logCenterService, IPnColorsService pnColorsService)
		{
			Name = path;
			_color = color;
			UpdateImage(scale, maxWidth, maxHeight, screenWidth, screenHeight, configProvider, logCenterService, pnColorsService);
			ToolTipText = new List<string>();
		}

		public void NotifyPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public ImageSource GetImage(float scale, ref int w, ref int h, int background, int screenWidth, int screenHeight, ILogCenterService logCenterService, IConfigProvider configProvider, IPnColorsService colorsService)
		{
			if (_db == null)
			{
				_db = new PN4000_2D_Database(logCenterService, Name, 0);
			}
			if (_imageSource == null)
			{
				_imageSource = new PnImageSource(logCenterService, colorsService, configProvider);
			}
			return _imageSource.GetCadGeoImageSourceScaled(_db, ref w, ref h, background, scale, null, screenWidth, screenHeight, _color);
		}

		public void UpdateImage(float scale, int maxWidth, int maxHeight, int screenWidth, int screenHeight, IConfigProvider configProvider, ILogCenterService logCenterService, IPnColorsService pnColorsService)
		{
			if (_lastScale != scale || _lastMaxWidth != maxWidth || _lastMaxHeight != maxHeight)
			{
				_lastScale = scale;
				_lastMaxWidth = maxWidth;
				_lastMaxHeight = maxHeight;
				if (_db == null)
				{
					_db = new PN4000_2D_Database(logCenterService, Name, 0);
				}
				if (_imageSource == null)
				{
					_imageSource = new PnImageSource(logCenterService, pnColorsService, configProvider);
				}
				ImgWidth = maxWidth;
				ImgHeight = maxHeight;
				if (configProvider.InjectOrCreate<GeneralUserSettingsConfig>().PartPaneScaleType == 0)
				{
					Image = _imageSource.GetCadGeoImageSourceScaled(_db, ref ImgWidth, ref ImgHeight, -2, scale, Image, screenWidth, screenHeight, _color);
				}
				else
				{
					int width = (int)(50f * (float)Math.Pow(2.0, scale));
					int height = (int)(30f * (float)Math.Pow(2.0, scale));
					Image = _imageSource.GetCadGeoImage(_db, width, height, -2, _color);
				}
				NotifyPropertyChanged("Image");
			}
		}
	}

	private const double MaxPartpaneWidth = 500.0;

	private const double MinPartpaneWidth = 115.0;

	private DragDropWindow _ddw;

	private bool _disableAll;

	private DispatcherTimer _dispatcherTimer;

	private Grid _gridm;

	private float _initialScalePartpane = 1f;

	private bool _isMdragdrop;

	private ILogCenterService _logCenterService;

	private MainWindow _mainWindow;

	private ObservableCollection<NestingPart> _nestingpartsData = new ObservableCollection<NestingPart>();

	private Image _partpane1;

	private Image _partpane2;

	private GridSplitter _partPaneGridSplitter;

	private IPKernelFlowGlobalDataService _pKernelFlowGlobalData;

	private IConfigProvider _configProvider;

	private IPnIconsService _pnIconsService;

	private IPnColorsService _pnColorsService;

	private ContextMenu _ppContextmenu1;

	private ContextMenu _ppContextmenu2;

	private List<object> _removedItems = new List<object>();

	private IScreen2D _screen;

	private object _svActive;

	private PartPaneWindow _window;

	public bool IsDragDropToScreen;

	private ExeFlow _exeFlow;

	public ObservableCollection<NestingPart> NestingPartsData => _nestingpartsData;

	public PartPaneControl()
	{
		base.DataContext = this;
		InitializeComponent();
	}

	private void PartPane_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
		{
			return;
		}
		System.Windows.Controls.ListBox listBox = sender as System.Windows.Controls.ListBox;
		if (e.RemovedItems.Count <= 0 || _removedItems.Contains(e.RemovedItems[0]))
		{
			return;
		}
		foreach (object removedItem in e.RemovedItems)
		{
			listBox.SelectedItems.Add(removedItem);
			_removedItems.Add(removedItem);
		}
	}

	private void PartPane_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (_removedItems.Count <= 0)
		{
			return;
		}
		System.Windows.Controls.ListBox listBox = sender as System.Windows.Controls.ListBox;
		foreach (object removedItem in _removedItems)
		{
			listBox.SelectedItems.Remove(removedItem);
		}
		_removedItems.Clear();
	}

	private void UserControl_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if (IsDragDropToScreen)
		{
			e.Handled = true;
		}
	}

	private void sv_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if (_disableAll)
		{
			return;
		}
		ListBoxItem obj = sender as ListBoxItem;
		if (obj != null && e.LeftButton == MouseButtonState.Pressed)
		{
			InitDragWindow();
			if (_ddw != null)
			{
				if (PartPane.SelectedItems.Count > 0)
				{
					int w = int.MaxValue;
					int h = int.MaxValue;
					ImageSource image = _nestingpartsData[PartPane.Items.IndexOf(PartPane.SelectedItems[0])].GetImage(GetCurrentScale(), ref w, ref h, -2, _screen.Width(), _screen.Height(), _logCenterService, _configProvider, _pnColorsService);
					_ddw.Width = w;
					_ddw.Height = h;
					_ddw.SetImage(image);
					if (PartPane.SelectedItems.Count > 1)
					{
						_ddw.SetText($"+{PartPane.SelectedItems.Count}");
					}
					else
					{
						_ddw.SetText(string.Empty);
					}
				}
				StartDragWindow();
				IsDragDropToScreen = true;
			}
		}
		if (obj != null && e.MiddleButton == MouseButtonState.Pressed && !_isMdragdrop)
		{
			InitDragWindow();
			StartDragWindow();
			IsDragDropToScreen = true;
		}
	}

	internal void SetServices(IConfigProvider configProvider, IPnIconsService pnIconsService, ILogCenterService logCenterService, IPnColorsService pnColorsService, ExeFlow exeFlow, MainWindow parent)
	{
		_mainWindow = parent;
		_exeFlow = exeFlow;
		_configProvider = configProvider;
		_pnIconsService = pnIconsService;
		_pnColorsService = pnColorsService;
		_logCenterService = logCenterService;
	}

	internal bool ScreenDragDropAction(bool middle, int x, int y)
	{
		if (!IsDragDropToScreen)
		{
			return false;
		}
		if (_dispatcherTimer != null && _dispatcherTimer.IsEnabled)
		{
			_dispatcherTimer.Stop();
		}
		base.IsHitTestVisible = true;
		int flag = 1;
		if (middle)
		{
			flag = 2;
		}
		_exeFlow.OnDropAction(x, y, flag);
		IsDragDropToScreen = false;
		if (_ddw != null && _ddw.Visibility == Visibility.Visible)
		{
			_ddw.Visibility = Visibility.Hidden;
		}
		return true;
	}

	private Point GetMousePositionAtScreen()
	{
		Tuple<int, int> mousePosition = _screen.GetMousePosition();
		return new Point(mousePosition.Item1, mousePosition.Item2);
	}

	private void dispatcherTimer_Tick(object sender, EventArgs e)
	{
		if (!_dispatcherTimer.IsEnabled)
		{
			return;
		}
		_dispatcherTimer.Stop();
		if (System.Windows.Forms.Control.MouseButtons != MouseButtons.Left && System.Windows.Forms.Control.MouseButtons != MouseButtons.Middle)
		{
			base.IsHitTestVisible = true;
			Point mousePositionAtScreen = GetMousePositionAtScreen();
			if (!(mousePositionAtScreen.X >= 0.0) || !(mousePositionAtScreen.Y >= 0.0) || !(mousePositionAtScreen.X < (double)_screen.Width()) || !(mousePositionAtScreen.Y < (double)_screen.Height()))
			{
				IsDragDropToScreen = false;
			}
			if (_ddw != null && _ddw.Visibility == Visibility.Visible)
			{
				_ddw.Visibility = Visibility.Hidden;
			}
		}
		else
		{
			if (_ddw != null && _ddw.Visibility == Visibility.Visible)
			{
				_ddw.SetDropWindowLocation();
			}
			_dispatcherTimer.Start();
		}
	}

	private void CalculateMaxPartPaneImageWidthHeight(out int w, out int h)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (generalUserSettingsConfig.PartPaneMode == 1)
		{
			if (_window != null)
			{
				w = (int)_window.Width - 50;
				h = (int)_window.Height - 120;
			}
			else
			{
				w = 50;
				h = 50;
			}
		}
		else
		{
			w = _screen.Width() - 30;
			h = generalUserSettingsConfig.PartPaneWidth - 75;
		}
		if (w < 10)
		{
			w = 10;
		}
		if (h < 10)
		{
			h = 10;
		}
	}

	private void PartPane_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (!_disableAll)
		{
			if (_svActive != null)
			{
				_exeFlow.OnDropAction(10, 10, 4);
			}
			else
			{
				_exeFlow.OnDropAction(10, 10, 5);
			}
		}
	}

	private void PartPane_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (_gridm != null)
		{
			GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
			if (generalUserSettingsConfig.PartPaneMode == 0 && _gridm.RowDefinitions[1].ActualHeight > 0.0)
			{
				generalUserSettingsConfig.PartPaneWidth = (int)_gridm.RowDefinitions[1].ActualHeight;
			}
			UpdateAllPartPaneImageWithScale(GetCurrentPartPaneZoom());
		}
	}

	private void UpdateAllPartPaneImageWithScale(float scale)
	{
		CalculateMaxPartPaneImageWidthHeight(out var w, out var h);
		foreach (NestingPart nestingpartsDatum in _nestingpartsData)
		{
			nestingpartsDatum.UpdateImage(scale, w, h, _screen.Width(), _screen.Height(), _configProvider, _logCenterService, _pnColorsService);
		}
	}

	private float GetCurrentPartPaneZoom()
	{
		if (_configProvider.InjectOrCreate<GeneralUserSettingsConfig>().PartPaneScaleType == 1)
		{
			return (float)PartPaneZoomSlider.Value;
		}
		return _initialScalePartpane * 0.25f * (float)Math.Pow(2.0, PartPaneZoomSlider.Value);
	}

	private void PartPaneZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (PartPane != null)
		{
			UpdateAllPartPaneImageWithScale(GetCurrentPartPaneZoom());
		}
	}

	private void PartPane_ContextMenuOpening(object sender, ContextMenuEventArgs e)
	{
		if (_disableAll)
		{
			e.Handled = true;
		}
		else if (_svActive != null)
		{
			PartPane.ContextMenu = _ppContextmenu1;
		}
		else
		{
			PartPane.ContextMenu = _ppContextmenu2;
		}
	}

	private void sv_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		_svActive = sender;
	}

	private void sv_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		_svActive = null;
	}

	private void ShowPartPaneAndComponents()
	{
		if (base.Visibility != 0)
		{
			GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
			PartPaneZoomSlider.Value = generalUserSettingsConfig.PartPaneScale;
			base.Visibility = Visibility.Visible;
			if (generalUserSettingsConfig.PartPaneMode == 1)
			{
				ShowFlyPartPane();
				return;
			}
			PartPaneButtonFly.Content = _partpane2;
			_partPaneGridSplitter.Visibility = Visibility.Visible;
			_gridm.RowDefinitions[1].MinHeight = 115.0;
			_gridm.RowDefinitions[1].MaxHeight = 500.0;
			_gridm.RowDefinitions[1].Height = new GridLength(generalUserSettingsConfig.PartPaneWidth);
			_gridm.RowDefinitions[2].Height = new GridLength(4.0);
		}
	}

	private void HidePartPaneAndComponents()
	{
		base.Visibility = Visibility.Collapsed;
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (_window != null)
		{
			generalUserSettingsConfig.PartPaneWindowWidth = (int)_window.Width;
			generalUserSettingsConfig.PartPaneWindowHeight = (int)_window.Height;
			generalUserSettingsConfig.PartPaneWindowX = (int)_window.Left;
			generalUserSettingsConfig.PartPaneWindowY = (int)_window.Top;
		}
		if (_gridm.RowDefinitions[1].ActualHeight > 115.0)
		{
			generalUserSettingsConfig.PartPaneWidth = (int)_gridm.RowDefinitions[1].ActualHeight;
		}
		generalUserSettingsConfig.PartPaneScale = (int)PartPaneZoomSlider.Value;
		if (generalUserSettingsConfig.PartPaneMode == 1)
		{
			if (_window != null)
			{
				_window.Hide();
			}
		}
		else
		{
			_partPaneGridSplitter.Visibility = Visibility.Collapsed;
			_gridm.RowDefinitions[1].MinHeight = 0.0;
			_gridm.RowDefinitions[1].MaxHeight = 0.0;
			_gridm.RowDefinitions[1].Height = GridLength.Auto;
			_gridm.RowDefinitions[2].Height = GridLength.Auto;
		}
	}

	private float GetCurrentScale()
	{
		try
		{
			float drawScale = Geo2DAdapter.DrawScale;
			double m = PresentationSource.FromVisual(_mainWindow).CompositionTarget.TransformToDevice.M11;
			drawScale /= (float)m;
			if (drawScale == 0f)
			{
				drawScale = 1f;
			}
			return drawScale;
		}
		catch
		{
			return 0.5f;
		}
	}

	private void GetInitialScale()
	{
		_initialScalePartpane = GetCurrentScale();
	}

	public string OnPartPane(string order)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		switch (order)
		{
		case "PARTPANE_SHOW":
			ShowPartPaneAndComponents();
			_screen.SetAllowDrop(allow: true);
			if (generalUserSettingsConfig.PartPaneContentOrientation)
			{
				PartPane.Style = (Style)PartPane.TryFindResource("PartPaneHorizontal");
			}
			else
			{
				PartPane.Style = (Style)PartPane.TryFindResource("PartPaneVertical");
			}
			return "1";
		case "PARTPANE_HIDE":
			HidePartPaneAndComponents();
			_screen.SetAllowDrop(allow: false);
			_initialScalePartpane = 1f;
			return "1";
		case "PARTPANE_UNBLOCK":
			_disableAll = false;
			return "1";
		default:
			if (order.Contains("DRAGBACK") && !order.Contains("STOPDRAGBACK"))
			{
				string[] array = order.Split(' ');
				string text = string.Empty;
				for (int i = 4; i < array.Count(); i++)
				{
					text += array[i];
					if (i < array.Count())
					{
						text += " ";
					}
				}
				PnImageSource pnImageSource = new PnImageSource(_logCenterService, _pnColorsService, _configProvider);
				InitDragWindow();
				int width = int.MaxValue;
				int height = int.MaxValue;
				PN4000_2D_Database db = new PN4000_2D_Database(_logCenterService, text, 0);
				ImageSource cadGeoImageSourceScaled = pnImageSource.GetCadGeoImageSourceScaled(db, ref width, ref height, -2, GetCurrentScale(), null, _screen.Width(), _screen.Height(), 0);
				_ddw.Width = width;
				_ddw.Height = height;
				_ddw.SetImage(cadGeoImageSourceScaled);
				StartDragWindow();
			}
			if (order.Contains("STOPDRAGBACK"))
			{
				StopDragWindow();
			}
			if (order == "PARTPANE_BLOCK")
			{
				_disableAll = true;
				return "1";
			}
			if (order.Length > 12 && order.Substring(0, 12) == "PARTPANE_ADD")
			{
				if (_initialScalePartpane == 1f)
				{
					GetInitialScale();
				}
				CalculateMaxPartPaneImageWidthHeight(out var w, out var h);
				int num = order.LastIndexOf(' ');
				int color = Convert.ToInt32(order.Substring(num + 1));
				string path = order.Substring(13, num - 13).Trim();
				float currentPartPaneZoom = GetCurrentPartPaneZoom();
				NestingPart item = new NestingPart(path, color, currentPartPaneZoom, w, h, _screen.Width(), _screen.Height(), _configProvider, _logCenterService, _pnColorsService);
				_nestingpartsData.Add(item);
				return "1";
			}
			if (order == "PARTPANE_CLEAR")
			{
				_nestingpartsData.Clear();
				return "1";
			}
			if (order.Contains("PARTPANE_SETTOPINDEX"))
			{
				try
				{
					int index = Convert.ToInt32(order.Split(' ')[1]) - 1;
					PartPane.ScrollIntoView(_nestingpartsData[index]);
				}
				catch (Exception e)
				{
					_logCenterService.CatchRaport(e);
				}
				return "1";
			}
			if (order.Contains("PARTPANE_GETSEL"))
			{
				try
				{
					int num2 = Convert.ToInt32(order.Split(' ')[1]) - 1;
					if (num2 < _nestingpartsData.Count)
					{
						if (PartPane.SelectedItems.Contains(_nestingpartsData[num2]))
						{
							return "1";
						}
						return "0";
					}
					return "0";
				}
				catch (Exception e2)
				{
					_logCenterService.CatchRaport(e2);
				}
				return "0";
			}
			if (order.Contains("PARTPANE_SEL"))
			{
				try
				{
					int index2 = Convert.ToInt32(order.Split(' ')[1]) - 1;
					if (!PartPane.SelectedItems.Contains(_nestingpartsData[index2]))
					{
						PartPane.SelectedItems.Add(_nestingpartsData[index2]);
					}
				}
				catch (Exception e3)
				{
					_logCenterService.CatchRaport(e3);
				}
				return "0";
			}
			if (order.Contains("PARTPANE_DESEL"))
			{
				try
				{
					int num3 = Convert.ToInt32(order.Split(' ')[1]) - 1;
					if (num3 < _nestingpartsData.Count && PartPane.SelectedItems.Contains(_nestingpartsData[num3]))
					{
						PartPane.SelectedItems.Remove(_nestingpartsData[num3]);
					}
				}
				catch (Exception e4)
				{
					_logCenterService.CatchRaport(e4);
				}
				return "0";
			}
			if (order.Contains("PARTPANE_SETTEXT"))
			{
				string[] array2 = order.Split(' ');
				if (array2.Length < 4)
				{
					return "0";
				}
				try
				{
					int num4 = Convert.ToInt32(array2[1]) - 1;
					int num5 = Convert.ToInt32(array2[2]);
					int num6 = Convert.ToInt32(array2[3]);
					StringBuilder stringBuilder = new StringBuilder();
					for (int j = 4; j < array2.Length; j++)
					{
						stringBuilder.AppendFormat("{0} ", array2[j]);
					}
					string text2 = stringBuilder.ToString().Trim();
					if (num4 < _nestingpartsData.Count)
					{
						switch (num5)
						{
						case 1:
							_nestingpartsData[num4].Label1 = text2;
							switch (num6)
							{
							case 2:
								_nestingpartsData[num4].Label1Color = new SolidColorBrush(Colors.Red);
								break;
							case 4:
								_nestingpartsData[num4].Label1Color = new SolidColorBrush(Colors.Blue);
								break;
							default:
								_nestingpartsData[num4].Label1Color = new SolidColorBrush(Colors.Black);
								break;
							}
							break;
						case 2:
							_nestingpartsData[num4].Label2 = text2;
							break;
						case 3:
							_nestingpartsData[num4].Label3 = text2;
							break;
						}
						if (num5 >= 4)
						{
							while (_nestingpartsData[num4].ToolTipText.Count < num5 - 3)
							{
								_nestingpartsData[num4].ToolTipText.Add(" ");
							}
							_nestingpartsData[num4].ToolTipText[num5 - 4] = text2;
							_nestingpartsData[num4].NotifyPropertyChanged("ToolTipText");
						}
					}
				}
				catch (Exception e5)
				{
					_logCenterService.CatchRaport(e5);
				}
				return "1";
			}
			if (order.Contains("PARTPANE_CONTEXTMENUCLEAR"))
			{
				_ppContextmenu1.Items.Clear();
				_ppContextmenu2.Items.Clear();
				return "1";
			}
			if (order.Contains("PARTPANE_CONTEXTMENUADD1"))
			{
				AddContextMenuItem(_ppContextmenu1, order);
				return "1";
			}
			if (order.Contains("PARTPANE_CONTEXTMENUADD2"))
			{
				AddContextMenuItem(_ppContextmenu2, order);
				return "1";
			}
			return "1";
		}
	}

	private void StopDragWindow()
	{
		if (_ddw != null)
		{
			_ddw.SetImage(null);
			_ddw.Hide();
		}
	}

	private void InitDragWindow()
	{
		StopDragWindow();
		if (_ddw == null)
		{
			_ddw = new DragDropWindow(_mainWindow);
			_ddw.ShowActivated = false;
		}
	}

	private void StartDragWindow()
	{
		_ddw.SetDropWindowLocation();
		_ddw.Show();
		base.IsHitTestVisible = false;
		PostUserEvent();
	}

	private void PostUserEvent()
	{
		WindowEvents.PostMessage(_mainWindow.Handle, 1024, new IntPtr(456), IntPtr.Zero);
	}

	public void UserEvent()
	{
		User32Wrap.PumpMesseges();
		if (System.Windows.Forms.Control.MouseButtons != MouseButtons.Left && System.Windows.Forms.Control.MouseButtons != MouseButtons.Middle)
		{
			base.IsHitTestVisible = true;
			Point mousePositionAtScreen = GetMousePositionAtScreen();
			if (!(mousePositionAtScreen.X >= 0.0) || !(mousePositionAtScreen.Y >= 0.0) || !(mousePositionAtScreen.X < (double)_screen.Width()) || !(mousePositionAtScreen.Y < (double)_screen.Height()))
			{
				IsDragDropToScreen = false;
			}
			if (_ddw != null && _ddw.Visibility == Visibility.Visible)
			{
				_ddw.Visibility = Visibility.Hidden;
				if (IsDragDropToScreen)
				{
					ScreenDragDropAction(System.Windows.Forms.Control.MouseButtons == MouseButtons.Middle, (int)mousePositionAtScreen.X, (int)mousePositionAtScreen.Y);
				}
			}
		}
		else
		{
			if (_ddw != null && _ddw.Visibility == Visibility.Visible)
			{
				_ddw.SetDropWindowLocation();
			}
			PostUserEvent();
		}
	}

	private void AddContextMenuItem(ContextMenu menu, string order)
	{
		try
		{
			string text = order.Substring(24).Trim();
			if (text == "|")
			{
				menu.Items.Add(new Separator());
				return;
			}
			string[] array = text.Split('|');
			MenuItem menuItem = new MenuItem();
			menuItem.Header = array[1];
			Image image = new Image();
			image.Source = _pnIconsService.GetSmallIcon(array[0]);
			menuItem.Icon = image;
			menuItem.Click += part_pane_menu_item_Click;
			menu.Items.Add(menuItem);
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	private void part_pane_menu_item_Click(object sender, RoutedEventArgs e)
	{
		if (!_disableAll)
		{
			if (_ppContextmenu1.Items.Contains(sender))
			{
				int num = _ppContextmenu1.Items.IndexOf(sender);
				_exeFlow.OnDropAction(0, 0, num + 100);
			}
			else if (_ppContextmenu2.Items.Contains(sender))
			{
				int num2 = _ppContextmenu2.Items.IndexOf(sender);
				_exeFlow.OnDropAction(0, 0, num2 + 150);
			}
		}
	}

	private void sv_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
	{
		e.Handled = true;
	}

	private void ShowFlyPartPane()
	{
		PartPaneButtonFly.Content = _partpane1;
		if (_window == null)
		{
			_window = new PartPaneWindow();
			_window.Owner = _mainWindow;
			GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
			if (generalUserSettingsConfig.PartPaneWindowX < 0)
			{
				_window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				_window.Width = _screen.Width() - 20;
				_window.Height = 400.0;
			}
			else
			{
				CorrectLocation();
				_window.WindowStartupLocation = WindowStartupLocation.Manual;
				_window.Width = generalUserSettingsConfig.PartPaneWindowWidth;
				_window.Height = generalUserSettingsConfig.PartPaneWindowHeight;
				_window.Left = generalUserSettingsConfig.PartPaneWindowX;
				_window.Top = generalUserSettingsConfig.PartPaneWindowY;
			}
		}
		if (_gridm.Children.Contains(this))
		{
			_gridm.Children.Remove(this);
			_window.gridm.Children.Add(this);
		}
		base.Visibility = Visibility.Visible;
		_window.Show();
	}

	private void CorrectLocation()
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if ((double)(generalUserSettingsConfig.PartPaneWindowY + generalUserSettingsConfig.PartPaneWindowHeight / 2) > SystemParameters.VirtualScreenHeight)
		{
			generalUserSettingsConfig.PartPaneWindowY = (int)SystemParameters.VirtualScreenHeight - generalUserSettingsConfig.PartPaneWindowHeight;
		}
		if ((double)(generalUserSettingsConfig.PartPaneWindowX + generalUserSettingsConfig.PartPaneWindowWidth / 2) > SystemParameters.VirtualScreenWidth)
		{
			generalUserSettingsConfig.PartPaneWindowX = (int)SystemParameters.VirtualScreenWidth - generalUserSettingsConfig.PartPaneWindowWidth;
		}
		if (generalUserSettingsConfig.PartPaneWindowY < 0)
		{
			generalUserSettingsConfig.PartPaneWindowY = 0;
		}
		if (generalUserSettingsConfig.PartPaneWindowX < 0)
		{
			generalUserSettingsConfig.PartPaneWindowX = 0;
		}
		_configProvider.Push(generalUserSettingsConfig);
		_configProvider.Save<GeneralUserSettingsConfig>();
	}

	private void PartPaneButtonFly_Click(object sender, RoutedEventArgs e)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (generalUserSettingsConfig.PartPaneMode == 0)
		{
			HidePartPaneAndComponents();
			generalUserSettingsConfig.PartPaneMode = 1;
			_configProvider.Push(generalUserSettingsConfig);
			_configProvider.Save<GeneralUserSettingsConfig>();
			ShowFlyPartPane();
		}
		else
		{
			_window.Hide();
			base.Visibility = Visibility.Hidden;
			_window.gridm.Children.Remove(this);
			_gridm.Children.Add(this);
			generalUserSettingsConfig.PartPaneMode = 0;
			_configProvider.Push(generalUserSettingsConfig);
			_configProvider.Save<GeneralUserSettingsConfig>();
			ShowPartPaneAndComponents();
		}
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
	}

	internal void SetUp(GridSplitter partPaneGridSplitter, Grid gridm, IScreen2D screen2d)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		_screen = screen2d;
		_partPaneGridSplitter = partPaneGridSplitter;
		_gridm = gridm;
		if (generalUserSettingsConfig.PartPaneWidth == 0)
		{
			generalUserSettingsConfig.PartPaneWidth = 250;
		}
		_ppContextmenu1 = new ContextMenu();
		_ppContextmenu2 = new ContextMenu();
		PartPane.ContextMenu = _ppContextmenu1;
		PartPane.ContextMenuOpening += PartPane_ContextMenuOpening;
		PartPaneZoomSlider.Value = generalUserSettingsConfig.PartPaneScale;
		HidePartPaneAndComponents();
		base.DataContext = this;
		_partpane1 = new Image();
		_partpane1.BeginInit();
		_partpane1.Source = _pnIconsService.GetSmallIcon("partpane1");
		_partpane1.EndInit();
		_partpane1.SnapsToDevicePixels = true;
		_partpane1.Stretch = Stretch.None;
		_partpane2 = new Image();
		_partpane2.BeginInit();
		_partpane2.Source = _pnIconsService.GetSmallIcon("partpane2");
		_partpane2.EndInit();
		_partpane2.SnapsToDevicePixels = true;
		_partpane2.Stretch = Stretch.None;
		base.SnapsToDevicePixels = true;
	}
}
