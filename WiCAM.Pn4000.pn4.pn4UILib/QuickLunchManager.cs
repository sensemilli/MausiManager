using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using pncommon.WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Screen;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public class QuickLunchManager
{
	private readonly IPnIconsService _pnIconsService;

	private readonly IScreen2D _screen2D;

	private readonly ILanguageDictionary _languageDictionary;

	private readonly ActivateProgramPart _activateProgramPart;

	private readonly ExeFlow _exeFlow;

	private TextBox _quickLaunch;

	private System.Windows.Controls.Primitives.Popup _codePopup;

	private ListBox _list;

	private ScrollViewer _scrollViewer;

	private List<RFileRecord> _itemsList = new List<RFileRecord>();

	private List<RFileRecord> _fullList = new List<RFileRecord>();

	private bool _setorglistready;

	private bool _editMode;

	public QuickLunchManager(IPnIconsService pnIconsService, IScreen2D screen2D, ILanguageDictionary languageDictionary, ActivateProgramPart activateProgramPart, ExeFlow exeFlow)
	{
		_pnIconsService = pnIconsService;
		_screen2D = screen2D;
		_languageDictionary = languageDictionary;
		_activateProgramPart = activateProgramPart;
		_exeFlow = exeFlow;
	}

	internal void ConfigureControl(TextBox quickLaunch)
	{
		if (quickLaunch != null)
		{
			_quickLaunch = quickLaunch;
			quickLaunch.LostFocus += QuickLaunch_LostFocus;
			quickLaunch.GotFocus += QuickLaunch_GotFocus;
			quickLaunch.KeyDown += QuickLaunch_KeyDown;
			quickLaunch.TextChanged += QuickLaunch_TextChanged;
			UnactiveControlStyle();
		}
	}

	internal void MoueWheelToMenu(int delta)
	{
		if (_codePopup != null && _codePopup.IsOpen && _list != null)
		{
			if (_scrollViewer == null)
			{
				_scrollViewer = GetScrollViewer(_list) as ScrollViewer;
			}
			if (_scrollViewer != null)
			{
				_scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset - (double)delta / 40.0);
			}
		}
	}

	public static DependencyObject GetScrollViewer(DependencyObject o)
	{
		if (o is ScrollViewer)
		{
			return o;
		}
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
		{
			DependencyObject scrollViewer = GetScrollViewer(VisualTreeHelper.GetChild(o, i));
			if (scrollViewer != null)
			{
				return scrollViewer;
			}
		}
		return null;
	}

	public bool IsQuickLunchMenuVisible()
	{
		if (_codePopup == null)
		{
			return false;
		}
		return _codePopup.IsOpen;
	}

	private void QuickLaunch_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (!_editMode)
		{
			return;
		}
		if (_codePopup == null)
		{
			_codePopup = new System.Windows.Controls.Primitives.Popup
			{
				PlacementTarget = _quickLaunch,
				StaysOpen = true
			};
			Border border = new Border();
			if (_list == null)
			{
				_list = new ListBox
				{
					Width = 200.0,
					Height = 400.0
				};
				_list.SelectionChanged += List_SelectionChanged;
			}
			border.Child = _list;
			border.BorderThickness = new Thickness(0.0);
			border.BorderBrush = new SolidColorBrush(Colors.Black);
			_codePopup.Child = border;
		}
		_codePopup.IsOpen = _quickLaunch.Text != string.Empty;
		if (_codePopup.IsOpen)
		{
			UpdateListContent();
		}
	}

	private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_list.SelectedIndex >= 0)
		{
			_exeFlow.CallPnCommand(new PnCommand(_itemsList[_list.SelectedIndex]));
		}
	}

	private void UpdateListContent()
	{
		_list.Items.Clear();
		_itemsList.Clear();
		SetOrgList();
		string text = _quickLaunch.Text.Trim().ToUpper();
		foreach (RFileRecord full in _fullList)
		{
			bool flag = true;
			string visualText = full.VisualText;
			if (visualText != null)
			{
				if (text != string.Empty && !visualText.ToUpper().Contains(text))
				{
					flag = false;
				}
				if (flag)
				{
					_list.Items.Add((StackPanel)full.tmp_object);
					_itemsList.Add(full);
				}
			}
		}
	}

	internal void Focus()
	{
		_quickLaunch.Focus();
	}

	public void ResetSettings()
	{
		_setorglistready = false;
		UpdateLanguage();
	}

	private void SetOrgList()
	{
		if (_setorglistready)
		{
			return;
		}
		_setorglistready = true;
		List<RFileRecord> currentRibbonFunctionsList = GetCurrentRibbonFunctionsList();
		_fullList.Clear();
		foreach (RFileRecord item in currentRibbonFunctionsList)
		{
			if (!IsAlredyThere(item))
			{
				StackPanel stackPanel = new StackPanel
				{
					Orientation = Orientation.Horizontal
				};
				Image element = new Image
				{
					Source = item.VisualIcon,
					Height = 16.0
				};
				Label element2 = new Label
				{
					Content = item.VisualText
				};
				stackPanel.Children.Add(element);
				stackPanel.Children.Add(element2);
				item.tmp_object = stackPanel;
				stackPanel = new StackPanel
				{
					Orientation = Orientation.Horizontal
				};
				element = new Image
				{
					Source = item.VisualIcon,
					Height = 16.0
				};
				element2 = new Label
				{
					Content = item.VisualText
				};
				stackPanel.Children.Add(element);
				stackPanel.Children.Add(element2);
				item.tmp_object2 = stackPanel;
				_fullList.Add(item);
			}
		}
	}

	private bool IsAlredyThere(RFileRecord r)
	{
		foreach (RFileRecord full in _fullList)
		{
			if (full.FunctionGroup == r.FunctionGroup && full.FunctionName == r.FunctionName && full.IdHelp == r.IdHelp && full.IdLabel == r.IdLabel)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsForAddInCurrentConfig(int commandGroup)
	{
		switch (commandGroup)
		{
		case 2:
			return false;
		case 4:
			return false;
		case 5:
			return false;
		case 6:
			return false;
		case 8:
			return false;
		case 100:
			return false;
		default:
			if (_activateProgramPart.ModuleName == "2D")
			{
				switch (commandGroup)
				{
				case 30:
					return false;
				case 50:
					return false;
				case 60:
					return false;
				}
			}
			if (_activateProgramPart.ModuleName == "3D")
			{
				return commandGroup switch
				{
					30 => true, 
					50 => true, 
					60 => true, 
					_ => false, 
				};
			}
			return true;
		}
	}

	public List<RFileRecord> GetCurrentRibbonFunctionsList()
	{
		List<RFileRecord> list = new List<RFileRecord>();
		foreach (PipLstEntry item in _languageDictionary.PipLst)
		{
			if (IsForAddInCurrentConfig(item.CommandGroup))
			{
				RFileRecord rFileRecord = new RFileRecord
				{
					FunctionGroup = item.CommandGroup,
					FunctionName = item.CommandName,
					IdLabel = item.Id1,
					IdHelp = item.Id2,
					IconSize = 16,
					Type = RFileType.RibbonButton,
					VisualText = item.CurrentText,
					DefaultLabel = item.DefaultText
				};
				if (rFileRecord.VisualText != null)
				{
					RFileRecord rFileRecord2 = rFileRecord;
					rFileRecord2.VisualText = rFileRecord2.VisualText + " (" + rFileRecord.FunctionGroup + "," + rFileRecord.FunctionName + ")";
				}
				rFileRecord.VisualIcon = _pnIconsService.GetSmallIcon(rFileRecord.FunctionName);
				list.Add(rFileRecord);
			}
		}
		return list;
	}

	private void QuickLaunch_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape || e.Key == Key.Return || e.Key == Key.Return || e.Key == Key.Tab)
		{
			ParseLostFocus();
			e.Handled = true;
		}
	}

	private void ParseLostFocus()
	{
		_screen2D.KeyboardFocus();
	}

	private void QuickLaunch_GotFocus(object sender, RoutedEventArgs e)
	{
		ActiveControlStyle();
	}

	private void QuickLaunch_LostFocus(object sender, RoutedEventArgs e)
	{
		UnactiveControlStyle();
	}

	private void UpdateLanguage()
	{
		_quickLaunch.Text = (string)_quickLaunch.TryFindResource("l_popup.PnInterfaceSettings.QuickLaunch");
	}

	private void UnactiveControlStyle()
	{
		_editMode = false;
		UpdateLanguage();
		_quickLaunch.Foreground = new SolidColorBrush(Colors.Gray);
		_quickLaunch.Background = new SolidColorBrush(Colors.White);
		if (_codePopup != null)
		{
			_codePopup.IsOpen = false;
		}
	}

	private void ActiveControlStyle()
	{
		_editMode = true;
		_quickLaunch.Text = string.Empty;
		_quickLaunch.Foreground = new SolidColorBrush(Colors.Black);
		_quickLaunch.Background = new SolidColorBrush(Colors.LightGreen);
	}
}
