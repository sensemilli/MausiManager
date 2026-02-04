using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts.Popups;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public class ToolSetupMemoryViewModel : ViewModelBase, IPopupViewModel
{
	public enum DialogResults
	{
		Undefined,
		Ok,
		Cancel
	}

	public class MemoryEntry
	{
		public FileInfo FileInfo { get; set; }

		public string Description => FileInfo.Name;

		public string Id { get; set; }

		public DateTime LastModified => FileInfo.LastWriteTime;
	}

	private readonly ITranslator _translator;

	private readonly IPnPathService _pathService;

	private MemoryEntry? _selectedMemoryEntry;

	private string _selectedDescription;

	private bool _saveMode;

	private int _machineNumber;

	public RadObservableCollection<MemoryEntry> MemoryEntries { get; } = new RadObservableCollection<MemoryEntry>();

	public MemoryEntry? SelectedMemoryEntry
	{
		get
		{
			return _selectedMemoryEntry;
		}
		set
		{
			if (_selectedMemoryEntry != value)
			{
				_selectedMemoryEntry = value;
				if (_selectedMemoryEntry != null)
				{
					_selectedDescription = _selectedMemoryEntry.Description;
				}
				RefreshProps();
			}
		}
	}

	public string SelectedDescription
	{
		get
		{
			return _selectedDescription;
		}
		set
		{
			if (!(_selectedDescription != value))
			{
				return;
			}
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			foreach (char value2 in invalidFileNameChars)
			{
				if (value.Contains(value2))
				{
					RefreshProps();
					return;
				}
			}
			_selectedDescription = value;
			if (_selectedMemoryEntry?.Description != _selectedDescription)
			{
				_selectedMemoryEntry = MemoryEntries.FirstOrDefault((MemoryEntry x) => x.Description.ToLower() == _selectedDescription.ToLower().Trim());
			}
			RefreshProps();
		}
	}

	public string OkText
	{
		get
		{
			if (_saveMode)
			{
				if (_selectedMemoryEntry != null)
				{
					return _translator.Translate("l_popup.ToolSetupMemory.Overwrite");
				}
				return _translator.Translate("l_popup.ToolSetupMemory.Add");
			}
			return _translator.Translate("l_popup.ToolSetupMemory.Load");
		}
	}

	public ICommand CmdOk { get; }

	public ICommand CmdCancel { get; }

	public string MachineMemoryDir => Path.Combine(_pathService.GetMachine3DFolder(_machineNumber), "ToolSetupMemory");

	public DialogResults Result { get; private set; }

	public string ResultFilename => Path.Combine(MachineMemoryDir, SelectedDescription.Trim());

	public event Action? CloseView;

	public ToolSetupMemoryViewModel(ITranslator translator, IPnPathService pathService)
	{
		_translator = translator;
		_pathService = pathService;
		CmdOk = new RelayCommand(CmdOkExecute, CmdOkCanExecute);
		CmdCancel = new RelayCommand(CmdCancelExecute);
		MemoryEntries.CollectionChanging += MemoryEntries_CollectionChanging;
	}

	public void Init(bool saveMode, int machineNumber)
	{
		_saveMode = saveMode;
		_machineNumber = machineNumber;
		Result = DialogResults.Undefined;
		MemoryEntries.SuspendNotifications();
		MemoryEntries.Clear();
		string machineMemoryDir = MachineMemoryDir;
		if (Directory.Exists(machineMemoryDir))
		{
			string[] files = Directory.GetFiles(machineMemoryDir);
			foreach (string fileName in files)
			{
				MemoryEntries.Add(new MemoryEntry
				{
					FileInfo = new FileInfo(fileName)
				});
			}
		}
		MemoryEntries.ResumeNotifications();
	}

	private void MemoryEntries_CollectionChanging(object? sender, CollectionChangingEventArgs e)
	{
		if (e.Action == CollectionChangeAction.Remove && e.Item is MemoryEntry memoryEntry)
		{
			memoryEntry.FileInfo.Delete();
		}
	}

	private bool CmdOkCanExecute()
	{
		if (!string.IsNullOrWhiteSpace(_selectedDescription))
		{
			if (!_saveMode)
			{
				return SelectedMemoryEntry != null;
			}
			return true;
		}
		return false;
	}

	private void CmdOkExecute()
	{
		if (CmdOkCanExecute())
		{
			Result = DialogResults.Ok;
			this.CloseView?.Invoke();
		}
	}

	private void CmdCancelExecute()
	{
		Result = DialogResults.Cancel;
		this.CloseView?.Invoke();
	}

	private void RefreshProps()
	{
		NotifyPropertyChanged("SelectedMemoryEntry");
		NotifyPropertyChanged("SelectedDescription");
		NotifyPropertyChanged("OkText");
	}

	public void ViewClosing(object? sender, CancelEventArgs e)
	{
		if (Result == DialogResults.Undefined)
		{
			Result = DialogResults.Cancel;
		}
	}
}
