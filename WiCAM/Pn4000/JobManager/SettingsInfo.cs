using System;
using System.Collections.Generic;
using System.Windows;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager.Classes;
using WiCAM.Pn4000.JobManager.Enumerators;
using WiCAM.Pn4000.Machine;

namespace WiCAM.Pn4000.JobManager;

public class SettingsInfo : ViewModelBase, IJobManagerSettings, ISettings, IService
{
	private readonly ISettingsNames _names;

	public static readonly string IniName = "JOBMANAGER.ini";

	private bool _isTouchScreen;

	public ModelViewManager ModelManager { get; private set; } = new ModelViewManager();


	public List<CppConfigurationLineInfo> JobListConfiguration { get; set; } = new List<CppConfigurationLineInfo>();


	public List<CppConfigurationLineInfo> PlateListConfiguration { get; set; } = new List<CppConfigurationLineInfo>();


	public List<CppConfigurationLineInfo> PlatePartListConfiguration { get; set; } = new List<CppConfigurationLineInfo>();


	public List<CppConfigurationLineInfo> PartListConfiguration { get; set; } = new List<CppConfigurationLineInfo>();


	public List<MachineViewInfo> Machines { get; set; } = new List<MachineViewInfo>();


	public string JobDataPath { get; set; }

	public string JobDataSavePath { get; set; }

	public ILopTemplatesHelper TemplatesHelper { get; set; }

	public bool IsJobDeleteEnabled { get; set; }

	public bool IsJobDeleteProducedEnabled { get; set; }

	public bool IsJobSaveProducedEnabled { get; set; }

	public bool IsJobProduceEnabled { get; set; }

	public bool IsPlateProduceEnabled { get; set; }

	public bool IsPlateStornoEnabled { get; set; }

	public bool IsPartRejectEnabled { get; set; }

	public bool IsSettingsVisible { get; set; } = true;


	public string PlateSaveBatchPath { get; set; }

	public string PlateDeleteBatchPath { get; set; }

	public string LastSelectedUser { get; set; }

	public bool HasToSaveProducedJobs { get; set; }

	public List<int> AllowedMachines { get; set; }

	public bool IsTouchScreen
	{
		get
		{
			return _isTouchScreen;
		}
		set
		{
			_isTouchScreen = value;
			NotifyPropertyChanged("IsTouchScreen");
		}
	}

	public int Column1Width { get; set; }

	public int GridJobHeight { get; set; }
    public int GridFilesHeight { get; set; }
	public int GridArchiveHeight { get; set; }
    public int GridPlateHeight { get; set; }

	public List<string> UserNames { get; set; }

	public IniFileInfo IniInfo { get; set; }

	public List<IniSettingInfo> Settings
	{
		get
		{
			if (IniInfo == null)
			{
				return null;
			}
			return IniInfo.Settings;
		}
	}

	public string CoraUrl { get; set; }

	public PlateBookTime PlateBookTimeValueType { get; set; }

	public bool IsChargeObligatory { get; set; }

	public bool IsUserObligatory { get; set; }

	public bool HasToSaveLastUser { get; set; }

	public int LabelPrintOrder { get; set; }

	public bool IsJobPrintLabelEnabled { get; set; } = true;


	public bool IsPlatePrintLabelEnabled { get; set; } = true;


	public bool IsPartPrintLabelEnabled { get; set; } = true;


	public bool HasToUseBatchForHtmlPreview { get; set; }

	public string HtmlPreviewBatchPath { get; set; }

	public bool IsButtonPlateResetVisible { get; set; }

	public string PrinterSettingsBatch { get; set; }
    public int GridFoldersHeight { get; set; }


	public SettingsInfo(ISettingsNames names)
	{
		_names = names;
		JobDataPath = Environment.ExpandEnvironmentVariables("P:\\u\\sfa\\jobdata");
	}

	public void SaveConfiguration(Window wnd)
	{
		Logger.Info("SaveConfiguration");
		IniInfo.SettingsOfWindow.ReadSettings(wnd);
		UpdateListSettings();
		ActualizeDynamicSettings();
		new IniManager().Write(IniInfo);
	}

	public void SaveListSettings()
	{
		Logger.Info("SaveListSettings");
		UpdateUsers();
		UpdateMachines();
		ActualizeDynamicSettings();
		new IniManager().Write(IniInfo);
	}

	public void ApplyWindowConfiguration(Window wnd)
	{
		IniInfo.SettingsOfWindow.Apply(wnd);
	}

	private void ActualizeDynamicSettings()
	{
		ActualSetting(_names.JobDataPath, JobDataPath);
		ActualSetting(_names.JobDataSavePath, JobDataSavePath);
		ActualSetting(_names.Column1Width, Column1Width);
		ActualSetting(_names.GridJobHeight, GridJobHeight);
		ActualSetting(_names.GridPlateHeight, GridPlateHeight);
        ActualSetting(_names.GridFoldersHeight, GridFoldersHeight);
        ActualSetting(_names.GridFilesHeight, GridFilesHeight);
        ActualSetting(_names.GridArchiveHeight, GridArchiveHeight);
        
        ActualSetting(_names.IsTouchScreen, IsTouchScreen ? 1 : 0);
		ActualSetting(_names.HasToSaveProducedJobs, HasToSaveProducedJobs ? 1 : 0);
		ActualSetting(_names.IsSettingsVisible, IsSettingsVisible ? 1 : 0);
		ActualSetting(_names.PlateDeleteBatchPath, PlateDeleteBatchPath);
		ActualSetting(_names.PlateSaveBatchPath, PlateSaveBatchPath);
		if (HasToSaveLastUser)
		{
			ActualSetting(_names.LastSelectedUser, LastSelectedUser);
		}
		ActualSetting(_names.CoraConfiguration, CoraUrl);
		ActualSetting(_names.PrinterSettingsBatch, PrinterSettingsBatch);
	}

	private void UpdateListSettings()
	{
		foreach (List<CppConfigurationLineInfo> value in IniInfo.Tables.Values)
		{
			foreach (CppConfigurationLineInfo item in value)
			{
				if (item.BoundColumn != null)
				{
					item.Width = (int)item.BoundColumn.ActualWidth;
					item.DisplayIndex = item.BoundColumn.DisplayIndex;
				}
			}
		}
	}

	private void ActualSetting(string settingsKey, object value)
	{
		IniSettingInfo iniSettingInfo = IniInfo.Settings.Find((IniSettingInfo x) => x.Key == settingsKey);
		if (iniSettingInfo == null)
		{
			iniSettingInfo = new IniSettingInfo
			{
				Key = settingsKey
			};
			IniInfo.Settings.Add(iniSettingInfo);
		}
		iniSettingInfo.Value = ((value != null) ? value.ToString() : string.Empty);
	}

	private void UpdateUsers()
	{
		IniSettingInfo iniSettingInfo = IniInfo.Settings.Find((IniSettingInfo x) => x.Key == _names.UserNames);
		if (iniSettingInfo != null)
		{
			iniSettingInfo.Value = string.Join(";", UserNames);
		}
	}

	private void UpdateMachines()
	{
		IniSettingInfo iniSettingInfo = IniInfo.Settings.Find((IniSettingInfo x) => x.Key == _names.AllowedMachines);
		if (iniSettingInfo != null)
		{
			iniSettingInfo.Value = string.Join(";", AllowedMachines);
		}
	}
}
