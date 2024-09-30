using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager.Enumerators;

namespace WiCAM.Pn4000.JobManager.Classes;

internal class SettingFinder : ISettingFinder
{
	private readonly ISettingsNames _names;

	private IniFileInfo _ini;

	public SettingFinder(ISettingsNames names)
	{
		_names = names;
	}

	public void Initialize(IniFileInfo ini)
	{
		_ini = ini;
	}

	public bool FindBoolKey(string key, string defaultValue = "1")
	{
		IniSettingInfo iniSettingInfo = _ini.Settings.Find((IniSettingInfo x) => x.Key == key);
		if (iniSettingInfo == null)
		{
			_ini.Settings.Add(new IniSettingInfo
			{
				Key = key,
				Value = defaultValue
			});
			return true;
		}
		return iniSettingInfo.Value.Equals("1", StringComparison.CurrentCultureIgnoreCase);
	}

	public string FindStringKey(string key, string defaultValue = "")
	{
		IniSettingInfo iniSettingInfo = _ini.Settings.Find((IniSettingInfo x) => x.Key == key);
		if (iniSettingInfo == null)
		{
			_ini.Settings.Add(new IniSettingInfo
			{
				Key = key,
				Value = defaultValue
			});
			return defaultValue;
		}
		return iniSettingInfo.Value;
	}

	public int FindLabelPrintOrder()
	{
		IniSettingInfo iniSettingInfo = _ini.Settings.Find((IniSettingInfo x) => x.Key == _names.LabelPrintOrder);
		if (iniSettingInfo == null)
		{
			_ini.Settings.Add(new IniSettingInfo
			{
				Key = _names.LabelPrintOrder,
				Value = "0"
			});
			return 0;
		}
		return StringHelper.ToInt(iniSettingInfo.Value);
	}

	public bool FindHasToSaveLastUser()
	{
		return FindBoolKey(_names.HasToSaveLastUser);
	}

	public bool FindUserIsObligatory()
	{
		return FindBoolKey(_names.IsUserObligatoryKey, "0");
	}

	public bool FindChargeIsObligatory()
	{
		return FindBoolKey(_names.IsChargeObligatoryKey, "0");
	}

	public string FindCoraUrl()
	{
		IniSettingInfo iniSettingInfo = _ini.Settings.Find((IniSettingInfo x) => x.Key == _names.CoraConfiguration);
		if (iniSettingInfo == null)
		{
			_ini.Settings.Add(new IniSettingInfo
			{
				Key = _names.CoraConfiguration,
				Value = string.Empty
			});
			return string.Empty;
		}
		return iniSettingInfo.Value;
	}

	public PlateBookTime FindPlateBookTimeSetting()
	{
		IniSettingInfo iniSettingInfo = _ini.Settings.Find((IniSettingInfo x) => x.Key == _names.PlateBookTimeKey);
		if (iniSettingInfo == null)
		{
			_ini.Settings.Add(new IniSettingInfo
			{
				Key = _names.PlateBookTimeKey,
				Value = PlateBookTime.PLATE_TIME_WORK.ToString()
			});
			return PlateBookTime.PLATE_TIME_WORK;
		}
		if (iniSettingInfo.Value.Equals(PlateBookTime.PLATE_TIME_WORK.ToString(), StringComparison.CurrentCultureIgnoreCase))
		{
			return PlateBookTime.PLATE_TIME_WORK;
		}
		return PlateBookTime.PLATE_TIME_TOTAL;
	}

	public bool FindBoolSetting(string key)
	{
		return FindSetting(key).Equals("1");
	}

	public int FindIntegerSetting(string key)
	{
		return StringHelper.ToInt(FindSetting(key));
	}

	public string FindSetting(string key)
	{
		IniSettingInfo iniSettingInfo = _ini.Settings.Find((IniSettingInfo x) => x.Key == key);
		if (iniSettingInfo != null)
		{
			return iniSettingInfo.Value;
		}
		return string.Empty;
	}

	public List<string> FindUsers()
	{
		string text = FindSetting(_names.UserNames);
		if (string.IsNullOrEmpty(text))
		{
			text = string.Join(";", Environment.UserName);
		}
		return new List<string>(text.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
	}

	public string FindSettingsAndCheckPnDrive(string key, string defaultValue)
	{
		string text = FindSetting(key);
		if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(defaultValue))
		{
			text = defaultValue;
		}
		return PnPathBuilder.CheckPnDriveKey(text).Trim();
	}
}
