using System.Collections.ObjectModel;
using System.Linq;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

public class ToolGeneralViewModel : ViewModelBase, IAddTab, IDeleteTab, ICopyTab
{
	private readonly IGlobalToolStorage _toolStorage;

	private ObservableCollection<ToolListViewModel> _selectedToolLists;

	private bool _isDeleteButtonEnabled;

	public MachineToolsViewModel MachineTools { get; set; }

	public ObservableCollection<ToolListViewModel> SelectedToolLists
	{
		get
		{
			return _selectedToolLists;
		}
		internal set
		{
			_selectedToolLists = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("SelectedToolLists");
		}
	}

	public bool IsAddButtonEnabled => true;

	public bool IsDeleteButtonEnabled
	{
		get
		{
			return _isDeleteButtonEnabled;
		}
		set
		{
			if (_isDeleteButtonEnabled != value)
			{
				_isDeleteButtonEnabled = value;
				NotifyPropertyChanged("IsDeleteButtonEnabled");
			}
		}
	}

	public bool IsCopyButtonEnabled
	{
		get
		{
			ObservableCollection<ToolListViewModel> selectedToolLists = SelectedToolLists;
			if (selectedToolLists == null)
			{
				return false;
			}
			return selectedToolLists.Count == 1;
		}
	}

	public ToolGeneralViewModel(IGlobalToolStorage toolStorage)
	{
		_toolStorage = toolStorage;
	}

	public ToolGeneralViewModel Init(MachineToolsViewModel machineTools)
	{
		MachineTools = machineTools;
		return this;
	}

	public void AddButtonClick()
	{
		ToolListViewModel toolListViewModel = new ToolListViewModel(_toolStorage);
		toolListViewModel.PropertyChanged += MachineTools.ToolList_PropertyChanged;
		MachineTools.AllToolLists.Add(toolListViewModel);
	}

	public void CopyButtonClick()
	{
		if (SelectedToolLists.Count == 1)
		{
			ToolListViewModel copy = SelectedToolLists.First();
			ToolListViewModel toolListViewModel = new ToolListViewModel(_toolStorage, copy);
			toolListViewModel.PropertyChanged += MachineTools.ToolList_PropertyChanged;
			MachineTools.AllToolLists.Add(toolListViewModel);
		}
	}

	public void DeleteButtonClick()
	{
		foreach (ToolListViewModel selectedToolList in SelectedToolLists)
		{
			MachineTools.DeleteToolList(selectedToolList);
		}
	}

	public void SetEditorEnableRules()
	{
		ObservableCollection<ToolListViewModel> selectedToolLists = SelectedToolLists;
		IsDeleteButtonEnabled = selectedToolLists != null && selectedToolLists.Count > 0;
	}
}
