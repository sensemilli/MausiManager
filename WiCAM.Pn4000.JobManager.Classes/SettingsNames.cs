namespace WiCAM.Pn4000.JobManager.Classes;

internal class SettingsNames : ISettingsNames
{
	public string IsJobDeleteEnabled { get; } = "IsJobDeleteEnabled";


	public string IsJobDeleteProducedEnabled { get; } = "IsJobDeleteProducedEnabled";


	public string IsJobProduceEnabled { get; } = "IsJobProduceEnabled";


	public string IsPlateProduceEnabled { get; } = "IsPlateProduceEnabled";


	public string IsPlateStornoEnabled { get; } = "IsPlateStornoEnabled";


	public string IsPartRejectEnabled { get; } = "IsPartRejectEnabled";


	public string IsJobSaveProducedEnabled { get; } = "IsJobSaveProducedEnabled";


	public string UserNames { get; } = "UserNames";


	public string IsTouchScreen { get; } = "IsTouchScreen";


	public string IsSettingsVisible { get; } = "IsSettingsVisible";


	public string PlateSaveBatchPath { get; } = "PlateSaveBatchPath";


	public string PlateDeleteBatchPath { get; } = "PlateDeleteBatchPath";


	public string LastSelectedUser { get; } = "LastSelectedUser";


	public string IsJobPrintLabelEnabled { get; } = "IsJobPrintLabelEnabled";


	public string IsPlatePrintLabelEnabled { get; } = "IsPlatePrintLabelEnabled";


	public string IsPartPrintLabelEnabled { get; } = "IsPartPrintLabelEnabled";


	public string HasToUseBatchForHtmlPreview { get; } = "HasToUseBatchForHtmlPreview";


	public string HtmlPreviewBatchPath { get; } = "HtmlPreviewBatchPath";


	public string IsButtonPlateResetVisible { get; } = "IsButtonPlateResetVisible";


	public string AllowedMachines { get; } = "AllowedMachines";


	public string Column1Width { get; } = "Column1Width";


	public string JobDataSavePath { get; } = "JobDataSavePath";


	public string GridJobHeight { get; } = "GridJobHeight";


	public string GridPlateHeight { get; } = "GridPlateHeight";

    public string GridFoldersHeight { get; } = "GridFoldersHeight";
    public string GridFilesHeight { get; } = "GridFilesHeight";
    public string GridArchiveHeight { get; } = "GridArchiveHeight";


    public string HasToSaveProducedJobs { get; } = "HasToSaveProducedJobs";


	public string CoraConfiguration { get; } = "CoraUrl";


	public string PlateBookTimeKey { get; } = "PlateBookTimeKey";


	public string IsChargeObligatoryKey { get; } = "IsChargeObligatory";


	public string IsUserObligatoryKey { get; } = "IsUserObligatory";


	public string HasToSaveLastUser { get; } = "HasToSaveLastUser";


	public string LabelPrintOrder { get; } = "LabelPrintOrder";


	public string JobDataPath { get; } = "JOBDATA_PATH";


	public string PrinterSettingsBatch { get; } = "PrinterSettingsBatch";

}
