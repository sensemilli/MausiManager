using System;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

[Serializable]
public class PartProductionInfo : ViewModelBase
{
	private int _RestOfProduction;

	private int _RestOfRejection;

	private int _AmountToReject;

	public PartInfo Part { get; set; }

	public int RestOfProduction
	{
		get
		{
			_RestOfProduction = Part.PART_ACOUNT - Part.PART_PRODUCED - Part.PART_STORNO;
			return _RestOfProduction;
		}
		set
		{
			_RestOfProduction = value;
			NotifyPropertyChanged("RestOfProduction");
		}
	}

	public int RestOfRejection
	{
		get
		{
			_RestOfRejection = Part.PART_ACOUNT - Part.PART_REJECT;
			return _RestOfRejection;
		}
		set
		{
			_RestOfRejection = value;
			NotifyPropertyChanged("RestOfRejection");
		}
	}

	public int AmountToReject
	{
		get
		{
			return _AmountToReject;
		}
		set
		{
			if (value <= RestOfRejection)
			{
				_AmountToReject = value;
				NotifyPropertyChanged("AmountToReject");
			}
		}
	}

	public PartProductionInfo()
	{
	}

	public PartProductionInfo(PartInfo part)
	{
		Part = part;
		RestOfProduction = Part.PART_ACOUNT - Part.PART_PRODUCED - Part.PART_STORNO;
		RestOfRejection = Part.PART_ACOUNT - Part.PART_REJECT;
	}
}
