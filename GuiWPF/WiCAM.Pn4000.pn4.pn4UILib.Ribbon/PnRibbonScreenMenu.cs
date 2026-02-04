using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.uicontrols.Buttons;

namespace WiCAM.Pn4000.pn4.pn4UILib.Ribbon;

public partial class PnRibbonScreenMenu : UserControl, IComponentConnector
{
	private readonly IRibbonMainWindowConnector _ribbonMainWindowConnector;

	private readonly ILanguageDictionary _languageDictionary;

	private readonly IPnToolTipService _toolTipService;

	private readonly IPnIconsService _pnIconsService;

	private Dictionary<PnRibbonButton, IPnRibbonNode> _buttonNodeDictionaryLv0;

	private Dictionary<PnRibbonButton, IPnRibbonNode> _buttonNodeDictionaryLv1;

	private Dictionary<PnRibbonButton, IPnRibbonNode> _buttonNodeDictionaryLv2;

	private Style _buttonOnMenu;

	private Style _buttonOnMenuL2;

	private readonly Brush _whiteSolidBrush = new SolidColorBrush(Colors.White);

	private readonly Brush _blackBrush = new SolidColorBrush(Colors.Black);

	private IPnCommand _commandForMainInfo;

	private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();

	private bool _isMenuRecently;

	private object _lastEnter;

	public PnRibbonScreenMenu(IRibbonMainWindowConnector ribbonMainWindowConnector, ILanguageDictionary languageDictionary, IPnToolTipService toolTipService, IPnIconsService pnIconsService)
	{
		_ribbonMainWindowConnector = ribbonMainWindowConnector;
		_languageDictionary = languageDictionary;
		_toolTipService = toolTipService;
		_pnIconsService = pnIconsService;
		_ribbonMainWindowConnector.OnDynamicSetup += DynamicSetup;
		_ribbonMainWindowConnector.OnChangeLanguage += ChangeLanguage;
		_ribbonMainWindowConnector.OnSetup += Setup;
		_dispatcherTimer.Tick += dispatcherTimer_Tick;
		_dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
		InitializeComponent();
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		_ribbonMainWindowConnector.ResetAllPreviewNeeds();
		_ribbonMainWindowConnector.SetMainScreenLayout(0);
	}

	internal void ChangeLanguage()
	{
		UpdateLabelsAndTooltip(_buttonNodeDictionaryLv0);
		UpdateLabelsAndTooltip(_buttonNodeDictionaryLv1);
		UpdateLabelsAndTooltip(_buttonNodeDictionaryLv2);
		if (_commandForMainInfo != null)
		{
			MainInfo.Text = _languageDictionary.GetButtonLabel(_commandForMainInfo.LabelId, _commandForMainInfo.DefaultLabel);
		}
	}

	private void UpdateLabelsAndTooltip(Dictionary<PnRibbonButton, IPnRibbonNode> d)
	{
		if (d == null)
		{
			return;
		}
		foreach (PnRibbonButton key in d.Keys)
		{
			IPnRibbonNode pnRibbonNode = d[key];
			key.Label = _languageDictionary.GetButtonLabel(pnRibbonNode.Command.LabelId, pnRibbonNode.Command.DefaultLabel);
			if (pnRibbonNode.Command.Group != 100)
			{
				_toolTipService.SetTooltip(key, pnRibbonNode.Command);
			}
		}
	}

	internal void Setup(IPnRibbonNode mainnode)
	{
		Setup(mainnode, 0, null);
	}

	internal void Setup(IPnRibbonNode mainnode, int level, PnRibbonButton sender)
	{
		_ribbonMainWindowConnector.ResetAllPreviewNeeds();
		DocumentPreview.Visibility = Visibility.Collapsed;
		DocumentDescription.Content = string.Empty;
		Dictionary<PnRibbonButton, IPnRibbonNode> dictionary = new Dictionary<PnRibbonButton, IPnRibbonNode>();
		StackPanel stackPanel = null;
		switch (level)
		{
		case 0:
			_buttonNodeDictionaryLv0 = dictionary;
			stackPanel = MainButtonsPanel;
			MainInfo.Text = string.Empty;
			LeftPanel.Children.Clear();
			RightPanel.Children.Clear();
			_buttonNodeDictionaryLv1 = null;
			_commandForMainInfo = null;
			break;
		case 1:
			foreach (PnRibbonButton key in _buttonNodeDictionaryLv0.Keys)
			{
				key.IsSelected = false;
			}
			sender.IsSelected = true;
			_buttonNodeDictionaryLv1 = dictionary;
			stackPanel = LeftPanel;
			RightPanel.Children.Clear();
			MainInfo.Text = _languageDictionary.GetButtonLabel(mainnode.Command.LabelId, mainnode.Command.DefaultLabel);
			_commandForMainInfo = mainnode.Command;
			break;
		case 2:
			foreach (PnRibbonButton key2 in _buttonNodeDictionaryLv1.Keys)
			{
				key2.IsSelected = false;
			}
			_buttonNodeDictionaryLv2 = dictionary;
			sender.IsSelected = true;
			stackPanel = RightPanel;
			break;
		}
		_isMenuRecently = false;
		stackPanel.Children.Clear();
		if (_buttonOnMenu == null)
		{
			_buttonOnMenu = (Style)TryFindResource("PnRibbonButtonOnMenu32");
		}
		if (_buttonOnMenuL2 == null)
		{
			_buttonOnMenuL2 = (Style)TryFindResource("PnRibbonButtonOnMenu32L2");
		}
		if (mainnode.Children != null)
		{
			foreach (IPnRibbonNode child in mainnode.Children)
			{
				PnRibbonButton pnRibbonButton = new PnRibbonButton();
				pnRibbonButton.SmallImage = _pnIconsService.GetSmallIcon(child.Command.Command);
				pnRibbonButton.LargeImage = _pnIconsService.GetBigIcon(child.Command.Command);
				pnRibbonButton.Label = _languageDictionary.GetButtonLabel(child.Command.LabelId, child.Command.DefaultLabel);
				pnRibbonButton.Foreground = _whiteSolidBrush;
				if (child.Children != null || (child.Command.AddValue3 != null && child.Command.AddValue3.Trim() != string.Empty))
				{
					pnRibbonButton.HaveArrow = true;
				}
				pnRibbonButton.Click += Btn_Click;
				pnRibbonButton.MouseEnter += Btn_MouseEnter;
				pnRibbonButton.MouseLeave += Btn_MouseLeave;
				if (child.Command.Group != 100)
				{
					_toolTipService.SetTooltip(pnRibbonButton, child.Command);
				}
				dictionary.Add(pnRibbonButton, child);
				switch (level)
				{
				case 0:
					pnRibbonButton.Style = _buttonOnMenu;
					break;
				case 1:
					pnRibbonButton.Style = _buttonOnMenuL2;
					pnRibbonButton.Foreground = _blackBrush;
					pnRibbonButton.Width = 300.0;
					break;
				case 2:
					pnRibbonButton.Style = _buttonOnMenu;
					pnRibbonButton.Style = _buttonOnMenuL2;
					pnRibbonButton.Foreground = _blackBrush;
					break;
				}
				stackPanel.Children.Add(pnRibbonButton);
			}
			return;
		}
		if (mainnode.Command.AddValue3 != null && mainnode.Command.AddValue3.Trim() != string.Empty)
		{
			_ribbonMainWindowConnector.FillMenu(1, stackPanel, mainnode.Command.AddValue3.ToUpper(), mainnode.Command.AddValue2, DocumentPreview, DocumentDescription);
			_isMenuRecently = true;
		}
	}

	private void Btn_MouseLeave(object sender, MouseEventArgs e)
	{
		_dispatcherTimer.Stop();
	}

	private void Btn_MouseEnter(object sender, MouseEventArgs e)
	{
		_dispatcherTimer.Stop();
		_lastEnter = sender;
		_dispatcherTimer.Start();
	}

	private IPnRibbonNode GetNodeFormButton(object sender, out int level)
	{
		if (_buttonNodeDictionaryLv0.ContainsKey(sender as PnRibbonButton))
		{
			level = 0;
			return _buttonNodeDictionaryLv0[sender as PnRibbonButton];
		}
		if (_buttonNodeDictionaryLv1.ContainsKey(sender as PnRibbonButton))
		{
			level = 1;
			return _buttonNodeDictionaryLv1[sender as PnRibbonButton];
		}
		if (_buttonNodeDictionaryLv2.ContainsKey(sender as PnRibbonButton))
		{
			level = 2;
			return _buttonNodeDictionaryLv2[sender as PnRibbonButton];
		}
		level = -1;
		return null;
	}

	private void dispatcherTimer_Tick(object sender, EventArgs e)
	{
		_dispatcherTimer.Stop();
		int level;
		IPnRibbonNode nodeFormButton = GetNodeFormButton(_lastEnter, out level);
		if (nodeFormButton.Children != null || (nodeFormButton.Command.AddValue3 != null && nodeFormButton.Command.AddValue3.Trim() != string.Empty))
		{
			Setup(nodeFormButton, level + 1, _lastEnter as PnRibbonButton);
		}
	}

	private void Btn_Click(object sender, RoutedEventArgs e)
	{
		_dispatcherTimer.Stop();
		int level;
		IPnRibbonNode nodeFormButton = GetNodeFormButton(_lastEnter, out level);
		if (nodeFormButton.Command.Group != 5 || !(nodeFormButton.Command.Command == "RECENTLY"))
		{
			if (nodeFormButton.Command.Group != 100)
			{
				_ribbonMainWindowConnector.SetMainScreenLayout(0);
				_ribbonMainWindowConnector.CallPnCommand(nodeFormButton.Command);
			}
			else if (nodeFormButton.Children != null || (nodeFormButton.Command.AddValue3 != null && nodeFormButton.Command.AddValue3.Trim() != string.Empty))
			{
				Setup(nodeFormButton, level + 1, sender as PnRibbonButton);
			}
		}
	}

	private void DynamicSetup()
	{
		DocumentPreview.Visibility = Visibility.Collapsed;
		if (_isMenuRecently)
		{
			_ribbonMainWindowConnector.UpdateLastMenu();
		}
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
	}
}
