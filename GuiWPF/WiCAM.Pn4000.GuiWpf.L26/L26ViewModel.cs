using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.L26;

public class L26ViewModel : WiCAM.Pn4000.Common.ViewModelBase
{
	private readonly L26Config _config;

	private readonly IConfigProvider _configProvider;

	private readonly ITranslator _translator;

	private string _workspaceName;

	private string _importXmlFolderPath;

	private string _exportXmlFolderPath;

	private string _exportGeoFolderPath;

	private string _importLstFolderPath;

	private bool _useJobForLstPath;

	private bool _useSpecificLstPath;

	private bool _useSamePathForGeos;

	private bool _useSamePathForGeosVisibility;

	private bool _useSameMaterials;

	private bool _useSameMaterialsVisibility;

	private string _selectedRemark;

	private int _machineNumber;

	private bool _useJobForLstPathEnabled;

	private bool _specifyLstPath;

	private bool _useMappingTable;

	private string _mappingTablePath;

	private bool _useMappingTableVisibility { get; set; }

	public ICommand OpenFileImportExplorer { get; }

	public ICommand OpenFileExportExplorer { get; }

	public ICommand OpenFileGeoExplorer { get; }

	public ICommand OpenMappingTablePathExplorer { get; }

	public ICommand Save { get; }

	public ICommand Cancel { get; }

	public List<string> Remarks => new List<string> { "1", "2", "3", "4", "5" };

	public string SelectedRemark
	{
		get
		{
			return _selectedRemark;
		}
		set
		{
			if (!(_selectedRemark == value))
			{
				_selectedRemark = value;
				NotifyPropertyChanged("SelectedRemark");
			}
		}
	}

	public string WorkspaceName
	{
		get
		{
			return _workspaceName;
		}
		set
		{
			if (!(_workspaceName == value))
			{
				_workspaceName = value;
				NotifyPropertyChanged("WorkspaceName");
			}
		}
	}

	public int MachineNumber
	{
		get
		{
			return _machineNumber;
		}
		set
		{
			if (_machineNumber != value)
			{
				_machineNumber = value;
				NotifyPropertyChanged("MachineNumber");
			}
		}
	}

	public string ImportXmlFolderPath
	{
		get
		{
			return _importXmlFolderPath;
		}
		set
		{
			if (!(_importXmlFolderPath == value))
			{
				_importXmlFolderPath = value;
				NotifyPropertyChanged("ImportXmlFolderPath");
			}
		}
	}

	public string ImportLstFolderPath
	{
		get
		{
			return _importLstFolderPath;
		}
		set
		{
			if (!(_importLstFolderPath == value))
			{
				_importLstFolderPath = value;
				NotifyPropertyChanged("ImportLstFolderPath");
			}
		}
	}

	public bool UseJobForLstPath
	{
		get
		{
			return _useJobForLstPath;
		}
		set
		{
			if (_useJobForLstPath != value)
			{
				_useJobForLstPath = value;
				UseJobForLstPathEnabled = !value;
				NotifyPropertyChanged("UseJobForLstPath");
			}
		}
	}

	public bool UseJobForLstPathEnabled
	{
		get
		{
			return _useJobForLstPathEnabled;
		}
		set
		{
			_useJobForLstPathEnabled = value;
			NotifyPropertyChanged("UseJobForLstPathEnabled");
		}
	}

	public bool SpecifyLstPath
	{
		get
		{
			return _specifyLstPath;
		}
		set
		{
			_specifyLstPath = value;
			NotifyPropertyChanged("SpecifyLstPath");
		}
	}

	public bool UseSpecificLstPath
	{
		get
		{
			return _useSpecificLstPath;
		}
		set
		{
			if (_useSpecificLstPath != value)
			{
				_useSpecificLstPath = value;
				SpecifyLstPath = value;
				NotifyPropertyChanged("UseSpecificLstPath");
			}
		}
	}

	public string ExportXmlFolderPath
	{
		get
		{
			return _exportXmlFolderPath;
		}
		set
		{
			if (!(_exportXmlFolderPath == value))
			{
				_exportXmlFolderPath = value;
				NotifyPropertyChanged("ExportXmlFolderPath");
			}
		}
	}

	public string ExportGeoFolderPath
	{
		get
		{
			return _exportGeoFolderPath;
		}
		set
		{
			if (!(_exportGeoFolderPath == value))
			{
				_exportGeoFolderPath = value;
				NotifyPropertyChanged("ExportGeoFolderPath");
			}
		}
	}

	public string MappingTablePath
	{
		get
		{
			return _mappingTablePath;
		}
		set
		{
			if (!(_mappingTablePath == value))
			{
				_mappingTablePath = value;
				NotifyPropertyChanged("MappingTablePath");
			}
		}
	}

	public bool UseSamePathForGeos
	{
		get
		{
			return _useSamePathForGeos;
		}
		set
		{
			if (_useSamePathForGeos != value)
			{
				_useSamePathForGeos = value;
				UseSamePathForGeosVisibility = !value;
				NotifyPropertyChanged("UseSamePathForGeos");
			}
		}
	}

	public bool UseSamePathForGeosVisibility
	{
		get
		{
			return _useSamePathForGeosVisibility;
		}
		set
		{
			if (_useSamePathForGeosVisibility != value)
			{
				_useSamePathForGeosVisibility = value;
				NotifyPropertyChanged("UseSamePathForGeosVisibility");
			}
		}
	}

	public bool UseSameMaterials
	{
		get
		{
			return _useSameMaterials;
		}
		set
		{
			if (_useSameMaterials != value)
			{
				_useSameMaterials = value;
				UseSameMaterialsVisibility = !value;
				NotifyPropertyChanged("UseSameMaterials");
			}
		}
	}

	public bool UseMappingTable
	{
		get
		{
			return _useMappingTable;
		}
		set
		{
			if (_useMappingTable != value)
			{
				_useMappingTable = value;
				UseMappingTableVisibility = value;
				NotifyPropertyChanged("UseMappingTable");
			}
		}
	}

	public bool UseMappingTableVisibility
	{
		get
		{
			return _useMappingTableVisibility;
		}
		set
		{
			if (_useMappingTableVisibility != value)
			{
				_useMappingTableVisibility = value;
				NotifyPropertyChanged("UseMappingTableVisibility");
			}
		}
	}

	public bool UseSameMaterialsVisibility
	{
		get
		{
			return _useSameMaterialsVisibility;
		}
		set
		{
			if (_useSameMaterialsVisibility != value)
			{
				_useSameMaterialsVisibility = value;
				NotifyPropertyChanged("UseSameMaterialsVisibility");
			}
		}
	}

	public L26ViewModel(ITranslator translator, IConfigProvider configProvider)
	{
		_translator = translator;
		_configProvider = configProvider;
		_config = _configProvider.InjectOrCreate<L26Config>();
		WorkspaceName = _config.WorkspaceName;
		MachineNumber = _config.MachineNumber;
		ImportXmlFolderPath = _config.ImportXmlFolderPath;
		ImportLstFolderPath = _config.ImportLstFolderPath;
		UseJobForLstPath = _config.UseJobForLstPath;
		UseSpecificLstPath = _config.UseSpecificLstPath;
		ExportXmlFolderPath = _config.ExportXmlFolderPath;
		ExportGeoFolderPath = _config.ExportGeoFolderPath;
		UseSamePathForGeos = _config.UseSamePathForGeos;
		UseSameMaterials = _config.UseSameMaterials;
		UseMappingTable = _config.UseMappingTable;
		MappingTablePath = _config.MappingTablePath;
		SelectedRemark = _config.MaterialRemark;
		OpenFileImportExplorer = new WiCAM.Pn4000.Common.RelayCommand(FileImportExplorer);
		OpenFileExportExplorer = new WiCAM.Pn4000.Common.RelayCommand(FileExportExplorer);
		OpenFileGeoExplorer = new WiCAM.Pn4000.Common.RelayCommand(FileGeoExplorer);
		OpenMappingTablePathExplorer = new WiCAM.Pn4000.Common.RelayCommand(MappingTableExplorer);
		Save = new WiCAM.Pn4000.ScreenD3D.Controls.Base.RelayCommand<Window>(SaveConfiguration);
		Cancel = new WiCAM.Pn4000.ScreenD3D.Controls.Base.RelayCommand<Window>(CloseWindow);
	}

	private void FileImportExplorer()
	{
		RadOpenFolderDialog radOpenFolderDialog = new RadOpenFolderDialog
		{
			InitialDirectory = _config.ImportXmlFolderPath,
			Header = _translator.Translate("l_popup.TrumpfL26.SelectFolder"),
			Multiselect = false
		};
		radOpenFolderDialog.ShowDialog();
		if (!((!radOpenFolderDialog.DialogResult) ?? true) || 1 == 0)
		{
			ImportXmlFolderPath = radOpenFolderDialog.FileName;
		}
	}

	private void FileExportExplorer()
	{
		RadOpenFolderDialog radOpenFolderDialog = new RadOpenFolderDialog
		{
			InitialDirectory = _config.ExportXmlFolderPath,
			Header = _translator.Translate("l_popup.TrumpfL26.SelectFolder"),
			Multiselect = false
		};
		radOpenFolderDialog.ShowDialog();
		if (!((!radOpenFolderDialog.DialogResult) ?? true) || 1 == 0)
		{
			ExportXmlFolderPath = radOpenFolderDialog.FileName;
		}
	}

	private void FileGeoExplorer()
	{
		RadOpenFolderDialog radOpenFolderDialog = new RadOpenFolderDialog
		{
			InitialDirectory = _config.ExportGeoFolderPath,
			Header = _translator.Translate("l_popup.TrumpfL26.SelectFolder"),
			Multiselect = false
		};
		radOpenFolderDialog.ShowDialog();
		if (!((!radOpenFolderDialog.DialogResult) ?? true) || 1 == 0)
		{
			ExportGeoFolderPath = radOpenFolderDialog.FileName;
		}
	}

	private void MappingTableExplorer()
	{
		RadOpenFileDialog radOpenFileDialog = new RadOpenFileDialog
		{
			InitialDirectory = _config.MappingTablePath,
			Header = _translator.Translate("l_popup.TrumpfL26.SelectFolder"),
			Multiselect = false
		};
		radOpenFileDialog.ShowDialog();
		if (!((!radOpenFileDialog.DialogResult) ?? true) || 1 == 0)
		{
			MappingTablePath = radOpenFileDialog.FileName;
		}
	}

	private void SaveConfiguration(Window window)
	{
		_config.WorkspaceName = WorkspaceName;
		_config.MachineNumber = MachineNumber;
		_config.ImportXmlFolderPath = ImportXmlFolderPath;
		_config.ImportLstFolderPath = ImportLstFolderPath;
		_config.UseJobForLstPath = UseJobForLstPath;
		_config.UseSpecificLstPath = UseSpecificLstPath;
		_config.ExportXmlFolderPath = ExportXmlFolderPath;
		_config.ExportGeoFolderPath = ExportGeoFolderPath;
		_config.UseSamePathForGeos = UseSamePathForGeos;
		_config.UseMappingTable = UseMappingTable;
		_config.MappingTablePath = MappingTablePath;
		_config.UseSameMaterials = UseSameMaterials;
		_config.MaterialRemark = SelectedRemark;
		_config.DirectExport = true;
		_configProvider.Push(_config);
		_configProvider.Save<L26Config>();
		window?.Close();
	}

	private static void CloseWindow(Window window)
	{
		window?.Close();
	}
}
