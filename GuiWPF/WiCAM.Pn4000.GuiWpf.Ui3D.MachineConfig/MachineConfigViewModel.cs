using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.BystronicConfig.Machine;
using Microsoft.Win32;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.MachineAndTools.Serialization.Serializables;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class MachineConfigViewModel : ViewModelBase
{
	private IBendMachine _bendMachine;

	private ICommand _importBystronicCommand;

	private ICommand _convertToC3mo;

	private ChangedConfigType _changed;

	private Action<ChangedConfigType> _dataChanged;

	private Action _refresh;

	private int _machineNumber;

	private string _machineFilePath;

	private string _machinePath;

	private string _remark1;

	private string _remark2;

	public MeasurementSettingsViewModel MeasurementSettingsViewModel { get; set; }

	public PressDataViewModel PressDataViewModel { get; set; }

	public GeometryDataViewModel GeometryDataViewModel { get; set; }

	public ICommand ImportBystronicClick => _importBystronicCommand ?? (_importBystronicCommand = new RelayCommand(ImportBystronic));

	public ICommand ConvertToC3moCommand => _convertToC3mo ?? (_convertToC3mo = new RelayCommand(ConvertToC3mo));

	public Action<ChangedConfigType> DataChanged
	{
		get
		{
			return _dataChanged;
		}
		set
		{
			_dataChanged = value;
			MeasurementSettingsViewModel.DataChanged = value;
			PressDataViewModel.DataChanged = value;
			GeometryDataViewModel.DataChanged = value;
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
		}
	}

	public string MachinePath
	{
		get
		{
			return _machinePath;
		}
		set
		{
		}
	}

	public string MachineFilePath
	{
		get
		{
			return _machineFilePath;
		}
		set
		{
		}
	}

	public string Remark1
	{
		get
		{
			return _remark1;
		}
		set
		{
			if (_remark1 != value)
			{
				_remark1 = value;
				_changed |= ChangedConfigType.MachineConfig;
			}
		}
	}

	public string Remark2
	{
		get
		{
			return _remark2;
		}
		set
		{
			if (_remark2 != value)
			{
				_remark2 = value;
				_changed |= ChangedConfigType.MachineConfig;
			}
		}
	}

	public MachineConfigViewModel(ITranslator translator)
	{
		MeasurementSettingsViewModel = new MeasurementSettingsViewModel();
		PressDataViewModel = new PressDataViewModel(translator);
		GeometryDataViewModel = new GeometryDataViewModel();
	}

	public void Init(IBendMachine bendMachine, Action refreshViewModels)
	{
		_machinePath = bendMachine.MachinePath;
		_machineNumber = bendMachine.MachineNo;
		_machineFilePath = SBendMachine.MachineFilePath;
		_refresh = refreshViewModels;
		_bendMachine = bendMachine;
		MeasurementSettingsViewModel.Init(bendMachine.AngleMeasurementSettings);
		PressDataViewModel.Init(bendMachine.PressBrakeData, bendMachine.PressBrakeInfo, bendMachine.PostProcessor);
		GeometryDataViewModel.Init(bendMachine.Geometry);
		_remark1 = bendMachine.Remark1;
		_remark2 = bendMachine.Remark2;
		_changed = ChangedConfigType.NoChanges;
	}

	private void ImportBystronic(object param)
	{
		if (!new Microsoft.Win32.OpenFileDialog
		{
			Filter = "XML files (*.xml)|*.xml",
			Title = "Select config-file"
		}.ShowDialog().Value || new FolderBrowserDialog
		{
			SelectedPath = _bendMachine?.ToString() + "\\Machine\\",
			Description = "Select folder to store dxf-files"
		}.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		try
		{
			throw new NotImplementedException("Bystronic import not available yet.");
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
		}
	}

	private void ConvertToC3mo(object param)
	{
		Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
		{
			Filter = "SMX files (*.smx)|*.smx|STL files (*.stl)|*.stl|DLD files (*.dld)|*.dld|BBX files (*.xml)|*.xml|All files (*.*)|*.*",
			Title = "Select config-file",
			Multiselect = true
		};
		if (!openFileDialog.ShowDialog().Value)
		{
			return;
		}
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
		{
			SelectedPath = _bendMachine?.ToString() + "\\Machine\\",
			Description = "Select folder to store c3mo-files"
		};
		if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		try
		{
			List<(string, Model)> list = new List<(string, Model)>();
			string[] fileNames = openFileDialog.FileNames;
			foreach (string text in fileNames)
			{
				switch (text.Split('.').Last().ToLower())
				{
				case "smx":
					list.Add((text, new SmxLoader().LoadSmx(text)));
					break;
				case "stl":
					list.Add((text, StlLoader.LoadStl(text)));
					break;
				case "dld":
					list.AddRange(DLDLoader.LoadDLD(text));
					break;
				case "xml":
				{
					BystronicGeometry bystronicGeometry = new BystronicGeometry().Deserialize(text);
					list.AddRange((from x in bystronicGeometry.GetAllModels()
						select (Key: x.Key, Value: x.Value)).ToList());
					break;
				}
				}
			}
			foreach (var item in list)
			{
				item.Item2.Transform *= Matrix4d.RotationX(-Math.PI / 2.0);
				item.Item2.Transform *= Matrix4d.RotationZ(-Math.PI / 2.0);
				ModelSerializer.Serialize(PnPathBuilder.CombinePath(folderBrowserDialog.SelectedPath, item.Item1.Split('\\').Last().Split('.')
					.First() + ".c3mo"), item.Item2);
				ModelSerializer.Serialize(_bendMachine.MachinePath + "\\Machine\\" + item.Item1.Split('\\').Last().Split('.')
					.First() + ".c3mo", item.Item2);
			}
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
		}
	}

	public void Save(IBendMachine saveTarget)
	{
		saveTarget.Remark1 = _remark1;
		saveTarget.Remark2 = _remark2;
		MeasurementSettingsViewModel.Save(saveTarget.AngleMeasurementSettings);
		PressDataViewModel.Save(saveTarget.PressBrakeData, saveTarget.PressBrakeInfo);
		GeometryDataViewModel.Save(saveTarget.Geometry);
		DataChanged?.Invoke(_changed);
	}

	public void Dispose()
	{
	}
}
