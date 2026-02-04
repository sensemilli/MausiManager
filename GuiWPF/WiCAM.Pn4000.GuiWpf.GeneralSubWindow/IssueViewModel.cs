using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.GeneralSubWindow;

internal class IssueViewModel : ViewModelBase, IIssueViewModel
{
	private readonly IPnCommandsUnfold _commandsUnfold;

	private readonly IPnCommandBasics _commandBasics;

	private Visibility _warningMaterialVisibility;

	private Visibility _warningKFactorVisibility;

	private Visibility _warningUpdateDocVisibility;

	private Visibility _defaultUpdateDocVisibility;

	private IDoc3d _doc;

	private readonly ITranslator _translator;

	private readonly IConfigProvider _configProvider;

	public Visibility WarningMaterialVisibility
	{
		get
		{
			return _warningMaterialVisibility;
		}
		set
		{
			if (_warningMaterialVisibility != value)
			{
				_warningMaterialVisibility = value;
				NotifyPropertyChanged("WarningMaterialVisibility");
			}
		}
	}

	public ICommand CmdWarningMaterial { get; }

	public Visibility WarningKFactorVisibility
	{
		get
		{
			return _warningKFactorVisibility;
		}
		set
		{
			if (_warningKFactorVisibility != value)
			{
				_warningKFactorVisibility = value;
				NotifyPropertyChanged("WarningKFactorVisibility");
			}
		}
	}

	public ICommand CmdWarningKFactor { get; }

	public Visibility WarningUpdateDocVisibility
	{
		get
		{
			return _warningUpdateDocVisibility;
		}
		set
		{
			if (_warningUpdateDocVisibility != value)
			{
				_warningUpdateDocVisibility = value;
				NotifyPropertyChanged("WarningUpdateDocVisibility");
			}
		}
	}

	public Visibility DefaultUpdateDocVisibility
	{
		get
		{
			return _defaultUpdateDocVisibility;
		}
		set
		{
			if (_defaultUpdateDocVisibility != value)
			{
				_defaultUpdateDocVisibility = value;
				NotifyPropertyChanged("DefaultUpdateDocVisibility");
			}
		}
	}

	public ICommand CmdWarningUpdateDoc { get; }

	public string WarningUpdateDocTooltip { get; set; }

	public ICommand CmdWarningToolShortage { get; }

	public Visibility WarningToolShortageVisibility { get; set; }

	public IssueViewModel(IPnCommandsUnfold commandsUnfold, IPnCommandBasics commandBasics, IDoc3d doc, ITranslator translator, IConfigProvider configProvider)
	{
		_commandsUnfold = commandsUnfold;
		_commandBasics = commandBasics;
		CmdWarningMaterial = new RelayCommand(CmdWarningMaterialExecute);
		CmdWarningKFactor = new RelayCommand(CmdWarningKFactorExecute);
		CmdWarningUpdateDoc = new RelayCommand(CmdWarningUpdateDocExecute);
		CmdWarningToolShortage = new RelayCommand(CmdWarningToolShortageExecute);
		_doc = doc;
		_translator = translator;
		_configProvider = configProvider;
		_doc.DocUpdated += Refresh;
		_doc.PnMaterialByUserChanged += Refresh;
		_doc.RefreshSimulation += DocOnRefreshSimulation;
		RefreshVisibilities();
	}

	private void DocOnRefreshSimulation(IPnBndDoc obj)
	{
		RefreshUpdateDocVisibilities();
		RefreshToolOverflow();
	}

	public void Init(IDoc3d doc)
	{
	}

	public void RefreshVisibilities()
	{
		if (_doc.HasModel)
		{
			bool p3D_WarningNoKFactor = _configProvider.InjectOrCreate<General3DConfig>().P3D_WarningNoKFactor;
			WarningMaterialVisibility = (_doc.PnMaterialByUser ? Visibility.Collapsed : Visibility.Visible);
			WarningKFactorVisibility = ((!p3D_WarningNoKFactor || !_doc.KFactorWarningsError) ? Visibility.Collapsed : Visibility.Visible);
		}
		else
		{
			WarningMaterialVisibility = Visibility.Collapsed;
			WarningKFactorVisibility = Visibility.Collapsed;
		}
		RefreshUpdateDocVisibilities();
		RefreshToolOverflow();
	}

	private void RefreshToolOverflow()
	{
		IToolsAndBends toolsAndBends = _doc.ToolsAndBends;
		List<IToolListAvailable> list = _doc.BendMachine?.ToolLists.ToList();
		bool flag = false;
		if (toolsAndBends != null && list != null && list.Count > 0)
		{
			foreach (IToolSetups toolSetup in toolsAndBends.ToolSetups)
			{
				foreach (IGrouping<IAliasPieceProfile, IAliasPieceProfile> item in from x in toolSetup.AllSections.SelectMany((IToolSection s) => s.Pieces.SelectMany((IToolPiece p) => p.PieceProfile.Aliases))
					group x by x)
				{
					IAliasPieceProfile key = item.Key;
					int num = item.Count();
					int num2 = 0;
					foreach (IToolListAvailable item2 in list)
					{
						num2 += item2.PiecesAvailable.GetValueOrDefault(key, 0);
					}
					if (num > num2)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (flag)
		{
			WarningToolShortageVisibility = Visibility.Visible;
		}
		else
		{
			WarningToolShortageVisibility = Visibility.Collapsed;
		}
		NotifyPropertyChanged("WarningToolShortageVisibility");
	}

	private void RefreshUpdateDocVisibilities()
	{
		if (_doc.HasModel)
		{
			if (_doc.IsUpdateDocNeeded())
			{
				WarningUpdateDocVisibility = Visibility.Visible;
				DefaultUpdateDocVisibility = Visibility.Collapsed;
			}
			else
			{
				WarningUpdateDocVisibility = Visibility.Collapsed;
				DefaultUpdateDocVisibility = Visibility.Visible;
			}
			string text = _translator.Translate("l_popup.PopupUnfoldInfo.UpdateDocWarning");
			if (_doc.SafeModeUnfoldErrorBendOrder.Count > 0)
			{
				text = text + Environment.NewLine + _translator.Translate("l_popup.PopupUnfoldInfo.UpdateDocWarningSafeMode", string.Join(", ", _doc.SafeModeUnfoldErrorBendOrder.Select((int x) => x + 1)));
			}
			WarningUpdateDocTooltip = text;
			NotifyPropertyChanged("WarningUpdateDocTooltip");
		}
		else
		{
			WarningUpdateDocVisibility = Visibility.Collapsed;
			DefaultUpdateDocVisibility = Visibility.Collapsed;
		}
	}

	public void Close()
	{
		if (_doc != null)
		{
			_doc.DocUpdated -= Refresh;
			_doc.PnMaterialByUserChanged -= Refresh;
		}
	}

	private void Refresh(IPnBndDoc obj)
	{
		RefreshVisibilities();
	}

	private void CmdWarningMaterialExecute()
	{
		_commandsUnfold.SetMaterial(_commandBasics.CreateCommandArg(_doc, "SetMaterial by UI"));
	}

	private void CmdWarningKFactorExecute()
	{
	}

	private void CmdWarningUpdateDocExecute()
	{
		if (_doc.SafeModeUnfold)
		{
			_doc.SafeModeUnfold = false;
			_doc.SafeModeUnfoldErrorBendOrder.Clear();
		}
		_doc.UpdateDoc();
		_doc.RecalculateSimulation();
	}

	private void CmdWarningToolShortageExecute()
	{
	}
}
