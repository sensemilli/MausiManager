using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using pncommon.WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.pn4.Contracts;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.uicontrols.Buttons;
using WiCAM.Pn4000.pn4.uicontrols.Tooltips;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4UILib.Ribbon;

public partial class PnRibbon : UserControl, IComponentConnector
{
	private readonly IRibbonMainWindowConnector _ribbonMainWindowConnector;

	private readonly List<SplitData> _splitCommandConnection = new List<SplitData>();

	private PnRibbonDatabase _ribbonDb;

	private PnRibbonTabButton _fileTabbutton;

	private readonly Dictionary<PnRibbonTabButton, TabInformation> _tabDictionary = new Dictionary<PnRibbonTabButton, TabInformation>();

	private object _lastSubButton;

	private Style _pnRibbonExtenderStyle;

	private List<PnRibbonButton> _buttonsConnectedWithOpenPopup;

	private Style _buttonStyle32;

	private Style _buttonStyle16Label;

	private Style _buttonStyle16;

	private Style _extendButtonStyle32;

	private Style _extendButtonStyle16Label;

	private Style _extendButtonStyle16;

	private int _latestSubtabGroup;

	private bool _isSubtab;

	private PnRibbonTabButton _subtabbtn;

	private bool _subtabshowsource;

	private string _moduleName;

	private string _ribbonModuleName;

	private bool _notShowByClick;

	private ContextMenu _simpleButtonContextMenu;

	private ContextMenu _splitPopupContextMenu;

	private object _lastContextMenuOpening;

	private System.Windows.Controls.Primitives.Popup _sub;

	private object _lastExtBtn;

	private Style _buttonStyleList;

	private System.Windows.Controls.Primitives.Popup _codePopup;

	private const string MenuName = "Menu";

	private const string FirmaName = "Firma";

	private readonly ILanguageDictionary _languageDictionary;

	private readonly IPnToolTipService _pnToolTipService;

	private readonly IPnPathService _pnPathService;

	private readonly IPnIconsService _pnIconsService;

	private readonly IPnColorsService _pnColorsService;

	private readonly ILogCenterService _logCenterService;

	private readonly IConfigProvider _configProvider;

	private readonly ISubMenuConnector _subMenuConnector;

	private SizeChangedEventArgs e_latest;

	private TabInformation CurrentTabInformation { get; set; }

	public bool Is3Dscreen { get; set; }

	public bool PulldownMode { get; set; }

	private Dictionary<object, IPnRibbonNode> PopupElementNodeDictionary { get; set; }

	public PnRibbon(IRibbonMainWindowConnector ribbonMainWindowConnector, ILanguageDictionary languageDictionary, IPnToolTipService pnToolTipService, IPnPathService pnPathService, IPnIconsService pnIconsService, IPnColorsService pnColorsService, ILogCenterService logCenterService, IConfigProvider configProvider, ISubMenuConnector subMenuConnector)
	{
		_ribbonMainWindowConnector = ribbonMainWindowConnector;
		_languageDictionary = languageDictionary;
		_pnToolTipService = pnToolTipService;
		_pnPathService = pnPathService;
		_pnIconsService = pnIconsService;
		_pnColorsService = pnColorsService;
		_logCenterService = logCenterService;
		_configProvider = configProvider;
		_subMenuConnector = subMenuConnector;
		InitializeComponent();
	}

	private TabInformation CreateRibbonTab(TabType type)
	{
		return CreateRibbonTab(type, is3D: false);
	}

	private TabInformation CreateRibbonTab(TabType type, bool is3D)
	{
		TabInformation tabInformation = new TabInformation
		{
			Tab = new PnRibbonTabButton(),
			ElementNodeDictionary = new Dictionary<object, IPnRibbonNode>(),
			ButtonCommandConnection = new Dictionary<PnRibbonButton, IPnCommand>(),
			Type = type
		};
		tabInformation.Tab.Style = (Style)TryFindResource("PnRibbonTabButtonStyle");
		_tabDictionary.Add(tabInformation.Tab, tabInformation);
		RibbonButtonsZone.Children.Add(tabInformation.Tab);
		tabInformation.Tab.Click += Tab_Click;
		return tabInformation;
	}

	public void LoadRibbon()
	{
		_tabDictionary.Clear();
		RibbonButtonsZone.Children.Clear();
		_splitCommandConnection.Clear();
		SplitCommandConnectionFillByCfg();
		int num = 0;
		_fileTabbutton = new PnRibbonTabButton
		{
			Content = _languageDictionary.GetMsg2Int("Menu"),
			IsScreenTab = true,
			Style = (Style)TryFindResource("PnRibbonTabButtonStyle")
		};
		_fileTabbutton.Click += File_tabbutton_Click;
		RibbonButtonsZone.Children.Add(_fileTabbutton);
		TabInformation tabInformation;
		if (_ribbonDb.Root.Children != null)
		{
			foreach (IPnRibbonNode child in _ribbonDb.Root.Children)
			{
				if (child.OrginalType == 12)
				{
					_ribbonMainWindowConnector.Setup(child);
					continue;
				}
				tabInformation = CreateRibbonTab(TabType.Regular, CheckFor3DTab(child.Command.AddValue2));
				tabInformation.Node = child;
				_pnToolTipService.SetTooltip(tabInformation.Tab, child.Command);
				tabInformation.ElementNodeDictionary.Add(tabInformation.Tab, child);
				tabInformation.Tab.Content = _languageDictionary.GetButtonLabel(child.Command.LabelId, child.Command.DefaultLabel);
				if (child.Command.AddValue2 < 900 && !_languageDictionary.CheckRibbonTabOff(child.Command.AddValue2))
				{
					tabInformation.Tab.Visibility = Visibility.Visible;
				}
				else
				{
					tabInformation.Tab.Visibility = Visibility.Collapsed;
				}
				if (num == 0)
				{
					CurrentTabInformation = tabInformation;
				}
				if (_ribbonDb.LatesHeader != string.Empty && _ribbonDb.LatesHeader == tabInformation.Tab.Content.ToString() && tabInformation.Tab.Visibility == Visibility.Visible)
				{
					CurrentTabInformation = tabInformation;
				}
				num++;
			}
		}
		tabInformation = CreateRibbonTab(TabType.User);
		tabInformation.Tab.Content = "Firma";
		if (_ribbonDb.LatesHeader == "Firma")
		{
			CurrentTabInformation = tabInformation;
		}
		tabInformation = CreateRibbonTab(TabType.Sub);
		tabInformation.Tab.Content = "SUB";
		tabInformation.Tab.Visibility = Visibility.Collapsed;
		UpdateTabButtonsSize();
		CreateTabPanel();
		CurrentTabInformation.Tab.IsTabSelected = true;
		MainRibbonZone.Child = CurrentTabInformation.Panel;
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		ChangeRibbonMode(generalUserSettingsConfig.RibbonCompactMode);
		CheckActiveScreen();
	}

	public void SelectButton(int group, string command, bool value)
	{
		foreach (TabInformation value2 in _tabDictionary.Values)
		{
			for (int i = 0; i < value2.ButtonCommandConnection.Keys.Count; i++)
			{
				IPnCommand pnCommand = value2.ButtonCommandConnection.Values.ElementAt(i);
				if (pnCommand.Group == group && pnCommand.Command == command)
				{
					value2.ButtonCommandConnection.Keys.ElementAt(i).IsSelected = value;
				}
			}
		}
	}

	private void File_tabbutton_Click(object sender, RoutedEventArgs e)
	{
		_ribbonMainWindowConnector.SetMainScreenLayout(1);
	}

	private void CreateTabPanel()
	{
		CurrentTabInformation.Panel = new StackPanel
		{
			Orientation = Orientation.Horizontal
		};
		if (CurrentTabInformation.Node == null)
		{
			return;
		}
		CurrentTabInformation.ElementNodeDictionary = new Dictionary<object, IPnRibbonNode>();
		CurrentTabInformation.ButtonCommandConnection = new Dictionary<PnRibbonButton, IPnCommand>();
		if (CurrentTabInformation.Node.Children != null)
		{
			foreach (IPnRibbonNode child in CurrentTabInformation.Node.Children)
			{
				GroupBox groupBox = new GroupBox
				{
					Header = _languageDictionary.GetButtonLabel(child.Command.LabelId, child.Command.DefaultLabel)
				};
				CurrentTabInformation.Panel.Children.Add(groupBox);
				groupBox.Style = (Style)TryFindResource("PnRibbonGroupBoxStyle");
				child.Runtime_VisualCompretionLevel = 0;
				CurrentTabInformation.ElementNodeDictionary.Add(groupBox, child);
				WrapPanel wrapPanel = (WrapPanel)(groupBox.Content = new WrapPanel
				{
					Orientation = Orientation.Vertical
				});
				if (child.Children == null)
				{
					continue;
				}
				foreach (IPnRibbonNode child2 in child.Children)
				{
					UIElement uIElement = CreateDifferentTypeGroupButton(child2);
					if (uIElement is PnRibbonButton)
					{
						CurrentTabInformation.ButtonCommandConnection.Add(uIElement as PnRibbonButton, child2.Command);
					}
					Grid grid = new Grid();
					grid.ColumnDefinitions.Add(new ColumnDefinition());
					grid.ColumnDefinitions.Add(new ColumnDefinition());
					grid.ColumnDefinitions[0].Width = GridLength.Auto;
					grid.ColumnDefinitions[1].Width = GridLength.Auto;
					Grid.SetColumn(uIElement, 0);
					grid.Children.Add(uIElement);
					wrapPanel.Children.Add(grid);
				}
			}
		}
		CurrentTabInformation.IsValid = true;
	}

	private void UpdateTabData(bool show)
	{
		if (_sub != null)
		{
			Border border = _sub.Child as Border;
			if (border.Child != null)
			{
				UIElement child = border.Child;
				border.Child = null;
				MainRibbonZone.Child = child;
			}
		}
		if (!CurrentTabInformation.IsValid)
		{
			CreateTabPanel();
		}
		MainRibbonZone.Child = CurrentTabInformation.Panel;
		UpdateTabItemsSize();
		if (PulldownMode)
		{
			if (!_notShowByClick && show && !_subtabshowsource)
			{
				Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				_sub.Width = base.ActualWidth;
				_sub.IsOpen = true;
			}
			MainRibbonZone.Child = null;
			(_sub.Child as Border).Child = CurrentTabInformation.Panel;
		}
		if (_subtabshowsource)
		{
			_lastSubButton = null;
		}
		_notShowByClick = false;
		CheckActiveScreen();
	}

	public void SetServiceProvider()
	{
		CreateContextMenuForQatOnly();
		CreateContextMenuForQatAndSplitPopup();
	}

	public bool Is3D()
	{
		Is3Dscreen = false;
		if (CurrentTabInformation.Node != null)
		{
			Is3Dscreen = CheckFor3DTab(CurrentTabInformation.Node.Command.AddValue2);
		}
		return Is3Dscreen;
	}

	private bool CheckFor3DTab(int AddValue2)
	{
		if (AddValue2 > 800)
		{
			return AddValue2 < 900;
		}
		return false;
	}

	private void CheckActiveScreen()
	{
		Is3Dscreen = false;
		if (CurrentTabInformation.Node != null)
		{
			Is3Dscreen = CheckFor3DTab(CurrentTabInformation.Node.Command.AddValue2);
		}
		EKernelType screenType = (Is3Dscreen ? EKernelType.PN_3D : EKernelType.PKernel_2D);
		_ribbonMainWindowConnector.ScreenTypeSetup(screenType);
	}

	public void SetActiveTab(int ID)
	{
		TabInformation tabInformation = _tabDictionary.Values.FirstOrDefault(delegate(TabInformation t)
		{
			IPnRibbonNode node = t.Node;
			return node != null && node.Command.AddValue2 == ID;
		});
		if (tabInformation != null)
		{
			ActivationFromSource(tabInformation);
		}
	}

	private void ActivationFromSource(TabInformation tab)
	{
		if (CurrentTabInformation != tab)
		{
			if (CurrentTabInformation != null && CurrentTabInformation.Tab != null)
			{
				CurrentTabInformation.Tab.IsTabSelected = false;
			}
			if (CurrentTabInformation != tab)
			{
				tab.Before = CurrentTabInformation;
			}
			CurrentTabInformation = tab;
			CurrentTabInformation.Tab.IsTabSelected = true;
		}
		UpdateTabData(show: false);
	}

	private void Tab_Click(object sender, RoutedEventArgs e)
	{
		if (!(sender is PnRibbonTabButton))
		{
			return;
		}
		PnRibbonTabButton pnRibbonTabButton = sender as PnRibbonTabButton;
		if (!_tabDictionary.Keys.Contains(pnRibbonTabButton))
		{
			return;
		}
		if (CurrentTabInformation.Tab == pnRibbonTabButton)
		{
			if (!PulldownMode)
			{
				return;
			}
			if (_lastSubButton == sender)
			{
				_lastSubButton = null;
				return;
			}
		}
		ActivateTab(pnRibbonTabButton);
	}

	public PnRibbonTabButton GetTabById(int id)
	{
		for (int i = 0; i < _tabDictionary.Count; i++)
		{
			IPnRibbonNode node = _tabDictionary.Values.ElementAt(i).Node;
			if (node != null && node.Command.AddValue2 == id)
			{
				return _tabDictionary.Keys.ElementAt(i);
			}
		}
		return null;
	}

	public int GetActiveTabId()
	{
		if (CurrentTabInformation == null)
		{
			return 0;
		}
		return CurrentTabInformation.Node.Command.AddValue2;
	}

	public void ActivateTab(PnRibbonTabButton tab)
	{
		if (CurrentTabInformation != null && CurrentTabInformation.Tab != null)
		{
			CurrentTabInformation.Tab.IsTabSelected = false;
		}
		TabInformation tabInformation = _tabDictionary[tab];
		if (tabInformation != CurrentTabInformation && CurrentTabInformation.Type != TabType.Sub)
		{
			tabInformation.Before = CurrentTabInformation;
		}
		CurrentTabInformation = tabInformation;
		UpdateTabData(show: true);
		if (CurrentTabInformation.Tab != null)
		{
			CurrentTabInformation.Tab.IsTabSelected = true;
		}
		if (CurrentTabInformation.Tab != null && CurrentTabInformation.Tab.Content != null && CurrentTabInformation.Tab.Content.ToString() != string.Empty)
		{
			SaveLastRibbonTab(_moduleName, _languageDictionary.GetButtonLabel(CurrentTabInformation.Node.Command.LabelId, CurrentTabInformation.Node.Command.DefaultLabel));
		}
		if (!_subtabshowsource)
		{
			_lastSubButton = tab;
			return;
		}
		_subtabshowsource = false;
		_lastSubButton = null;
	}

	private void Sub_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (_codePopup != null && _codePopup.IsOpen && !_codePopup.IsMouseCaptured)
		{
			_codePopup.IsOpen = false;
		}
	}

	private void BackWithPanel()
	{
		if (_sub != null)
		{
			Border border = _sub.Child as Border;
			if (border.Child != null)
			{
				UIElement child = border.Child;
				border.Child = null;
				MainRibbonZone.Child = child;
			}
		}
	}

	private void Sub_Closed(object sender, EventArgs e)
	{
		BackWithPanel();
		if (_lastSubButton != null && !((UIElement)_lastSubButton).IsMouseCaptured)
		{
			_lastSubButton = null;
		}
	}

	public void SaveLastRibbonTab(string module, string tab)
	{
		try
		{
			StreamWriter streamWriter = new StreamWriter("pn.rfiles\\" + module + "\\last_ribbon_tab");
			streamWriter.WriteLine(tab);
			streamWriter.Close();
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	public void SwapIcons(string keyName, string newIcon)
	{
		foreach (TabInformation value in _tabDictionary.Values)
		{
			SwapIconsBasedOnTabInformation(value, keyName, newIcon);
		}
	}

	private void SwapIconsBasedOnTabInformation(TabInformation ti, string keyName, string newIcon)
	{
		for (int i = 0; i < ti.ButtonCommandConnection.Values.Count; i++)
		{
			if (ti.ButtonCommandConnection.Values.ElementAt(i).Command == keyName)
			{
				PnRibbonButton pnRibbonButton = ti.ButtonCommandConnection.Keys.ElementAt(i);
				pnRibbonButton.LargeImage = _pnIconsService.GetBigIcon(newIcon);
				pnRibbonButton.SmallImage = _pnIconsService.GetSmallIcon(newIcon);
			}
		}
	}

	private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		UpdateTabItemsSize();
		UpdateTabButtonsSize(e);
	}

	private void UpdateTabItemsSize()
	{
		if (CurrentTabInformation != null && CurrentTabInformation.Panel != null)
		{
			StackPanel panel = CurrentTabInformation.Panel;
			double num = base.ActualWidth - 5.0;
			if (MeasurePanel(panel) > num)
			{
				TryZoomOutCurrentPanel();
			}
			else
			{
				TryZoomInCurrentPanel();
			}
		}
	}

	private void UpdateTabButtonsSize(SizeChangedEventArgs e = null)
	{
		if (e != null)
		{
			e_latest = e;
		}
		if (RibbonButtonsZone.Children.Count == 0)
		{
			return;
		}
		int num = 0;
		for (int i = 1; i < RibbonButtonsZone.Children.Count; i++)
		{
			PnRibbonTabButton pnRibbonTabButton = RibbonButtonsZone.Children[i] as PnRibbonTabButton;
			TabInformation tabInformation = _tabDictionary[pnRibbonTabButton];
			if (tabInformation.Node != null)
			{
				TextBlock textBlock = new TextBlock();
				textBlock.Text = _languageDictionary.GetButtonLabel(tabInformation.Node.Command.LabelId, tabInformation.Node.Command.DefaultLabel);
				textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
				pnRibbonTabButton.Content = textBlock;
			}
			if (pnRibbonTabButton.Visibility == Visibility.Visible)
			{
				num++;
			}
		}
		if (num <= 0 || e_latest == null)
		{
			return;
		}
		num += 3;
		double num2 = (e_latest.NewSize.Width - 20.0) / (double)num;
		double num3 = 100.0;
		if (num2 < num3)
		{
			num3 = num2;
		}
		if (num3 > 0.0)
		{
			for (int j = 1; j < RibbonButtonsZone.Children.Count; j++)
			{
				(RibbonButtonsZone.Children[j] as PnRibbonTabButton).Width = num3;
			}
		}
	}

	private void TryZoomInCurrentPanel()
	{
		StackPanel panel = CurrentTabInformation.Panel;
		bool flag = false;
		while (!flag)
		{
			MeasurePanel(panel);
			int num = int.MinValue;
			foreach (object child in panel.Children)
			{
				IPnRibbonNode pnRibbonNode = CurrentTabInformation.ElementNodeDictionary[child];
				if (pnRibbonNode.Runtime_VisualCompretionLevel > num)
				{
					num = pnRibbonNode.Runtime_VisualCompretionLevel;
				}
			}
			if (num > 0)
			{
				int count = panel.Children.Count;
				for (int i = 0; i < count; i++)
				{
					IPnRibbonNode pnRibbonNode2 = CurrentTabInformation.ElementNodeDictionary[panel.Children[i]];
					if (pnRibbonNode2.Runtime_VisualCompretionLevel != num)
					{
						continue;
					}
					WrapPanel wrapPanel = (WrapPanel)((GroupBox)panel.Children[i]).Content;
					pnRibbonNode2.Runtime_VisualCompretionLevel--;
					if (wrapPanel.Children.Count <= 1)
					{
						break;
					}
					Dictionary<PnRibbonButton, Style> dictionary = new Dictionary<PnRibbonButton, Style>();
					Dictionary<StackPanel, Orientation> dictionary2 = new Dictionary<StackPanel, Orientation>();
					foreach (object child2 in wrapPanel.Children)
					{
						object obj = child2;
						if (obj is Grid && (obj as Grid).Children.Count == 1)
						{
							obj = (obj as Grid).Children[0];
						}
						if (obj is PnRibbonButton)
						{
							dictionary.Add(obj as PnRibbonButton, ((PnRibbonButton)obj).Style);
						}
						if (obj is StackPanel)
						{
							dictionary2.Add((StackPanel)obj, ((StackPanel)obj).Orientation);
							dictionary.Add((PnRibbonButton)((StackPanel)obj).Children[0], ((PnRibbonButton)((StackPanel)obj).Children[0]).Style);
						}
					}
					foreach (object child3 in wrapPanel.Children)
					{
						object obj2 = child3;
						if (obj2 is Grid && (obj2 as Grid).Children.Count == 1)
						{
							obj2 = (obj2 as Grid).Children[0];
						}
						IPnRibbonNode pnRibbonNode3 = CurrentTabInformation.ElementNodeDictionary[obj2];
						if (pnRibbonNode3.IsDefaultSplit)
						{
							SetSplitButtonStyle(obj2 as StackPanel, pnRibbonNode2.Runtime_VisualCompretionLevel);
						}
						else if (IsExtendNode(pnRibbonNode3))
						{
							SetExtendButtonStyle(obj2 as PnRibbonButton, pnRibbonNode2.Runtime_VisualCompretionLevel);
						}
						else
						{
							SetButtonStyle(obj2 as PnRibbonButton, pnRibbonNode2.Runtime_VisualCompretionLevel);
						}
					}
					if (!(MeasurePanel(panel) > base.ActualWidth - 5.0))
					{
						break;
					}
					pnRibbonNode2.Runtime_VisualCompretionLevel++;
					flag = true;
					foreach (PnRibbonButton key in dictionary.Keys)
					{
						key.Style = dictionary[key];
					}
					foreach (StackPanel key2 in dictionary2.Keys)
					{
						key2.Orientation = dictionary2[key2];
					}
					break;
				}
			}
			else
			{
				flag = true;
			}
		}
	}

	private void TryZoomOutCurrentPanel()
	{
		StackPanel panel = CurrentTabInformation.Panel;
		double num = base.ActualWidth - 5.0;
		MeasurePanel(panel);
		bool flag = false;
		while (!flag)
		{
			int num2 = int.MaxValue;
			foreach (object child in panel.Children)
			{
				IPnRibbonNode pnRibbonNode = CurrentTabInformation.ElementNodeDictionary[child];
				if (pnRibbonNode.Runtime_VisualCompretionLevel < num2)
				{
					num2 = pnRibbonNode.Runtime_VisualCompretionLevel;
				}
			}
			if (num2 < 2)
			{
				for (int num3 = panel.Children.Count - 1; num3 >= 0; num3--)
				{
					IPnRibbonNode pnRibbonNode2 = CurrentTabInformation.ElementNodeDictionary[panel.Children[num3]];
					if (pnRibbonNode2.Runtime_VisualCompretionLevel == num2)
					{
						GroupBox groupBox = (GroupBox)panel.Children[num3];
						WrapPanel wrapPanel = (WrapPanel)groupBox.Content;
						pnRibbonNode2.Runtime_VisualCompretionLevel++;
						if (wrapPanel.Children.Count <= 1)
						{
							break;
						}
						Dictionary<PnRibbonButton, Style> dictionary = new Dictionary<PnRibbonButton, Style>();
						Dictionary<StackPanel, Orientation> dictionary2 = new Dictionary<StackPanel, Orientation>();
						foreach (object child2 in wrapPanel.Children)
						{
							object obj = child2;
							if (obj is PnRibbonButton)
							{
								dictionary.Add(obj as PnRibbonButton, ((PnRibbonButton)obj).Style);
							}
							if (obj is Grid && (obj as Grid).Children.Count == 1)
							{
								obj = (obj as Grid).Children[0];
							}
							if (obj is StackPanel)
							{
								dictionary2.Add((StackPanel)obj, ((StackPanel)obj).Orientation);
								dictionary.Add((PnRibbonButton)((StackPanel)obj).Children[0], ((PnRibbonButton)((StackPanel)obj).Children[0]).Style);
							}
						}
						groupBox.UpdateLayout();
						groupBox.Measure(new Size(base.ActualWidth, base.ActualHeight));
						double width = groupBox.DesiredSize.Width;
						foreach (object child3 in wrapPanel.Children)
						{
							object obj2 = child3;
							if (obj2 is Grid && (obj2 as Grid).Children.Count == 1)
							{
								obj2 = (obj2 as Grid).Children[0];
							}
							IPnRibbonNode pnRibbonNode3 = CurrentTabInformation.ElementNodeDictionary[obj2];
							if (pnRibbonNode3.IsDefaultSplit)
							{
								SetSplitButtonStyle(obj2 as StackPanel, pnRibbonNode2.Runtime_VisualCompretionLevel);
							}
							else if (IsExtendNode(pnRibbonNode3))
							{
								SetExtendButtonStyle(obj2 as PnRibbonButton, pnRibbonNode2.Runtime_VisualCompretionLevel);
							}
							else
							{
								SetButtonStyle(obj2 as PnRibbonButton, pnRibbonNode2.Runtime_VisualCompretionLevel);
							}
						}
						groupBox.UpdateLayout();
						groupBox.Measure(new Size(base.ActualWidth, base.ActualHeight));
						if (groupBox.DesiredSize.Width >= width)
						{
							foreach (PnRibbonButton key in dictionary.Keys)
							{
								key.Style = dictionary[key];
							}
							foreach (StackPanel key2 in dictionary2.Keys)
							{
								key2.Orientation = dictionary2[key2];
							}
						}
						else
						{
							pnRibbonNode2.Runtime_RealVisualCompretionLevel = pnRibbonNode2.Runtime_VisualCompretionLevel;
						}
						break;
					}
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				flag = MeasurePanel(panel) < num;
			}
		}
	}

	public void ChangeLanguage()
	{
		foreach (PnRibbonTabButton key in _tabDictionary.Keys)
		{
			TabInformation tabInformation = _tabDictionary[key];
			if (tabInformation.Node != null)
			{
				key.Content = _languageDictionary.GetButtonLabel(tabInformation.Node.Command.LabelId, tabInformation.Node.Command.DefaultLabel);
				_pnToolTipService.SetTooltip(key, tabInformation.Node.Command);
			}
			foreach (object key2 in tabInformation.ElementNodeDictionary.Keys)
			{
				IPnRibbonNode pnRibbonNode = tabInformation.ElementNodeDictionary[key2];
				if (key2 is PnRibbonButton)
				{
					(key2 as PnRibbonButton).Label = _languageDictionary.GetButtonLabel(pnRibbonNode.Command.LabelId, pnRibbonNode.Command.DefaultLabel);
					GeneratePnButtonToolTip(key2 as PnRibbonButton, pnRibbonNode);
				}
				if (key2 is GroupBox)
				{
					(key2 as GroupBox).Header = _languageDictionary.GetButtonLabel(pnRibbonNode.Command.LabelId, pnRibbonNode.Command.DefaultLabel);
				}
			}
		}
		UpdateTabButtonsSize();
		UpdateTabItemsSize();
		if (_fileTabbutton != null)
		{
			_fileTabbutton.Content = _languageDictionary.GetMsg2Int("Menu");
		}
		SetLanguageContextMenu();
		SetLanguageContextMenu1();
	}

	private double MeasurePanel(StackPanel panel)
	{
		double num = 0.0;
		foreach (GroupBox child in panel.Children)
		{
			child.UpdateLayout();
			child.Measure(new Size(base.ActualWidth, base.ActualHeight));
			num += child.DesiredSize.Width;
		}
		return num;
	}

	private UIElement CreateDifferentTypeGroupButton(IPnRibbonNode node)
	{
		PnRibbonButton pnRibbonButton = new PnRibbonButton
		{
			SmallImage = _pnIconsService.GetSmallIcon(node.Command.Command),
			LargeImage = _pnIconsService.GetBigIcon(node.Command.Command),
			Label = _languageDictionary.GetButtonLabel(node.Command.LabelId, node.Command.DefaultLabel),
			Focusable = false,
			IsTabStop = false
		};
		if (node.Command.PnColorIndex > 0)
		{
			pnRibbonButton.Background = _pnColorsService.GetWpfBrush(node.Command.PnColorIndex);
		}
		GeneratePnButtonToolTip(pnRibbonButton, node);
		int level = (node.IsDefaultSmall ? 1 : 0);
		if (node.IsDefaultSplit)
		{
			StackPanel stackPanel = new StackPanel
			{
				Style = (Style)TryFindResource("SplitButtonStackPane")
			};
			stackPanel.Children.Add(pnRibbonButton);
			PnRibbonButton pnRibbonButton2 = new PnRibbonButton();
			if (_pnRibbonExtenderStyle == null)
			{
				_pnRibbonExtenderStyle = (Style)TryFindResource("PnRibbonExtender");
			}
			pnRibbonButton2.Style = _pnRibbonExtenderStyle;
			pnRibbonButton2.IsExtenderForSplit = true;
			stackPanel.Children.Add(pnRibbonButton2);
			CurrentTabInformation.ElementNodeDictionary.Add(stackPanel, node);
			CurrentTabInformation.ElementNodeDictionary.Add(pnRibbonButton, node);
			CurrentTabInformation.ElementNodeDictionary.Add(pnRibbonButton2, node);
			SetSplitButtonStyle(stackPanel, level);
			GenerateSpecjalConnectionForSplit(pnRibbonButton, pnRibbonButton2, node);
			pnRibbonButton.Click += Split_Click;
			pnRibbonButton2.Click += SplitSubbutton_Click;
			return stackPanel;
		}
		CurrentTabInformation.ElementNodeDictionary.Add(pnRibbonButton, node);
		if (IsExtendNode(node))
		{
			pnRibbonButton.Click += ExtendButton_Click;
			SetExtendButtonStyle(pnRibbonButton, level);
			return pnRibbonButton;
		}
		pnRibbonButton.Click += Standard_Click;
		SetButtonStyle(pnRibbonButton, level);
		return pnRibbonButton;
	}

	private void SplitCommandConnectionFillByCfg()
	{
		foreach (Tuple<RFileRecord, RFileRecord> item2 in _ribbonDb.SplitbuttonCurrentConfig)
		{
			SplitData item = new SplitData
			{
				SplitCommand = new PnCommand(item2.Item1),
				CurrentCommand = new PnCommand(item2.Item2)
			};
			_splitCommandConnection.Add(item);
		}
	}

	private SplitData GetSplitData(IPnCommand splitCommand)
	{
		foreach (SplitData item in _splitCommandConnection)
		{
			if (item.SplitCommand.Command == splitCommand.Command)
			{
				return item;
			}
		}
		return null;
	}

	private SplitData GetSplitDataByExtenderButton(PnRibbonButton btnExtender)
	{
		foreach (SplitData item in _splitCommandConnection)
		{
			if (item.BtnExtender == btnExtender)
			{
				return item;
			}
		}
		return null;
	}

	private void GenerateSpecjalConnectionForSplit(PnRibbonButton btnMain, PnRibbonButton btnExtender, IPnRibbonNode command)
	{
		SplitData splitData = GetSplitData(command.SplitCommand);
		if (splitData == null)
		{
			splitData = new SplitData();
			_splitCommandConnection.Add(splitData);
			splitData.CurrentCommand = command.Command;
		}
		if (splitData.CurrentCommand.Command == command.Command.Command)
		{
			splitData.CurrentCommand.ToolTipId = command.Command.ToolTipId;
			splitData.CurrentCommand.LabelId = command.Command.LabelId;
			splitData.CurrentCommand.Group = command.Command.Group;
		}
		splitData.BtnMain = btnMain;
		splitData.BtnExtender = btnExtender;
		splitData.SplitCommand = command.SplitCommand;
		if (splitData.CurrentCommand != command.Command)
		{
			UpdateSplitButton(splitData);
		}
	}

	private void UpdateSplitButton(SplitData sd)
	{
		sd.BtnMain.SmallImage = _pnIconsService.GetSmallIcon(sd.CurrentCommand.Command);
		sd.BtnMain.LargeImage = _pnIconsService.GetBigIcon(sd.CurrentCommand.Command);
		sd.BtnMain.Label = _languageDictionary.GetButtonLabel(sd.CurrentCommand.LabelId, sd.CurrentCommand.DefaultLabel);
		PnRibbonNode pnRibbonNode = new PnRibbonNode(sd.CurrentCommand);
		GeneratePnButtonToolTip(sd.BtnMain, pnRibbonNode);
		CurrentTabInformation.ElementNodeDictionary[sd.BtnMain] = pnRibbonNode;
	}

	private void ExtendButton_Click(object sender, RoutedEventArgs e)
	{
		if (!CurrentTabInformation.ElementNodeDictionary.Keys.Contains(sender))
		{
			MessageBox.Show("Error - no node");
			return;
		}
		if (_lastExtBtn == e.OriginalSource)
		{
			_lastExtBtn = null;
			return;
		}
		IPnRibbonNode pnRibbonNode = CurrentTabInformation.ElementNodeDictionary[sender];
		if (pnRibbonNode.Children == null || pnRibbonNode.Children.Count == 0)
		{
			return;
		}
		_codePopup = new System.Windows.Controls.Primitives.Popup();
		PopupElementNodeDictionary = new Dictionary<object, IPnRibbonNode>();
		int num = 220;
		ScrollViewer scrollViewer = new ScrollViewer
		{
			Background = new SolidColorBrush(Colors.White),
			Width = num * pnRibbonNode.UnspecifiedAdditionalInt1 + 17
		};
		StackPanel stackPanel2 = (StackPanel)(scrollViewer.Content = new StackPanel());
		stackPanel2.Orientation = Orientation.Vertical;
		foreach (IPnRibbonNode child2 in pnRibbonNode.Children)
		{
			if (child2.OrginalType != 11)
			{
				continue;
			}
			Label element = new Label
			{
				Background = new SolidColorBrush(Colors.LightGray),
				Content = _languageDictionary.GetButtonLabel(child2.Command.LabelId, child2.Command.DefaultLabel)
			};
			stackPanel2.Children.Add(element);
			WrapPanel wrapPanel = new WrapPanel();
			if (child2.Children == null && child2.Command.AddValue3 != null && child2.Command.AddValue3.Trim() != string.Empty)
			{
				_ribbonMainWindowConnector.FillMenu(0, wrapPanel, child2.Command.AddValue3.ToUpper(), child2.Command.AddValue2, null, null);
			}
			if (child2.Children != null && child2.Children.Count > 0)
			{
				foreach (IPnRibbonNode child3 in child2.Children)
				{
					if (_buttonStyleList == null)
					{
						_buttonStyleList = (Style)TryFindResource("PnRibbonButtonOnList");
					}
					PnRibbonButton pnRibbonButton = new PnRibbonButton
					{
						SmallImage = _pnIconsService.GetSmallIcon(child3.Command.Command),
						LargeImage = _pnIconsService.GetBigIcon(child3.Command.Command),
						Label = _languageDictionary.GetButtonLabel(child3.Command.LabelId, child3.Command.DefaultLabel),
						Style = _buttonStyleList
					};
					pnRibbonButton.Click += PopuUpButtonClick;
					pnRibbonButton.MouseLeave += Btn_MouseLeave;
					PopupElementNodeDictionary.Add(pnRibbonButton, child3);
					wrapPanel.Children.Add(pnRibbonButton);
					GeneratePnButtonToolTip(pnRibbonButton, child3);
				}
			}
			stackPanel2.Children.Add(wrapPanel);
		}
		Border child = new Border
		{
			Child = scrollViewer,
			BorderThickness = new Thickness(1.0),
			BorderBrush = new SolidColorBrush(Colors.Black)
		};
		_codePopup.Child = child;
		_codePopup.PlacementTarget = (UIElement)sender;
		_codePopup.StaysOpen = false;
		_codePopup.IsOpen = true;
		_lastExtBtn = e.OriginalSource;
		_codePopup.Closed += CodePopup_Closed;
		MarkButtonsAsConnectedWithOpenPopup(sender);
		scrollViewer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		_codePopup.Width = scrollViewer.DesiredSize.Width;
		_codePopup.Height = scrollViewer.DesiredSize.Height + 2.0;
	}

	private void MarkButtonsAsConnectedWithOpenPopup(object sender)
	{
		if (_buttonsConnectedWithOpenPopup == null)
		{
			_buttonsConnectedWithOpenPopup = new List<PnRibbonButton>();
		}
		if (sender is StackPanel && (sender as StackPanel).Children.Count == 2)
		{
			((sender as StackPanel).Children[0] as PnRibbonButton).IsButtonPopupOpen = true;
			((sender as StackPanel).Children[1] as PnRibbonButton).IsButtonPopupOpen = true;
			_buttonsConnectedWithOpenPopup.Add((sender as StackPanel).Children[0] as PnRibbonButton);
			_buttonsConnectedWithOpenPopup.Add((sender as StackPanel).Children[1] as PnRibbonButton);
		}
		if (sender is PnRibbonButton)
		{
			(sender as PnRibbonButton).IsButtonPopupOpen = true;
			_buttonsConnectedWithOpenPopup.Add(sender as PnRibbonButton);
		}
	}

	public bool IsSubMenu()
	{
		return CurrentTabInformation.Type == TabType.Sub;
	}

	private void UnMarkButtonsConnectedWithOpenPopup()
	{
		if (_buttonsConnectedWithOpenPopup == null)
		{
			return;
		}
		foreach (PnRibbonButton item in _buttonsConnectedWithOpenPopup)
		{
			item.IsButtonPopupOpen = false;
		}
		_buttonsConnectedWithOpenPopup.Clear();
	}

	private void Btn_MouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is PnRibbonButton)
		{
			PnRibbonButton pnRibbonButton = sender as PnRibbonButton;
			if (pnRibbonButton.ToolTip != null && (pnRibbonButton.ToolTip as PnFunctionToolTip).IsOpen)
			{
				(pnRibbonButton.ToolTip as PnFunctionToolTip).IsOpen = false;
			}
		}
	}

	private void Btn_LostFocus(object sender, RoutedEventArgs e)
	{
	}

	private void GeneratePnButtonToolTip(PnRibbonButton btn, IPnRibbonNode ns)
	{
		_pnToolTipService.SetTooltip(btn, ns.Command);
		btn.ToolTipOpening += Btn_ToolTipOpening;
		if (!IsExtendNode(ns))
		{
			if (ns.OrginalType == 10)
			{
				btn.ContextMenu = _splitPopupContextMenu;
			}
			else
			{
				btn.ContextMenu = _simpleButtonContextMenu;
			}
			btn.ContextMenuOpening += Btn_ContextMenuOpening;
		}
	}

	private void Btn_ToolTipOpening(object sender, ToolTipEventArgs e)
	{
		if (!(sender as PnRibbonButton).IsMouseOver)
		{
			e.Handled = true;
		}
	}

	private void CodePopup_Closed(object sender, EventArgs e)
	{
		_ribbonMainWindowConnector.ResetAllPreviewNeeds();
		if (_lastExtBtn != null)
		{
			if (!((UIElement)_lastExtBtn).IsMouseCaptured)
			{
				_lastExtBtn = null;
			}
			UnMarkButtonsConnectedWithOpenPopup();
		}
	}

	private void SplitSubbutton_Click(object sender, RoutedEventArgs e)
	{
		object parent = LogicalTreeHelper.GetParent(sender as UIElement);
		ExtendButton_Click(parent, e);
	}

	public bool IsEndByButton3Possible()
	{
		TabInformation currentTabInformation = CurrentTabInformation;
		if (currentTabInformation == null)
		{
			return false;
		}
		return currentTabInformation.Type == TabType.Sub;
	}

	private void Standard_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentTabInformation.ElementNodeDictionary.ContainsKey(sender))
		{
			if (CurrentTabInformation.Type == TabType.Sub && CurrentTabInformation.ElementNodeDictionary[sender].Command.Group != 81)
			{
				_subMenuConnector.CallPnCommandSubmenuEdition(CurrentTabInformation.ElementNodeDictionary[sender].Command);
			}
			else
			{
				_ribbonMainWindowConnector.CallPnCommand(CurrentTabInformation.ElementNodeDictionary[sender].Command);
			}
		}
		if (_sub != null)
		{
			_sub.IsOpen = false;
		}
	}

	private void Split_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentTabInformation.ElementNodeDictionary.ContainsKey(sender))
		{
			_ribbonMainWindowConnector.CallPnCommand(CurrentTabInformation.ElementNodeDictionary[sender].Command);
		}
		if (_sub != null)
		{
			_sub.IsOpen = false;
		}
	}

	public void CloseCodePopup()
	{
		if (_codePopup != null)
		{
			_codePopup.IsOpen = false;
		}
		if (_sub != null)
		{
			_sub.IsOpen = false;
		}
	}

	private void PopuUpButtonClick(object sender, RoutedEventArgs e)
	{
		CloseCodePopup();
		if (_sub != null)
		{
			_sub.IsOpen = false;
		}
		if (!PopupElementNodeDictionary.ContainsKey(sender))
		{
			return;
		}
		_ribbonMainWindowConnector.CallPnCommand(PopupElementNodeDictionary[sender].Command);
		if (_configProvider.InjectOrCreate<GeneralUserSettingsConfig>().RibbonSplitbuttonAutoset)
		{
			SplitData splitDataByExtenderButton = GetSplitDataByExtenderButton(_lastExtBtn as PnRibbonButton);
			if (splitDataByExtenderButton != null && splitDataByExtenderButton.BtnMain != null)
			{
				splitDataByExtenderButton.CurrentCommand = PopupElementNodeDictionary[sender].Command;
				UpdateSplitButton(splitDataByExtenderButton);
			}
		}
	}

	private void SetAsDefaultSplitButtonCommand_Click(object sender, RoutedEventArgs e)
	{
		CloseCodePopup();
		if (_lastContextMenuOpening != null && _lastContextMenuOpening is PnRibbonButton)
		{
			SplitData splitDataByExtenderButton = GetSplitDataByExtenderButton(_lastExtBtn as PnRibbonButton);
			if (splitDataByExtenderButton != null && splitDataByExtenderButton.BtnMain != null)
			{
				splitDataByExtenderButton.CurrentCommand = PopupElementNodeDictionary[_lastContextMenuOpening as PnRibbonButton].Command;
				UpdateSplitButton(splitDataByExtenderButton);
			}
		}
	}

	private bool IsExtendNode(IPnRibbonNode node)
	{
		if (node.Command.Group != 100)
		{
			return false;
		}
		if (node.Children == null)
		{
			return false;
		}
		return node.Children.Count > 0;
	}

	private void SetSplitButtonStyle(StackPanel stackButton, int level)
	{
		IPnRibbonNode pnRibbonNode = CurrentTabInformation.ElementNodeDictionary[stackButton];
		SetButtonStyle((PnRibbonButton)stackButton.Children[0], level, pnRibbonNode);
		if (level == 0 && !pnRibbonNode.IsDefaultSmall)
		{
			stackButton.Orientation = Orientation.Vertical;
		}
		else
		{
			stackButton.Orientation = Orientation.Horizontal;
		}
	}

	private void SetButtonStyle(PnRibbonButton button, int level)
	{
		SetButtonStyle(button, level, CurrentTabInformation.ElementNodeDictionary[button]);
	}

	private void SetButtonStyle(PnRibbonButton button, int level, IPnRibbonNode node)
	{
		if (_buttonStyle32 == null)
		{
			_buttonStyle32 = (Style)TryFindResource("PnRibbonButtonStyle32_full");
		}
		if (_buttonStyle16Label == null)
		{
			_buttonStyle16Label = (Style)TryFindResource("PnRibbonButtonStyle16_full");
		}
		if (_buttonStyle16 == null)
		{
			_buttonStyle16 = (Style)TryFindResource("PnRibbonButtonStyle16_small");
		}
		switch (level)
		{
		case 0:
			if (node.IsDefaultSmall)
			{
				button.Style = _buttonStyle16Label;
			}
			else
			{
				button.Style = _buttonStyle32;
			}
			break;
		case 1:
			button.Style = _buttonStyle16Label;
			break;
		case 2:
			button.Style = _buttonStyle16;
			break;
		}
	}

	private void SetExtendButtonStyle(PnRibbonButton button, int level)
	{
		if (_extendButtonStyle32 == null)
		{
			_extendButtonStyle32 = (Style)TryFindResource("PnRibbonExtendButtonStyle32_full");
		}
		if (_extendButtonStyle16Label == null)
		{
			_extendButtonStyle16Label = (Style)TryFindResource("PnRibbonExtendButtonStyle16_full");
		}
		if (_extendButtonStyle16 == null)
		{
			_extendButtonStyle16 = (Style)TryFindResource("PnRibbonExtendButtonStyle16_small");
		}
		IPnRibbonNode pnRibbonNode = CurrentTabInformation.ElementNodeDictionary[button];
		switch (level)
		{
		case 0:
			if (pnRibbonNode.IsDefaultSmall)
			{
				button.Style = _extendButtonStyle16Label;
			}
			else
			{
				button.Style = _extendButtonStyle32;
			}
			break;
		case 1:
			button.Style = _extendButtonStyle16Label;
			break;
		case 2:
			button.Style = _extendButtonStyle16;
			break;
		}
	}

	private PnRibbonTabButton GetRibbonTabButtonByVisualGroup(int id)
	{
		foreach (PnRibbonTabButton key in _tabDictionary.Keys)
		{
			if (_tabDictionary[key].Node != null && _tabDictionary[key].Node.Command.AddValue2 == id)
			{
				return key;
			}
		}
		return null;
	}

	public void ExternalAppearanceModification(int command, int id)
	{
		PnRibbonTabButton ribbonTabButtonByVisualGroup = GetRibbonTabButtonByVisualGroup(id);
		if (ribbonTabButtonByVisualGroup != null)
		{
			switch (command)
			{
			case 1:
				ActivateTab(ribbonTabButtonByVisualGroup);
				break;
			case 2:
				ribbonTabButtonByVisualGroup.Visibility = Visibility.Visible;
				UpdateTabButtonsSize();
				break;
			case 3:
				ribbonTabButtonByVisualGroup.Visibility = Visibility.Collapsed;
				UpdateTabButtonsSize();
				break;
			}
		}
	}

	public void OnRibbonSubTabShow(int v)
	{
		_latestSubtabGroup = v;
		_subtabbtn = GetRibbonTabButtonByVisualGroup(_latestSubtabGroup);
		_subtabbtn.Visibility = Visibility.Visible;
		_subtabbtn.IsEnabled = true;
		_subtabshowsource = true;
		Tab_Click(_subtabbtn, null);
		SetTabEnableForSubtab(flag: false, _subtabbtn);
		_isSubtab = true;
	}

	public void OnRibbonSubTabHide()
	{
		PnRibbonTabButton ribbonTabButtonByVisualGroup = GetRibbonTabButtonByVisualGroup(_latestSubtabGroup);
		if (ribbonTabButtonByVisualGroup != null)
		{
			ribbonTabButtonByVisualGroup.Visibility = Visibility.Collapsed;
			SetTabEnableForSubtab(flag: true, null);
			_notShowByClick = true;
			_subtabshowsource = true;
			Tab_Click(_tabDictionary[ribbonTabButtonByVisualGroup].Before.Tab, null);
			_isSubtab = false;
		}
	}

	public void ActivateModule(string moduleName, string ribbonModuleName)
	{
		if (moduleName != null && moduleName != string.Empty)
		{
			SaveSettings();
		}
		_moduleName = moduleName;
		_ribbonModuleName = ribbonModuleName;
		_ribbonDb = new PnRibbonDatabase(_pnPathService, _logCenterService);
		_ribbonDb.ImportDatabase(moduleName, ribbonModuleName);
		LoadRibbon();
	}

	public void SaveSettings()
	{
		if (_moduleName == null)
		{
			return;
		}
		try
		{
			string text = _pnPathService.BuildPath("pn.rfiles\\" + _moduleName);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			using StreamWriter streamWriter = File.CreateText(_pnPathService.BuildPath(text, "\\SplitButtonsConfigP4"));
			foreach (SplitData item in _splitCommandConnection)
			{
				streamWriter.WriteLine(item.SplitCommand.GetRFileRecord().GetOutputText());
				streamWriter.WriteLine(item.CurrentCommand.GetRFileRecord().GetOutputText());
			}
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	public void BlockMenuEvent()
	{
		foreach (TabInformation value in _tabDictionary.Values)
		{
			if (_latestSubtabGroup > 0 && GetRibbonTabButtonByVisualGroup(_latestSubtabGroup) != null && _tabDictionary[GetRibbonTabButtonByVisualGroup(_latestSubtabGroup)] == value)
			{
				continue;
			}
			value.Tab.IsEnabled = false;
			foreach (object key in value.ElementNodeDictionary.Keys)
			{
				if (key is PnRibbonButton)
				{
					(key as PnRibbonButton).IsEnabled = false;
					(key as PnRibbonButton).Opacity = 0.5;
				}
				if (key is StackPanel)
				{
					(key as StackPanel).IsEnabled = false;
				}
			}
		}
	}

	public void UnBlockMenuEvent()
	{
		foreach (TabInformation value in _tabDictionary.Values)
		{
			if (_latestSubtabGroup > 0 && GetRibbonTabButtonByVisualGroup(_latestSubtabGroup) != null && _tabDictionary[GetRibbonTabButtonByVisualGroup(_latestSubtabGroup)] == value)
			{
				continue;
			}
			value.Tab.IsEnabled = true;
			foreach (object key in value.ElementNodeDictionary.Keys)
			{
				if (key is PnRibbonButton)
				{
					(key as PnRibbonButton).IsEnabled = true;
					(key as PnRibbonButton).Opacity = 1.0;
				}
				if (key is StackPanel)
				{
					(key as StackPanel).IsEnabled = true;
				}
			}
		}
	}

	public void BlockMenuEventNotDisplay()
	{
		foreach (TabInformation value in _tabDictionary.Values)
		{
			if (_latestSubtabGroup > 0 && GetRibbonTabButtonByVisualGroup(_latestSubtabGroup) != null && _tabDictionary[GetRibbonTabButtonByVisualGroup(_latestSubtabGroup)] == value)
			{
				continue;
			}
			value.Tab.IsEnabled = false;
			foreach (object key in value.ElementNodeDictionary.Keys)
			{
				if (key is PnRibbonButton)
				{
					(key as PnRibbonButton).IsEnabled = false;
					(key as PnRibbonButton).Opacity = 0.5;
				}
				if (key is StackPanel)
				{
					(key as StackPanel).IsEnabled = false;
				}
			}
		}
	}

	public void UnBlockMenuEventNotDisplay()
	{
		foreach (TabInformation value in _tabDictionary.Values)
		{
			if (_latestSubtabGroup > 0 && GetRibbonTabButtonByVisualGroup(_latestSubtabGroup) != null && _tabDictionary[GetRibbonTabButtonByVisualGroup(_latestSubtabGroup)] == value)
			{
				continue;
			}
			value.Tab.IsEnabled = true;
			foreach (object key in value.ElementNodeDictionary.Keys)
			{
				if (key is PnRibbonButton)
				{
					(key as PnRibbonButton).IsEnabled = true;
					(key as PnRibbonButton).Opacity = 1.0;
				}
				if (key is StackPanel)
				{
					(key as StackPanel).IsEnabled = true;
				}
			}
		}
	}

	private void SetTabEnableForSubmenu(TabInformation tabSub, bool flag)
	{
		foreach (PnRibbonTabButton key in _tabDictionary.Keys)
		{
			key.IsEnabled = flag;
		}
		_fileTabbutton.IsEnabled = flag;
	}

	public void SetTabEnableForSubtab(bool flag, PnRibbonTabButton notthis)
	{
		foreach (PnRibbonTabButton key in _tabDictionary.Keys)
		{
			if (key != notthis)
			{
				key.IsEnabled = flag;
			}
		}
		_fileTabbutton.IsEnabled = flag;
	}

	public void OnSubMenuHide()
	{
		TabInformation tab = GetTab(TabType.Sub);
		if (tab != null && CurrentTabInformation == tab)
		{
			tab.Tab.Visibility = Visibility.Collapsed;
			if (!_isSubtab)
			{
				SetTabEnableForSubmenu(tab, flag: true);
			}
			if (tab.Before != null)
			{
				_notShowByClick = true;
				Tab_Click(tab.Before.Tab, null);
			}
			_lastSubButton = null;
			if (_subtabbtn != null)
			{
				_subtabbtn.IsEnabled = true;
			}
		}
	}

	public void SetCustomToolbar(List<IRFileRecord> items, bool visible)
	{
		if (_moduleName == string.Empty)
		{
			return;
		}
		TabInformation tab = GetTab(TabType.User);
		if (tab == null)
		{
			return;
		}
		tab.ElementNodeDictionary.Clear();
		tab.IsValid = false;
		PnRibbonNode pnRibbonNode = new PnRibbonNode(items);
		pnRibbonNode.Command.DefaultLabel = "Firma";
		tab.Node = pnRibbonNode;
		tab.ElementNodeDictionary.Add(tab.Tab, pnRibbonNode);
		if (visible)
		{
			tab.Tab.Visibility = Visibility.Visible;
			if (CurrentTabInformation == tab)
			{
				ActivationFromSource(CurrentTabInformation);
			}
		}
		else
		{
			if (tab.Tab.Visibility == Visibility.Visible && tab.Tab.IsTabSelected)
			{
				ActivateFirstVisibleTab();
			}
			tab.Tab.Visibility = Visibility.Collapsed;
		}
	}

	private TabInformation GetTab(TabType type)
	{
		foreach (TabInformation value in _tabDictionary.Values)
		{
			if (value.Type == type)
			{
				return value;
			}
		}
		return null;
	}

	public void ActivateFirstVisibleTab()
	{
		int num = 0;
		bool flag = false;
		while (!flag && num < _tabDictionary.Count)
		{
			TabInformation tabInformation = _tabDictionary.Values.ElementAt(num);
			if (tabInformation.Tab.Visibility == Visibility.Visible)
			{
				flag = true;
				Tab_Click(tabInformation.Tab, null);
			}
			num++;
		}
	}

	public void OnSubMenuShow(MFileExpert expert)
	{
		TabInformation tab = GetTab(TabType.Sub);
		tab.Tab.Content = $"SUB ({expert.m_filename})";
		if (tab != null)
		{
			PnRibbonNode pnRibbonNode = new PnRibbonNode(expert);
			tab.ElementNodeDictionary.Clear();
			tab.IsValid = false;
			tab.Node = pnRibbonNode;
			pnRibbonNode.Command.DefaultLabel = "SUB";
			tab.ElementNodeDictionary.Add(tab.Tab, pnRibbonNode);
			tab.Tab.Visibility = Visibility.Visible;
			ActivationFromSource(tab);
			SetTabEnableForSubmenu(tab, flag: false);
			tab.Tab.IsEnabled = true;
		}
	}

	private void Userdata_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
	}

	public void CreateContextMenuForQatOnly()
	{
		_simpleButtonContextMenu = new ContextMenu();
		SetLanguageContextMenu1();
	}

	public void CreateContextMenuForQatAndSplitPopup()
	{
		_splitPopupContextMenu = new ContextMenu();
		SetLanguageContextMenu();
	}

	private void SetLanguageContextMenu1()
	{
		_simpleButtonContextMenu.Items.Clear();
		_simpleButtonContextMenu.Items.Add(Create_MenuItem_QAT_BR());
		_simpleButtonContextMenu.Items.Add(Create_MenuItem_QAT_BL());
		_simpleButtonContextMenu.Items.Add(Create_MenuItem_QAT_TR());
		_simpleButtonContextMenu.Items.Add(Create_MenuItem_QAT_TL());
	}

	private void SetLanguageContextMenu()
	{
		_splitPopupContextMenu.Items.Clear();
		_splitPopupContextMenu.Items.Add(Create_RibbonMenuItem_SetSplit());
		_splitPopupContextMenu.Items.Add(new Separator());
		_splitPopupContextMenu.Items.Add(Create_MenuItem_QAT_BR());
		_splitPopupContextMenu.Items.Add(Create_MenuItem_QAT_BL());
		_splitPopupContextMenu.Items.Add(Create_MenuItem_QAT_TR());
		_splitPopupContextMenu.Items.Add(Create_MenuItem_QAT_TL());
	}

	private void Btn_ContextMenuOpening(object sender, ContextMenuEventArgs e)
	{
		_lastContextMenuOpening = sender;
	}

	private object Create_RibbonMenuItem_SetSplit()
	{
		MenuItem menuItem = new MenuItem();
		menuItem.Header = _languageDictionary.GetMsg2Int("Set as default SplitButton command");
		menuItem.Click += SetAsDefaultSplitButtonCommand_Click;
		return menuItem;
	}

	private object Create_MenuItem_QAT_BR()
	{
		MenuItem menuItem = new MenuItem();
		menuItem.Header = _languageDictionary.GetMsg2Int("Add to Quick Access Toolbar: Bottom Right");
		menuItem.Click += AddToQAT_BR_Click;
		return menuItem;
	}

	private IRFileRecord GetContrloForSubMenu(PnRibbonButton btn)
	{
		if (CurrentTabInformation.ElementNodeDictionary.ContainsKey(btn))
		{
			return CurrentTabInformation.ElementNodeDictionary[btn].Command.GetRFileRecord();
		}
		if (PopupElementNodeDictionary != null && PopupElementNodeDictionary.ContainsKey(btn))
		{
			CloseCodePopup();
			return PopupElementNodeDictionary[btn].Command.GetRFileRecord();
		}
		return null;
	}

	private void AddToQAT_BR_Click(object sender, RoutedEventArgs e)
	{
		if (_lastContextMenuOpening != null && _lastContextMenuOpening is PnRibbonButton)
		{
			_ribbonMainWindowConnector.AddQatButtonToToolbar(4, GetContrloForSubMenu(_lastContextMenuOpening as PnRibbonButton));
		}
	}

	private void AddToQAT_BL_Click(object sender, RoutedEventArgs e)
	{
		if (_lastContextMenuOpening != null && _lastContextMenuOpening is PnRibbonButton)
		{
			_ribbonMainWindowConnector.AddQatButtonToToolbar(3, GetContrloForSubMenu(_lastContextMenuOpening as PnRibbonButton));
		}
	}

	private void AddToQAT_TR_Click(object sender, RoutedEventArgs e)
	{
		if (_lastContextMenuOpening != null && _lastContextMenuOpening is PnRibbonButton)
		{
			_ribbonMainWindowConnector.AddQatButtonToToolbar(2, GetContrloForSubMenu(_lastContextMenuOpening as PnRibbonButton));
		}
	}

	private void AddToQAT_TL_Click(object sender, RoutedEventArgs e)
	{
		if (_lastContextMenuOpening != null && _lastContextMenuOpening is PnRibbonButton)
		{
			_ribbonMainWindowConnector.AddQatButtonToToolbar(1, GetContrloForSubMenu(_lastContextMenuOpening as PnRibbonButton));
		}
	}

	private object Create_MenuItem_QAT_BL()
	{
		MenuItem menuItem = new MenuItem();
		menuItem.Header = _languageDictionary.GetMsg2Int("Add to Quick Access Toolbar: Bottom Left");
		menuItem.Click += AddToQAT_BL_Click;
		return menuItem;
	}

	private object Create_MenuItem_QAT_TR()
	{
		MenuItem menuItem = new MenuItem();
		menuItem.Header = _languageDictionary.GetMsg2Int("Add to Quick Access Toolbar: Top Right");
		menuItem.Click += AddToQAT_TR_Click;
		return menuItem;
	}

	private object Create_MenuItem_QAT_TL()
	{
		MenuItem menuItem = new MenuItem();
		menuItem.Header = _languageDictionary.GetMsg2Int("Add to Quick Access Toolbar: Top Left");
		menuItem.Click += AddToQAT_TL_Click;
		return menuItem;
	}

	private void UserControl_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
	}

	public void ChangeRibbonMode(bool result)
	{
		if (result != PulldownMode)
		{
			ChangeRibbonMode();
		}
	}

	public void ChangeRibbonMode()
	{
		if (!PulldownMode)
		{
			PulldownMode = true;
			base.Height = 24.0;
		}
		else
		{
			PulldownMode = false;
			base.Height = double.NaN;
			if (_sub?.Child != null && (_sub.Child as Border).Child == CurrentTabInformation.Panel)
			{
				(_sub.Child as Border).Child = null;
				MainRibbonZone.Child = CurrentTabInformation.Panel;
			}
		}
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		generalUserSettingsConfig.RibbonCompactMode = PulldownMode;
		_configProvider.Push(generalUserSettingsConfig);
		_configProvider.Save<GeneralUserSettingsConfig>();
		if (_sub == null)
		{
			_sub = new System.Windows.Controls.Primitives.Popup
			{
				Height = 110.0,
				Width = base.ActualWidth
			};
			Border border = new Border
			{
				BorderThickness = new Thickness(1.0)
			};
			border.SetResourceReference(Control.BackgroundProperty, "Pn4Ribbon.RibbonBackgroundBrush");
			border.SetResourceReference(Control.BorderBrushProperty, "Pn4Ribbon.RibbonBorderBrush");
			_sub.Child = border;
			_sub.PlacementTarget = this;
			_sub.IsOpen = false;
			_sub.StaysOpen = false;
			_sub.Closed += Sub_Closed;
			_sub.PreviewMouseDown += Sub_PreviewMouseDown;
		}
	}

	private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
	}

	public void ShowSubMenuRibbonStyle(string tabName, string ribbonFileName, int assignedToTabId)
	{
		List<IRFileRecord> rfiledb = RFiles.ReadFile("2D", ribbonFileName, _pnPathService);
		TabInformation tab = GetTab(TabType.Sub);
		if (tab != null)
		{
			tab.Tab.Content = $"SUB ({tabName})";
			PnRibbonNode pnRibbonNode = new PnRibbonNode
			{
				Command = new PnCommand()
			};
			new PnRibbonDatabase(_pnPathService, _logCenterService).RFileChildGenerate(pnRibbonNode, rfiledb);
			tab.ElementNodeDictionary.Clear();
			tab.IsValid = false;
			tab.Node = pnRibbonNode;
			pnRibbonNode.Command.DefaultLabel = "SUB_RF";
			pnRibbonNode.Command.AddValue2 = assignedToTabId;
			tab.ElementNodeDictionary.Add(tab.Tab, pnRibbonNode);
			tab.Tab.Visibility = Visibility.Visible;
			ActivationFromSource(tab);
			SetTabEnableForSubmenu(tab, flag: false);
			tab.Tab.IsEnabled = true;
		}
	}

	private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
	{
	}

	public void CloseSubMenuRibbonStyle()
	{
		OnSubMenuHide();
	}
}
