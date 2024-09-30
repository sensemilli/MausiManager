using System.Collections.Generic;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager.Enumerators;

namespace WiCAM.Pn4000.JobManager.Classes;

public interface ISettingFinder
{
	void Initialize(IniFileInfo ini);

	bool FindBoolKey(string key, string defaultValue = "1");

	string FindStringKey(string key, string defaultValue = "");

	int FindLabelPrintOrder();

	bool FindHasToSaveLastUser();

	bool FindUserIsObligatory();

	bool FindChargeIsObligatory();

	string FindCoraUrl();

	PlateBookTime FindPlateBookTimeSetting();

	bool FindBoolSetting(string key);

	int FindIntegerSetting(string key);

	string FindSetting(string key);

	List<string> FindUsers();

	string FindSettingsAndCheckPnDrive(string key, string defaultValue);
}
