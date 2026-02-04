using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using pncommon.WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public partial class CenterMenu : Window, IComponentConnector
{
	private bool _blockEdit = true;

	private LikeModalMode _likeModalMode;

	private ILanguageDictionary _languageDictionary;

	private IPKernelFlowGlobalDataService _pKernelFlowGlobalData;

	private MainWindow _mainWindow;

	private IPnToolTipService _pnToolTipService;

	private IConfigProvider _configProvider;

	private IPnPathService _pnPathService;

	private IPnIconsService _pnIconsService;

	private ILogCenterService _logCenterService;

	private int _xNumber;

	private int _yNumber;

	private int _xCellSize;

	private int _yCellSize;

	private int _btnMargin;

	private Dictionary<Button, IPnCommand> _buttons;

	private Image _padlock1;

	private Image _padlock2;

	private Image _close;

	private Button _candidateToMove;

	private Button _slidingButton;

	private Point _startMovePoint;

	private int _lastLocationX = -1;

	private int _lastLocationY = -1;

	private const int AniTime = 150;

	private const double GrowTo = 1.1;

	private const double DeclineTo = 0.9;

	private int _oldCenterMenuWidth = -1;

	private int _oldCenterMenuHeight = -1;

	private int _oldIconsize = -1;

	private int _oldTextlocation = -1;

	private string _moduleName;

	private int _currTab;

	private TextBox _tbedit;

	private int _lastEditId = -1;

	public bool FinalClosePermition { get; set; }

	public CenterMenu(IPnToolTipService pnToolTipService, ILogCenterService logCenterService, IPnIconsService pnIconsService, IPnPathService pnPathService, IPKernelFlowGlobalDataService pKernelFlowGlobalData, IConfigProvider configProvider)
	{
		_pnToolTipService = pnToolTipService;
		_logCenterService = logCenterService;
		_pnIconsService = pnIconsService;
		_pnPathService = pnPathService;
		_pKernelFlowGlobalData = pKernelFlowGlobalData;
		_configProvider = configProvider;
		_mainWindow = _pKernelFlowGlobalData.MainWindow as MainWindow;
		//_likeModalMode = _mainWindow.LikeModalMode;
		//_languageDictionary = _mainWindow.LanguageDictionary;
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		StoreLocallyCurrentConfiguration();
		if ((double)generalUserSettingsConfig.CenterMenuWidth < base.MinWidth || (double)generalUserSettingsConfig.CenterMenuHeight < base.MinHeight)
		{
			CalculateSizeBaseOnCurrentConfiguration();
		}
		else
		{
			_oldCenterMenuWidth = generalUserSettingsConfig.CenterMenuWidth;
			_oldCenterMenuHeight = generalUserSettingsConfig.CenterMenuHeight;
		}
		FinalClosePermition = false;
		InitializeComponent();
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (base.Visibility == Visibility.Visible)
		{
			_blockEdit = true;
			SetBlockEditView();
			Hide();
			_likeModalMode.EndMode();
			SaveCurrentTab();
			CloseEditTab();
		}
		if (!FinalClosePermition)
		{
			e.Cancel = true;
		}
	}

	private Point GetM11(Point p)
	{
		return GetM11((int)p.X, (int)p.Y);
	}

	private Point GetM11(int x, int y)
	{
		Matrix transformToDevice = PresentationSource.FromVisual(_mainWindow).CompositionTarget.TransformToDevice;
		double m = transformToDevice.M11;
		double m2 = transformToDevice.M22;
		return new Point((double)x / m, (double)y / m2);
	}

	public void OnCenterMenuShow(int x, int y, string filename, bool reload)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (!generalUserSettingsConfig.CenterMenuDeactivate)
		{
			AnalizeConfigurationModification(reload);
		//	Point m = GetM11(_mainWindow.KernelScreenPanel.PointToScreen(GetM11(x, y)));
			if (generalUserSettingsConfig.CenterMenuLocateLikeContext)
			{
			//	base.Left = m.X + 5.0;
			//	base.Top = m.Y + 5.0;
			}
			else
			{
				//SetCenterButtonLocation(m);
			}
			Show();
			Activate();
			_likeModalMode.SetMode(EndLikeModalMode);
		}
	}

	private bool EndLikeModalMode()
	{
		if (base.Visibility == Visibility.Visible)
		{
			Close();
		}
		return true;
	}

	private void button1_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void Window_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.RightButton == MouseButtonState.Pressed)
		{
			Close();
		}
	}

	public void SetCenterMenu()
	{
		_btnMargin = 0;
		_buttons = new Dictionary<Button, IPnCommand>();
		SetButtonsLocation();
	}

	private void SetButtonsLocation()
	{
		_xCellSize = (int)(grid0.ActualWidth / (double)_xNumber);
		_yCellSize = (int)(row1.ActualHeight / (double)_yNumber);
		foreach (Button key in _buttons.Keys)
		{
			IPnCommand pnCommand = _buttons[key];
			Rect rect = CalculateButtonGeometry(pnCommand.AddValue1, pnCommand.AddValue2);
			key.Width = rect.Width - (double)(2 * _btnMargin);
			key.Height = rect.Height - (double)(2 * _btnMargin);
			Canvas.SetLeft(key, rect.Left + (double)_btnMargin);
			Canvas.SetTop(key, rect.Top + (double)_btnMargin);
			key.Visibility = Visibility.Visible;
		}
	}

	private Rect CalculateButtonGeometry(int x, int y)
	{
		return new Rect(x * _xCellSize, y * _yCellSize, _xCellSize, _yCellSize);
	}

	public double YCenterModification()
	{
		if (row0 != null)
		{
			return row0.ActualHeight;
		}
		return 25.0;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		base.Width = _oldCenterMenuWidth;
		base.Height = _oldCenterMenuHeight;
		SetCenterMenu();
		_padlock1 = new Image();
		_padlock1.BeginInit();
		_padlock1.Source = _pnIconsService.GetSmallIcon("padlock1");
		_padlock1.EndInit();
		_padlock1.SnapsToDevicePixels = true;
		_padlock1.Stretch = Stretch.None;
		_padlock2 = new Image();
		_padlock2.BeginInit();
		_padlock2.Source = _pnIconsService.GetSmallIcon("padlock2");
		_padlock2.EndInit();
		_padlock2.SnapsToDevicePixels = true;
		_padlock2.Stretch = Stretch.None;
		_close = new Image();
		_close.BeginInit();
		_close.Source = _pnIconsService.GetSmallIcon("close");
		_close.EndInit();
		_close.SnapsToDevicePixels = true;
		_close.Stretch = Stretch.None;
		button1.Content = _close;
		SetBlockEditView();
	}

	private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (_buttons != null)
		{
			SetButtonsLocation();
		}
		double width = (base.Width - 70.0) / 5.0;
		tabItem1.Width = width;
		tabItem2.Width = width;
		tabItem3.Width = width;
		tabItem4.Width = width;
		tabItem5.Width = width;
	}

	private void btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_candidateToMove = (Button)sender;
		_startMovePoint = e.GetPosition(_candidateToMove);
	}

	private void btn_Click(object sender, RoutedEventArgs e)
	{
		if (_slidingButton == (Button)sender)
		{
			if (_lastLocationX >= 0 && _lastLocationY >= 0 && _lastLocationX < _xNumber && _lastLocationY < _yNumber)
			{
				IPnCommand pnCommand = _buttons[_slidingButton];
				CheckRunOutButton(_lastLocationX, _lastLocationY, pnCommand.AddValue1, pnCommand.AddValue2);
				pnCommand.AddValue1 = _lastLocationX;
				pnCommand.AddValue2 = _lastLocationY;
				_lastLocationX = -1;
				SetNewButtonLocation(_slidingButton);
			}
			else
			{
				_pnIconsService.UnregisterIconData(_slidingButton);
				canvas1.Children.Remove(_slidingButton);
				_buttons.Remove(_slidingButton);
				_slidingButton = null;
			}
			AnimationForMoveEnd(_slidingButton);
			_slidingButton = null;
			_candidateToMove = null;
		}
		else
		{
			_candidateToMove = null;
			Close();
			if (_buttons.ContainsKey((Button)sender))
			{
				IPnCommand command = _buttons[(Button)sender];
				_mainWindow.ExeFlow.CallPnCommand(command);
			}
		}
	}

	private void canvas1_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		if (_blockEdit || (_candidateToMove == null && _slidingButton == null))
		{
			return;
		}
		if (_candidateToMove != null)
		{
			if (!(Point.Subtract(_startMovePoint, e.GetPosition(_candidateToMove)).Length > 5.0))
			{
				return;
			}
			_slidingButton = _candidateToMove;
			_candidateToMove = null;
			AnimationForMoveStart(_slidingButton);
		}
		if (_slidingButton != null)
		{
			Point position = e.GetPosition(canvas1);
			Canvas.SetLeft(_slidingButton, position.X - _startMovePoint.X);
			Canvas.SetTop(_slidingButton, position.Y - _startMovePoint.Y);
			_lastLocationX = (int)(position.X / (double)_xCellSize);
			_lastLocationY = (int)(position.Y / (double)_yCellSize);
		}
	}

	private void SetNewButtonLocation(Button btn)
	{
		IPnCommand pnCommand = _buttons[btn];
		Rect rect = CalculateButtonGeometry(pnCommand.AddValue1, pnCommand.AddValue2);
		Canvas.SetLeft(btn, rect.Left + (double)_btnMargin);
		Canvas.SetTop(btn, rect.Top + (double)_btnMargin);
	}

	private void CheckRunOutButton(int locationX, int locationY, int gotoX, int gotoY)
	{
		foreach (Button key in _buttons.Keys)
		{
			if (key != _slidingButton)
			{
				IPnCommand pnCommand = _buttons[key];
				if (locationX == pnCommand.AddValue1 && locationY == pnCommand.AddValue2)
				{
					Rect rect = CalculateButtonGeometry(gotoX, gotoY);
					Canvas.SetLeft(key, rect.Left + (double)_btnMargin);
					Canvas.SetTop(key, rect.Top + (double)_btnMargin);
					pnCommand.AddValue1 = gotoX;
					pnCommand.AddValue2 = gotoY;
					break;
				}
			}
		}
	}

	private void AnimationForMoveStart(Button testButton)
	{
		Storyboard storyboard = new Storyboard();
		DoubleAnimation doubleAnimation = new DoubleAnimation();
		doubleAnimation.Duration = TimeSpan.FromMilliseconds(150L, 0L);
		doubleAnimation.From = 1.0;
		doubleAnimation.To = 1.1;
		storyboard.Children.Add(doubleAnimation);
		Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("RenderTransform.ScaleX"));
		DoubleAnimation doubleAnimation2 = new DoubleAnimation();
		doubleAnimation2.Duration = TimeSpan.FromMilliseconds(150L, 0L);
		doubleAnimation2.From = 1.0;
		doubleAnimation2.To = 1.1;
		storyboard.Children.Add(doubleAnimation2);
		Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("RenderTransform.ScaleY"));
		DoubleAnimation doubleAnimation3 = new DoubleAnimation();
		doubleAnimation3.Duration = TimeSpan.FromMilliseconds(150L, 0L);
		doubleAnimation3.From = 1.0;
		doubleAnimation3.To = 0.9;
		DoubleAnimation doubleAnimation4 = new DoubleAnimation();
		doubleAnimation4.Duration = TimeSpan.FromMilliseconds(150L, 0L);
		doubleAnimation4.From = 1.0;
		doubleAnimation4.To = 0.9;
		if (_buttons.Keys.Count > 1)
		{
			storyboard.Children.Add(doubleAnimation4);
			Storyboard.SetTargetProperty(doubleAnimation4, new PropertyPath("RenderTransform.ScaleY"));
			storyboard.Children.Add(doubleAnimation3);
			Storyboard.SetTargetProperty(doubleAnimation3, new PropertyPath("RenderTransform.ScaleX"));
		}
		ScaleTransform renderTransform = new ScaleTransform(0.9, 0.9, _xCellSize / 2, _yCellSize / 2);
		ScaleTransform renderTransform2 = new ScaleTransform(1.1, 1.1, _xCellSize / 2, _yCellSize / 2);
		foreach (Button key in _buttons.Keys)
		{
			if (key != testButton)
			{
				Panel.SetZIndex(key, 0);
				key.RenderTransform = renderTransform;
				Storyboard.SetTarget(doubleAnimation3, key);
				Storyboard.SetTarget(doubleAnimation4, key);
			}
			else
			{
				Panel.SetZIndex(key, 1);
				key.Opacity = 0.75;
				key.RenderTransform = renderTransform2;
				Storyboard.SetTarget(doubleAnimation, key);
				Storyboard.SetTarget(doubleAnimation2, key);
			}
		}
		storyboard.Begin();
	}

	private void AnimationForMoveEnd(Button testButton)
	{
		if (testButton == null && _buttons.Keys.Count == 0)
		{
			return;
		}
		Storyboard storyboard = new Storyboard();
		DoubleAnimation doubleAnimation = new DoubleAnimation();
		doubleAnimation.Duration = TimeSpan.FromMilliseconds(150L, 0L);
		doubleAnimation.From = 1.1;
		doubleAnimation.To = 1.0;
		DoubleAnimation doubleAnimation2 = new DoubleAnimation();
		doubleAnimation2.Duration = TimeSpan.FromMilliseconds(150L, 0L);
		doubleAnimation2.From = 1.1;
		doubleAnimation2.To = 1.0;
		if (testButton != null)
		{
			storyboard.Children.Add(doubleAnimation);
			Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("RenderTransform.ScaleX"));
			storyboard.Children.Add(doubleAnimation2);
			Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("RenderTransform.ScaleY"));
		}
		DoubleAnimation doubleAnimation3 = new DoubleAnimation();
		doubleAnimation3.Duration = TimeSpan.FromMilliseconds(150L, 0L);
		doubleAnimation3.From = 0.9;
		doubleAnimation3.To = 1.0;
		DoubleAnimation doubleAnimation4 = new DoubleAnimation();
		doubleAnimation4.Duration = TimeSpan.FromMilliseconds(150L, 0L);
		doubleAnimation4.From = 0.9;
		doubleAnimation4.To = 1.0;
		ScaleTransform renderTransform = new ScaleTransform(1.0, 1.0, _xCellSize / 2, _yCellSize / 2);
		ScaleTransform renderTransform2 = new ScaleTransform(1.0, 1.0, _xCellSize / 2, _yCellSize / 2);
		if (_buttons.Keys.Count > 1)
		{
			storyboard.Children.Add(doubleAnimation3);
			Storyboard.SetTargetProperty(doubleAnimation3, new PropertyPath("RenderTransform.ScaleX"));
			storyboard.Children.Add(doubleAnimation4);
			Storyboard.SetTargetProperty(doubleAnimation4, new PropertyPath("RenderTransform.ScaleY"));
		}
		foreach (Button key in _buttons.Keys)
		{
			if (key != testButton)
			{
				key.RenderTransform = renderTransform;
				Storyboard.SetTarget(doubleAnimation3, key);
				Storyboard.SetTarget(doubleAnimation4, key);
			}
			else
			{
				key.Opacity = 1.0;
				key.RenderTransform = renderTransform2;
				Storyboard.SetTarget(doubleAnimation, key);
				Storyboard.SetTarget(doubleAnimation2, key);
			}
		}
		storyboard.Begin();
	}

	public void AnalizeConfigurationModification(bool reload)
	{
		if (CheckRecalculateNeed())
		{
			StoreLocallyCurrentConfiguration();
			CalculateSizeBaseOnCurrentConfiguration();
			base.Width = _oldCenterMenuWidth;
			base.Height = _oldCenterMenuHeight;
			foreach (Button key in _buttons.Keys)
			{
				SetButtonStyle(key);
			}
		}
		if (reload)
		{
			LoadDefinition();
		}
	}

	private bool CheckRecalculateNeed()
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (_xNumber != generalUserSettingsConfig.CenterMenuX)
		{
			return true;
		}
		if (_yNumber != generalUserSettingsConfig.CenterMenuY)
		{
			return true;
		}
		if (_oldIconsize != generalUserSettingsConfig.CenterMenuIconSize)
		{
			return true;
		}
		if (_oldTextlocation != generalUserSettingsConfig.CenterMenuTextLocation)
		{
			return true;
		}
		return false;
	}

	private void CalculateSizeBaseOnCurrentConfiguration()
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		int num = 25;
		int num2 = 25;
		switch (generalUserSettingsConfig.CenterMenuIconSize)
		{
		case 0:
			switch (generalUserSettingsConfig.CenterMenuTextLocation)
			{
			case 0:
				num = 20;
				num2 = 20;
				break;
			case 1:
				num = 115;
				num2 = 32;
				break;
			case 2:
				num = 70;
				num2 = 45;
				break;
			}
			break;
		case 16:
			switch (generalUserSettingsConfig.CenterMenuTextLocation)
			{
			case 0:
				num = 32;
				num2 = 32;
				break;
			case 1:
				num = 130;
				num2 = 28;
				break;
			case 2:
				num = 80;
				num2 = 60;
				break;
			}
			break;
		case 32:
			switch (generalUserSettingsConfig.CenterMenuTextLocation)
			{
			case 0:
				num = 45;
				num2 = 45;
				break;
			case 1:
				num = 150;
				num2 = 45;
				break;
			case 2:
				num = 80;
				num2 = 80;
				break;
			}
			break;
		}
		_oldCenterMenuWidth = num * generalUserSettingsConfig.CenterMenuX;
		_oldCenterMenuHeight = num2 * generalUserSettingsConfig.CenterMenuY + (int)YCenterModification();
	}

	private void StoreLocallyCurrentConfiguration()
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		_xNumber = generalUserSettingsConfig.CenterMenuX;
		_yNumber = generalUserSettingsConfig.CenterMenuY;
		_oldIconsize = generalUserSettingsConfig.CenterMenuIconSize;
		_oldTextlocation = generalUserSettingsConfig.CenterMenuTextLocation;
	}

	private bool GetFirstEmpty(out int xout, out int yout)
	{
		bool[,] array = new bool[_xNumber, _yNumber];
		for (int i = 0; i < _xNumber; i++)
		{
			for (int j = 0; j < _yNumber; j++)
			{
				array[i, j] = false;
			}
		}
		foreach (IPnCommand value in _buttons.Values)
		{
			if (value.AddValue1 < _xNumber && value.AddValue2 < _yNumber)
			{
				array[value.AddValue1, value.AddValue2] = true;
			}
		}
		for (int k = 0; k < _yNumber; k++)
		{
			for (int l = 0; l < _xNumber; l++)
			{
				if (!array[l, k])
				{
					xout = l;
					yout = k;
					return false;
				}
			}
		}
		xout = _xNumber - 1;
		yout = _yNumber - 1;
		return true;
	}

	private void SetButtonStyleAndData(Button btn, IPnCommand cmd)
	{
		SetButtonData(btn, cmd);
		SetButtonStyle(btn);
		btn.Focusable = false;
	}

	private void SetButtonData(Button btn, IPnCommand cmd)
	{
		if (!_buttons.ContainsKey(btn))
		{
			_buttons.Add(btn, cmd);
		}
		else
		{
			_buttons[btn] = cmd;
		}
	}

	private void SetButtonStyle(Button btn)
	{
		if (btn.Content != null)
		{
			if (btn.Content is Image)
			{
				_pnIconsService.UnregisterIconData((Image)btn.Content);
			}
			else if (btn.Content is Panel)
			{
				_pnIconsService.UnregisterIconData((Image)((Panel)btn.Content).Children[0]);
			}
		}
		btn.Style = (Style)btn.TryFindResource("ToolBarButtonBaseStyle");
		IPnCommand pnCommand = _buttons[btn];
		btn.Content = null;
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (!generalUserSettingsConfig.HideToolTips)
		{
			_pnToolTipService.SetTooltip(btn, pnCommand);
		}
		else
		{
			btn.ToolTip = null;
		}
		Image image = null;
		if (generalUserSettingsConfig.CenterMenuIconSize > 0)
		{
			image = new Image();
			image.Stretch = Stretch.UniformToFill;
			image.SnapsToDevicePixels = true;
			image.BeginInit();
			if (generalUserSettingsConfig.CenterMenuIconSize == 32)
			{
				_pnIconsService.SetBigIcon(image, Image.SourceProperty, pnCommand.Command);
				image.Height = 32.0;
				image.Width = 32.0;
			}
			else
			{
				_pnIconsService.SetSmallIcon(image, Image.SourceProperty, pnCommand.Command);
				image.Height = 16.0;
				image.Width = 16.0;
			}
			image.EndInit();
		}
		switch (generalUserSettingsConfig.CenterMenuTextLocation)
		{
		case 0:
			if (image != null)
			{
				btn.Content = image;
			}
			break;
		case 1:
		{
			TextBlock textBlock = new TextBlock();
			textBlock.Text = _languageDictionary.GetButtonLabel(pnCommand.LabelId, pnCommand.CurrentLabel);
			textBlock.SnapsToDevicePixels = true;
			textBlock.VerticalAlignment = VerticalAlignment.Center;
			textBlock.HorizontalAlignment = HorizontalAlignment.Left;
			textBlock.TextAlignment = TextAlignment.Center;
			textBlock.Padding = new Thickness(1.0);
			textBlock.TextWrapping = TextWrapping.Wrap;
			if (image == null)
			{
				btn.Content = textBlock;
				textBlock.TextAlignment = TextAlignment.Center;
				textBlock.HorizontalAlignment = HorizontalAlignment.Center;
				break;
			}
			StackPanel stackPanel = new StackPanel();
			stackPanel.SnapsToDevicePixels = true;
			stackPanel.Children.Add(image);
			stackPanel.Children.Add(textBlock);
			stackPanel.Orientation = Orientation.Horizontal;
			stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
			btn.HorizontalAlignment = HorizontalAlignment.Left;
			stackPanel.VerticalAlignment = VerticalAlignment.Top;
			btn.VerticalAlignment = VerticalAlignment.Top;
			btn.Content = stackPanel;
			break;
		}
		case 2:
		{
			TextBlock textBlock = new TextBlock();
			textBlock.Text = _languageDictionary.GetButtonLabel(pnCommand.LabelId, pnCommand.CurrentLabel);
			textBlock.VerticalAlignment = VerticalAlignment.Bottom;
			textBlock.HorizontalAlignment = HorizontalAlignment.Center;
			textBlock.TextAlignment = TextAlignment.Center;
			textBlock.Padding = new Thickness(1.0);
			textBlock.TextWrapping = TextWrapping.Wrap;
			textBlock.SnapsToDevicePixels = true;
			if (image == null)
			{
				btn.Content = textBlock;
				break;
			}
			StackPanel stackPanel = new StackPanel();
			stackPanel.SnapsToDevicePixels = true;
			stackPanel.Children.Add(image);
			stackPanel.Children.Add(textBlock);
			stackPanel.Orientation = Orientation.Vertical;
			stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
			btn.HorizontalAlignment = HorizontalAlignment.Center;
			btn.Content = stackPanel;
			break;
		}
		}
	}

	private Button GetLast()
	{
		foreach (Button key in _buttons.Keys)
		{
			IPnCommand pnCommand = _buttons[key];
			if (pnCommand.AddValue1 == _xNumber - 1 && pnCommand.AddValue2 == _yNumber - 1)
			{
				return key;
			}
		}
		return null;
	}

	public void AddCommand(IPnCommand cmd)
	{
		if (_blockEdit)
		{
			return;
		}
		foreach (IPnCommand value in _buttons.Values)
		{
			if (value.Command == cmd.Command && value.Group == cmd.Group)
			{
				return;
			}
		}
		int xout = 0;
		int yout = 0;
		Button last;
		if (GetFirstEmpty(out xout, out yout))
		{
			last = GetLast();
			if (last != null)
			{
				cmd.AddValue1 = _xNumber - 1;
				cmd.AddValue2 = _yNumber - 1;
				SetButtonStyleAndData(last, cmd);
			}
			return;
		}
		last = new Button();
		last.SnapsToDevicePixels = true;
		Rect rect = CalculateButtonGeometry(xout, yout);
		last.Width = rect.Width - (double)(2 * _btnMargin);
		last.Height = rect.Height - (double)(2 * _btnMargin);
		Canvas.SetLeft(last, rect.Left + (double)_btnMargin);
		Canvas.SetTop(last, rect.Top + (double)_btnMargin);
		last.Visibility = Visibility.Visible;
		canvas1.Children.Add(last);
		last.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
		last.Click += btn_Click;
		cmd.AddValue1 = xout;
		cmd.AddValue2 = yout;
		SetButtonStyleAndData(last, cmd);
	}

	private void LoadDefinition()
	{
		LoadTabNames();
		foreach (DependencyObject child in canvas1.Children)
		{
			_pnIconsService.UnregisterIconData(child);
		}
		canvas1.Children.Clear();
		_buttons.Clear();
		List<RFileRecord> list = RFiles.ReadFlatFileAndClearDouble(_moduleName, "center_menu_" + _currTab, _pnPathService);
		if (list == null)
		{
			return;
		}
		foreach (RFileRecord item in list)
		{
			if (item.AddValue1 < _xNumber && item.AddValue2 < _yNumber)
			{
				Button btn = CreateButtonInLocation(item.AddValue1, item.AddValue2);
				SetButtonStyleAndData(btn, new PnCommand(item));
			}
		}
	}

	private Button CreateButtonInLocation(int x, int y)
	{
		Button button = new Button();
		button.SnapsToDevicePixels = true;
		Rect rect = CalculateButtonGeometry(x, y);
		button.Width = rect.Width - (double)(2 * _btnMargin);
		button.Height = rect.Height - (double)(2 * _btnMargin);
		Canvas.SetLeft(button, rect.Left + (double)_btnMargin);
		Canvas.SetTop(button, rect.Top + (double)_btnMargin);
		button.Visibility = Visibility.Visible;
		canvas1.Children.Add(button);
		button.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
		button.Click += btn_Click;
		return button;
	}

	public void SaveCurrentTab()
	{
		string path = "pn.rfiles\\" + _moduleName + "\\center_menu_" + _currTab;
		try
		{
			StreamWriter streamWriter = new StreamWriter(path);
			foreach (IPnCommand value in _buttons.Values)
			{
				if (value.AddValue1 < _xNumber && value.AddValue2 < _yNumber)
				{
					streamWriter.WriteLine(new RFileRecord(value).GetOutputText());
				}
			}
			streamWriter.Close();
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		generalUserSettingsConfig.CenterMenuWidth = (int)base.Width;
		generalUserSettingsConfig.CenterMenuHeight = (int)base.Height;
		_configProvider.Push(generalUserSettingsConfig);
		_configProvider.Save<GeneralUserSettingsConfig>();
	}

	public void ActivateModule(string moduleName)
	{
		_moduleName = moduleName;
		LoadDefinition();
	}

	private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (tabControl1.SelectedIndex != _currTab)
		{
			SaveCurrentTab();
			_currTab = tabControl1.SelectedIndex;
			LoadDefinition();
		}
	}

	public void SetCenterButtonLocation(Point p)
	{
		Rect rect = CalculateButtonGeometry((int)((double)_xNumber / 2.0), (int)((double)_yNumber / 2.0));
		Point point = PointFromScreen(canvas1.PointToScreen(rect.Location));
		base.Left = p.X - point.X - rect.Width / 2.0;
		base.Top = p.Y - point.Y - rect.Height / 2.0;
	}

	private void button2_Click(object sender, RoutedEventArgs e)
	{
		_blockEdit = !_blockEdit;
		_candidateToMove = null;
		_slidingButton = null;
		SetBlockEditView();
	}

	private void SetBlockEditView()
	{
		if (_blockEdit)
		{
			button2.Content = _padlock1;
		}
		else
		{
			button2.Content = _padlock2;
		}
	}

	private void tabControl1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		CloseEditTab();
		_lastEditId = tabControl1.SelectedIndex;
		_tbedit = new TextBox();
		_tbedit.Text = ((TabItem)tabControl1.Items[_lastEditId]).Header.ToString();
		_tbedit.LostFocus += tbedit_LostFocus;
		_tbedit.PreviewKeyDown += tbedit_PreviewKeyDown;
		((TabItem)tabControl1.Items[_lastEditId]).Header = _tbedit;
		_tbedit.Width = ((TabItem)tabControl1.Items[_lastEditId]).Width - 13.0;
		base.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)delegate
		{
			Keyboard.Focus(_tbedit);
		});
	}

	private void tbedit_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape || e.Key == Key.Return)
		{
			CloseEditTab();
			e.Handled = true;
		}
	}

	private void tbedit_LostFocus(object sender, RoutedEventArgs e)
	{
		CloseEditTab();
	}

	private void CloseEditTab()
	{
		if (_tbedit != null && _lastEditId != -1)
		{
			((TabItem)tabControl1.Items[_lastEditId]).Header = _tbedit.Text;
			_tbedit = null;
			_lastEditId = -1;
			SaveTabNames();
		}
	}

	private void SaveTabNames()
	{
		string path = "pn.rfiles\\" + _moduleName + "\\center_menu_pagenames";
		try
		{
			StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.WriteLine(tabItem1.Header.ToString());
			streamWriter.WriteLine(tabItem2.Header.ToString());
			streamWriter.WriteLine(tabItem3.Header.ToString());
			streamWriter.WriteLine(tabItem4.Header.ToString());
			streamWriter.WriteLine(tabItem5.Header.ToString());
			streamWriter.Close();
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	private void LoadTabNames()
	{
		string path = "pn.rfiles\\" + _moduleName + "\\center_menu_pagenames";
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			using TextReader textReader = File.OpenText(path);
			tabItem1.Header = textReader.ReadLine();
			tabItem2.Header = textReader.ReadLine();
			tabItem3.Header = textReader.ReadLine();
			tabItem4.Header = textReader.ReadLine();
			tabItem5.Header = textReader.ReadLine();
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	private void tabControl1_SizeChanged(object sender, SizeChangedEventArgs e)
	{
	}

	private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		CloseEditTab();
	}
}
