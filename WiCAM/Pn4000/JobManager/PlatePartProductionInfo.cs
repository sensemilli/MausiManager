using System;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

[Serializable]
public class PlatePartProductionInfo : ViewModelBase
{
	private int _amountToProduce;

	private int _amountToStorno;

	private int _amountToReject;

	private int _restOfProduction;

	private int _restOfRejection;

	public PlatePartInfo PlatePart { get; set; }

	public int AmountToProduce
	{
		get
		{
			return _amountToProduce;
		}
		set
		{
			_amountToProduce = value;
			NotifyPropertyChanged("AmountToProduce");
		}
	}

	public int AmountToStorno
	{
		get
		{
			return _amountToStorno;
		}
		set
		{
			_amountToStorno = value;
			NotifyPropertyChanged("AmountToStorno");
		}
	}

	public int AmountToReject
	{
		get
		{
			return _amountToReject;
		}
		set
		{
			if (value <= RestOfRejection)
			{
				_amountToReject = value;
				PlatePart.PLATE_PART_AMOUNT_RE = value;
				RestOfRejection = FindRestOfRejection(PlatePart);
				NotifyPropertyChanged("AmountToReject");
			}
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

	public int RestOfRejection
	{
		get
		{
			return _restOfRejection;
		}
		set
		{
			_restOfRejection = value;
			NotifyPropertyChanged("RestOfRejection");
		}
	}

	public PlatePartProductionInfo()
	{
	}

	public PlatePartProductionInfo(PlatePartInfo platePart)
	{
		PlatePart = platePart;
		AmountToProduce = platePart.PLATE_PART_AMOUNT;
		RestOfProduction = FindRestOfProduction(platePart);
		RestOfRejection = FindRestOfRejection(platePart);
	}

	private int FindRestOfProduction(PlatePartInfo part)
	{
		return part.Plate.NUMBER_OF_PLATES * part.PLATE_PART_AMOUNT - part.Part.PART_PRODUCED;
	}

	private int FindRestOfRejection(PlatePartInfo part)
	{
		return part.Plate.NUMBER_OF_PLATES * part.PLATE_PART_AMOUNT - part.Part.PART_REJECT;
	}
}
