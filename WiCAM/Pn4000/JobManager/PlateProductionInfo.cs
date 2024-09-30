using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager.Cora;
using WiCAM.Pn4000.JobManager.Enumerators;

namespace WiCAM.Pn4000.JobManager;

[Serializable]
public class PlateProductionInfo : ViewModelBase
{
	private int _amountToProduce;

	private int _amountStorno;

	private int _restOfProduction;

	public bool IsSavedForCora;

	public PlateInfo Plate { get; set; }

	public int AmountToProduce
	{
		get
		{
			return _amountToProduce;
		}
		set
		{
			if (Plate != null)
			{
				if (value <= Plate.RestOfProduction())
				{
					_amountToProduce = value;
					PlateTimeReal = Plate.PLATE_TIME_WORK * (double)AmountToProduce;
					UpdatePartsAmount(_amountToProduce);
				}
			}
			else
			{
				_amountToProduce = value;
				UpdatePartsAmount(_amountToProduce);
			}
			NotifyPropertyChanged("AmountToProduce");
		}
	}

	[XmlIgnore]
	public double PlateTimeReal
	{
		get
		{
			return Plate.PLATE_TIME_REAL;
		}
		set
		{
			Plate.PLATE_TIME_REAL = value;
			NotifyPropertyChanged("PlateTimeReal");
		}
	}

	public int AmountStorno
	{
		get
		{
			return _amountStorno;
		}
		set
		{
			_amountStorno = value;
			NotifyPropertyChanged("AmountStorno");
		}
	}

	public int RestOfProduction
	{
		get
		{
			return _restOfProduction;
		}
		set
		{
			_restOfProduction = value;
			NotifyPropertyChanged("RestOfProduction");
		}
	}

	public ProductionInfo ProductionData { get; set; }

	public List<PlatePartProductionInfo> PlateParts { get; set; } = new List<PlatePartProductionInfo>();


	[XmlIgnore]
	public JobInfo JobReference { get; private set; }

	[XmlIgnore]
	public JobManagerFeedback PlateFeedback { get; set; }

	public PlateProductionInfo()
	{
	}

	public PlateProductionInfo(PlateInfo plate, ProductionInfo productionInfo, PlateBookTime type)
		: this()
	{
		Plate = plate;
		AmountToProduce = Plate.NUMBER_OF_PLATES - Plate.PLATE_PRODUCED - Plate.PLATE_STORNO;
		ProductionData = productionInfo;
		ProductionData.UpdateFromPlate(plate);
		RestOfProduction = plate.RestOfProduction();
		if (type == PlateBookTime.PLATE_TIME_TOTAL)
		{
			Plate.PLATE_TIME_REAL = Plate.PLATE_TIME_TOTAL * (double)AmountToProduce;
		}
		else
		{
			Plate.PLATE_TIME_REAL = Plate.PLATE_TIME_WORK * (double)AmountToProduce;
		}
		JobReference = plate.JobReference;
		foreach (PlatePartInfo platePart in plate.PlateParts)
		{
			PlatePartProductionInfo platePartProductionInfo = new PlatePartProductionInfo(platePart);
			UpdatePartAmount(this, platePartProductionInfo);
			PlateParts.Add(platePartProductionInfo);
		}
	}

	public void ChangeStorno()
	{
		AmountStorno = AmountToProduce;
		AmountToProduce = 0;
	}

	public void UpdateAdditionalData()
	{
		foreach (PlatePartProductionInfo platePart in PlateParts)
		{
			platePart.PlatePart.PartsProducedTotal = platePart.PlatePart.PLATE_PART_AMOUNT * AmountToProduce;
			platePart.PlatePart.PartsProducedGood = platePart.PlatePart.PLATE_PART_AMOUNT * AmountToProduce - platePart.AmountToReject;
			platePart.PlatePart.PartsProducedBad = platePart.AmountToReject;
		}
	}

	private void UpdatePartsAmount(int plates)
	{
		foreach (PlatePartProductionInfo platePart in PlateParts)
		{
			UpdatePartAmount(this, platePart);
		}
	}

	private void UpdatePartAmount(PlateProductionInfo plate, PlatePartProductionInfo part)
	{
		part.RestOfProduction = part.PlatePart.PLATE_PART_AMOUNT * plate.AmountToProduce;
		part.AmountToProduce = part.PlatePart.PLATE_PART_AMOUNT * plate.AmountToProduce - part.AmountToReject;
		part.PlatePart.PartsProducedTotal = part.PlatePart.PLATE_PART_AMOUNT * plate.AmountToProduce;
		part.PlatePart.PartsProducedGood = part.PlatePart.PLATE_PART_AMOUNT * plate.AmountToProduce - part.AmountToReject;
		part.PlatePart.PartsProducedBad = part.AmountToReject;
		if (part.AmountToProduce > part.RestOfProduction)
		{
			part.AmountToProduce = part.RestOfProduction;
		}
	}
}
