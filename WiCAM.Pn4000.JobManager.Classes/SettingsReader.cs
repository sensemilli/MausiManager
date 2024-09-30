using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;
using WiCAM.Pn4000.JobManager.Lop;

namespace WiCAM.Pn4000.JobManager.Classes;

internal class SettingsReader
{
	private readonly ISettingFinder _finder;

	private readonly ISettingsNames _names;

	private readonly ISettingsMachineReader _machinesReader;

	private static readonly string __iniName = "JOBMANAGER.ini";

	private static readonly string __translationName = "JOBMANAGER.TXT";

	private static readonly string JOBDATA_PATH = "JOBDATA_PATH";

	private static readonly string JOBS = "JOBS_";

	private static readonly string PLATES = "PLATES_";

	private static readonly string PLATE_PARTS = "PLATEPART_";

	private static readonly string PARTS = "PARTS_";

	public SettingsReader(ISettingsMachineReader machinesReader, ISettingFinder finder, ISettingsNames names)
	{
		_finder = finder;
		_names = names;
		_machinesReader = machinesReader;
	}

	public SettingsInfo Read()
	{
		SettingsInfo settingsInfo = ReadConfiguration();
		settingsInfo.Machines = _machinesReader.ReadMachines(settingsInfo.AllowedMachines);
		new EmbeddedResourceHelper(Assembly.GetExecutingAssembly()).CheckAll();
		settingsInfo.TemplatesHelper = new LopTemplatesHelper(new LopTemplateReader());
		string path = PnPathBuilder.PathInPnDrive("u\\pn\\gfiles\\JobManagerTemplates.txt");
		settingsInfo.TemplatesHelper.PrepareTemplates(path);
		return settingsInfo;
	}

	private SettingsInfo ReadConfiguration()
	{
		SettingsInfo settingsInfo = new SettingsInfo(_names);
		List<string> listNames = new List<string> { JOBS, PLATES, PLATE_PARTS, PARTS };
		IniManager iniManager = new IniManager();
		settingsInfo.IniInfo = iniManager.Read(__iniName, __translationName, listNames);
		if (settingsInfo.IniInfo == null)
		{
			return settingsInfo;
		}
		_finder.Initialize(settingsInfo.IniInfo);
		settingsInfo.JobListConfiguration = settingsInfo.IniInfo.FindConfiguration(JOBS);
		settingsInfo.PlateListConfiguration = settingsInfo.IniInfo.FindConfiguration(PLATES);
		settingsInfo.PlatePartListConfiguration = settingsInfo.IniInfo.FindConfiguration(PLATE_PARTS);
		settingsInfo.PartListConfiguration = settingsInfo.IniInfo.FindConfiguration(PARTS);
		Logger.Info("----- PlatePartListConfiguration={0}", settingsInfo.PlatePartListConfiguration.Count);
		BuildReference(settingsInfo.PlatePartListConfiguration, typeof(PlatePartInfo).GetProperties());
		CreateTexts(settingsInfo.IniInfo.Texts);
		string text = _finder.FindSetting(JOBDATA_PATH);
		if (!string.IsNullOrEmpty(text))
		{
			settingsInfo.JobDataPath = PnPathBuilder.CheckPnDriveKey(text);
		}
		string text2 = _finder.FindSetting(_names.JobDataSavePath);
		if (!string.IsNullOrEmpty(text2))
		{
			settingsInfo.JobDataSavePath = PnPathBuilder.CheckPnDriveKey(text2);
		}
		settingsInfo.IsJobDeleteEnabled = _finder.FindBoolSetting(_names.IsJobDeleteEnabled);
		settingsInfo.IsJobDeleteProducedEnabled = _finder.FindBoolSetting(_names.IsJobDeleteProducedEnabled);
		settingsInfo.IsJobSaveProducedEnabled = _finder.FindBoolSetting(_names.IsJobSaveProducedEnabled);
		settingsInfo.IsJobProduceEnabled = _finder.FindBoolSetting(_names.IsJobProduceEnabled);
		settingsInfo.IsPlateProduceEnabled = _finder.FindBoolSetting(_names.IsPlateProduceEnabled);
		settingsInfo.IsPlateStornoEnabled = _finder.FindBoolSetting(_names.IsPlateStornoEnabled);
		settingsInfo.IsPartRejectEnabled = _finder.FindBoolSetting(_names.IsPartRejectEnabled);
		settingsInfo.UserNames = _finder.FindUsers();
		settingsInfo.IsTouchScreen = _finder.FindBoolSetting(_names.IsTouchScreen);
		settingsInfo.AllowedMachines = FindAllowedMachines();
		settingsInfo.Column1Width = _finder.FindIntegerSetting(_names.Column1Width);
		if (settingsInfo.Column1Width < 0)
		{
			settingsInfo.Column1Width = 300;
		}
		settingsInfo.GridJobHeight = _finder.FindIntegerSetting(_names.GridJobHeight);
		settingsInfo.GridPlateHeight = _finder.FindIntegerSetting(_names.GridPlateHeight);
        settingsInfo.GridFoldersHeight = _finder.FindIntegerSetting(_names.GridFoldersHeight);
        settingsInfo.GridFilesHeight = _finder.FindIntegerSetting(_names.GridFilesHeight);

        settingsInfo.GridArchiveHeight = _finder.FindIntegerSetting(_names.GridArchiveHeight);

        settingsInfo.HasToSaveProducedJobs = _finder.FindBoolSetting(_names.HasToSaveProducedJobs);
		settingsInfo.IsSettingsVisible = _finder.FindBoolSetting(_names.IsSettingsVisible);
		settingsInfo.LastSelectedUser = _finder.FindSetting(_names.LastSelectedUser);
		settingsInfo.CoraUrl = _finder.FindCoraUrl();
		settingsInfo.PlateBookTimeValueType = _finder.FindPlateBookTimeSetting();
		settingsInfo.IsChargeObligatory = _finder.FindChargeIsObligatory();
		settingsInfo.IsUserObligatory = _finder.FindUserIsObligatory();
		settingsInfo.HasToSaveLastUser = _finder.FindHasToSaveLastUser();
		settingsInfo.LabelPrintOrder = _finder.FindLabelPrintOrder();
		settingsInfo.IsJobPrintLabelEnabled = _finder.FindBoolKey(_names.IsJobPrintLabelEnabled);
		settingsInfo.IsPlatePrintLabelEnabled = _finder.FindBoolKey(_names.IsPlatePrintLabelEnabled);
		settingsInfo.IsPartPrintLabelEnabled = _finder.FindBoolKey(_names.IsPartPrintLabelEnabled);
		settingsInfo.HasToUseBatchForHtmlPreview = _finder.FindBoolKey(_names.HasToUseBatchForHtmlPreview, "0");
		settingsInfo.PlateSaveBatchPath = _finder.FindSettingsAndCheckPnDrive(_names.PlateSaveBatchPath, "$PNDRIVE$\\u\\pn\\bin\\JobManagerPlate.bat");
		settingsInfo.PlateDeleteBatchPath = _finder.FindSettingsAndCheckPnDrive(_names.PlateDeleteBatchPath, "$PNDRIVE$\\u\\pn\\bin\\JobManagerPlate.bat");
		settingsInfo.HtmlPreviewBatchPath = _finder.FindStringKey(_names.HtmlPreviewBatchPath, PnPathBuilder.PathInPnDrive("u\\pn\\bin\\jobManagerPreview.bat"));
		settingsInfo.IsButtonPlateResetVisible = _finder.FindBoolKey(_names.IsButtonPlateResetVisible, "0");
		settingsInfo.PrinterSettingsBatch = _finder.FindSettingsAndCheckPnDrive(_names.PrinterSettingsBatch, string.Empty);
		return settingsInfo;
	}

	private void CreateTexts(Dictionary<string, string> texts)
	{
		ResourceDictionary resourceDictionary = new ResourceDictionary();
		foreach (KeyValuePair<string, string> text in texts)
		{
			resourceDictionary.Add(text.Key, text.Value);
		}
		Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
	}

	private void BuildReference(List<CppConfigurationLineInfo> configuration, PropertyInfo[] itemProperties)
	{
		if (!EnumerableHelper.IsNullOrEmpty(configuration))
		{
			foreach (PropertyInfo property in new List<PropertyInfo>(itemProperties))
			{
				CppConfigurationLineInfo cppConfigurationLineInfo = configuration.Find((CppConfigurationLineInfo x) => x.Key.Equals(property.Name, StringComparison.CurrentCultureIgnoreCase));
				if (cppConfigurationLineInfo == null)
				{
					DatKeyAttribute attribute = CustomAttributeHelper.FindCustomAttribute<DatKeyAttribute>(property);
					if (attribute != null)
					{
						cppConfigurationLineInfo = configuration.Find((CppConfigurationLineInfo x) => x.Key.Equals(attribute.Key, StringComparison.CurrentCultureIgnoreCase));
					}
				}
				if (cppConfigurationLineInfo != null)
				{
					cppConfigurationLineInfo.Property = property;
					cppConfigurationLineInfo.PropertyName = property.Name;
				}
			}
			return;
		}
		Logger.Error("BuildReference failed");
	}

	private List<int> FindAllowedMachines()
	{
		List<int> list = new List<int>();
		string text = _finder.FindSetting(_names.AllowedMachines);
		if (!string.IsNullOrEmpty(text))
		{
			string[] array = text.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (array.Any())
			{
				string[] array2 = array;
				foreach (string input in array2)
				{
					list.Add(StringHelper.ToInt(input));
				}
			}
		}
		return list;
	}
}
