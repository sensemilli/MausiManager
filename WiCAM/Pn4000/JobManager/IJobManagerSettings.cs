using System.Collections.Generic;
using System.Windows;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager.Enumerators;
using WiCAM.Pn4000.Machine;

namespace WiCAM.Pn4000.JobManager;

public interface IJobManagerSettings : ISettings, IService
{
	ModelViewManager ModelManager { get; }

	List<MachineViewInfo> Machines { get; }

	List<CppConfigurationLineInfo> JobListConfiguration { get; set; }

	List<CppConfigurationLineInfo> PlateListConfiguration { get; set; }

	List<CppConfigurationLineInfo> PlatePartListConfiguration { get; set; }

	List<CppConfigurationLineInfo> PartListConfiguration { get; set; }

	string JobDataPath { get; set; }

	string JobDataSavePath { get; set; }

	ILopTemplatesHelper TemplatesHelper { get; }

	bool IsJobDeleteEnabled { get; set; }

	bool IsJobDeleteProducedEnabled { get; set; }

	bool IsJobSaveProducedEnabled { get; set; }

	bool IsJobProduceEnabled { get; set; }

	bool IsPlateProduceEnabled { get; set; }

	bool IsPlateStornoEnabled { get; set; }

	bool IsPartRejectEnabled { get; set; }

	bool IsTouchScreen { get; set; }

	bool IsSettingsVisible { get; set; }

	string PlateSaveBatchPath { get; set; }

	string PlateDeleteBatchPath { get; set; }

	bool HasToSaveProducedJobs { get; set; }

	string LastSelectedUser { get; set; }

	List<string> UserNames { get; set; }

	int Column1Width { get; set; }

	int GridJobHeight { get; set; }

	int GridPlateHeight { get; set; }

	string CoraUrl { get; set; }

	PlateBookTime PlateBookTimeValueType { get; set; }

	bool IsChargeObligatory { get; set; }

	bool IsUserObligatory { get; set; }

	bool HasToSaveLastUser { get; set; }

	int LabelPrintOrder { get; set; }

	bool IsJobPrintLabelEnabled { get; set; }

	bool IsPlatePrintLabelEnabled { get; set; }

	bool IsPartPrintLabelEnabled { get; set; }

	bool HasToUseBatchForHtmlPreview { get; set; }

	string HtmlPreviewBatchPath { get; set; }

	bool IsButtonPlateResetVisible { get; set; }

	string PrinterSettingsBatch { get; set; }
    int GridFoldersHeight { get; set; }
    int GridFilesHeight { get; set; }

	int GridArchiveHeight { get; set; }

    void SaveConfiguration(Window wnd);

	void SaveListSettings();

	void ApplyWindowConfiguration(Window wnd);
}
