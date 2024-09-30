namespace WiCAM.Pn4000.JobManager.Classes;

public interface ISettingsNames
{
	string IsJobDeleteEnabled { get; }

	string IsJobDeleteProducedEnabled { get; }

	string IsJobProduceEnabled { get; }

	string IsPlateProduceEnabled { get; }

	string IsPlateStornoEnabled { get; }

	string IsPartRejectEnabled { get; }

	string IsJobSaveProducedEnabled { get; }

	string UserNames { get; }

	string IsTouchScreen { get; }

	string IsSettingsVisible { get; }

	string PlateSaveBatchPath { get; }

	string PlateDeleteBatchPath { get; }

	string LastSelectedUser { get; }

	string IsJobPrintLabelEnabled { get; }

	string IsPlatePrintLabelEnabled { get; }

	string IsPartPrintLabelEnabled { get; }

	string HasToUseBatchForHtmlPreview { get; }

	string HtmlPreviewBatchPath { get; }

	string IsButtonPlateResetVisible { get; }

	string AllowedMachines { get; }

	string Column1Width { get; }

	string JobDataSavePath { get; }

	string GridJobHeight { get; }

	string GridPlateHeight { get; }
    string GridFoldersHeight { get; }
    string GridFilesHeight { get; }

    string GridArchiveHeight { get; }
    
    string HasToSaveProducedJobs { get; }

	string CoraConfiguration { get; }

	string PlateBookTimeKey { get; }

	string IsChargeObligatoryKey { get; }

	string IsUserObligatoryKey { get; }

	string HasToSaveLastUser { get; }

	string LabelPrintOrder { get; }

	string JobDataPath { get; }

	string PrinterSettingsBatch { get; }
}
