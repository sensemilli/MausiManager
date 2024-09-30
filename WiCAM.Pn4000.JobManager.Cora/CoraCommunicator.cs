using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WiCAM.Apis.CoraAdapters.Contracts;
using WiCAM.Apis.CoraAdapters.Extensions;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Jobdata.Classes;
using WiCAM.Pn4000.Jobdata.Helpers;
using WiCAM.Pn4000.Jobdata.Interfaces;

namespace WiCAM.Pn4000.JobManager.Cora;

internal class CoraCommunicator
{
	private class ProductionStatus
	{
		public const int Produce = 3;

		public const int Storno = 1;

		public const int Reject = 0;
	}

	private readonly IJobManagerSettings _settings;

	private readonly IBufferedLogger _logger;

	private readonly ICoraAdapter _coraAdapter;

	public CoraCommunicator(IJobManagerSettings settings, IBufferedLogger logger)
	{
		_settings = settings;
		_logger = logger;
		_coraAdapter = BuildAdapter(settings);
	}

	public async Task<bool> BookSavedToCora()
	{
		_logger.Info("--- JobManager_TO_CORA : BookSavedToCora");
		if (!new CoraHandler().CheckConnection(_settings.CoraUrl))
		{
			_logger.Info("--- JobManager_TO_CORA : BookSavedToCora : Cory is not running");
			return false;
		}
		CoraBackupManager manager = new CoraBackupManager();
		if (manager.CheckIsRunning())
		{
			_logger.Info("--- JobManager_TO_CORA : BookSavedToCora : cora update is running");
			return false;
		}
		List<SaveUnit> list = manager.ReadSaved();
		if (list.Count == 0)
		{
			_logger.Info("--- JobManager_TO_CORA : BookSavedToCora : no saved items");
			return true;
		}
		manager.StartRunning();
		JobDefaultReader reader = new JobDefaultReader();
		foreach (SaveUnit unit in list)
		{
			try
			{
				Plate plateDat = reader.ReadPlate(unit.Plate.Plate.Path);
				bool flag = false;
				switch (unit.ProductionStatus)
				{
				case 3:
					flag = await BookProduced(unit.Plate, plateDat);
					break;
				case 1:
					flag = await BookStorno(unit.Plate, plateDat);
					break;
				case 0:
					flag = await BookReject(unit.Plate, plateDat);
					break;
				}
				if (flag)
				{
					manager.BackUp(unit.Path);
				}
			}
			catch (Exception exception)
			{
				_logger.Exception(exception);
			}
		}
		manager.StopRunning();
		return true;
	}

	public async Task<bool> BookProduced(PlateProductionInfo plate, IPlate plateDat)
	{
		ICoraAdapter coraAdapter = await ControlAdapter(_settings, _coraAdapter, plate, plateDat, 3);
		if (coraAdapter == null)
		{
			_logger.Info("--- JobManager_TO_CORA : SaveProduced : " + plateDat.PlateHeaderTxt1);
			return false;
		}
		_logger.Info("--- JobManager_TO_CORA : BookProduced: " + plateDat.PlateHeaderTxt1);
		ModifyDataForProduced(plate, plateDat);
		await coraAdapter.SendPlateProducedFeedbackAsync(plateDat);
		return true;
	}

	public async Task<bool> BookStorno(PlateProductionInfo plate, IPlate plateDat)
	{
		ICoraAdapter coraAdapter = await ControlAdapter(_settings, _coraAdapter, plate, plateDat, 1);
		if (coraAdapter == null)
		{
			_logger.Info("--- JobManager_TO_CORA : SaveStorno: " + plateDat.PlateHeaderTxt1);
			return false;
		}
		_logger.Info("--- JobManager_TO_CORA : BookStorno: " + plateDat.PlateHeaderTxt1);
		ModifyPlateData(plate, plateDat);
		await coraAdapter.SendPlateStornoFeedbackAsync(plateDat);
		return true;
	}

	public async Task<bool> BookReject(PlateProductionInfo plate, IPlate plateDat)
	{
		ICoraAdapter coraAdapter = await ControlAdapter(_settings, _coraAdapter, plate, plateDat, 0);
		if (coraAdapter == null)
		{
			_logger.Info("--- JobManager_TO_CORA : SaveReject: " + plateDat.PlateHeaderTxt1);
			return false;
		}
		_logger.Info("--- JobManager_TO_CORA : BookReject: " + plateDat.PlateHeaderTxt1);
		ModifyPlateData(plate, plateDat);
		ModifyDataForReject(plate, plateDat);
		await coraAdapter.SendRejectFeedbackAsync(plateDat);
		return true;
	}

	private async Task<ICoraAdapter> ControlAdapter(IJobManagerSettings settings, ICoraAdapter coraAdapter, PlateProductionInfo plate, IPlate plateDat, int productionStatus)
	{
		_logger.Info("--- JobManager_TO_CORA : ControlAdapter");
		if (!new CoraHandler().CheckConnection(settings.CoraUrl))
		{
			coraAdapter = null;
			if (!plate.IsSavedForCora)
			{
				new CoraBackupManager().Save(plate, plateDat, productionStatus);
			}
			return coraAdapter;
		}
		if (coraAdapter == null)
		{
			coraAdapter = BuildAdapter(settings);
			await Task.Delay(1);
		}
		return coraAdapter;
	}

	private double ModifyPlateData(PlateProductionInfo plate, IPlate plateDat)
	{
		plateDat.ProductionUser = plate.ProductionData.UserName;
		plateDat.ProductionCharge = plate.ProductionData.ChargeNumber;
		plateDat.ProductionMachine = plate.ProductionData.Machine.Number.ToString();
		plateDat.PlateNameIdent = plate.ProductionData.ChargeNumber;
		plateDat.PlateTimeReal = plate.PlateTimeReal.ToString("0.00", CultureInfo.InvariantCulture);
		plateDat.PlateProducedAct = plate.AmountToProduce.ToString();
		double num = StringHelper.ToDouble(plateDat.PlateTimeReal);
		double result = num / (double)plate.AmountToProduce / StringHelper.ToDouble(plateDat.PlateTimeWork);
		TimeSpan value = TimeSpan.FromMinutes(num);
		DateTime dateTime = DateTime.Now.Subtract(value);
		plate.PlateFeedback = new JobManagerFeedback
		{
			StartTime = dateTime.ToString("yyyy.MM.dd HH.mm.ss"),
			TimeStamp = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss"),
			UserName = Environment.UserName
		};
		return result;
	}

	private void ModifyDataForReject(PlateProductionInfo plate, IPlate plateDat)
	{
		foreach (IPlatePart part in plateDat.PlatePartList)
		{
			PlatePartProductionInfo platePartProductionInfo = plate.PlateParts.Find((PlatePartProductionInfo x) => x.PlatePart.PLATE_PART_NUMBER == StringHelper.ToInt(part.PlatePartNumber));
			if (platePartProductionInfo != null)
			{
				part.PlatePartAmountRe = platePartProductionInfo.AmountToReject.ToString();
				continue;
			}
			_logger.Error("Part is not found in DAT file:");
			_logger.Error("{0}; {1}; {2}; {3}", part.PlatePartName, part.PlatePartOrder, part.PlatePartPosition, part.PlatePartRemark);
		}
	}

	private void ModifyDataForProduced(PlateProductionInfo plate, IPlate plateDat)
	{
		double num = ModifyPlateData(plate, plateDat);
		foreach (IPlatePart part in plateDat.PlatePartList)
		{
			PlatePartProductionInfo platePartProductionInfo = plate.PlateParts.Find((PlatePartProductionInfo x) => x.PlatePart.PLATE_PART_NUMBER == StringHelper.ToInt(part.PlatePartNumber));
			if (platePartProductionInfo != null)
			{
				double num2 = num * StringHelper.ToDouble(part.PlatePartTime);
				part.PlatePartTimeReal = num2.ToString("0.00", CultureInfo.InvariantCulture);
				part.PlatePartAmountRe = platePartProductionInfo.AmountToReject.ToString();
				continue;
			}
			_logger.Error("Part is not found in DAT file:");
			_logger.Error("{0}; {1}; {2}; {3}", part.PlatePartName, part.PlatePartOrder, part.PlatePartPosition, part.PlatePartRemark);
		}
	}

	private ICoraAdapter BuildAdapter(IJobManagerSettings settings)
	{
		ServiceCollection services = new ServiceCollection();
		services.AddCoraAdapter(settings.CoraUrl, "JobManager");
		ServiceProvider serviceProvider = services.BuildServiceProvider();
		Task.Run(async delegate
		{
			await serviceProvider.UseCoraAdapter();
		}).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter()
			.GetResult();
		return serviceProvider.GetService<ICoraAdapter>();
	}
}
