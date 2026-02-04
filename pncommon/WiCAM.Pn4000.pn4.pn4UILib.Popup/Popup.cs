using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Extensions;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PKernelFlow.Adapters.Data;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.pn4Services.CADGEO;
using WiCAM.Pn4000.pn4.pn4UILib.Popup.Information;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup;

public partial class Popup : Window, IComponentConnector
{
	public delegate void PopupAnswer(int answer);

	private class ExpanderInfo
	{
		public TextBox Textbox { get; set; }

		public int Type { get; set; }
	}

	[Serializable]
	public class PopUpInfo
	{
		private double _popupLeft;

		private double _popupTop;

		private double _popupWidth;

		private double _popupHeight;

		public string[] Filters = new string[5];

		public double PopupLeft
		{
			get
			{
				return this._popupLeft;
			}
			set
			{
				this._popupLeft = value;
			}
		}

		public double PopupTop
		{
			get
			{
				return this._popupTop;
			}
			set
			{
				this._popupTop = value;
			}
		}

		public double PopupWidth
		{
			get
			{
				return this._popupWidth;
			}
			set
			{
				this._popupWidth = value;
			}
		}

		public double PopupHeight
		{
			get
			{
				return this._popupHeight;
			}
			set
			{
				this._popupHeight = value;
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public static PopUpInfo Load(string filename, ILogCenterService logCenterService)
		{
			if (!File.Exists(filename))
			{
				return null;
			}
			PopUpInfo popUpInfo = null;
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(PopUpInfo));
			try
			{
				using FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				return (PopUpInfo)xmlSerializer.Deserialize(stream);
			}
			catch (Exception e)
			{
				logCenterService.CatchRaport(e);
				return null;
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public void Save(string filename, ILogCenterService logCenterService)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(PopUpInfo));
			try
			{
				using TextWriter textWriter = new StreamWriter(filename);
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception e)
			{
				logCenterService.CatchRaport(e);
			}
		}
	}

	private string _storeFolder;

	private readonly PopupKernelDataInterface _popupInterface;

	private readonly IPKernelFlowGlobalDataService _pKernelFlowGlobalData;

	private readonly ILanguageDictionary _languageDictionary;

	private readonly LikeModalMode _likeModalMode;

	private readonly IPnIconsService _pnIconsService;

	private readonly IPnColorsService _pnColorsService;

	private readonly ILogCenterService _logCenterService;

	private readonly IConfigProvider _configProvider;

	private readonly bool _useOnlineHelp;

	private PopupAnswer _onPopupAnswer;

	private readonly Window _mainWindow;

	private string _mPfileName;

	private string _mWindowLabel;

	private int[] _bval;

	private int _maxId4;

	public bool ListMode;

	private int _staticListMode;

	private bool _oldStyleData;

	private int _lastVisibleTab;

	private PnImageSource _imageSource;

	private FontFamily _stdFont;

	private FontFamily _monospaceFont;

	private bool _updateFilters;

	private Dictionary<Expander, ExpanderInfo> _mExpanders;

	private bool _fullyDynamicMode;

	private FullyDynamicPopupDefinition _definition;

	private bool _useSortingButtons;

	private bool is_EditButtonVisible;

	private bool _usePopupTableType1;

	private IQuickTable MyQuickTable;

	private ArchiveControlPopupModel _archiveControlModel;

	private Control _focusControl;

	private Dictionary<ToolTip, string> _tooltipHtext;

	private int _canvasMaxx;

	private int _canvasMaxy;

	private StackPanel _currentStackpanel;

	private readonly QuickTable _internalTable;

	private TextBox _textBoxConnectedWithCallendar;

	private Expander _lastExpander;

	private Dictionary<UIElement, PopupLine> _uiElementPopupLineConnection;

	public List<PopupLine> Lines;

	private int _popupButtonMode = -1;

	private int _oldLanguageBtnsSetup = -1;

	private int _filtersCount;

	private int _lastExitcode = -1;

	private string _lastPopupname = string.Empty;

	public bool JustClose;

	private string[] _lastFilters = new string[5];

	private bool _ignoreNextUp;

	public int ExitCode;

	private Dictionary<object, string> _logicalDict;

	private global::System.Windows.Controls.Primitives.Popup _subPopup;

	private readonly global::System.Windows.Controls.Calendar _calendar;

	private DispatcherTimer dispatcherTimer;

	private string dispatcher_string = string.Empty;

	private string dispatcher_string1 = string.Empty;

	private string dispatcher_string2 = string.Empty;

	private global::System.Windows.Controls.Primitives.Popup _codePopup;

	private readonly IFactorio _factorio;

	private Image latest_codePopupImage;

	private const uint WsExContexthelp = 1024u;

	private const uint WsMinimizebox = 131072u;

	private const uint WsMaximizebox = 65536u;

	private const int GwlStyle = -16;

	private const int GwlExstyle = -20;

	private const int SwpNosize = 1;

	private const int SwpNomove = 2;

	private const int SwpNozorder = 4;

	private const int SwpFramechanged = 32;

	private const int WmSyscommand = 274;

	private const int ScContexthelp = 61824;

	public nint Handle => new WindowInteropHelper(this).Handle;

	public Popup(IFactorio factorio, PopupKernelDataInterface popupInterface, ILogCenterService logCenterService, IConfigProvider configProvider, IPnIconsService iconsService, IPnColorsService colorsService, IPKernelFlowGlobalDataService pKernelFlowGlobalDataService, ILanguageDictionary languageDictionary, LikeModalMode likeModalMode, QuickTable quickTable)
	{
		base.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentUICulture.IetfLanguageTag);
		this._factorio = factorio;
		this._pnIconsService = iconsService;
		this._pnColorsService = colorsService;
		this._logCenterService = logCenterService;
		this._configProvider = configProvider;
		this._internalTable = quickTable;
		this._pKernelFlowGlobalData = pKernelFlowGlobalDataService;
		this._mainWindow = this._pKernelFlowGlobalData.MainWindow;
		this._popupInterface = popupInterface;
		this._internalTable.SetInterface(this._popupInterface);
		base.Owner = this._mainWindow;
		this._languageDictionary = languageDictionary;
		this._likeModalMode = likeModalMode;
		this._useOnlineHelp = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>().UseOnlineHelp;
		this._calendar = new global::System.Windows.Controls.Calendar();
	}

	public void Init(PopupAnswer onPopupAnswer, string storeFolder)
	{
		this._onPopupAnswer = onPopupAnswer;
		this._storeFolder = storeFolder;
		this.InitializeComponent();
		EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.ToolTipOpeningEvent, new ToolTipEventHandler(ToolTipHandler));
	}

	private void ToolTipHandler(object sender, ToolTipEventArgs e)
	{
		if (this._tooltipHtext != null && e.Source is FrameworkElement frameworkElement && frameworkElement.ToolTip is ToolTip)
		{
			ToolTip toolTip = (ToolTip)frameworkElement.ToolTip;
			if (this._tooltipHtext.ContainsKey(toolTip))
			{
				Image image = new Image
				{
					SnapsToDevicePixels = true
				};
				image.BeginInit();
				image.Source = this._imageSource.GetImageSource(this._tooltipHtext[toolTip], 200, 200, 0);
				image.EndInit();
				toolTip.Content = image;
				this._tooltipHtext.Remove(toolTip);
			}
		}
	}

	public void FullyDynamicSetup(FullyDynamicPopupDefinition definition)
	{
		this._fullyDynamicMode = true;
		this._definition = definition;
		this._mPfileName = this._definition.Name;
		this._updateFilters = !(this._mPfileName != string.Empty) || !(this._lastPopupname == this._mPfileName) || this._lastExitcode != 21;
		if (!this._updateFilters)
		{
			return;
		}
		this._lastPopupname = this._mPfileName;
		this.SetupFontsAndElse();
		if (this._updateFilters)
		{
			this.UpdateFilters();
		}
		if (this.ListMode)
		{
			this.MyQuickTable.SetFilter(string.Empty, string.Empty, string.Empty);
		}
		this._mWindowLabel = definition.Label;
		this.ListMode = definition.ListMode;
		this._staticListMode = definition.ListModeType;
		this.SetupTableMode();
		this.SetWindowTitle();
		this._bval = definition.Manipulators;
		this.StandardButtonsSetup(useSortingButtons: false);
		this.Lines = definition.Lines;
		if (definition.Tabs.Count == 0)
		{
			this.SetupNoTabs();
		}
		else
		{
			this.SetupTabs();
			foreach (string tab in definition.Tabs)
			{
				TabItem newItem = new TabItem
				{
					Header = tab
				};
				this.tabs.Items.Add(newItem);
			}
		}
		if (definition.OldStyleData && !this.ListMode)
		{
			this.SetupOldStyleDialog();
		}
		this.FinalizePopupSetup();
	}

	public void Setup()
	{
		this._usePopupTableType1 = File.Exists("UsePopupTableType1.txt");
		if (this._usePopupTableType1)
		{
			this.MyQuickTable = this.MyQuickTable_Type1;
			this.MyQuickTable_Type2.Visibility = Visibility.Collapsed;
			this.MyQuickTable_Type1.Visibility = Visibility.Visible;
		}
		else
		{
			this.MyQuickTable = new QuickTable2();
			this.MyQuickTable_Type2.Content = this.MyQuickTable;
			this.MyQuickTable_Type1.Visibility = Visibility.Collapsed;
			this.MyQuickTable_Type2.Visibility = Visibility.Visible;
		}
		this._fullyDynamicMode = false;
		this._mPfileName = this._popupInterface.GetPopupFileName();
		this._updateFilters = !(this._mPfileName != string.Empty) || !(this._lastPopupname == this._mPfileName) || this._lastExitcode != 21;
		if (!this._updateFilters)
		{
			return;
		}
		this._lastPopupname = this._mPfileName;
		this.SetupFontsAndElse();
		if (this._updateFilters)
		{
			this.UpdateFilters();
		}
		if (this.ListMode)
		{
			this.MyQuickTable.SetFilter(string.Empty, string.Empty, string.Empty);
		}
		this._mWindowLabel = this._popupInterface.GetPopupName();
		if (this._mWindowLabel.Contains("[T]") || this._mWindowLabel.Contains("[H]") || this._mWindowLabel.Contains("[S]"))
		{
			this.ListMode = true;
			this._staticListMode = 0;
			if (this._mWindowLabel.Contains("[H]"))
			{
				this._staticListMode = 1;
			}
			if (this._mWindowLabel.Contains("[S]"))
			{
				this._staticListMode = 2;
			}
			this._mWindowLabel = this._mWindowLabel.Substring(0, this._mWindowLabel.Length - 3);
		}
		else
		{
			this.ListMode = false;
		}
		this.SetupTableMode();
		this.SetWindowTitle();
		this._oldStyleData = this._popupInterface.GetPopupManipulatorDada(19) == 0;
		this._bval = new int[18];
		for (int i = 0; i < 18; i++)
		{
			this._bval[i] = this._popupInterface.GetPopupManipulatorDada(i);
		}
		if (this._bval[4] > 1)
		{
			this._useSortingButtons = true;
			this._bval[4] = ((this._bval[4] == 3) ? 1 : 0);
		}
		else
		{
			this._useSortingButtons = false;
		}
		this.is_EditButtonVisible = this._bval[1] == 1;
		this.StandardButtonsSetup(this._useSortingButtons);
		this.Lines = new List<PopupLine>();
		PopupLine popupLine = null;
		int popupLinesCount = this._popupInterface.GetPopupLinesCount();
		for (int j = 0; j < popupLinesCount; j++)
		{
			try
			{
				PopupLine popupLine2 = PopupAdapter.GetPopupLine(j);
				if (popupLine2.ltext == "DISABLE")
				{
					popupLine2.is_disable = true;
				}
				if (popupLine2.ltext == "PRESENTATION")
				{
					popupLine2.is_presentation_mode = true;
				}
				if (popupLine2.typ == 9 && popupLine != null && popupLine.typ == 9 && popupLine.pyn == popupLine2.pyn)
				{
					if (popupLine2.sel == 1)
					{
						popupLine.inte1 = popupLine.multitext.Count;
					}
					popupLine.multitext.Add(popupLine2.text);
					continue;
				}
				if (!this.ListMode && popupLine2.id4 > this._maxId4)
				{
					this._maxId4 = popupLine2.id4;
				}
				this.Lines.Add(popupLine2);
				popupLine = popupLine2;
			}
			catch (Exception e)
			{
				this._logCenterService.CatchRaport(e);
			}
		}
		if (this._maxId4 == 0)
		{
			this.SetupNoTabs();
		}
		else
		{
			this.SetupTabs();
			for (int k = 1; k <= this._maxId4; k++)
			{
				TabItem newItem = new TabItem
				{
					Header = this.GetTabPageText(k)
				};
				this.tabs.Items.Add(newItem);
			}
		}
		if (this._oldStyleData && !this.ListMode)
		{
			this.SetupOldStyleDialog();
		}
		this.FinalizePopupSetup();
	}

	private void FinalizePopupSetup()
	{
		if (!this.ListMode)
		{
			this.GenerateCtrlsForCurrentScreen();
		}
		else if (this.isArchiveTreeTab())
		{
			this.GenerateTreeViewForCurrentScreen();
		}
		else
		{
			this.GenerateTabbleForListMode();
		}
		this._lastVisibleTab = -1;
		this.SetVisibleTab(1);
		bool flag = false;
		PopUpInfo popUpInfo = PopUpInfo.Load($"{this._storeFolder}\\{this._mPfileName}.xml", this._logCenterService);
		if (popUpInfo != null)
		{
			for (int i = 0; i < 5; i++)
			{
				this._lastFilters[i] = popUpInfo.Filters[i];
			}
		}
		if (this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>().PopupPosMode == 1 && popUpInfo != null)
		{
			this.CorectionForPopupLocation(popUpInfo);
			flag = true;
			base.WindowStartupLocation = WindowStartupLocation.Manual;
			base.Left = popUpInfo.PopupLeft;
			base.Top = popUpInfo.PopupTop;
			if (this._mPfileName != "DYNAMIC")
			{
				base.Width = popUpInfo.PopupWidth;
				base.Height = popUpInfo.PopupHeight;
			}
		}
		if (!flag)
		{
			Point point = this._mainWindow.PointToScreen(new Point(this.GetWindowField(this._mainWindow, "_actualLeft"), this.GetWindowField(this._mainWindow, "_actualTop")));
			double x = point.X + this._mainWindow.ActualWidth / 2.0 - base.Width / 2.0;
			double y = point.Y + this._mainWindow.ActualHeight / 2.0 - base.Height / 2.0;
			Point point2 = this._mainWindow.PointFromScreen(new Point(x, y));
			if (point2.X < 1.0)
			{
				point2.X = 1.0;
			}
			if (point2.Y < 1.0)
			{
				point2.Y = 1.0;
			}
			base.WindowStartupLocation = WindowStartupLocation.Manual;
			base.Left = point2.X;
			base.Top = point2.Y;
		}
		if (this._bval[17] > 0)
		{
			this.tabs.SelectedIndex = this._bval[17] - 1;
		}
		this.FocusByMessage();
	}

	private void SetupOldStyleDialog()
	{
		int num = 15;
		int num2 = 0;
		foreach (PopupLine line in this.Lines)
		{
			if (line.typ >= 2 && line.typ <= 5)
			{
				Label label = new Label
				{
					Content = line.text
				};
				label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				if (label.DesiredSize.Width > (double)num2)
				{
					num2 = (int)label.DesiredSize.Width;
				}
			}
		}
		int num3 = 450 - num2 - 30;
		if (num3 < 150)
		{
			num3 = 150;
		}
		foreach (PopupLine line2 in this.Lines)
		{
			if (line2.typ == 0)
			{
				line2.box1_x = 28;
			}
			else
			{
				line2.box1_x = 10;
			}
			line2.box1_y = num;
			line2.box2_x = num2 + 40;
			line2.box2_y = num;
			line2.box2_w = num3;
			line2.box2_h = 22;
			num += 23;
		}
	}

	private void SetupTabs()
	{
		this.tabs.Visibility = Visibility.Visible;
		this.canvasborder.BorderThickness = new Thickness(1.0, 0.0, 1.0, 1.0);
		this.tabs.Items.Clear();
	}

	private void SetupNoTabs()
	{
		this.tabs.Visibility = Visibility.Collapsed;
		this.canvasborder.BorderThickness = new Thickness(0.0);
	}

	private void SetWindowTitle()
	{
		base.Title = $"{this._mWindowLabel} ({this._mPfileName})";
	}

	private bool isArchiveTreeTab()
	{
		if (File.Exists("NoArBrInPopup.txt"))
		{
			return false;
		}
		return this._mPfileName == "TAB011";
	}

	private void ArchiveTreeSendAnswer()
	{
		for (int i = 1; i < this.Lines.Count; i++)
		{
			bool num = this._archiveControlModel.IsSelectedPopupLine(this.Lines[i]);
			int value = 0;
			if (num)
			{
				value = 1;
			}
			PopupAdapter.Popup_Line_IPOSEL_set(i, value);
		}
	}

	private void SetupTableMode()
	{
		if (this.ListMode)
		{
			if (this.isArchiveTreeTab())
			{
				this.canvasborder.Visibility = Visibility.Collapsed;
				this.MyQuickTable.SetVisibility(Visibility.Collapsed);
				this.tabbleborder.Visibility = Visibility.Collapsed;
				this.ArchiveFilter.Visibility = Visibility.Visible;
			}
			else
			{
				this.MyQuickTable.ScrollReset();
				this.canvasborder.Visibility = Visibility.Collapsed;
				this.MyQuickTable.SetVisibility(Visibility.Visible);
				this.tabbleborder.Visibility = Visibility.Visible;
				this.ArchiveFilter.Visibility = Visibility.Collapsed;
			}
		}
		else
		{
			this.scroll.ScrollToVerticalOffset(0.0);
			this.scroll.ScrollToHorizontalOffset(0.0);
			this.canvasborder.Visibility = Visibility.Visible;
			this.MyQuickTable.SetVisibility(Visibility.Collapsed);
			this.tabbleborder.Visibility = Visibility.Collapsed;
			this.ArchiveFilter.Visibility = Visibility.Collapsed;
		}
	}

	private void GenerateTreeViewForCurrentScreen()
	{
		this._archiveControlModel = new ArchiveControlPopupModel();
		this._archiveControlModel.Texts.TextSearchArchive = "Archive";
		this._archiveControlModel.Texts.TextSearchSubArchive = "SubArchive";
		this._archiveControlModel.Texts.TextSelectAll = "SelectAll";
		this._archiveControlModel.HasSubArchives = false;
		this._archiveControlModel.InitializeModel(this.ArchiveFilter, this.Lines);
		ThreadPool.QueueUserWorkItem(delegate
		{
			Thread.Sleep(100);
			this.ArchiveFilter.Dispatcher.Invoke(delegate
			{
				this.ArchiveFilter.SetFocusOnTextBox();
			});
		});
	}

	private void ArchiveFilter_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.OriginalSource is TextBlock)
		{
			this.ExitCode2();
		}
	}

	private void SetupFontsAndElse()
	{
		this._mExpanders = new Dictionary<Expander, ExpanderInfo>();
		this._calendar.Language = XmlLanguage.GetLanguage(this._languageDictionary.GetCurrentLanguageXmlString());
		this._stdFont = base.FontFamily;
		this._monospaceFont = new FontFamily(this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>().MonospaceFont);
		this._focusControl = null;
		this._imageSource = new PnImageSource(this._logCenterService, this._pnColorsService, this._configProvider);
		this.ExitCode = 0;
		this._maxId4 = 0;
		this._uiElementPopupLineConnection = new Dictionary<UIElement, PopupLine>();
		this._logicalDict = new Dictionary<object, string>();
		this._tooltipHtext = new Dictionary<ToolTip, string>();
	}

	private void UpdateFilters()
	{
		this.filter_text.Text = string.Empty;
		this.filter_text1.Text = string.Empty;
		this.filter_text2.Text = string.Empty;
		this.filter_image.BeginInit();
		this.filter_image.Source = this._pnIconsService.GetSmallIcon("WINDOW-1");
		this.filter_image.Height = 16.0;
		this.filter_image.Width = 16.0;
		this.filter_image.SnapsToDevicePixels = true;
		this.filter_image.EndInit();
		this.filter_image1.BeginInit();
		this.filter_image1.Source = this._pnIconsService.GetSmallIcon("WINDOW-1");
		this.filter_image1.Height = 16.0;
		this.filter_image1.Width = 16.0;
		this.filter_image1.SnapsToDevicePixels = true;
		this.filter_image1.EndInit();
		this.filter_image2.BeginInit();
		this.filter_image2.Source = this._pnIconsService.GetSmallIcon("WINDOW-1");
		this.filter_image2.Height = 16.0;
		this.filter_image2.Width = 16.0;
		this.filter_image2.SnapsToDevicePixels = true;
		this.filter_image2.EndInit();
		try
		{
			this.filter_combo.Items.Clear();
		}
		catch (Exception e)
		{
			this._logCenterService.CatchRaport(e);
		}
		this.filter_combo.Items.Add(string.Empty);
		this.filter_combo.SelectedIndex = 0;
		this._lastFilters = new string[5];
	}

	private void CorectionForPopupLocation(PopUpInfo info)
	{
		if (info.PopupTop + info.PopupHeight / 2.0 > SystemParameters.VirtualScreenHeight)
		{
			info.PopupTop = SystemParameters.VirtualScreenHeight - info.PopupHeight;
		}
		if (info.PopupLeft + info.PopupWidth / 2.0 > SystemParameters.VirtualScreenWidth)
		{
			info.PopupLeft = SystemParameters.VirtualScreenWidth - info.PopupWidth;
		}
		if (info.PopupTop < SystemParameters.VirtualScreenTop)
		{
			info.PopupTop = SystemParameters.VirtualScreenTop;
		}
		if (info.PopupLeft < SystemParameters.VirtualScreenLeft)
		{
			info.PopupLeft = SystemParameters.VirtualScreenLeft;
		}
	}

	private bool EndLikeModalMode()
	{
		if (base.Visibility == Visibility.Visible)
		{
			this.ExitCode128();
		}
		return true;
	}

	private double GetWindowField(Window window, string key)
	{
		return (double)typeof(Window).GetField(key, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
	}

	private double GetWindowLeft(Window window)
	{
		if (window.WindowState == WindowState.Maximized)
		{
			return (double)typeof(Window).GetField("_actualLeft", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
		}
		return window.Left;
	}

	private double GetWindowTop(Window window)
	{
		if (window.WindowState == WindowState.Maximized)
		{
			return (double)typeof(Window).GetField("_actualTop", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
		}
		return window.Top;
	}

	private void UpdateEnableSortingButtons(bool enable)
	{
		this.MoveToStartButton.IsEnabled = (this.MoveOneUpButton.IsEnabled = (this.MoveOneDownButton.IsEnabled = (this.MoveToEndButton.IsEnabled = enable)));
	}

	private void GenerateTabbleForListMode()
	{
		this.MyQuickTable.MainPopupWindow = this;
		this.MyQuickTable.SetControls(this.filter_panel1, this.filter_panel2, this.filter_combo1, this.filter_combo2, this.filter_text, this.filter_text1, this.filter_text2);
		this.MyQuickTable.Setup(this._factorio, this.Lines, this._imageSource, ExitCode2, this._staticListMode, this.filter_combo, this._updateFilters, UpdateEnableSortingButtons, this._filtersCount, this.is_EditButtonVisible, this._mPfileName);
		base.Width = SystemParameters.PrimaryScreenWidth - 200.0;
		base.Height = 400.0;
	}

	private void SetVisibleTab(int p)
	{
		if (this.ListMode || this._lastVisibleTab == p)
		{
			return;
		}
		this.scroll.ScrollToHome();
		this._lastVisibleTab = p;
		foreach (UIElement key in this._uiElementPopupLineConnection.Keys)
		{
			PopupLine popupLine = this._uiElementPopupLineConnection[key];
			if (popupLine.id4 == p || popupLine.id4 == 0)
			{
				key.Visibility = Visibility.Visible;
			}
			else
			{
				key.Visibility = Visibility.Hidden;
			}
		}
	}

	private void GenerateCtrlsForCurrentScreen()
	{
		this.canvas.Children.Clear();
		this._canvasMaxx = 0;
		this._canvasMaxy = 0;
		int num = 0;
		PopupLine popupLine = null;
		List<PopupLine> list = null;
		int staticListMode = 0;
		this._currentStackpanel = null;
		int x = 0;
		int y = 0;
		string text = string.Empty;
		double fontSize = 12.0;
		foreach (PopupLine line in this.Lines)
		{
			if (popupLine != null && popupLine.typ == 6 && line.typ != 6)
			{
				num++;
			}
			switch (line.typ)
			{
			case 0:
			{
				if (list != null)
				{
					list.Add(line);
					break;
				}
				bool flag = false;
				if (line.ntext == string.Empty || !flag)
				{
					line.text = line.text.Replace("_", "__");
					TextBlock textBlock2 = new TextBlock
					{
						Text = line.text
					};
					textBlock2.FontSize = fontSize;
					if (line.is_disable)
					{
						textBlock2.Foreground = new SolidColorBrush(Colors.Gray);
					}
					if (line.inte3 > 0)
					{
						textBlock2.FontFamily = this._monospaceFont;
					}
					if (line.inte2 != 1)
					{
						this.SetCanvasLocation(textBlock2, line, line.box1_x, line.box1_y);
					}
					if (line.inte2 == 1)
					{
						textBlock2.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
						this.SetCanvasLocation(textBlock2, line, line.box1_x - (int)textBlock2.DesiredSize.Width, line.box1_y);
					}
				}
				break;
			}
			case 1:
			{
				if (list != null)
				{
					list.Add(line);
					break;
				}
				line.text = line.text.Replace("_", "__");
				CheckBox checkBox = new CheckBox
				{
					VerticalContentAlignment = VerticalAlignment.Center,
					Content = line.text,
					IsChecked = (line.sel == 1),
					FontSize = fontSize
				};
				if (line.inte3 > 0)
				{
					checkBox.FontFamily = this._monospaceFont;
				}
				if (line.is_disable)
				{
					checkBox.IsEnabled = false;
				}
				this.SetCanvasLocation(checkBox, line, line.box1_x, line.box1_y);
				line.ctrls.Add(checkBox);
				if (this._focusControl == null)
				{
					this._focusControl = checkBox;
				}
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(checkBox, line.ltext);
					checkBox.Click += cb_Click;
				}
				break;
			}
			case 2:
			{
				line.text = line.text.Replace("_", "__");
				Label label2 = new Label
				{
					Content = line.text,
					FontSize = fontSize
				};
				if (line.is_disable)
				{
					label2.Foreground = new SolidColorBrush(Colors.Gray);
				}
				this.SetCanvasLocation(label2, line, line.box1_x - 5, line.box1_y - 5);
				TextBox textBox = new TextBox
				{
					Text = line.real1.ToString(Thread.CurrentThread.CurrentUICulture),
					Width = line.box2_w,
					Height = line.box2_h,
					VerticalContentAlignment = VerticalAlignment.Center,
					HorizontalContentAlignment = HorizontalAlignment.Right,
					CharacterCasing = CharacterCasing.Upper
				};
				if (line.real4 != 0.0)
				{
					textBox.Background = this._pnColorsService.GetWpfBrush((int)line.real4);
					textBox.Style = (Style)base.TryFindResource("PnPopupTextBoxStyleColored");
				}
				if (line.box2_h < 22)
				{
					textBox.FontSize *= 0.85;
				}
				textBox.PreviewTextInput += RealPreviewTextInput;
				if (line.is_disable)
				{
					textBox.IsEnabled = false;
				}
				this.SetCanvasLocation(textBox, line, line.box2_x, line.box2_y);
				line.ctrls.Add(textBox);
				if (this._focusControl == null)
				{
					this._focusControl = textBox;
				}
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(textBox, line.ltext);
					textBox.LostFocus += TextBoxLogicalLostFocus;
				}
				if (line.is_presentation_mode)
				{
					textBox.IsEnabled = false;
				}
				break;
			}
			case 3:
			{
				line.text = line.text.Replace("_", "__");
				Label label5 = new Label
				{
					Content = line.text,
					FontSize = fontSize
				};
				if (line.is_disable)
				{
					label5.Foreground = new SolidColorBrush(Colors.Gray);
				}
				this.SetCanvasLocation(label5, line, line.box1_x - 5, line.box1_y - 5);
				TextBox textBox4 = new TextBox
				{
					Text = line.inte1.ToString(CultureInfo.InvariantCulture),
					Width = line.box2_w,
					Height = line.box2_h,
					VerticalContentAlignment = VerticalAlignment.Center,
					HorizontalContentAlignment = HorizontalAlignment.Right,
					CharacterCasing = CharacterCasing.Upper
				};
				textBox4.PreviewTextInput += IntPreviewTextInput;
				if (line.real4 != 0.0)
				{
					textBox4.Background = this._pnColorsService.GetWpfBrush((int)line.real4);
					textBox4.Style = (Style)base.TryFindResource("PnPopupTextBoxStyleColored");
				}
				if (line.box2_h < 22)
				{
					textBox4.FontSize *= 0.85;
				}
				if (line.is_disable)
				{
					textBox4.IsEnabled = false;
				}
				this.SetCanvasLocation(textBox4, line, line.box2_x, line.box2_y);
				line.ctrls.Add(textBox4);
				if (this._focusControl == null)
				{
					this._focusControl = textBox4;
				}
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(textBox4, line.ltext);
					textBox4.LostFocus += TextBoxLogicalLostFocus;
				}
				if (line.is_presentation_mode)
				{
					textBox4.IsEnabled = false;
				}
				break;
			}
			case 4:
			{
				line.text = line.text.Replace("_", "__");
				Label label3 = new Label
				{
					Content = line.text,
					FontSize = fontSize
				};
				if (line.is_disable)
				{
					label3.Foreground = new SolidColorBrush(Colors.Gray);
				}
				this.SetCanvasLocation(label3, line, line.box1_x - 5, line.box1_y - 5);
				TextBox textBox2 = new TextBox
				{
					Text = line.ntext,
					Width = line.box2_w,
					Height = line.box2_h,
					VerticalContentAlignment = VerticalAlignment.Center,
					HorizontalContentAlignment = HorizontalAlignment.Left
				};
				if (line.real4 != 0.0)
				{
					textBox2.Background = this._pnColorsService.GetWpfBrush((int)line.real4);
					textBox2.Style = (Style)base.TryFindResource("PnPopupTextBoxStyleColored");
				}
				if (line.inte4 != 0)
				{
					textBox2.MaxLength = line.inte4;
				}
				if (this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>().LowerCaseAllowed)
				{
					textBox2.CharacterCasing = CharacterCasing.Normal;
				}
				else
				{
					textBox2.CharacterCasing = CharacterCasing.Upper;
				}
				if (line.box2_h < 22)
				{
					textBox2.FontSize *= 0.85;
				}
				if (line.is_disable)
				{
					textBox2.IsEnabled = false;
				}
				if (line.is_presentation_mode)
				{
					textBox2.IsEnabled = false;
				}
				this.SetCanvasLocation(textBox2, line, line.box2_x, line.box2_y);
				line.ctrls.Add(textBox2);
				if (this._focusControl == null)
				{
					this._focusControl = textBox2;
				}
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(textBox2, line.ltext);
					textBox2.LostFocus += TextBoxLogicalLostFocus;
				}
				textBox2.LostFocus += TextBoxLostFocusTreatment;
				break;
			}
			case 5:
			{
				line.text = line.text.Replace("_", "__");
				Label label = new Label
				{
					Content = line.text,
					FontSize = fontSize
				};
				if (line.is_disable)
				{
					label.Foreground = new SolidColorBrush(Colors.Gray);
				}
				this.SetCanvasLocation(label, line, line.box1_x - 5, line.box1_y - 5);
				RadioButton radioButton = new RadioButton
				{
					VerticalContentAlignment = VerticalAlignment.Center,
					Content = this._languageDictionary.GetMsg2Int("Yes")
				};
				if (line.is_disable)
				{
					radioButton.IsEnabled = false;
				}
				radioButton.IsChecked = line.pyn == 1;
				this.SetCanvasLocation(radioButton, line, line.box2_x, line.box2_y);
				radioButton.GroupName = num.ToString();
				RadioButton radioButton2 = new RadioButton
				{
					VerticalContentAlignment = VerticalAlignment.Center,
					Content = this._languageDictionary.GetMsg2Int("No")
				};
				if (line.is_disable)
				{
					radioButton2.IsEnabled = false;
				}
				radioButton2.IsChecked = line.pyn == 0;
				this.SetCanvasLocation(radioButton2, line, line.box2_x + line.box2_w / 2, line.box2_y);
				radioButton2.GroupName = num.ToString();
				line.ctrls.Add(radioButton);
				line.ctrls.Add(radioButton2);
				num++;
				if (this._focusControl == null)
				{
					this._focusControl = radioButton;
				}
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(radioButton, line.ltext);
					radioButton.Click += cb_Click;
				}
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(radioButton2, line.ltext);
					radioButton2.Click += cb_Click;
				}
				break;
			}
			case 6:
			{
				if (list != null)
				{
					list.Add(line);
					break;
				}
				line.text = line.text.Replace("_", "__");
				RadioButton radioButton3 = new RadioButton
				{
					VerticalContentAlignment = VerticalAlignment.Center,
					Content = line.text,
					FontSize = fontSize
				};
				if (line.is_disable)
				{
					radioButton3.IsEnabled = false;
				}
				radioButton3.IsChecked = line.sel == 1;
				this.SetCanvasLocation(radioButton3, line, line.box1_x, line.box1_y);
				radioButton3.GroupName = $"RB{line.pyn}";
				line.ctrls.Add(radioButton3);
				if (this._focusControl == null)
				{
					this._focusControl = radioButton3;
				}
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(radioButton3, line.ltext);
					radioButton3.Click += cb_Click;
				}
				radioButton3.PreviewMouseDown += multisel_Click;
				break;
			}
			case 7:
			{
				Button button2 = new Button();
				Image image2 = (Image)(button2.Content = new Image
				{
					SnapsToDevicePixels = true,
					Source = this._imageSource.GetImageSource(line.text, line.box1_w, line.box1_h, line.inte1)
				});
				if (line.sel == 0)
				{
					button2.BorderThickness = new Thickness(0.0);
				}
				else
				{
					button2.BorderThickness = new Thickness(3.0);
				}
				button2.BorderBrush = new SolidColorBrush(Colors.Red);
				button2.Click += ButtonType7_Click;
				if ((line.box1_w != 0 && line.box1_h != 0) || image2.Source == null)
				{
					button2.Width = line.box1_w;
					button2.Height = line.box1_h;
				}
				else
				{
					button2.Width = image2.Source.Width + 6.0;
					button2.Height = image2.Source.Height + 6.0;
				}
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(button2, line.ltext);
				}
				line.ctrls.Add(button2);
				this.SetCanvasLocation(button2, line, line.box1_x, line.box1_y);
				if (this._focusControl == null)
				{
					this._focusControl = button2;
				}
				break;
			}
			case 8:
			{
				Button button = new Button
				{
					VerticalContentAlignment = VerticalAlignment.Center,
					Width = line.box1_w,
					Height = line.box1_h,
					Content = line.text,
					FontSize = fontSize
				};
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(button, line.ltext);
					button.Click += ButtonType8_Click;
				}
				line.ctrls.Add(button);
				this.SetCanvasLocation(button, line, line.box1_x, line.box1_y);
				if (this._focusControl == null)
				{
					this._focusControl = button;
				}
				break;
			}
			case 9:
			{
				Label label6 = new Label
				{
					Content = line.text,
					FontSize = fontSize
				};
				if (line.is_disable)
				{
					label6.Foreground = new SolidColorBrush(Colors.Gray);
				}
				this.SetCanvasLocation(label6, line, line.box1_x - 5, line.box1_y - 5);
				ComboBox comboBox = new ComboBox
				{
					VerticalContentAlignment = VerticalAlignment.Center,
					Width = line.box2_w,
					Height = 25.0,
					FontSize = fontSize
				};
				this.SetCanvasLocation(comboBox, line, line.box2_x, line.box2_y);
				foreach (string item in line.multitext)
				{
					ComboBoxItem newItem = new ComboBoxItem
					{
						Content = item
					};
					comboBox.Items.Add(newItem);
				}
				comboBox.SelectedIndex = line.inte1;
				line.ctrls.Add(comboBox);
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(comboBox, line.ltext);
					comboBox.SelectionChanged += ComboBoxLogicalSelectionChange;
				}
				if (this._focusControl == null)
				{
					this._focusControl = comboBox;
				}
				break;
			}
			case 11:
			{
				line.text = line.text.Replace("_", "__");
				Label label4 = new Label
				{
					Content = line.text,
					FontSize = fontSize
				};
				if (line.is_disable)
				{
					label4.Foreground = new SolidColorBrush(Colors.Gray);
				}
				this.SetCanvasLocation(label4, line, line.box1_x - 5, line.box1_y - 5);
				TextBox textBox3 = new TextBox
				{
					Text = this.Type11GetTextFromInt(line.inte1),
					Width = line.box2_w - line.box2_h,
					Height = line.box2_h,
					VerticalContentAlignment = VerticalAlignment.Center,
					HorizontalContentAlignment = HorizontalAlignment.Right,
					CharacterCasing = CharacterCasing.Upper
				};
				textBox3.PreviewTextInput += DataPreviewTextInput;
				textBox3.LostFocus += box_LostFocus;
				if (line.box2_h < 22)
				{
					textBox3.FontSize *= 0.85;
				}
				if (line.is_disable)
				{
					textBox3.IsEnabled = false;
				}
				this.SetCanvasLocation(textBox3, line, line.box2_x, line.box2_y);
				line.ctrls.Add(textBox3);
				if (!line.is_disable && !line.is_presentation_mode)
				{
					Expander expander = new Expander
					{
						Width = line.box2_h,
						Height = line.box2_h,
						BorderThickness = new Thickness(0.0)
					};
					expander.Collapsed += expander_Collapsed;
					expander.Expanded += expander_Expanded;
					expander.LostFocus += expander_LostFocus;
					this.SetCanvasLocation(expander, line, line.box2_x + line.box2_w - line.box2_h + 10, line.box2_y);
					this._mExpanders.Add(expander, new ExpanderInfo
					{
						Textbox = textBox3,
						Type = 1
					});
					line.ctrls.Add(expander);
				}
				if (this._focusControl == null)
				{
					this._focusControl = textBox3;
				}
				if (line.ltext != string.Empty)
				{
					this._logicalDict.Add(textBox3, line.ltext);
					textBox3.LostFocus += TextBoxLogicalLostFocus;
				}
				if (line.is_presentation_mode)
				{
					textBox3.IsEnabled = false;
				}
				break;
			}
			case 20:
				if (line.text != string.Empty && line.text.Trim(' ') != string.Empty)
				{
					this.SetCanvasLocation(new GroupBox
					{
						Header = line.text,
						Width = line.box1_w,
						Height = line.box1_h,
						FontSize = fontSize
					}, line, line.box1_x, line.box1_y);
				}
				else
				{
					this.SetCanvasLocation(new Border
					{
						Width = line.box1_w,
						Height = line.box1_h,
						BorderThickness = new Thickness(1.0),
						CornerRadius = new CornerRadius(4.0),
						Padding = new Thickness(5.0),
						BorderBrush = (Application.Current.FindResource("PnPopup.BorderBrush") as Brush)
					}, line, line.box1_x, line.box1_y);
				}
				break;
			case 22:
				text += $"    {line.text}     ";
				break;
			case 21:
			{
				Image image = new Image
				{
					SnapsToDevicePixels = true,
					Source = this._imageSource.GetImageSource(line.text, line.box1_w, line.box1_h, line.inte1)
				};
				if (line.inte2 > 0)
				{
					image.Effect = new DropShadowEffect
					{
						BlurRadius = line.inte2,
						ShadowDepth = 0.0
					};
				}
				if (line.box1_w != 0 && line.box1_h != 0)
				{
					image.Width = line.box1_w;
					image.Height = line.box1_h;
				}
				else if (image.Source != null)
				{
					image.Width = image.Source.Width;
					image.Height = image.Source.Height;
				}
				else
				{
					image.Width = 1.0;
					image.Height = 1.0;
				}
				this.SetCanvasLocation(image, line, line.box1_x, line.box1_y);
				break;
			}
			case 30:
				list = new List<PopupLine>();
				this._internalTable.Width = line.box1_w;
				this._internalTable.Height = line.box1_h;
				this.SetCanvasLocation(this._internalTable, line, line.box1_x, line.box1_y);
				staticListMode = 0;
				if (line.text.Contains("[H]"))
				{
					staticListMode = 1;
				}
				if (line.text.Contains("[S]"))
				{
					staticListMode = 2;
				}
				line.ctrls.Add(this._internalTable);
				break;
			case 31:
				this._internalTable.MainPopupWindow = this;
				this._internalTable.Setup(list, this._imageSource, ExitCode2, staticListMode, null, updateFilters: false, null);
				list = null;
				break;
			case 32:
				try
				{
					TextBlock textBlock = new TextBlock();
					Hyperlink hyperlink = new Hyperlink
					{
						NavigateUri = new Uri(line.ntext)
					};
					hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
					hyperlink.Inlines.Add(line.text);
					textBlock.Inlines.Add(hyperlink);
					this.SetCanvasLocation(textBlock, line, line.box1_x, line.box1_y);
				}
				catch (Exception e)
				{
					this._logCenterService.CatchRaport(e);
				}
				break;
			case 33:
				this._currentStackpanel = new StackPanel();
				if (line.inte1 == 1)
				{
					this._currentStackpanel.Orientation = Orientation.Vertical;
				}
				else
				{
					this._currentStackpanel.Orientation = Orientation.Horizontal;
				}
				x = line.box1_x;
				y = line.box1_y;
				break;
			case 34:
			{
				StackPanel currentStackpanel = this._currentStackpanel;
				this._currentStackpanel = null;
				this.SetCanvasLocation(currentStackpanel, line, x, y);
				break;
			}
			}
			popupLine = line;
		}
		if (this._canvasMaxx < 310)
		{
			this._canvasMaxx = 310;
		}
		if (text != string.Empty)
		{
			Label label7 = new Label
			{
				Content = text
			};
			label7.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			if (label7.DesiredSize.Width > (double)this._canvasMaxx)
			{
				this._canvasMaxx = (int)label7.DesiredSize.Width;
			}
		}
		int num2 = this._canvasMaxx + 10;
		int num3 = this._canvasMaxy + 10;
		this.canvas.Width = num2;
		this.canvas.Height = num3;
		num2 += 33;
		num3 += 55;
		if (this._popupButtonMode < 2)
		{
			num2 += 105;
		}
		else
		{
			num3 += 30;
		}
		if (this._maxId4 > 0)
		{
			num3 += 30;
		}
		int num4 = (int)SystemParameters.PrimaryScreenWidth - 200;
		int num5 = (int)SystemParameters.PrimaryScreenHeight - 200;
		if (num2 > num4)
		{
			num2 = num4;
		}
		if (num3 > num5)
		{
			num3 = num5;
		}
		int num6 = 600;
		int num7 = 380;
		if (num2 < num6)
		{
			num2 = num6;
		}
		if (num3 < num7)
		{
			num3 = num7;
		}
		base.Width = num2;
		base.Height = num3;
		foreach (object item2 in this.Lines.SelectMany((PopupLine l) => l.ctrls))
		{
			if (item2 is TextBox textBox5)
			{
				textBox5.SelectAll();
			}
		}
	}

	private void box_LostFocus(object sender, RoutedEventArgs e)
	{
		TextBox textBox = (TextBox)sender;
		textBox.Text = this.Type11ValidateTextFormat(textBox.Text);
	}

	private void expander_LostFocus(object sender, RoutedEventArgs e)
	{
	}

	private void expander_MouseMove(object sender, MouseEventArgs e)
	{
	}

	private void expander_MouseEnter(object sender, MouseEventArgs e)
	{
	}

	private string Type11GetTextFromInt(int p)
	{
		string text = p.ToString(CultureInfo.InvariantCulture);
		if (text.Length != 8)
		{
			return this.Type11GetDateTimeString(DateTime.Now);
		}
		return $"{text.Substring(6, 2)}.{text.Substring(4, 2)}.{text.Substring(0, 4)}";
	}

	private int Type11GetIntFromString(string p)
	{
		DateTime dateTime = this.Type11TextToDateTime(this.Type11ValidateTextFormat(p));
		return 0 + dateTime.Day + dateTime.Month * 100 + dateTime.Year * 10000;
	}

	private string Type11GetDateTimeString(DateTime dateTime)
	{
		return string.Format("{0}.{1}.{2}", dateTime.Day.ToString("D2"), dateTime.Month.ToString("D2"), dateTime.Year.ToString("D4"));
	}

	private void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
	{
		if (this._textBoxConnectedWithCallendar != null)
		{
			this._textBoxConnectedWithCallendar.Text = this.Type11GetDateTimeString(this._calendar.SelectedDate.Value);
			this.CollapsAllExpended(null);
		}
	}

	private DateTime Type11TextToDateTime(string p)
	{
		string[] array = p.Split('.');
		return new DateTime(Convert.ToInt32(array[2]), Convert.ToInt32(array[1]), Convert.ToInt32(array[0]));
	}

	private string Type11ValidateTextFormat(string p)
	{
		if (p == string.Empty)
		{
			return this.Type11GetDateTimeString(DateTime.Now);
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (p == ".")
		{
			num = DateTime.Now.Year;
			num2 = DateTime.Now.Month;
			num3 = DateTime.Now.Day;
		}
		else if (p[0] == '+')
		{
			try
			{
				int days = Convert.ToInt32(p.Substring(1));
				DateTime dateTime = DateTime.Now + new TimeSpan(days, 0, 0, 0);
				num = dateTime.Year;
				num2 = dateTime.Month;
				num3 = dateTime.Day;
			}
			catch (Exception e)
			{
				this._logCenterService.CatchRaport(e);
			}
		}
		else if (p[0] == '-')
		{
			try
			{
				int days2 = Convert.ToInt32(p.Substring(1));
				DateTime dateTime2 = DateTime.Now - new TimeSpan(days2, 0, 0, 0);
				num = dateTime2.Year;
				num2 = dateTime2.Month;
				num3 = dateTime2.Day;
			}
			catch (Exception e2)
			{
				this._logCenterService.CatchRaport(e2);
			}
		}
		else
		{
			string[] array = p.Split('.');
			try
			{
				switch (array.Length)
				{
				case 1:
					num3 = Convert.ToInt32(array[0]);
					num2 = DateTime.Now.Month;
					num = DateTime.Now.Year;
					break;
				case 2:
					num3 = Convert.ToInt32(array[0]);
					num2 = Convert.ToInt32(array[1]);
					num = DateTime.Now.Year;
					break;
				case 3:
					num = Convert.ToInt32(array[2]);
					num2 = Convert.ToInt32(array[1]);
					num3 = Convert.ToInt32(array[0]);
					break;
				}
			}
			catch (Exception e3)
			{
				this._logCenterService.CatchRaport(e3);
			}
		}
		if (num < 1900)
		{
			num = 1900;
		}
		if (num > 2200)
		{
			num = 2200;
		}
		if (num2 < 1)
		{
			num2 = 1;
		}
		if (num2 > 12)
		{
			num2 = 12;
		}
		if (num3 < 1)
		{
			num3 = 1;
		}
		if (num3 > 31)
		{
			num3 = 31;
		}
		try
		{
			return this.Type11GetDateTimeString(new DateTime(num, num2, num3));
		}
		catch (Exception e4)
		{
			this._logCenterService.CatchRaport(e4);
		}
		return this.Type11GetDateTimeString(DateTime.Now);
	}

	private void CollapsAllExpended(Expander sender)
	{
		foreach (Expander key in this._mExpanders.Keys)
		{
			if (key != sender)
			{
				key.IsExpanded = false;
				this._mExpanders[key].Textbox.IsEnabled = true;
			}
		}
		this._subPopup.IsOpen = false;
	}

	private void expander_Expanded(object sender, RoutedEventArgs e)
	{
		if (sender is Expander)
		{
			Expander expander = (this._lastExpander = (Expander)sender);
			this.CollapsAllExpended(expander);
			if (this._mExpanders.ContainsKey(expander))
			{
				ExpanderInfo expanderInfo = this._mExpanders[expander];
				expanderInfo.Textbox.Text = this.Type11ValidateTextFormat(expanderInfo.Textbox.Text);
				this._textBoxConnectedWithCallendar = null;
				this._calendar.DisplayDate = this.Type11TextToDateTime(expanderInfo.Textbox.Text);
				this._calendar.SelectedDate = this._calendar.DisplayDate;
				this._mExpanders[expander].Textbox.IsEnabled = false;
				this._textBoxConnectedWithCallendar = this._mExpanders[expander].Textbox;
				Point point = expander.PointToScreen(new Point(0.0, expander.Height));
				this._subPopup.HorizontalOffset = point.X;
				this._subPopup.VerticalOffset = point.Y;
				this._subPopup.IsOpen = true;
			}
		}
	}

	private void expander_Collapsed(object sender, RoutedEventArgs e)
	{
		_ = sender is Expander;
	}

	private void RealPreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		if ("1234567890-,.".IndexOf(e.Text) == -1)
		{
			e.Handled = true;
		}
	}

	private void IntPreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		if ("1234567890-".IndexOf(e.Text) == -1)
		{
			e.Handled = true;
		}
	}

	private void DataPreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		if ("1234567890.+-".IndexOf(e.Text) == -1)
		{
			e.Handled = true;
		}
	}

	private void SetCanvasLocation(FrameworkElement element, PopupLine line, int x, int y)
	{
		this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>().SetWpfToolTipTimes(element);
		if (this._currentStackpanel != null)
		{
			element.Visibility = Visibility.Hidden;
			this._currentStackpanel.Children.Add(element);
			this._uiElementPopupLineConnection.Add(element, line);
			this.CreateToolTip(element, line);
			return;
		}
		Canvas.SetLeft(element, x);
		Canvas.SetTop(element, y);
		element.Visibility = Visibility.Hidden;
		this.canvas.Children.Add(element);
		this._uiElementPopupLineConnection.Add(element, line);
		this.CreateToolTip(element, line);
		element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		int num = x + (int)element.DesiredSize.Width;
		int num2 = y + (int)element.DesiredSize.Height;
		if (num > this._canvasMaxx)
		{
			this._canvasMaxx = num;
		}
		if (num2 > this._canvasMaxy)
		{
			this._canvasMaxy = num2;
		}
	}

	private void CreateToolTip(FrameworkElement element, PopupLine line)
	{
		if (line.htext != string.Empty)
		{
			ToolTip toolTip2 = (ToolTip)(element.ToolTip = new ToolTip());
			if (line.htext[0] == '#')
			{
				if (line.htext.Length >= 2)
				{
					toolTip2.Content = line.htext.Substring(1);
				}
			}
			else
			{
				this._tooltipHtext.Add(toolTip2, line.htext);
			}
		}
		else if (line.id3 > 0)
		{
			string pophlp = this._languageDictionary.GetPophlp(line.id3);
			if (pophlp != null)
			{
				element.ToolTip = new ToolTip
				{
					Content = pophlp
				};
			}
		}
	}

	private string GetTabPageText(int id)
	{
		foreach (PopupLine line in this.Lines)
		{
			if (line.typ == 22 && line.id4 == id)
			{
				return $"   {line.text}   ";
			}
		}
		return $"   No name {id + 1}   ";
	}

	private void StandardButtonsSetup(bool useSortingButtons)
	{
		if (this._bval[16] == 1)
		{
			base.FontFamily = this._monospaceFont;
		}
		bool flag = false;
		this._filtersCount = this._bval[15];
		if (this._usePopupTableType1)
		{
			switch (this._filtersCount)
			{
			case 0:
				this.filter_panel.Visibility = Visibility.Collapsed;
				this.filter_panel1.Visibility = Visibility.Collapsed;
				this.filter_panel2.Visibility = Visibility.Collapsed;
				break;
			case 1:
				this.filter_panel.Visibility = Visibility.Visible;
				this.filter_panel1.Visibility = Visibility.Collapsed;
				this.filter_panel2.Visibility = Visibility.Collapsed;
				flag = true;
				break;
			case 2:
				this.filter_panel.Visibility = Visibility.Visible;
				this.filter_panel1.Visibility = Visibility.Visible;
				this.filter_panel2.Visibility = Visibility.Collapsed;
				flag = true;
				break;
			case 3:
				this.filter_panel.Visibility = Visibility.Visible;
				this.filter_panel1.Visibility = Visibility.Visible;
				this.filter_panel2.Visibility = Visibility.Visible;
				flag = true;
				break;
			}
		}
		else
		{
			flag = true;
			this.filter_panel.Visibility = Visibility.Collapsed;
			this.filter_panel1.Visibility = Visibility.Collapsed;
			this.filter_panel2.Visibility = Visibility.Collapsed;
		}
		if (useSortingButtons)
		{
			this.sorting_btns_panel.Visibility = Visibility.Visible;
		}
		else
		{
			this.sorting_btns_panel.Visibility = Visibility.Collapsed;
		}
		for (int i = 0; i < 17; i++)
		{
			try
			{
				Button button = (Button)this.zone_btn.FindName($"B{i}");
				if (this._bval[i] == 1 || i == 16 || (i == 15 && flag))
				{
					button.Visibility = Visibility.Visible;
				}
				else
				{
					button.Visibility = Visibility.Collapsed;
				}
			}
			catch (Exception e)
			{
				this._logCenterService.CatchRaport(e);
			}
		}
		this.zone_btn.UpdateLayout();
		GeneralUserSettingsConfig generalUserSettingsConfig = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (this._popupButtonMode != generalUserSettingsConfig.PopupButtonMode || this._oldLanguageBtnsSetup != this._languageDictionary.GetCurrentLanguage())
		{
			this._popupButtonMode = generalUserSettingsConfig.PopupButtonMode;
			this._oldLanguageBtnsSetup = this._languageDictionary.GetCurrentLanguage();
			((Grid)this.zone_btn.Parent).Children.Remove(this.zone_btn);
			if (this._popupButtonMode < 2)
			{
				if (this._popupButtonMode == 0)
				{
					Grid.SetColumn(this.zone_btn, 2);
				}
				else
				{
					Grid.SetColumn(this.zone_btn, 0);
				}
				this.vert_grid.Children.Add(this.zone_btn);
				Grid.SetRow(this.main_stack1, 0);
				Grid.SetRow(this.main_stack2, 2);
				Grid.SetColumn(this.main_stack1, 0);
				Grid.SetColumn(this.main_stack2, 0);
				this.main_stack1.Orientation = Orientation.Vertical;
				this.main_stack2.Orientation = Orientation.Vertical;
				this.stack1.Orientation = Orientation.Vertical;
				this.stack2.Orientation = Orientation.Vertical;
				this.stack3.Orientation = Orientation.Vertical;
				this.stack4.Orientation = Orientation.Vertical;
				this.stack5.Orientation = Orientation.Vertical;
				this.stack6.Orientation = Orientation.Vertical;
				this.stack7.Orientation = Orientation.Vertical;
				for (int j = 0; j < 17; j++)
				{
					try
					{
						Button obj = (Button)this.zone_btn.FindName($"B{j}");
						string text = $"Popup_{j + 1}";
						global::System.Windows.Shapes.Path btnImage = this.GetBtnImage(text, j);
						btnImage.SnapsToDevicePixels = true;
						string msg2Int = this._languageDictionary.GetMsg2Int(text);
						TextBlock element = new TextBlock
						{
							Margin = new Thickness(2.0),
							Text = msg2Int
						};
						StackPanel stackPanel = new StackPanel
						{
							Orientation = Orientation.Horizontal
						};
						stackPanel.Children.Add(btnImage);
						stackPanel.Children.Add(element);
						stackPanel.SnapsToDevicePixels = true;
						obj.Content = stackPanel;
						obj.SnapsToDevicePixels = true;
						obj.ToolTip = null;
						obj.HorizontalContentAlignment = HorizontalAlignment.Left;
						obj.Width = 100.0;
						obj.Height = 25.0;
					}
					catch (Exception e2)
					{
						this._logCenterService.CatchRaport(e2);
					}
				}
			}
			else
			{
				if (this._popupButtonMode == 2)
				{
					Grid.SetRow(this.zone_btn, 0);
				}
				else
				{
					Grid.SetRow(this.zone_btn, 2);
				}
				this.main_grid.Children.Add(this.zone_btn);
				Grid.SetRow(this.main_stack1, 0);
				Grid.SetRow(this.main_stack2, 0);
				Grid.SetColumn(this.main_stack1, 0);
				Grid.SetColumn(this.main_stack2, 2);
				this.main_stack1.Orientation = Orientation.Horizontal;
				this.main_stack2.Orientation = Orientation.Horizontal;
				this.stack1.Orientation = Orientation.Horizontal;
				this.stack2.Orientation = Orientation.Horizontal;
				this.stack3.Orientation = Orientation.Horizontal;
				this.stack4.Orientation = Orientation.Horizontal;
				this.stack5.Orientation = Orientation.Horizontal;
				this.stack6.Orientation = Orientation.Horizontal;
				this.stack7.Orientation = Orientation.Horizontal;
				for (int k = 0; k < 17; k++)
				{
					try
					{
						Button obj2 = (Button)this.zone_btn.FindName($"B{k}");
						string text2 = $"Popup_{k + 1}";
						global::System.Windows.Shapes.Path btnImage2 = this.GetBtnImage(text2, k);
						string msg2Int2 = this._languageDictionary.GetMsg2Int(text2);
						btnImage2.SnapsToDevicePixels = true;
						obj2.SnapsToDevicePixels = true;
						obj2.Content = btnImage2;
						obj2.ToolTip = new ToolTip
						{
							Content = msg2Int2
						};
						obj2.HorizontalContentAlignment = HorizontalAlignment.Center;
						obj2.Width = 25.0;
						obj2.Height = 25.0;
					}
					catch (Exception e3)
					{
						this._logCenterService.CatchRaport(e3);
					}
				}
			}
		}
		Visibility visibility = Visibility.Collapsed;
		if (this._useSortingButtons)
		{
			visibility = Visibility.Visible;
		}
		this.MoveToStartButton.Visibility = (this.MoveOneUpButton.Visibility = (this.MoveOneDownButton.Visibility = (this.MoveToEndButton.Visibility = visibility)));
	}

	private global::System.Windows.Shapes.Path GetBtnImage(string name, int id)
	{
		global::System.Windows.Shapes.Path path = new global::System.Windows.Shapes.Path();
		switch (id)
		{
		case 0:
			path.Data = Application.Current.FindResource("popup.Button0_DeletePath") as Geometry;
			break;
		case 1:
			path.Data = Application.Current.FindResource("popup.Button1_EditPath") as Geometry;
			break;
		case 2:
			path.Data = Application.Current.FindResource("popup.Button2_AddPath") as Geometry;
			break;
		case 3:
			path.Data = Application.Current.FindResource("popup.Button3_BackPath") as Geometry;
			break;
		case 4:
			path.Data = Application.Current.FindResource("popup.Button4_SwapPath") as Geometry;
			break;
		case 5:
			path.Data = Application.Current.FindResource("popup.Button5_CancelPath") as Geometry;
			break;
		case 6:
			path.Data = Application.Current.FindResource("popup.Button6_MarkPath") as Geometry;
			break;
		case 7:
			path.Data = Application.Current.FindResource("popup.Button7_LearnPath") as Geometry;
			break;
		case 8:
			path.Data = Application.Current.FindResource("popup.Button8_CopyPath") as Geometry;
			break;
		case 9:
			path.Data = Application.Current.FindResource("popup.Button9_ListPath") as Geometry;
			break;
		case 10:
			path.Data = Application.Current.FindResource("popup.Button10_GraphicsPath") as Geometry;
			break;
		case 11:
			path.Data = Application.Current.FindResource("popup.Button11_LoadingPath") as Geometry;
			break;
		case 12:
			path.Data = Application.Current.FindResource("popup.Button12_Certain1Path") as Geometry;
			break;
		case 13:
			path.Data = Application.Current.FindResource("popup.Button13_Certain2Path") as Geometry;
			break;
		case 14:
			path.Data = Application.Current.FindResource("popup.Button14_Certain3Path") as Geometry;
			break;
		case 15:
			path.Data = Application.Current.FindResource("popup.Button15_PrintPath") as Geometry;
			break;
		case 16:
			path.Data = Application.Current.FindResource("popup.Button16_OkPath") as Geometry;
			break;
		}
		path.Style = Application.Current.FindResource("popup.PathOnButton") as Style;
		return path;
	}

	private void StoreLastFilter()
	{
		string text = this.filter_text.Text.Trim(' ');
		if (text == string.Empty)
		{
			return;
		}
		List<string> list = new List<string>(this._lastFilters);
		for (int num = 4; num >= 0; num--)
		{
			if (list[num] == text)
			{
				list.RemoveAt(num);
			}
		}
		list.Insert(0, text);
		while (list.Count > 5)
		{
			list.RemoveAt(list.Count - 1);
		}
		this._lastFilters = list.ToArray();
	}

	public void StartLikeModalMode()
	{
		base.Dispatcher.Invoke((Action)delegate
		{
		}, DispatcherPriority.Render, null);
		base.Show();
		base.Activate();
		this._likeModalMode.SetMode(EndLikeModalMode);
		this._likeModalMode.WaitEndModal();
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (this.JustClose)
		{
			return;
		}
		if (this._subPopup != null && this._subPopup.IsOpen)
		{
			this._subPopup.IsOpen = false;
		}
		if (this.ExitCode == 16)
		{
			this.PrintScreen();
		}
		this.StoreLastFilter();
		string filterString = this.MyQuickTable.FilterString;
		string filterString2 = this.MyQuickTable.FilterString1;
		string filterString3 = this.MyQuickTable.FilterString2;
		if (this.ListMode)
		{
			this.MyQuickTable.SetFilter(string.Empty, string.Empty, string.Empty);
		}
		if (this._bval.Length > 16 && this._bval[16] == 1)
		{
		 base.FontFamily = this._stdFont;
		}
		if (this.ExitCode == 0)
		{
			this.ExitCode = 0;
			if (this._bval[5] == 1)
			{
				this.ExitCode = 6;
			}
			else if (this._bval[3] == 1)
			{
				this.ExitCode = 4;
			}
		}
		if (this.ExitCode == 128)
		{
			this.ExitCode = 0;
		}
		if (this.ExitCode == 16)
		{
			this.ExitCode = 21;
		}
		if (this.ExitCode == 17)
		{
			this.ExitCode = 0;
		}
		if (this.tabs.IsVisible)
		{
			this.ExitCode += 1000 * (this.tabs.SelectedIndex + 1);
		}
		base.Hide();
		e.Cancel = true;
		if (!this.ListMode)
		{
			foreach (PopupLine line in this.Lines)
			{
				if (line.ctrls.Count <= 0)
				{
					continue;
				}
				switch (line.typ)
				{
				case 1:
				{
					int num5 = (((CheckBox)line.ctrls[0]).IsChecked.Value ? 1 : 0);
					if (num5 != line.sel)
					{
						PopupAdapter.Popup_Line_IPOSEL_set(line.idx, num5);
					}
					break;
				}
				case 2:
					try
					{
						if (!double.TryParse(((TextBox)line.ctrls[0]).Text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
						{
							result2 = 0.0;
						}
						if (result2 != line.real1)
						{
							PopupAdapter.Popup_Line_RPOPZ_set(line.idx, result2);
						}
					}
					catch (Exception e4)
					{
						this._logCenterService.CatchRaport(e4);
					}
					break;
				case 3:
					try
					{
						if (!int.TryParse(((TextBox)line.ctrls[0]).Text.Trim(), out var result))
						{
							result = 0;
						}
						if (result != line.inte1)
						{
							PopupAdapter.Popup_Line_IPOPZ_set(line.idx, result);
						}
					}
					catch (Exception e3)
					{
						this._logCenterService.CatchRaport(e3);
					}
					break;
				case 4:
				{
					string text = ((TextBox)line.ctrls[0]).Text;
					if (text != line.ntext)
					{
						PopupAdapter.Popup_Line_POPVAR_set(line.idx, text);
					}
					break;
				}
				case 5:
				{
					int num3 = (((RadioButton)line.ctrls[0]).IsChecked.Value ? 1 : 0);
					if (num3 != line.pyn)
					{
						PopupAdapter.Popup_Line_IPOPYN_set(line.idx, num3);
					}
					break;
				}
				case 6:
				{
					int num2 = (((RadioButton)line.ctrls[0]).IsChecked.Value ? 1 : 0);
					if (num2 != line.sel)
					{
						PopupAdapter.Popup_Line_IPOSEL_set(line.idx, num2);
					}
					break;
				}
				case 7:
				{
					Button obj = (Button)line.ctrls[0];
					int num4 = 0;
					if (obj.BorderThickness.Left != 0.0)
					{
						num4 = 1;
					}
					if (num4 != line.sel)
					{
						PopupAdapter.Popup_Line_IPOSEL_set(line.idx, num4);
					}
					break;
				}
				case 8:
					if (line.is_change)
					{
						PopupAdapter.Popup_Line_IPOSEL_set(line.idx, 1);
					}
					break;
				case 9:
				{
					ComboBox comboBox = (ComboBox)line.ctrls[0];
					int selectedIndex = comboBox.SelectedIndex;
					if (selectedIndex == line.inte1)
					{
						break;
					}
					for (int i = 0; i < comboBox.Items.Count; i++)
					{
						if (selectedIndex == i)
						{
							PopupAdapter.Popup_Line_IPOSEL_set(line.idx + i + 1, 1);
						}
						else
						{
							PopupAdapter.Popup_Line_IPOSEL_set(line.idx + i + 1, 0);
						}
					}
					break;
				}
				case 11:
					try
					{
						int num = ((!(((TextBox)line.ctrls[0]).Text.Trim() == string.Empty)) ? this.Type11GetIntFromString(((TextBox)line.ctrls[0]).Text) : 0);
						if (num != line.inte1)
						{
							PopupAdapter.Popup_Line_IPOPZ_set(line.idx, num);
						}
					}
					catch (Exception e2)
					{
						this._logCenterService.CatchRaport(e2);
					}
					break;
				case 30:
					((QuickTable)line.ctrls[0]).SendAnswer(line.idx + 2, null);
					break;
				}
			}
		}
		else if (!this.isArchiveTreeTab())
		{
			this.MyQuickTable.SendAnswer(filterString, filterString2, filterString3);
		}
		else
		{
			this.ArchiveTreeSendAnswer();
		}
		this._lastExitcode = this.ExitCode;
		this._onPopupAnswer?.Invoke(this.ExitCode);
		this.SaveCurrentWindowLocation();
		if (this.ExitCode == 21 && this.ListMode)
		{
			this.MyQuickTable.SetFilter(filterString, filterString2, filterString3);
		}
		this._likeModalMode.ResetLikeModalMode();
	}

	private void PrintScreen()
	{
		if (this.ListMode)
		{
			return;
		}
		this.ctr_grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)this.ctr_grid.DesiredSize.Width, (int)this.ctr_grid.DesiredSize.Height, 96.0, 96.0, PixelFormats.Pbgra32);
		renderTargetBitmap.Render(this.ctr_grid);
		PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
		pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
		using FileStream stream = File.Create("DRPLOT.png");
		pngBitmapEncoder.Save(stream);
	}

	private void SaveCurrentWindowLocation()
	{
		Directory.CreateDirectory(this._storeFolder);
		PopUpInfo popUpInfo = new PopUpInfo
		{
			PopupLeft = base.Left,
			PopupTop = base.Top,
			PopupHeight = base.Height,
			PopupWidth = base.Width
		};
		for (int i = 0; i < 5; i++)
		{
			popUpInfo.Filters[i] = this._lastFilters[i];
		}
		popUpInfo.Save($"{this._storeFolder}\\{this._mPfileName}.xml", this._logCenterService);
	}

	public void ExitCode128()
	{
		this.ExitCode = 128;
		base.Close();
		this._ignoreNextUp = true;
	}

	private void Window_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		this.ExitCode = 128;
		base.Close();
		e.Handled = true;
		this._ignoreNextUp = true;
	}

	private void Window_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (this._ignoreNextUp)
		{
			e.Handled = true;
			this._ignoreNextUp = false;
		}
	}

	private void ButtonType7_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Button)
		{
			Button button = (Button)sender;
			if (button.BorderThickness.Left == 0.0)
			{
				button.BorderThickness = new Thickness(3.0);
			}
			else
			{
				button.BorderThickness = new Thickness(0.0);
			}
			if (this._logicalDict != null && this._logicalDict.ContainsKey(sender))
			{
				this.LogicalInterpretation(this._logicalDict[sender]);
			}
		}
	}

	private void multisel_Click(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed && ((RadioButton)sender).IsChecked.Value)
		{
			((RadioButton)sender).IsChecked = false;
			e.Handled = true;
		}
	}

	private void cb_Click(object sender, RoutedEventArgs e)
	{
		if (this._logicalDict != null && this._logicalDict.ContainsKey(sender))
		{
			this.LogicalInterpretation(this._logicalDict[sender]);
		}
	}

	private void TextBoxLostFocusTreatment(object sender, RoutedEventArgs e)
	{
		(sender as TextBox).Text = (sender as TextBox).Text.Trim();
	}

	private void TextBoxLogicalLostFocus(object sender, RoutedEventArgs e)
	{
		if (this._logicalDict == null)
		{
			return;
		}
		foreach (PopupLine line in this.Lines)
		{
			if (line.ctrls.Contains(sender))
			{
				line.is_change = true;
			}
		}
		if (this._logicalDict.ContainsKey(sender))
		{
			this.LogicalInterpretation(this._logicalDict[sender]);
		}
	}

	private void ButtonType8_Click(object sender, RoutedEventArgs e)
	{
		if (this._logicalDict == null)
		{
			return;
		}
		foreach (PopupLine line in this.Lines)
		{
			if (line.ctrls.Contains(sender))
			{
				line.is_change = true;
			}
		}
		if (this._logicalDict.ContainsKey(sender))
		{
			this.LogicalInterpretation(this._logicalDict[sender]);
		}
	}

	private void ComboBoxLogicalSelectionChange(object sender, SelectionChangedEventArgs e)
	{
		if (this._logicalDict != null && this._logicalDict.ContainsKey(sender))
		{
			this.LogicalInterpretation(this._logicalDict[sender]);
		}
	}

	private void LogicalInterpretation(string p)
	{
		if (p.Trim().ToUpper() == "CLOSE")
		{
			this.ExitCode = 128;
			base.Close();
		}
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		try
		{
			this.ExitCode = Convert.ToInt32(button.Name.Substring(1)) + 1;
		}
		catch (Exception e2)
		{
			this._logCenterService.CatchRaport(e2);
		}
		base.Close();
	}

	private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.F1)
		{
			this.HelpForThatPopup();
			e.Handled = true;
			return;
		}
		if (e.Key == Key.Next)
		{
			e.Handled = true;
			if (!this.ListMode)
			{
				this.scroll.PageDown();
			}
			else
			{
				this.MyQuickTable.PageDown();
			}
		}
		if (e.Key == Key.Prior)
		{
			e.Handled = true;
			if (!this.ListMode)
			{
				this.scroll.PageUp();
			}
			else
			{
				this.MyQuickTable.PageUp();
			}
		}
		if (e.Key == Key.Escape)
		{
			e.Handled = true;
			base.Close();
		}
		if (e.Key == Key.Return)
		{
			this.ExitCode = 128;
			e.Handled = true;
			base.Close();
		}
	}

	private void HelpForThatPopup()
	{
		int num = this._lastVisibleTab;
		if (this._lastVisibleTab < 1)
		{
			num = 1;
		}
		string popupHelpName = GeneralSystemComponentsAdapter.GetPopupHelpName(this._pKernelFlowGlobalData.LastPipeFunctionName, this._lastPopupname, num);
		HtmlDataManager htmlDataManager = new HtmlDataManager();
		string text = $"{popupHelpName}({this._lastPopupname}/{num})";
		string text2 = text;
		string text3 = htmlDataManager.HelpFindHtmlPage(text, this._languageDictionary.GetCurrentLanguage(), this._useOnlineHelp);
		if (text3 == string.Empty)
		{
			text3 = htmlDataManager.HelpFindHtmlPage(popupHelpName + "(" + this._lastPopupname + ")", this._languageDictionary.GetCurrentLanguage(), this._useOnlineHelp);
		}
		if (text3 == string.Empty)
		{
			text3 = htmlDataManager.HelpFindHtmlPage(popupHelpName, this._languageDictionary.GetCurrentLanguage(), this._useOnlineHelp);
		}
		string empty = string.Empty;
		
		PnExternalCall.Start(empty, this._logCenterService);
	}

	private void tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		this.SetVisibleTab(this.tabs.SelectedIndex + 1);
	}

	private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (this.MyQuickTable.CheckVisibility() != 0)
		{
			if (this.canvas.Width > (double)this._canvasMaxx)
			{
				this.canvas.Width = this._canvasMaxx;
			}
			if (this.ctr_grid.ActualWidth > this.canvas.Width + 19.0)
			{
				this.canvas.Width = this.ctr_grid.ActualWidth - 19.0;
			}
			if (this.canvas.Height > (double)this._canvasMaxy)
			{
				this.canvas.Height = this._canvasMaxy;
			}
			if (this.ctr_grid.RowDefinitions[1].ActualHeight > this.canvas.Height + 19.0)
			{
				this.canvas.Height = this.ctr_grid.RowDefinitions[1].ActualHeight - 19.0;
			}
		}
	}

	private void ExitCode2()
	{
		this.ExitCode = 2;
		base.Close();
	}

	private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
		}
		catch (Exception e2)
		{
			this._logCenterService.CatchRaport(e2);
		}
		e.Handled = true;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		this._subPopup = new global::System.Windows.Controls.Primitives.Popup();
		this._subPopup.PreviewMouseRightButtonDown += subPopup_PreviewMouseRightButtonDown;
		this._calendar.SelectedDatesChanged += calendar_SelectedDatesChanged;
		this._subPopup.Child = this._calendar;
		(PresentationSource.FromVisual(this) as HwndSource).AddHook(WndProc);
	}

	private void subPopup_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		this.CollapsAllExpended(null);
	}

	private void MyControl_IsVisibileChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
	}

	private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (msg == 1024 && wParam == new IntPtr(126))
		{
			if (this.ListMode)
			{
				this.MyQuickTable.KeyboardFocusRightFilter();
			}
			else if (this._focusControl != null)
			{
				this._focusControl.Focusable = true;
				this._focusControl.Focus();
				Keyboard.Focus(this._focusControl);
			}
			return IntPtr.Zero;
		}
		return IntPtr.Zero;
	}

	private void FocusByMessage()
	{
		WindowEvents.PostMessage(this.Handle, 1024, new IntPtr(126), IntPtr.Zero);
	}

	private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.Visibility == Visibility.Visible)
		{
			this.FocusByMessage();
		}
	}

	private void filter_text_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (this.ListMode)
		{
			this.dispatcher_string = this.filter_text.Text;
			this.dispatcher_string1 = this.filter_text1.Text;
			this.dispatcher_string2 = this.filter_text2.Text;
			if (this.dispatcherTimer != null)
			{
				this.dispatcherTimer.Stop();
			}
			this.dispatcherTimer = new DispatcherTimer();
			this.dispatcherTimer.Tick += dispatcherTimer_Tick;
			this.dispatcherTimer.Interval = new TimeSpan(5000000L);
			this.dispatcherTimer.Start();
		}
	}

	private void dispatcherTimer_Tick(object sender, EventArgs e)
	{
		this.dispatcherTimer.Stop();
		this.MyQuickTable.SetFilter(this.filter_text.Text, this.filter_text1.Text, this.filter_text2.Text);
	}

	private void filter_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		this.MyQuickTable.SetFilter(this.dispatcher_string, this.dispatcher_string1, this.dispatcher_string2);
	}

	private void filter_image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (this._codePopup != null && this._codePopup.IsOpen)
		{
			this._codePopup.IsOpen = false;
			return;
		}
		if (this._codePopup == null)
		{
			this._codePopup = new global::System.Windows.Controls.Primitives.Popup();
			ListBox listBox = new ListBox
			{
				Width = 200.0
			};
			this._codePopup.Child = listBox;
			listBox.SelectionChanged += list_SelectionChanged;
		}
		else
		{
			((ListBox)this._codePopup.Child).Items.Clear();
		}
		string[] lastFilters = this._lastFilters;
		foreach (string text in lastFilters)
		{
			if (text == null)
			{
				((ListBox)this._codePopup.Child).Items.Add(string.Empty);
			}
			else
			{
				((ListBox)this._codePopup.Child).Items.Add(text);
			}
		}
		this.latest_codePopupImage = sender as Image;
		Point point = this.latest_codePopupImage.PointToScreen(new Point(0.0, 17.0));
		this._codePopup.HorizontalOffset = point.X;
		this._codePopup.VerticalOffset = point.Y;
		this._codePopup.IsOpen = true;
	}

	private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		this._codePopup.IsOpen = false;
		ListBox listBox = (ListBox)this._codePopup.Child;
		if (listBox.SelectedIndex >= 0)
		{
			if (this.latest_codePopupImage == this.filter_image)
			{
				this.filter_text.Text = (string)listBox.Items[listBox.SelectedIndex];
			}
			if (this.latest_codePopupImage == this.filter_image1)
			{
				this.filter_text1.Text = (string)listBox.Items[listBox.SelectedIndex];
			}
			if (this.latest_codePopupImage == this.filter_image2)
			{
				this.filter_text2.Text = (string)listBox.Items[listBox.SelectedIndex];
			}
		}
	}

	private void i_Click(object sender, RoutedEventArgs e)
	{
		string text = ((MenuItem)sender).Header.ToString();
		this.filter_text.Text = text;
	}

	private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (this._codePopup != null && this._codePopup.IsOpen)
		{
			this._codePopup.IsOpen = false;
		}
		if (this._subPopup != null && this._subPopup.IsOpen)
		{
			this.CollapsAllExpended(null);
			if (e.Source == this._lastExpander)
			{
				e.Handled = true;
			}
			this._lastExpander = null;
		}
	}

	private void Window_LocationChanged(object sender, EventArgs e)
	{
		this.HideActivePopups();
	}

	private void HideActivePopups()
	{
		if (this._codePopup != null && this._codePopup.IsOpen)
		{
			this._codePopup.IsOpen = false;
		}
		if (this._subPopup != null && this._subPopup.IsOpen)
		{
			this.CollapsAllExpended(null);
			this._lastExpander = null;
		}
	}

	private void Window_LostFocus(object sender, RoutedEventArgs e)
	{
	}

	private void Window_Deactivated(object sender, EventArgs e)
	{
		this.HideActivePopups();
	}

	private void Window_Activated(object sender, EventArgs e)
	{
	}

	[DllImport("user32.dll")]
	private static extern uint GetWindowLong(nint hwnd, int index);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(nint hwnd, int index, uint newStyle);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(nint hwnd, nint hwndInsertAfter, int x, int y, int width, int height, uint flags);

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		nint handle = new WindowInteropHelper(this).Handle;
		Popup.SetWindowLong(handle, -16, Popup.GetWindowLong(handle, -16) & 0xFFFCFFFFu);
		Popup.SetWindowLong(handle, -20, Popup.GetWindowLong(handle, -20) | 0x400);
		Popup.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 39u);
		((HwndSource)PresentationSource.FromVisual(this)).AddHook(HelpHook);
	}

	private void MoveToStartClick(object sender, RoutedEventArgs e)
	{
		if (this.ListMode)
		{
			this.MyQuickTable.MoveSelectedToStart();
		}
	}

	private void MoveOneUpClick(object sender, RoutedEventArgs e)
	{
		if (this.ListMode)
		{
			this.MyQuickTable.MoveSelectedOneUp();
		}
	}

	private void MoveOneDownClick(object sender, RoutedEventArgs e)
	{
		if (this.ListMode)
		{
			this.MyQuickTable.MoveSelectedOneDown();
		}
	}

	private void MoveToEndClick(object sender, RoutedEventArgs e)
	{
		if (this.ListMode)
		{
			this.MyQuickTable.MoveSelectedToEnd();
		}
	}

	private void Boo_Click(object sender, RoutedEventArgs e)
	{
		string empty = string.Empty;
		File.WriteAllText(contents: XamlWriter.Save(this.canvas), path: "C:\\" + Guid.NewGuid().ToString() + ".xaml");
	}

	private nint HelpHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (msg == 274 && ((int)wParam & 0xFFF0) == 61824)
		{
			this.HelpForThatPopup();
			handled = true;
		}
		return IntPtr.Zero;
	}

}
