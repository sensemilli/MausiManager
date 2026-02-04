using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Input;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig;
using Microsoft.Win32;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.BendPP;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class PPSettingsViewModel : ViewModelBase
{
	private IPnPathService _pnPathService;

	private IBendMachine _machine;

	private ICommand _selectOutputPathCommand;

	private ICommand _selectReportOutputPathCommand;

	private ICommand _selectDllPathCommand;

	private ChangedConfigType _changed;

	private PpNumber _ppNumber;

	private string _ppNumberFileName;

	private string _pp;

	private string _ncDirectory;

	private bool _ncDirectoryUserSelect;

	private string _reportDirectory;

	private string _extension;

	private bool _addDateTime;

	private bool _addGeometryToNcFile;

	private bool _openNcFileAfterCreation;

	private string _ppNameFormatSingle;

	private string _ppNameFormatMulti;

	private bool _ppNameAuto;

	public Action<ChangedConfigType> DataChanged;

	private string _ncMachineName;

	private string _ncMachineName2;

	private string _ncMachineType;

	private string _ncMachineType2;

	public IBendMachine Machine
	{
		get
		{
			return _machine;
		}
		set
		{
			_machine = value;
			NotifyPropertyChanged("Machine");
		}
	}

	public string PP
	{
		get
		{
			return _pp;
		}
		set
		{
			if (_pp != value)
			{
				_pp = value;
				NotifyPropertyChanged("PP");
			}
		}
	}

	public string NCDirectory
	{
		get
		{
			return _ncDirectory;
		}
		set
		{
			if (_ncDirectory != value)
			{
				_ncDirectory = value;
				NotifyPropertyChanged("NCDirectory");
			}
		}
	}

	public bool NCDirectoryUserSelect
	{
		get
		{
			return _ncDirectoryUserSelect;
		}
		set
		{
			if (_ncDirectoryUserSelect != value)
			{
				_ncDirectoryUserSelect = value;
				NotifyPropertyChanged("NCDirectoryUserSelect");
			}
		}
	}

	public string ReportDirectory
	{
		get
		{
			return _reportDirectory;
		}
		set
		{
			if (_reportDirectory != value)
			{
				_reportDirectory = value;
				NotifyPropertyChanged("ReportDirectory");
			}
		}
	}

	public string Extension
	{
		get
		{
			return _extension;
		}
		set
		{
			string text = value;
			if (!string.IsNullOrEmpty(text) && !text.StartsWith("."))
			{
				text = "." + text;
			}
			if (_extension != text)
			{
				_extension = text;
				NotifyPropertyChanged("Extension");
			}
		}
	}

	public bool AddDateTime
	{
		get
		{
			return _addDateTime;
		}
		set
		{
			if (_addDateTime != value)
			{
				_addDateTime = value;
				NotifyPropertyChanged("AddDateTime");
			}
		}
	}

	public bool AddGeometryToNcFile
	{
		get
		{
			return _addGeometryToNcFile;
		}
		set
		{
			if (_addGeometryToNcFile != value)
			{
				_addGeometryToNcFile = value;
				NotifyPropertyChanged("AddGeometryToNcFile");
			}
		}
	}

	public bool OpenNcFileAfterCreation
	{
		get
		{
			return _openNcFileAfterCreation;
		}
		set
		{
			if (_openNcFileAfterCreation != value)
			{
				_openNcFileAfterCreation = value;
				NotifyPropertyChanged("OpenNcFileAfterCreation");
			}
		}
	}

	public string PpNameFormatSingle
	{
		get
		{
			return _ppNameFormatSingle;
		}
		set
		{
			if (_ppNameFormatSingle != value)
			{
				_ppNameFormatSingle = value;
				NotifyPropertyChanged("PpNameFormatSingle");
			}
		}
	}

	public string PpNameFormatMulti
	{
		get
		{
			return _ppNameFormatMulti;
		}
		set
		{
			if (_ppNameFormatMulti != value)
			{
				_ppNameFormatMulti = value;
				NotifyPropertyChanged("PpNameFormatMulti");
			}
		}
	}

	public bool PpNameAuto
	{
		get
		{
			return _ppNameAuto;
		}
		set
		{
			if (_ppNameAuto != value)
			{
				_ppNameAuto = value;
				NotifyPropertyChanged("PpNameAuto");
			}
		}
	}

	public ICommand SelectOutputPathCommand => _selectOutputPathCommand ?? (_selectOutputPathCommand = new RelayCommand(SelectOutputPath));

	public ICommand SelectReportOutputPathCommand => _selectReportOutputPathCommand ?? (_selectReportOutputPathCommand = new RelayCommand(SelectReportOutputPath));

	public ICommand SelectDllPathCommand => _selectDllPathCommand ?? (_selectDllPathCommand = new RelayCommand(SelectDllPath));

	public string NcNameFormat
	{
		get
		{
			return _ppNameFormatSingle;
		}
		set
		{
			if (_ppNameFormatSingle != value)
			{
				_ppNameFormatSingle = value;
				NotifyPropertyChanged("NcNameFormat");
				NotifyPropertyChanged("NcNameFormatExample");
				NotifyPropertyChanged("NcNameFormatExampleMulti");
			}
		}
	}

	public string NcNameFormatExample
	{
		get
		{
			try
			{
				string arg = "Test";
				string text = PpNameFormatSingle;
				if (string.IsNullOrEmpty(text))
				{
					text = PpNameFormatMulti;
				}
				return string.Format(text, 1, arg) + Environment.NewLine + string.Format(text, 102, arg) + Environment.NewLine + string.Format(text, 103, arg) + Environment.NewLine + string.Format(text, 1024, arg) + Environment.NewLine;
			}
			catch (Exception)
			{
			}
			return "invalid Format";
		}
	}

	public string NcNameFormatMulti
	{
		get
		{
			return _ppNameFormatMulti;
		}
		set
		{
			if (_ppNameFormatMulti != value)
			{
				_ppNameFormatMulti = value;
				NotifyPropertyChanged("NcNameFormatMulti");
				NotifyPropertyChanged("NcNameFormatExampleMulti");
				NotifyPropertyChanged("NcNameFormatExample");
			}
		}
	}

	public string NcNameFormatExampleMulti
	{
		get
		{
			try
			{
				string arg = "Test";
				int num = 2;
				string text = PpNameFormatMulti;
				if (string.IsNullOrEmpty(text))
				{
					text = PpNameFormatSingle;
				}
				return string.Format(text, 1, arg, num) + Environment.NewLine + string.Format(text, 102, arg, num) + Environment.NewLine + string.Format(text, 103, arg, num) + Environment.NewLine + string.Format(text, 1024, arg, num) + Environment.NewLine;
			}
			catch (Exception)
			{
			}
			return "invalid Format";
		}
	}

	public int NcNameNextNo
	{
		get
		{
			return _ppNumber.NextNo;
		}
		set
		{
			if (_ppNumber.NextNo != value)
			{
				_ppNumber.NextNo = value;
				NotifyPropertyChanged("NcNameNextNo");
			}
		}
	}

	public string NcMachineName
	{
		get
		{
			return _ncMachineName;
		}
		set
		{
			if (_ncMachineName != value)
			{
				_ncMachineName = value;
				NotifyPropertyChanged("NcMachineName");
			}
		}
	}

	public string NcMachineName2
	{
		get
		{
			return _ncMachineName2;
		}
		set
		{
			if (_ncMachineName2 != value)
			{
				_ncMachineName2 = value;
				NotifyPropertyChanged("NcMachineName2");
			}
		}
	}

	public string NcMachineType
	{
		get
		{
			return _ncMachineType;
		}
		set
		{
			if (_ncMachineType != value)
			{
				_ncMachineType = value;
				NotifyPropertyChanged("NcMachineType");
			}
		}
	}

	public string NcMachineType2
	{
		get
		{
			return _ncMachineType2;
		}
		set
		{
			if (_ncMachineType2 != value)
			{
				_ncMachineType2 = value;
				NotifyPropertyChanged("NcMachineType2");
			}
		}
	}

	public IPostProcessorSettings SpecificSettings { get; set; }

	public PPSettingsViewModel(IPnPathService pathService)
	{
		_pnPathService = pathService;
	}

	public PPSettingsViewModel Init(IBendMachine machine)
	{
		Machine = machine;
		NCDirectory = Machine.PressBrakeData.NCDirectory;
		NCDirectoryUserSelect = Machine.PressBrakeData.NCDirectoryUserSelect;
		ReportDirectory = Machine.PressBrakeData.ReportDirectory;
		PP = Machine.PressBrakeInfo.PP;
		Extension = Machine.PressBrakeData.OutputNCFileExtension;
		AddDateTime = Convert.ToBoolean(Machine.PressBrakeData.AddDateTimePPName);
		AddGeometryToNcFile = Convert.ToBoolean(Machine.PressBrakeData.AddGeometryToNcFile);
		OpenNcFileAfterCreation = Convert.ToBoolean(Machine.PressBrakeData.OpenNcFileAfterCreation);
		PpNameFormatSingle = Machine.PressBrakeData.PpNameFormatSingle;
		PpNameFormatMulti = Machine.PressBrakeData.PpNameFormatMulti;
		PpNameAuto = Machine.PressBrakeData.PpNameAuto;
		NcMachineName = _machine.NCName;
		NcMachineName2 = _machine.NCName2;
		NcMachineType = _machine.NCMachineType;
		NcMachineType2 = _machine.NCMachineType2;
		_ppNumberFileName = Path.Combine(machine.MachinePath, "programNo.json");
		if (File.Exists(_ppNumberFileName))
		{
			string json = File.ReadAllText(_ppNumberFileName);
			_ppNumber = JsonSerializer.Deserialize<PpNumber>(json);
		}
		else
		{
			_ppNumber = new PpNumber();
		}
		LoadSpecificSettings();
		return this;
	}

	private void SelectOutputPath(object param)
	{
		string selectedPath = (string.IsNullOrEmpty(_pnPathService.PNMASTER) ? _pnPathService.PNDRIVE : _pnPathService.PNMASTER);
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
		{
			SelectedPath = selectedPath,
			Description = "Select output folder for NC-Files."
		};
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			NCDirectory = folderBrowserDialog.SelectedPath + "\\";
		}
	}

	private void SelectReportOutputPath(object param)
	{
		string selectedPath = (string.IsNullOrEmpty(_pnPathService.PNMASTER) ? _pnPathService.PNDRIVE : _pnPathService.PNMASTER);
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
		{
			SelectedPath = selectedPath,
			Description = "Select output folder for Report Files."
		};
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			ReportDirectory = folderBrowserDialog.SelectedPath + "\\";
		}
	}

	private void SelectDllPath(object param)
	{
		string initialDirectory = (string.IsNullOrEmpty(_pnPathService.PNMASTER) ? _pnPathService.PNDRIVE : _pnPathService.PNMASTER) + "\\u\\pn\\pprun_bend";
		Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
		{
			InitialDirectory = initialDirectory
		};
		bool? flag = openFileDialog.ShowDialog();
		if (flag.HasValue && flag.Value)
		{
			PP = openFileDialog.SafeFileName;
			_changed = ChangedConfigType.PostProcessor;
		}
	}

	public void Save()
	{
		Machine.PressBrakeData.NCDirectory = NCDirectory;
		Machine.PressBrakeData.NCDirectoryUserSelect = NCDirectoryUserSelect;
		Machine.PressBrakeData.ReportDirectory = ReportDirectory;
		Machine.PressBrakeInfo.PP = PP;
		Machine.PressBrakeData.OutputNCFileExtension = Extension;
		Machine.PressBrakeData.AddDateTimePPName = Convert.ToInt32(AddDateTime);
		Machine.PressBrakeData.AddGeometryToNcFile = Convert.ToInt32(AddGeometryToNcFile);
		Machine.PressBrakeData.OpenNcFileAfterCreation = Convert.ToInt32(OpenNcFileAfterCreation);
		Machine.PressBrakeData.PpNameFormatSingle = PpNameFormatSingle;
		Machine.PressBrakeData.PpNameFormatMulti = PpNameFormatMulti;
		Machine.PressBrakeData.PpNameAuto = PpNameAuto;
		Machine.NCName = NcMachineName;
		Machine.NCName2 = NcMachineName2;
		Machine.NCMachineType = NcMachineType;
		Machine.NCMachineType2 = NcMachineType2;
		File.WriteAllText(_ppNumberFileName, JsonSerializer.Serialize(_ppNumber));
		DataChanged?.Invoke(_changed);
		SaveSpecificSettings();
	}

	public void Dispose()
	{
	}

	private void LoadSpecificSettings()
	{
		SpecificSettings = Machine.PostProcessor?.GetSettings(Machine.MachinePath);
	}

	private void SaveSpecificSettings()
	{
		Machine?.PostProcessor?.SetAndSaveSettings(SpecificSettings, Machine.MachinePath);
	}
}
