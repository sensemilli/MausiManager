using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using pncommon.WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public class Toolbars
{
	public enum CustomToolbarType
	{
		None,
		Ribbon,
		Tray,
		LastCommandTray,
		Qat,
		RightMenu
	}

	public enum CustomToolbarModification
	{
		LikeBefore,
		Items,
		Style,
		Created
	}

	public enum CustomToolbarLocation
	{
		Top,
		Bottom,
		Left,
		Right
	}

	public enum CustomToolbarText
	{
		No,
		Bottom,
		Right
	}

	public class ToolBarDef
	{
		public ToolBar Toolbar;

		public CustomToolbarLocation ToolbarLocation;

		public CustomToolbarText ToolbarText;

		public CustomToolbarModification Modification { get; set; }

		public CustomToolbarType Type { get; set; }

		public bool Visible { get; set; }

		public bool SmallIcons { get; set; }

		public string Name { get; set; }

		public List<IRFileRecord> Items { get; set; }

		public double ReadedWidth { get; set; }

		public int ReadedBand { get; set; }

		public int ReadedBandIndex { get; set; }

		public ToolBarDef(ToolBarDef org)
		{
			Modification = org.Modification;
			Type = org.Type;
			Visible = org.Visible;
			SmallIcons = org.SmallIcons;
			Name = org.Name;
			Items = new List<IRFileRecord>();
			ToolbarText = org.ToolbarText;
			ToolbarLocation = org.ToolbarLocation;
			foreach (IRFileRecord item in org.Items)
			{
				Items.Add(new RFileRecord(item));
			}
			Toolbar = org.Toolbar;
		}

		public ToolBarDef()
		{
			Items = new List<IRFileRecord>();
			Modification = CustomToolbarModification.Created;
			ReadedWidth = double.NaN;
			ReadedBand = -1;
			ReadedBandIndex = -1;
			ToolbarLocation = CustomToolbarLocation.Top;
			ToolbarText = CustomToolbarText.No;
		}
	}

	private ContextMenu _toolbarButtonContextMenu;

	private MainWindow _mainWindow;

	private readonly ILanguageDictionary _languageDictionary;

	private readonly IRibbonMainWindowConnector _ribbonMainWindowConnector;

	private readonly IPnToolTipService _pnToolTipService;

	private readonly IPnPathService _pnPathService;

	private readonly IPnIconsService _pnIconsService;

	private readonly ILogCenterService _logCenterService;

	private List<ToolBarDef> _toolbars;

	private bool _is3D;

	public const int MaxCustomToolbar = 10;

	private object _lastContextMenuOpening;

	private bool _mBlockNonDispalay;

	private const int LastToolbarCount = 10;

	private string _moduleName = string.Empty;

	public bool IsSpecjalRibbontab { get; set; }

	public Toolbars(IPnPathService pathService, ILogCenterService logCenter, IPnToolTipService toolTip, IPnIconsService iconsService, ILanguageDictionary languageDictionary, IRibbonMainWindowConnector ribbonMainWindowConnector)
	{
		_logCenterService = logCenter;
		_pnPathService = pathService;
		_pnToolTipService = toolTip;
		_pnIconsService = iconsService;
		_languageDictionary = languageDictionary;
		_ribbonMainWindowConnector = ribbonMainWindowConnector;
		_ribbonMainWindowConnector.OnAddQatButtonToToolbar += AddQatButton;
	}

	public void Init1(MainWindow mainWindow)
	{
		_mainWindow = mainWindow;
	}

	public void Init2()
	{
		_toolbarButtonContextMenu = new ContextMenu();
		SetLanguageContextMenu();
		Reset();
	}

	private void SetLanguageContextMenu()
	{
		_toolbarButtonContextMenu.Items.Clear();
		_toolbarButtonContextMenu.Items.Add(Create_MenuItem_QAT_BR());
		_toolbarButtonContextMenu.Items.Add(Create_MenuItem_QAT_BL());
		_toolbarButtonContextMenu.Items.Add(Create_MenuItem_QAT_TR());
		_toolbarButtonContextMenu.Items.Add(Create_MenuItem_QAT_TL());
	}

	private object Create_MenuItem_QAT_BR()
	{
		MenuItem menuItem = new MenuItem();
		menuItem.Header = _languageDictionary.GetMsg2Int("Add to Quick Access Toolbar: Bottom Right");
		menuItem.Click += AddToQAT_BR_Click;
		return menuItem;
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

	private void AddToQAT_BR_Click(object sender, RoutedEventArgs e)
	{
		if (_lastContextMenuOpening == null)
		{
			return;
		}
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Toolbar == null)
			{
				continue;
			}
			for (int i = 0; i < toolbar.Toolbar.Items.Count; i++)
			{
				if (toolbar.Toolbar.Items[i] == _lastContextMenuOpening)
				{
					AddQatButton(4, toolbar.Items[i]);
					return;
				}
			}
		}
	}

	private void AddToQAT_BL_Click(object sender, RoutedEventArgs e)
	{
		if (_lastContextMenuOpening == null)
		{
			return;
		}
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Toolbar == null)
			{
				continue;
			}
			for (int i = 0; i < toolbar.Toolbar.Items.Count; i++)
			{
				if (toolbar.Toolbar.Items[i] == _lastContextMenuOpening)
				{
					AddQatButton(3, toolbar.Items[i]);
					return;
				}
			}
		}
	}

	private void AddToQAT_TR_Click(object sender, RoutedEventArgs e)
	{
		if (_lastContextMenuOpening == null)
		{
			return;
		}
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Toolbar == null)
			{
				continue;
			}
			for (int i = 0; i < toolbar.Toolbar.Items.Count; i++)
			{
				if (toolbar.Toolbar.Items[i] == _lastContextMenuOpening)
				{
					AddQatButton(2, toolbar.Items[i]);
					return;
				}
			}
		}
	}

	private void AddToQAT_TL_Click(object sender, RoutedEventArgs e)
	{
		if (_lastContextMenuOpening == null)
		{
			return;
		}
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Toolbar == null)
			{
				continue;
			}
			for (int i = 0; i < toolbar.Toolbar.Items.Count; i++)
			{
				if (toolbar.Toolbar.Items[i] == _lastContextMenuOpening)
				{
					AddQatButton(1, toolbar.Items[i]);
					return;
				}
			}
		}
	}

	public void Reset()
	{
		if (_toolbars != null)
		{
			foreach (ToolBarDef toolbar in _toolbars)
			{
				if (toolbar.Toolbar == null)
				{
					continue;
				}
				for (int i = 0; i < toolbar.Toolbar.Items.Count; i++)
				{
					if (toolbar.Items[i].FunctionGroup > 0)
					{
						_pnIconsService.UnregisterIconData((DependencyObject)toolbar.Toolbar.Items[i]);
					}
				}
			}
		}
		//_mainWindow.toolbartray_top.ToolBars.Clear();
	//	_mainWindow.toolbartray_left.ToolBars.Clear();
		//_mainWindow.toolbartray_right.ToolBars.Clear();
		//_mainWindow.QAT_bl.Items.Clear();
		//_mainWindow.QAT_br.Items.Clear();
		_toolbars = new List<ToolBarDef>();
	}

	public List<ToolBarDef> GetToolbarsDefinitionCopy()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			toolbar.Modification = CustomToolbarModification.LikeBefore;
		}
		return _toolbars.ToList();
	}

	private void LoadXmlConfig()
	{
		if (string.IsNullOrEmpty(_moduleName))
		{
			return;
		}
		try
		{
			string text = ModifyFileNameIf3D("\\pn2toolbars.xml");
			string text2 = "pn.rfiles\\" + _moduleName + text;
			if (File.Exists(text2))
			{
				ToolBarDef toolBarDef = null;
				XmlTextReader xmlTextReader = new XmlTextReader(text2);
				while (xmlTextReader.Read())
				{
					if (xmlTextReader.NodeType != XmlNodeType.Element || !(xmlTextReader.Name.ToString() == "Toolbar"))
					{
						continue;
					}
					toolBarDef = new ToolBarDef();
					toolBarDef.Name = xmlTextReader.GetAttribute("Name");
					toolBarDef.Visible = xmlTextReader.GetAttribute("Visible") == "True";
					toolBarDef.SmallIcons = xmlTextReader.GetAttribute("SmallIcons") == "True";
					switch (xmlTextReader.GetAttribute("CustomToolbarLocation"))
					{
					case "Top":
						toolBarDef.ToolbarLocation = CustomToolbarLocation.Top;
						break;
					case "Bottom":
						toolBarDef.ToolbarLocation = CustomToolbarLocation.Bottom;
						break;
					case "Left":
						toolBarDef.ToolbarLocation = CustomToolbarLocation.Left;
						break;
					case "Right":
						toolBarDef.ToolbarLocation = CustomToolbarLocation.Right;
						break;
					}
					switch (xmlTextReader.GetAttribute("CustomToolbarText"))
					{
					case "No":
						toolBarDef.ToolbarText = CustomToolbarText.No;
						break;
					case "Bottom":
						toolBarDef.ToolbarText = CustomToolbarText.Bottom;
						break;
					case "Right":
						toolBarDef.ToolbarText = CustomToolbarText.Right;
						break;
					}
					switch (xmlTextReader.GetAttribute("Type"))
					{
					case "Ribbon":
						toolBarDef.Type = CustomToolbarType.Ribbon;
						break;
					case "Tray":
						toolBarDef.Type = CustomToolbarType.Tray;
						break;
					case "LastCommandTray":
						toolBarDef.Type = CustomToolbarType.LastCommandTray;
						break;
					case "Qat":
						toolBarDef.Type = CustomToolbarType.Qat;
						break;
					case "RightMenu":
						toolBarDef.Type = CustomToolbarType.RightMenu;
						break;
					}
					string attribute = xmlTextReader.GetAttribute("Width");
					if (attribute != null)
					{
						try
						{
							toolBarDef.ReadedWidth = Convert.ToDouble(attribute);
						}
						catch (Exception e)
						{
							_logCenterService.CatchRaport(e);
						}
					}
					attribute = xmlTextReader.GetAttribute("Band");
					if (attribute != null)
					{
						try
						{
							toolBarDef.ReadedBand = Convert.ToInt32(attribute);
						}
						catch (Exception e2)
						{
							_logCenterService.CatchRaport(e2);
						}
					}
					attribute = xmlTextReader.GetAttribute("BandIndex");
					if (attribute != null)
					{
						try
						{
							toolBarDef.ReadedBandIndex = Convert.ToInt32(attribute);
						}
						catch (Exception e3)
						{
							_logCenterService.CatchRaport(e3);
						}
					}
					bool flag = true;
					if ((toolBarDef.Type == CustomToolbarType.Tray && toolBarDef.Name == "LastToolbar") || toolBarDef.Type == CustomToolbarType.None)
					{
						flag = false;
					}
					if (flag)
					{
						_toolbars.Add(toolBarDef);
					}
				}
				xmlTextReader.Close();
				xmlTextReader.Dispose();
			}
		}
		catch (Exception e4)
		{
			_logCenterService.CatchRaport(e4);
		}
		ScanForNotLoadedToolbars();
		foreach (ToolBarDef toolbar in _toolbars)
		{
			string name = ModifyFileNameIf3D(toolbar.Name);
			toolbar.Items = RFiles.ReadFile(_moduleName, name, _pnPathService);
			ToolBarDef toolBarDef2 = toolbar;
			if (toolBarDef2.Items == null)
			{
				List<IRFileRecord> list2 = (toolBarDef2.Items = new List<IRFileRecord>());
			}
		}
		foreach (ToolBarDef toolbar2 in _toolbars)
		{
			if (toolbar2.Type == CustomToolbarType.LastCommandTray)
			{
				for (int i = toolbar2.Items.Count; i < 10; i++)
				{
					toolbar2.Items.Add(new RFileRecord());
				}
			}
		}
		foreach (ToolBarDef toolbar3 in _toolbars)
		{
			CreateToolTipForToolBar(toolbar3);
		}
	}

	private void CreateToolTipForToolBar(ToolBarDef bar)
	{
		if (bar.Toolbar != null)
		{
			ToolTip toolTip = new ToolTip();
			toolTip.Content = bar.Name;
			bar.Toolbar.ToolTip = toolTip;
		}
	}

	public void SaveXmlConfig()
	{
		if (_moduleName == string.Empty)
		{
			return;
		}
		try
		{
			string text = ModifyFileNameIf3D("\\pn2toolbars.xml");
			XmlTextWriter xmlTextWriter = new XmlTextWriter("pn.rfiles\\" + _moduleName + text, null);
			xmlTextWriter.Formatting = Formatting.Indented;
			xmlTextWriter.WriteStartDocument();
			xmlTextWriter.WriteStartElement("CustomToolbars");
			foreach (ToolBarDef toolbar in _toolbars)
			{
				xmlTextWriter.WriteStartElement("Toolbar");
				xmlTextWriter.WriteAttributeString("Name", toolbar.Name);
				xmlTextWriter.WriteAttributeString("Visible", toolbar.Visible.ToString());
				xmlTextWriter.WriteAttributeString("SmallIcons", toolbar.SmallIcons.ToString());
				xmlTextWriter.WriteAttributeString("Type", toolbar.Type.ToString());
				xmlTextWriter.WriteAttributeString("CustomToolbarLocation", toolbar.ToolbarLocation.ToString());
				xmlTextWriter.WriteAttributeString("CustomToolbarText", toolbar.ToolbarText.ToString());
				if (toolbar.Toolbar != null)
				{
					xmlTextWriter.WriteAttributeString("Band", toolbar.Toolbar.Band.ToString());
					xmlTextWriter.WriteAttributeString("BandIndex", toolbar.Toolbar.BandIndex.ToString());
					if (!double.IsNaN(toolbar.Toolbar.Width))
					{
						xmlTextWriter.WriteAttributeString("Width", toolbar.Toolbar.Width.ToString());
					}
				}
				xmlTextWriter.WriteEndElement();
			}
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.WriteEndDocument();
			xmlTextWriter.Close();
			xmlTextWriter.Dispose();
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
		try
		{
			Directory.CreateDirectory("pn.rfiles");
			foreach (ToolBarDef toolbar2 in _toolbars)
			{
				string path = "pn.rfiles\\" + _moduleName + "\\" + ModifyFileNameIf3D(toolbar2.Name);
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
		}
		catch (Exception e2)
		{
			_logCenterService.CatchRaport(e2);
		}
		foreach (ToolBarDef toolbar3 in _toolbars)
		{
			if (toolbar3.Items.Count <= 0)
			{
				continue;
			}
			string path2 = "pn.rfiles\\" + _moduleName + "\\" + ModifyFileNameIf3D(toolbar3.Name);
			try
			{
				StreamWriter streamWriter = new StreamWriter(path2);
				foreach (IRFileRecord item in toolbar3.Items)
				{
					streamWriter.WriteLine(item.GetOutputText());
				}
				streamWriter.Close();
			}
			catch (Exception e3)
			{
				_logCenterService.CatchRaport(e3);
			}
		}
	}

	private string ModifyFileNameIf3D(string name)
	{
		if (_is3D)
		{
			return $"{name}_3D";
		}
		return name;
	}

	private void ScanForNotLoadedToolbars()
	{
		if (!IsRibbonUserTabLoaded())
		{
			ToolBarDef toolBarDef = new ToolBarDef();
			toolBarDef.Type = CustomToolbarType.Ribbon;
			toolBarDef.Name = "RibbonUserTab";
			toolBarDef.Visible = false;
			_toolbars.Add(toolBarDef);
		}
		if (!IsLastCommandTrayLoaded())
		{
			ToolBarDef toolBarDef2 = new ToolBarDef();
			toolBarDef2.Type = CustomToolbarType.LastCommandTray;
			toolBarDef2.Name = "LastToolbar";
			toolBarDef2.Visible = !_is3D;
			toolBarDef2.SmallIcons = false;
			toolBarDef2.ToolbarText = CustomToolbarText.Bottom;
			toolBarDef2.ToolbarLocation = CustomToolbarLocation.Left;
			_toolbars.Insert(1, toolBarDef2);
		}
		for (int i = 0; i < 10; i++)
		{
			if (!IsUserToolbarLoaded(i + 1))
			{
				ToolBarDef toolBarDef3 = new ToolBarDef();
				toolBarDef3.Type = CustomToolbarType.Tray;
				toolBarDef3.Name = GetUserToolbarName(i + 1);
				toolBarDef3.Visible = false;
				toolBarDef3.SmallIcons = false;
				toolBarDef3.ToolbarText = CustomToolbarText.Bottom;
				if (i == 0)
				{
					toolBarDef3.Visible = !_is3D;
					toolBarDef3.ToolbarLocation = CustomToolbarLocation.Right;
				}
				_toolbars.Add(toolBarDef3);
			}
		}
		for (int j = 0; j < 4; j++)
		{
			if (!IsQatLoaded(j + 1))
			{
				ToolBarDef toolBarDef4 = new ToolBarDef();
				toolBarDef4.Type = CustomToolbarType.Qat;
				toolBarDef4.Name = GetQatToolbarName(j + 1);
				toolBarDef4.Visible = true;
				_toolbars.Add(toolBarDef4);
			}
		}
		if (!IsRightMenuLoaded())
		{
			ToolBarDef toolBarDef5 = new ToolBarDef();
			toolBarDef5.Type = CustomToolbarType.RightMenu;
			toolBarDef5.Name = "RightMenu";
			toolBarDef5.Visible = false;
			toolBarDef5.ToolbarLocation = CustomToolbarLocation.Right;
			toolBarDef5.ToolbarText = CustomToolbarText.Right;
			toolBarDef5.SmallIcons = true;
			_toolbars.Add(toolBarDef5);
		}
	}

	private bool IsRibbonUserTabLoaded()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Type == CustomToolbarType.Ribbon)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsLastCommandTrayLoaded()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Type == CustomToolbarType.LastCommandTray)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsRightMenuLoaded()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Type == CustomToolbarType.RightMenu)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsQatLoaded(int id)
	{
		string qatToolbarName = GetQatToolbarName(id);
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Type == CustomToolbarType.Qat && toolbar.Name == qatToolbarName)
			{
				return true;
			}
		}
		return false;
	}

	private string GetQatToolbarName(int id)
	{
		string text = "Qat";
		switch (id)
		{
		case 1:
			text += "TopLeft";
			break;
		case 2:
			text += "TopRight";
			break;
		case 3:
			text += "BottomLeft";
			break;
		case 4:
			text += "BottomRight";
			break;
		}
		return text;
	}

	private bool IsUserToolbarLoaded(int id)
	{
		string userToolbarName = GetUserToolbarName(id);
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Type == CustomToolbarType.Tray && toolbar.Name == userToolbarName)
			{
				return true;
			}
		}
		return false;
	}

	private string GetUserToolbarName(int id)
	{
		return "UserToolbar" + id;
	}

	public void SetNewSettingsFromDialog(List<ToolBarDef> list)
	{
		_toolbars = list;
		PropagateDatas();
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Toolbar != null)
			{
				toolbar.Toolbar.Width = double.NaN;
				toolbar.Toolbar.Height = double.NaN;
			}
		}
	//	_mainWindow.toolbartray_top.UpdateLayout();
	//	_mainWindow.toolbartray_bottom.UpdateLayout();
	//	_mainWindow.toolbartray_left.UpdateLayout();
	//	_mainWindow.toolbartray_right.UpdateLayout();
		SaveXmlConfig();
	}

	private void PropagateDatas()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			switch (toolbar.Type)
			{
			case CustomToolbarType.Ribbon:
				//_mainWindow.ribbon1.SetCustomToolbar(toolbar.Items, toolbar.Visible);
				break;
			case CustomToolbarType.Tray:
				SetCustomToolbar(toolbar);
				break;
			case CustomToolbarType.LastCommandTray:
				SetCustomToolbar(toolbar);
				break;
			case CustomToolbarType.Qat:
				SetQatToolbar(toolbar);
				break;
			case CustomToolbarType.RightMenu:
				SetCustomToolbar(toolbar);
				break;
			}
			toolbar.Modification = CustomToolbarModification.LikeBefore;
		}
	//	_mainWindow.toolbartray_top.UpdateLayout();
	//	_mainWindow.toolbartray_bottom.UpdateLayout();
//		_mainWindow.toolbartray_left.UpdateLayout();
	//	_mainWindow.toolbartray_right.UpdateLayout();
	}

	private void SetQatToolbar(ToolBarDef bar)
	{
		if (bar.Toolbar == null)
		{
			if (bar.Name == GetQatToolbarName(1))
			{
			//	bar.Toolbar = _mainWindow.QAT_tl;
			}
			else if (bar.Name == GetQatToolbarName(2))
			{
				//bar.Toolbar = _mainWindow.QAT_tr;
			}
			else if (bar.Name == GetQatToolbarName(3))
			{
				//bar.Toolbar = _mainWindow.QAT_bl;
			}
			else if (bar.Name == GetQatToolbarName(4))
			{
				//bar.Toolbar = _mainWindow.QAT_br;
			}
		}
		if (bar.Toolbar == null)
		{
			return;
		}
		foreach (DependencyObject item in (IEnumerable)bar.Toolbar.Items)
		{
			_pnIconsService.UnregisterIconData(item);
		}
		bar.Toolbar.Items.Clear();
		if (!bar.Visible || bar.Items.Count == 0)
		{
			return;
		}
		if (bar.Items.Count == 0)
		{
			bar.Toolbar.Visibility = Visibility.Hidden;
		}
		else
		{
			bar.Toolbar.Visibility = Visibility.Visible;
		}
		foreach (IRFileRecord item2 in bar.Items)
		{
			IRFileRecord iRFileRecord = item2;
			if (iRFileRecord.FunctionName == null)
			{
				string text = (iRFileRecord.FunctionName = string.Empty);
			}
			if (item2.FunctionGroup == 0 && item2.FunctionName == string.Empty)
			{
				item2.FunctionGroup = -1;
			}
			if (item2.FunctionGroup == -1)
			{
				bar.Toolbar.Items.Add(new Separator());
				continue;
			}
			Button button = new Button();
			_pnToolTipService.SetTooltip(button, new PnCommand(item2));
			button.Click += btn_Click;
			Image image = new Image();
			image.BeginInit();
			_pnIconsService.SetSmallIcon(image, Image.SourceProperty, item2.FunctionName);
			image.Height = 16.0;
			image.Width = 16.0;
			image.EndInit();
			button.Content = image;
			bar.Toolbar.Items.Add(button);
		}
	}

	private void SetCustomToolbar(ToolBarDef bar)
	{
		if (bar.Items.Count == 0 && bar.Visible && bar != GetRightMenuToolbar())
		{
			bar.Visible = false;
		}
		if (bar.Toolbar == null)
		{
			bar.Toolbar = new ToolBar();
			CreateToolTipForToolBar(bar);
			if (!double.IsNaN(bar.ReadedWidth))
			{
				bar.Toolbar.Width = bar.ReadedWidth;
			}
			if (bar.ReadedBand >= 0)
			{
				bar.Toolbar.Band = bar.ReadedBand;
			}
			if (bar.ReadedBandIndex >= 0)
			{
				bar.Toolbar.BandIndex = bar.ReadedBandIndex;
			}
		}
		else
		{
			foreach (DependencyObject item in (IEnumerable)bar.Toolbar.Items)
			{
				_pnIconsService.UnregisterIconData(item);
			}
			bar.Toolbar.Items.Clear();
		}
		foreach (IRFileRecord item2 in bar.Items)
		{
			IRFileRecord iRFileRecord = item2;
			if (iRFileRecord.FunctionName == null)
			{
				string text = (iRFileRecord.FunctionName = string.Empty);
			}
			if (item2.FunctionGroup == 0 && item2.FunctionName == string.Empty)
			{
				item2.FunctionGroup = -1;
			}
			if (item2.FunctionGroup == -1)
			{
				bar.Toolbar.Items.Add(new Separator());
				continue;
			}
			Button button = new Button();
			button.ContextMenu = _toolbarButtonContextMenu;
			button.ContextMenuOpening += btn_ContextMenuOpening;
			_pnToolTipService.SetTooltip(button, new PnCommand(item2));
			button.Click += btn_Click;
			Image image = new Image();
			image.BeginInit();
			if (!bar.SmallIcons)
			{
				_pnIconsService.SetBigIcon(button, image, Image.SourceProperty, item2.FunctionName);
				image.Height = 32.0;
				image.Width = 32.0;
			}
			else
			{
				_pnIconsService.SetSmallIcon(button, image, Image.SourceProperty, item2.FunctionName);
				image.Height = 16.0;
				image.Width = 16.0;
			}
			image.EndInit();
			switch (bar.ToolbarText)
			{
			case CustomToolbarText.No:
				button.Content = image;
				break;
			case CustomToolbarText.Right:
			{
				StackPanel stackPanel = new StackPanel();
				TextBlock textBlock = new TextBlock();
				textBlock.Text = _languageDictionary.GetButtonLabel(item2.IdLabel, item2.DefaultLabel);
				if (item2.DrawArrow)
				{
					textBlock.Text += " →";
				}
				textBlock.VerticalAlignment = VerticalAlignment.Center;
				textBlock.Padding = new Thickness(2.0);
				stackPanel.Children.Add(image);
				stackPanel.Children.Add(textBlock);
				stackPanel.Orientation = Orientation.Horizontal;
				stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
				button.HorizontalAlignment = HorizontalAlignment.Left;
				button.Content = stackPanel;
				if ((bar.Type == CustomToolbarType.RightMenu || bar.Type == CustomToolbarType.LastCommandTray) && (bar.ToolbarLocation == CustomToolbarLocation.Right || bar.ToolbarLocation == CustomToolbarLocation.Left))
				{
					if (bar.SmallIcons)
					{
						stackPanel.Width = 120.0;
					}
					else
					{
						stackPanel.Width = 140.0;
					}
				}
				break;
			}
			case CustomToolbarText.Bottom:
			{
				StackPanel stackPanel = new StackPanel();
				TextBlock textBlock = new TextBlock();
				textBlock.Text = _languageDictionary.GetButtonLabel(item2.IdLabel, item2.DefaultLabel);
				stackPanel.Children.Add(image);
				stackPanel.Children.Add(textBlock);
				stackPanel.Orientation = Orientation.Vertical;
				button.Content = stackPanel;
				stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
				button.HorizontalAlignment = HorizontalAlignment.Center;
				if ((bar.Type == CustomToolbarType.RightMenu || bar.Type == CustomToolbarType.LastCommandTray) && (bar.ToolbarLocation == CustomToolbarLocation.Right || bar.ToolbarLocation == CustomToolbarLocation.Left))
				{
					button.Width = 90.0;
				}
				break;
			}
			}
			bar.Toolbar.Items.Add(button);
		}
		if (!bar.Visible)
		{
			RemoveToolbarFromAnyTrays(bar.Toolbar);
			bar.Toolbar = null;
			bar.ReadedWidth = double.NaN;
			bar.ReadedBand = -1;
			bar.ReadedBandIndex = -1;
			return;
		}
		/*
		switch (bar.ToolbarLocation)
		{
		case CustomToolbarLocation.Top:
			if (!_mainWindow.toolbartray_top.ToolBars.Contains(bar.Toolbar))
			{
				RemoveToolbarFromAnyTrays(bar.Toolbar);
				_mainWindow.toolbartray_top.ToolBars.Add(bar.Toolbar);
			}
			break;
		case CustomToolbarLocation.Bottom:
			if (!_mainWindow.toolbartray_bottom.ToolBars.Contains(bar.Toolbar))
			{
				RemoveToolbarFromAnyTrays(bar.Toolbar);
				_mainWindow.toolbartray_bottom.ToolBars.Add(bar.Toolbar);
			}
			break;
		case CustomToolbarLocation.Left:
			if (!_mainWindow.toolbartray_left.ToolBars.Contains(bar.Toolbar))
			{
				RemoveToolbarFromAnyTrays(bar.Toolbar);
				_mainWindow.toolbartray_left.ToolBars.Add(bar.Toolbar);
			}
			break;
		case CustomToolbarLocation.Right:
			if (!_mainWindow.toolbartray_right.ToolBars.Contains(bar.Toolbar))
			{
				RemoveToolbarFromAnyTrays(bar.Toolbar);
				_mainWindow.toolbartray_right.ToolBars.Add(bar.Toolbar);
			}
			break;
		}
		*/
	}

	private void btn_ContextMenuOpening(object sender, ContextMenuEventArgs e)
	{
		_lastContextMenuOpening = sender;
	}

	private void RemoveToolbarFromAnyTrays(ToolBar toolBar)
	{
		/*
		if (toolBar != null)
		{
			if (_mainWindow.toolbartray_top.ToolBars.Contains(toolBar))
			{
				_mainWindow.toolbartray_top.ToolBars.Remove(toolBar);
			}
			else if (_mainWindow.toolbartray_bottom.ToolBars.Contains(toolBar))
			{
				_mainWindow.toolbartray_bottom.ToolBars.Remove(toolBar);
			}
			else if (_mainWindow.toolbartray_left.ToolBars.Contains(toolBar))
			{
				_mainWindow.toolbartray_left.ToolBars.Remove(toolBar);
			}
			else if (_mainWindow.toolbartray_right.ToolBars.Contains(toolBar))
			{
				_mainWindow.toolbartray_right.ToolBars.Remove(toolBar);
			}
		}
		*/
	}

	private void btn_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			foreach (ToolBarDef toolbar in _toolbars)
			{
				if (toolbar.Toolbar == null)
				{
					continue;
				}
				for (int i = 0; i < toolbar.Toolbar.Items.Count; i++)
				{
					if (toolbar.Toolbar.Items[i] == sender)
					{
						_mainWindow.ExeFlow.CallPnCommand(new PnCommand(toolbar.Items[i]));
						return;
					}
				}
			}
		}
		catch (Exception e2)
		{
			_logCenterService.CatchRaport(e2);
		}
	}

	public void BlockMenuEvent()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Toolbar == null)
			{
				continue;
			}
			for (int i = 0; i < toolbar.Toolbar.Items.Count; i++)
			{
				if (toolbar.Items[i].FunctionGroup > 0)
				{
					((Button)toolbar.Toolbar.Items[i]).IsEnabled = false;
					((Button)toolbar.Toolbar.Items[i]).Opacity = 0.5;
				}
			}
		}
	}

	public void UnBlockMenuEvent()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Toolbar == null)
			{
				continue;
			}
			for (int i = 0; i < toolbar.Toolbar.Items.Count; i++)
			{
				if (toolbar.Items[i].FunctionGroup > 0)
				{
					((Button)toolbar.Toolbar.Items[i]).IsEnabled = true;
					((Button)toolbar.Toolbar.Items[i]).Opacity = 1.0;
				}
			}
		}
		if (_mBlockNonDispalay)
		{
			BlockMenuEventNotDisplay();
		}
	}

	public void BlockMenuEventNotDisplay()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Toolbar == null)
			{
				continue;
			}
			for (int i = 0; i < toolbar.Toolbar.Items.Count; i++)
			{
				if (toolbar.Items[i].FunctionGroup > 0 && BlockUnblockItems.NonDisplay(toolbar.Items[i]))
				{
					((Button)toolbar.Toolbar.Items[i]).IsEnabled = false;
					((Button)toolbar.Toolbar.Items[i]).Opacity = 0.5;
				}
			}
		}
		_mBlockNonDispalay = true;
	}

	public void UnBlockMenuEventNotDisplay()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Toolbar == null)
			{
				continue;
			}
			for (int i = 0; i < toolbar.Toolbar.Items.Count; i++)
			{
				if (toolbar.Items[i].FunctionGroup > 0 && BlockUnblockItems.NonDisplay(toolbar.Items[i]))
				{
					((Button)toolbar.Toolbar.Items[i]).IsEnabled = true;
					((Button)toolbar.Toolbar.Items[i]).Opacity = 1.0;
				}
			}
		}
		_mBlockNonDispalay = false;
	}

	private ToolBarDef GetRightMenuToolbar()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Type == CustomToolbarType.RightMenu)
			{
				return toolbar;
			}
		}
		return null;
	}

	private ToolBarDef GetLastToolbar()
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Type == CustomToolbarType.LastCommandTray)
			{
				return toolbar;
			}
		}
		return null;
	}

	private ToolBarDef GetToolbarByName(string name)
	{
		foreach (ToolBarDef toolbar in _toolbars)
		{
			if (toolbar.Name == name)
			{
				return toolbar;
			}
		}
		return null;
	}

	public void AddQatButton(int qatId, IPnCommand command)
	{
		RFileRecord rFileRecord = new RFileRecord();
		rFileRecord.FunctionGroup = command.Group;
		rFileRecord.FunctionName = command.Command;
		rFileRecord.DefaultLabel = command.CurrentLabel;
		rFileRecord.IdLabel = command.LabelId;
		rFileRecord.IdHelp = command.ToolTipId;
		AddQatButton(qatId, rFileRecord);
	}

	private void AddQatButton(int qatId, IRFileRecord rec)
	{
		if (rec == null || rec.FunctionGroup == 2 || rec.FunctionGroup == 25 || rec.FunctionGroup == 4 || qatId == 5)
		{
			return;
		}
		ToolBarDef toolbarByName = GetToolbarByName(GetQatToolbarName(qatId));
		if (toolbarByName != null)
		{
			toolbarByName.Items.Add(rec);
			if (toolbarByName.Visible)
			{
				SetQatToolbar(toolbarByName);
			}
		}
	}

	public void AddLastButton(string statement)
	{
		string[] array = null;
		try
		{
			array = statement.Split(';');
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
		if (array != null && array.Length == 7)
		{
			RFileRecord rFileRecord = new RFileRecord();
			try
			{
				rFileRecord.FunctionGroup = Convert.ToInt32(array[1]);
				rFileRecord.FunctionName = array[2];
				rFileRecord.DefaultLabel = array[3];
				rFileRecord.IdLabel = Convert.ToInt32(array[5]);
				rFileRecord.IdHelp = Convert.ToInt32(array[6]);
			}
			catch (Exception e2)
			{
				_logCenterService.CatchRaport(e2);
				return;
			}
			if (rFileRecord.FunctionGroup != 2 && rFileRecord.FunctionGroup != 25 && rFileRecord.FunctionGroup != 4)
			{
				AddLastButton(rFileRecord);
			}
		}
	}

	public void AddLastButton(IPnCommand cmd)
	{
		if (cmd.AddValue2 != 255)
		{
			AddLastButton(cmd.GetRFileRecord());
		}
	}

	public void AddLastButton(IRFileRecord rec)
	{
		ToolBarDef lastToolbar = GetLastToolbar();
		if (lastToolbar == null)
		{
			return;
		}
		for (int i = 0; i < lastToolbar.Items.Count; i++)
		{
			if (lastToolbar.Items[i].FunctionName == rec.FunctionName)
			{
				lastToolbar.Items.RemoveAt(i);
				break;
			}
		}
		lastToolbar.Items.Insert(0, rec);
		while (lastToolbar.Items.Count > 10)
		{
			lastToolbar.Items.RemoveAt(lastToolbar.Items.Count - 1);
		}
		if (lastToolbar.Toolbar != null && lastToolbar.Visible)
		{
			SetCustomToolbar(lastToolbar);
		}
	}

	public void SetRightMenu(MFileExpert expert)
	{
		if (IsSpecjalRibbontab)
		{
			return;
		}
		ToolBarDef rightMenuToolbar = GetRightMenuToolbar();
		if (rightMenuToolbar == null)
		{
			return;
		}
		rightMenuToolbar.Items.Clear();
		foreach (MFileData line in expert.lines)
		{
			RFileRecord rFileRecord = new RFileRecord();
			if (line.PnGroup == 0)
			{
				rFileRecord.FunctionGroup = -1;
			}
			else
			{
				rFileRecord.FunctionName = line.PnCommand;
				rFileRecord.FunctionGroup = line.PnGroup;
				rFileRecord.IdHelp = line.PnTooltipId;
				rFileRecord.IdLabel = line.PnTextId;
				rFileRecord.DefaultLabel = line.PnText;
				if (line.PnGroup == 2 || line.PnGroup == 4)
				{
					rFileRecord.DrawArrow = true;
				}
			}
			rightMenuToolbar.Items.Add(rFileRecord);
		}
		if (rightMenuToolbar.Toolbar != null && rightMenuToolbar.Visible)
		{
			SetCustomToolbar(rightMenuToolbar);
		}
	}

	public void ChangeFor3D(bool is3D)
	{
		if (!string.IsNullOrEmpty(_moduleName) && is3D != _is3D)
		{
			SaveXmlConfig();
			Reset();
			_is3D = is3D;
			LoadXmlConfig();
			PropagateDatas();
		}
	}

	public void ActivateModule(string moduleName, bool is3D)
	{
		if (_moduleName != string.Empty)
		{
			SaveXmlConfig();
		}
		_moduleName = moduleName;
		Reset();
		_is3D = is3D;
		LoadXmlConfig();
		PropagateDatas();
	}

	public void ChangeLanguage()
	{
		SaveXmlConfig();
		SetLanguageContextMenu();
		Reset();
		LoadXmlConfig();
		PropagateDatas();
	}
}
