#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Jobdata.Classes;
using WiCAM.Pn4000.Jobdata.Helpers;
using WiCAM.Pn4000.JobManager.Classes;
using WiCAM.Pn4000.JobManager.Cora;
using WiCAM.Pn4000.JobManager.Helpers;
using WiCAM.Pn4000.JobManager.Lop;

namespace WiCAM.Pn4000.JobManager;

public class ProductionHelper : IProductionHelper
{
	private readonly IJobManagerSettings _settings;

	private readonly CoraCommunicator _communicator;

	public IBufferedLogger Logger { get; } = new BufferedLogger();


	public ProductionHelper(IJobManagerSettings settings)
	{
		_settings = settings;
		if (!string.IsNullOrWhiteSpace(settings.CoraUrl))
		{
			_communicator = new CoraCommunicator(settings, Logger);
		}
	}

	public void ProducePlate(PlateProductionInfo plate)
	{
		IEnumerable<LopTemplateInfo> templates = _settings.TemplatesHelper.FindTemplates(LopTemplatesHelper.ActionProduced);
		DumpTemplates(templates);
		JobDefaultReader jobDefaultReader = new JobDefaultReader();
		Plate plateDat = jobDefaultReader.ReadPlate(plate.Plate.Path);
		WritePlateLops(plateDat, plate, templates);
		if (_communicator != null)
		{
			Task<bool> task = Task.Run(() => _communicator.BookProduced(plate, plateDat));
			Task.WaitAll(task);
			task = Task.Run(() => _communicator.BookSavedToCora());
			Task.WaitAll(task);
		}
		try
		{
			if (ProductionRest(plate) <= 0)
			{
				Job job = jobDefaultReader.ReadJob(Path.GetDirectoryName(plate.Plate.JobReference.Path));
				new SchachtelBasisUpdater(job.JobArchiveNumber, job.JobData1, plateDat.PlateNumber).ModifySchbas(3);
			}
		}
		catch (Exception exception)
		{
			Logger.Exception(exception);
		}
	}

	[Conditional("TRACE")]
	private void DumpTemplates(IEnumerable<LopTemplateInfo> templates)
	{
		Logger.Info("DumpTemplates");
		if (templates == null)
		{
			Logger.Error("   templates = null");
			return;
		}
		foreach (LopTemplateInfo template in templates)
		{
			Logger.Info("  {0};  {1};  {2}", template.Type, template.Company, template.LopType);
		}
	}

	public void StornoPlate(PlateProductionInfo plate)
	{
		IEnumerable<LopTemplateInfo> templates = _settings.TemplatesHelper.FindTemplates(LopTemplatesHelper.ActionStorno);
		DumpTemplates(templates);
		JobDefaultReader jobDefaultReader = new JobDefaultReader();
		Job job = jobDefaultReader.ReadJob(Path.GetDirectoryName(plate.Plate.JobReference.Path));
		Plate plateDat = jobDefaultReader.ReadPlate(plate.Plate.Path);
		WritePlateLops(plateDat, plate, templates);
		if (_communicator != null)
		{
			Task<bool> task = Task.Run(() => _communicator.BookStorno(plate, plateDat));
			Task.WaitAll(task);
			task = Task.Run(() => _communicator.BookSavedToCora());
			Task.WaitAll(task);
		}
		try
		{
			if (ProductionRest(plate) <= 0)
			{
				new SchachtelBasisUpdater(job.JobArchiveNumber, job.JobData1, plateDat.PlateNumber).ModifySchbas(1);
			}
		}
		catch (Exception exception)
		{
			Logger.Exception(exception);
		}
	}

	private int ProductionRest(PlateProductionInfo plate)
	{
		return plate.Plate.NUMBER_OF_PLATES - plate.Plate.PLATE_PRODUCED - plate.Plate.PLATE_STORNO;
	}

	private void WritePlateLops(Plate plateDat, PlateProductionInfo plate, IEnumerable<LopTemplateInfo> templates)
	{
		Logger.Info("WritePlateLops");
		if (EnumerableHelper.IsNullOrEmpty(templates))
		{
			return;
		}
		new DatReader<PlateInfo>().UpdatePlateDat(plate);
		foreach (LopTemplateInfo template in templates)
		{
			ProcessTemplate(plate, template, plateDat);
		}
	}

	private void ProcessTemplate(PlateProductionInfo plate, LopTemplateInfo template, Plate plateDat)
	{
		Logger.Info("ProcessTemplate");
		if (template.Type.Equals(LopTemplatesHelper.TypeMatpool))
		{
			WritePlateLop(plate, template, plateDat);
		}
		else if (template.Type.Equals(LopTemplatesHelper.TypeMatpoolRest))
		{
			if (!string.IsNullOrEmpty(plate.Plate.PLATE_NAME_REST))
			{
				WritePlateLop(plate, template, plateDat);
			}
		}
		else
		{
			WritePartProductionLops(plate, template, plateDat);
		}
	}

	private void WritePlateLop(PlateProductionInfo plateProduction, LopTemplateInfo template, Plate plateDat)
	{
		if (IOHelper.DirectoryExists(template.DestinationPath))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(plateProduction.ProductionData.ToLopString());
			stringBuilder.Append(template.Content);
			PlateInfo plateInfo = EnumerableHelper.CopyItem(plateProduction.Plate);
			plateInfo.PLATE_PRODUCED_ACT = FindActualAmount(plateProduction, template);
			OutputContentBuilder outputContentBuilder = new OutputContentBuilder(stringBuilder);
			foreach (KeyValuePair<string, PropertyReference> item in template.Mapping)
			{
				if (item.Key.IndexOf("$PRODUCTION_", StringComparison.CurrentCultureIgnoreCase) == 0)
				{
					if (item.Value != null)
					{
						outputContentBuilder.AddValue(item.Key, item.Value.Property, plateProduction.ProductionData, item.Value.Property.PropertyType);
					}
				}
				else if (item.Value.ItemType == typeof(JobInfo))
				{
					outputContentBuilder.AddValue(item.Key, item.Value.Property, plateProduction.Plate.JobReference, item.Value.Property.PropertyType);
				}
				else if (item.Value != null)
				{
					outputContentBuilder.AddValue(item.Key, item.Value.Property, plateInfo, item.Value.Property.PropertyType);
				}
			}
			string text = new PlateLopNameCreator(template).CreateFilePath(plateDat, plateProduction);
			Logger.Info("PLATE_FEEDBACK. path = {0}", text);
			Logger.Verbose(stringBuilder.ToString());
			IOHelper.FileAppendText(text, stringBuilder.ToString());
		}
		else
		{
			Logger.Error("Directory '{0}' doesn't exists", template.DestinationPath);
		}
	}

	private int FindActualAmount(PlateProductionInfo plateProduction, LopTemplateInfo template)
	{
		int result = plateProduction.AmountToProduce;
		if (template.LopType.Equals("STORNO"))
		{
			result = plateProduction.AmountStorno;
		}
		return result;
	}

	private void WritePartProductionLops(PlateProductionInfo plateProduction, LopTemplateInfo template, Plate plateDat)
	{
		if (IOHelper.DirectoryExists(template.DestinationPath))
		{
			double factor = plateProduction.Plate.PLATE_TIME_REAL / (double)plateProduction.AmountToProduce / plateProduction.Plate.PLATE_TIME_WORK;
			IEnumerable<LopTemplateInfo> rejectTemplates = _settings.TemplatesHelper.FindTemplates(LopTemplatesHelper.ActionReject, LopTemplatesHelper.TypeGapool);
			{
				foreach (PlatePartProductionInfo platePart in plateProduction.PlateParts)
				{
					platePart.PlatePart.Part.PartAmountOnPlate = platePart.PlatePart.PLATE_PART_AMOUNT * plateProduction.Plate.NUMBER_OF_PLATES;
					platePart.PlatePart.PartsProducedTotal = platePart.PlatePart.PLATE_PART_AMOUNT * plateProduction.AmountToProduce;
					if (platePart.PlatePart.CollectiveOrders.Count == 0)
					{
						PlatePartProductionStandard(plateProduction, template, platePart, rejectTemplates, plateDat, factor);
					}
					else
					{
						PlatePartProductionCollectiveOrder(plateProduction, template, platePart, rejectTemplates, plateDat, factor);
					}
				}
				return;
			}
		}
		Logger.Error("Directory '{0}' doesn't exists", template.DestinationPath);
	}

	private void PlatePartProductionCollectiveOrder(PlateProductionInfo plateProduction, LopTemplateInfo template, PlatePartProductionInfo platePart, IEnumerable<LopTemplateInfo> rejectTemplates, Plate plateDat, double factor)
	{
		CollectiveOrderFeedbackHelper collectiveOrderFeedbackHelper = new CollectiveOrderFeedbackHelper(this);
		if (template.LopType.Equals(LopTemplatesHelper.ActionStorno))
		{
			collectiveOrderFeedbackHelper.StornoPlatePart(plateProduction, template, platePart, plateDat);
			return;
		}
		platePart.PlatePart.PLATE_PART_TIME_REAL = platePart.PlatePart.PLATE_PART_TIME * factor;
		collectiveOrderFeedbackHelper.ProducePlatePart(plateProduction, platePart, rejectTemplates, template, plateDat);
	}

	private void PlatePartProductionStandard(PlateProductionInfo plateProduction, LopTemplateInfo template, PlatePartProductionInfo platePart, IEnumerable<LopTemplateInfo> rejectTemplates, Plate plateDat, double factor)
	{
		if (template.LopType.Equals(LopTemplatesHelper.ActionStorno))
		{
			platePart.AmountToStorno = platePart.PlatePart.PLATE_PART_AMOUNT * plateProduction.AmountStorno;
			WritePartLop(platePart, platePart.AmountToStorno, template, plateProduction, plateDat);
			return;
		}
		platePart.PlatePart.PLATE_PART_TIME_REAL = platePart.PlatePart.PLATE_PART_TIME * factor;
		if (platePart.AmountToProduce > 0)
		{
			WritePartLop(platePart, platePart.AmountToProduce, template, plateProduction, plateDat);
		}
		if (!template.Company.Equals("PN4000", StringComparison.CurrentCultureIgnoreCase) || platePart.AmountToReject <= 0)
		{
			return;
		}
		foreach (LopTemplateInfo rejectTemplate in rejectTemplates)
		{
			WritePartLop(platePart, platePart.AmountToReject, rejectTemplate, plateProduction, plateDat);
		}
	}

	public void WritePartLop(PlatePartProductionInfo part, int amount, LopTemplateInfo template, PlateProductionInfo plateProduction, Plate plateDat)
	{
		StringBuilder stringBuilder = new StringBuilder();
		PartInfo partInfo = EnumerableHelper.CopyItem(part.PlatePart.Part);
		partInfo.PART_PRODUCED = amount;
		if (template.TypeOfFeedback == FeedbackFileType.LOP)
		{
			stringBuilder.Append(plateProduction.ProductionData.ToLopString());
		}
		else if (template.TypeOfFeedback == FeedbackFileType.CSV)
		{
			if (!string.IsNullOrEmpty(template.Header))
			{
				stringBuilder.AppendLine(template.Header);
			}
			partInfo.PART_REJECT = part.AmountToReject;
		}
		stringBuilder.Append(template.Content);
		OutputContentBuilder outputContentBuilder = new OutputContentBuilder(stringBuilder);
		foreach (KeyValuePair<string, PropertyReference> item in template.Mapping)
		{
			if (item.Value == null)
			{
				continue;
			}
			try
			{
				if (item.Key.IndexOf("$PRODUCTION_", StringComparison.CurrentCultureIgnoreCase) == 0)
				{
					outputContentBuilder.AddValue(item.Key, item.Value.Property, plateProduction.ProductionData, item.Value.Property.PropertyType);
				}
				else if (item.Value.ItemType == typeof(PartInfo))
				{
					outputContentBuilder.AddValue(item.Key, item.Value.Property, partInfo, item.Value.Property.PropertyType);
				}
				else if (item.Value.ItemType == typeof(JobInfo))
				{
					outputContentBuilder.AddValue(item.Key, item.Value.Property, plateProduction.Plate.JobReference, item.Value.Property.PropertyType);
				}
				else if (item.Value.ItemType == typeof(PlateInfo))
				{
					outputContentBuilder.AddValue(item.Key, item.Value.Property, plateProduction.Plate, item.Value.Property.PropertyType);
				}
				else if (item.Value.ItemType == typeof(PlatePartInfo))
				{
					outputContentBuilder.AddValue(item.Key, item.Value.Property, part.PlatePart, item.Value.Property.PropertyType);
				}
				else
				{
					outputContentBuilder.AddValue(item.Key, item.Value.Property, partInfo, item.Value.Property.PropertyType);
				}
			}
			catch (Exception exception)
			{
				Logger.Exception(exception);
			}
		}
		string text = new PartLopNameCreator(template).CreateFilePath(plateDat, plateProduction, part);
		Logger.Info("PART_FEEDBACK. path = {0}", text);
		Logger.Verbose(stringBuilder.ToString());
		IOHelper.FileAppendText(text, stringBuilder.ToString());
	}

	public void RejectPart(PlatePartProductionInfo part, PlateProductionInfo plateProduction, Plate plateDat)
	{
		foreach (LopTemplateInfo item in _settings.TemplatesHelper.FindTemplates(LopTemplatesHelper.ActionReject, LopTemplatesHelper.TypeGapool))
		{
			WritePartLop(part, part.AmountToReject, item, plateProduction, plateDat);
		}
		if (_communicator != null)
		{
			PlatePartProductionInfo platePartProductionInfo = plateProduction.PlateParts.Find((PlatePartProductionInfo x) => x.PlatePart.PLATE_PART_NUMBER == part.PlatePart.PLATE_PART_NUMBER);
			if (platePartProductionInfo != null)
			{
				plateProduction.PlateParts.Remove(platePartProductionInfo);
				plateProduction.PlateParts.Add(part);
			}
			Task<bool> task = Task.Run(() => _communicator.BookReject(plateProduction, plateDat));
			Task.WaitAll(task);
			task = Task.Run(() => _communicator.BookSavedToCora());
			Task.WaitAll(task);
		}
		new DatReader<PartInfo>().UpdatePartDatReject(part);
	}
}
