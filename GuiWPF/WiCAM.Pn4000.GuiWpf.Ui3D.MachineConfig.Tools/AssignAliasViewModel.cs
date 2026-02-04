using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.pn4.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public class AssignAliasViewModel : ToolViewModelBase
{
	public Action<ChangedConfigType> DataChanged;

	private ToolViewModel? _currentToolProfile;

	private ToolPieceViewModel? _currentToolPiece;

	private ToolViewModel? _selectedToolProfile;

	private ToolPieceViewModel? _selectedToolPiece;

	private bool _internalSelection;

	private MultiToolViewModel _selectedMultiTool;

	public new IBendMachine BendMachine { get; private set; }

	public MachineToolsViewModel MachineToolsViewModel { get; private set; }

	public ToolConfigModel ToolModel { get; private set; }

	public IDoc3d Doc { get; private set; }

	public ObservableCollection<ToolPieceViewModel> AllToolPieces { get; set; }

	public ObservableCollection<ToolViewModel> AllToolProfiles { get; set; }

	public List<ToolPieceViewModel> InterestingToolPieces { get; set; }

	public List<ToolViewModel> InterestingToolProfiles { get; set; }

	public HashSet<ToolPieceViewModel> PinnedToolPieces { get; set; } = new HashSet<ToolPieceViewModel>();

	public HashSet<ToolViewModel> PinnedToolProfiles { get; set; } = new HashSet<ToolViewModel>();

	private MultiToolViewModel? _currentMultiTool
	{
		get
		{
			object obj = _currentToolProfile?.MultiTool;
			if (obj == null)
			{
				ToolPieceViewModel? currentToolPiece = _currentToolPiece;
				if (currentToolPiece == null)
				{
					return null;
				}
				obj = currentToolPiece.MultiTool;
			}
			return (MultiToolViewModel?)obj;
		}
	}

	public ToolViewModel? SelectedToolProfileByUser
	{
		get
		{
			return _selectedToolProfile;
		}
		set
		{
			if (_selectedToolProfile == value || _internalSelection)
			{
				return;
			}
			_internalSelection = true;
			_selectedToolProfile = value;
			if (_selectedToolProfile?.MultiTool != null)
			{
				SelectedMultiTool = _selectedToolProfile.MultiTool;
				if (SelectedToolPieceByUser != null)
				{
					_selectedToolPiece = null;
					NotifyPropertyChanged("SelectedToolPieceByUser");
				}
			}
			NotifyPropertyChanged("SelectedToolProfileByUser");
			_internalSelection = false;
		}
	}

	public ToolPieceViewModel? SelectedToolPieceByUser
	{
		get
		{
			return _selectedToolPiece;
		}
		set
		{
			if (_selectedToolPiece == value || _internalSelection)
			{
				return;
			}
			_internalSelection = true;
			_selectedToolPiece = value;
			if (_selectedToolPiece?.MultiTool != null)
			{
				SelectedMultiTool = _selectedToolPiece.MultiTool;
				if (SelectedToolProfileByUser != null)
				{
					_selectedToolProfile = null;
					NotifyPropertyChanged("SelectedToolProfileByUser");
				}
			}
			NotifyPropertyChanged("SelectedToolPieceByUser");
			_internalSelection = false;
		}
	}

	public MultiToolViewModel SelectedMultiTool
	{
		get
		{
			return _selectedMultiTool;
		}
		set
		{
			if (_selectedMultiTool != value)
			{
				_selectedMultiTool = value;
				NotifyPropertyChanged("SelectedMultiTool");
			}
		}
	}

	public ToolViewModel? SelectedInterestingToolProfileByUser { get; set; }

	public ToolPieceViewModel? SelectedInterestingToolPieceByUser { get; set; }

	public ICommand CmdSelectCurrentItem { get; }

	public ICommand CmdAssignMultiToolCurrentItem { get; }

	public ICommand CmdAssignAliasToolCurrentItem { get; }

	public ICommand CmdNewMultiToolCurrentItem { get; }

	public ICommand CmdNewAliasToolCurrentItem { get; }

	public AssignAliasViewModel(IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, IPnPathService pnPathService, IModelFactory modelFactory)
		: base(globals, mainWindowDataProvider, pnPathService, modelFactory)
	{
		CmdSelectCurrentItem = new RelayCommand(SelectCurrentItem);
		CmdAssignMultiToolCurrentItem = new RelayCommand(AssignMultiToolCurrentItem);
		CmdAssignAliasToolCurrentItem = new RelayCommand(AssignAliasToolCurrentItem);
		CmdNewMultiToolCurrentItem = new RelayCommand(NewMultiToolCurrentItem);
		CmdNewAliasToolCurrentItem = new RelayCommand(NewAliasToolCurrentItem);
	}

	public void Init(IBendMachine bendMachine, MachineToolsViewModel machineToolsViewModel, ToolConfigModel toolModel, IDoc3d doc)
	{
		BendMachine = bendMachine;
		MachineToolsViewModel = machineToolsViewModel;
		ToolModel = toolModel;
		Doc = doc;
		AllToolProfiles = MachineToolsViewModel.UpperTools.Cast<ToolViewModel>().Concat(MachineToolsViewModel.LowerTools).Concat(MachineToolsViewModel.UpperAdapters)
			.Concat(MachineToolsViewModel.LowerAdapters)
			.ToObservableCollection();
		AllToolPieces = MachineToolsViewModel.UpperPieces.Cast<ToolPieceViewModel>().Concat(MachineToolsViewModel.LowerPieces).Concat(MachineToolsViewModel.LowerToolExtentionPieces)
			.ToObservableCollection();
		SelectCurrentItem();
	}

	private void SelectCurrentItem()
	{
		_currentToolProfile = _selectedToolProfile;
		_currentToolPiece = _selectedToolPiece;
		RefreshInterestingLists();
	}

	private void AssignMultiToolCurrentItem()
	{
		if (_currentMultiTool != null && SelectedToolProfileByUser != null)
		{
			SelectedToolProfileByUser.MultiTool = _currentMultiTool;
		}
		RefreshUiLists();
	}

	private void AssignAliasToolCurrentItem()
	{
		if (_currentMultiTool != null && SelectedToolProfileByUser != null)
		{
			SelectedToolProfileByUser.MultiTool.AliasMultiToolViewModel = _currentMultiTool.AliasMultiToolViewModel;
		}
		else if (_currentToolPiece != null && SelectedToolPieceByUser != null)
		{
			SelectedToolPieceByUser.SetAliasPieces(_currentToolPiece.AliasPieces);
		}
		RefreshUiLists();
	}

	private void NewMultiToolCurrentItem()
	{
		if (SelectedToolProfileByUser != null)
		{
			SelectedToolProfileByUser.MultiTool = MachineToolsViewModel.CreateMultiTool("");
		}
		RefreshUiLists();
	}

	private void NewAliasToolCurrentItem()
	{
		if (SelectedToolProfileByUser != null)
		{
			SelectedToolProfileByUser.MultiTool.AliasMultiToolViewModel = MachineToolsViewModel.CreateAliasMultiTool();
		}
		else if (SelectedToolPieceByUser != null)
		{
			SelectedToolPieceByUser.SetAliasPieces(MachineToolsViewModel.CreateAliasPiece().ToIEnumerable());
		}
		RefreshUiLists();
	}

	private void RefreshUiLists()
	{
		RefreshInterestingLists();
	}

	private void RefreshInterestingLists()
	{
		_internalSelection = true;
		InterestingToolPieces = new List<ToolPieceViewModel>();
		InterestingToolProfiles = new List<ToolViewModel>();
		if (_currentMultiTool != null)
		{
			AliasMultiToolViewModel aliasMultiToolViewModel = _currentMultiTool.AliasMultiToolViewModel;
			foreach (ToolViewModel allToolProfile in AllToolProfiles)
			{
				if (allToolProfile.MultiTool.AliasMultiToolViewModel == aliasMultiToolViewModel || PinnedToolProfiles.Contains(allToolProfile))
				{
					InterestingToolProfiles.Add(allToolProfile);
				}
			}
			HashSet<AliasPieceViewModel> hashSet = _currentToolPiece?.AliasPieces.ToHashSet();
			foreach (ToolPieceViewModel allToolPiece in AllToolPieces)
			{
				if (PinnedToolPieces.Contains(allToolPiece) || (hashSet?.ContainsAny(allToolPiece.AliasPieces) ?? (allToolPiece.MultiTool.AliasMultiToolViewModel == aliasMultiToolViewModel)))
				{
					InterestingToolPieces.Add(allToolPiece);
				}
			}
		}
		NotifyPropertyChanged("InterestingToolPieces");
		NotifyPropertyChanged("InterestingToolProfiles");
		_internalSelection = false;
	}
}
