using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;
using WiCAM.Pn4000.JobManager.Classes;
using WiCAM.Pn4000.Machine;

namespace WiCAM.Pn4000.JobManager;

[Serializable]
public class ProductionInfo : ViewModelBase
{
	private bool _produceWithRejection;

	private string _chargeNumber;

	private MachineViewInfo _machine;

	private string _userName;

	private DateTime _timeStamp;

	public bool ProduceWithRejection
	{
		get
		{
			return _produceWithRejection;
		}
		set
		{
			_produceWithRejection = value;
			NotifyPropertyChanged("ProduceWithRejection");
		}
	}

	[DatKey("PRODUCTION_CHARGE")]
	public string ChargeNumber
	{
		get
		{
			return _chargeNumber;
		}
		set
		{
			_chargeNumber = value;
			NotifyPropertyChanged("ChargeNumber");
		}
	}

	[XmlIgnore]
	public MachineViewInfo Machine
	{
		get
		{
			return _machine;
		}
		set
		{
			_machine = value;
			NotifyPropertyChanged("Machine");
		}
	}

	[DatKey("PRODUCTION_MACHINE")]
	[XmlIgnore]
	public string MachineName
	{
		get
		{
			if (_machine == null)
			{
				return string.Empty;
			}
			return _machine.Name;
		}
	}

	[DatKey("PRODUCTION_USER")]
	public string UserName
	{
		get
		{
			return _userName;
		}
		set
		{
			_userName = value;
			NotifyPropertyChanged("UserName");
		}
	}

	public DateTime TimeStamp
	{
		get
		{
			return _timeStamp;
		}
		set
		{
			_timeStamp = value;
			NotifyPropertyChanged("TimeStamp");
		}
	}

	[DatKey("PRODUCTION_DATE")]
	[XmlIgnore]
	public string ProductionDate => _timeStamp.ToShortDateString();

	[DatKey("PRODUCTION_TIME")]
	[XmlIgnore]
	public string ProductionTime => _timeStamp.ToLongTimeString();

	[XmlIgnore]
	public List<string> Users { get; set; } = new List<string>();


	[XmlIgnore]
	public List<MachineViewInfo> Machines { get; set; } = new List<MachineViewInfo>();


	public ProductionInfo()
	{
		TimeStamp = DateTime.Now;
		new TypeInitializer().Initialize(this);
	}

	public ProductionInfo(List<string> users, List<MachineViewInfo> machines)
	{
		Users.AddRange(users);
		Machines.AddRange(machines);
		TimeStamp = DateTime.Now;
	}

	public string ToLopString()
	{
		string format = "# {0,-20} = {1}{2}";
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(WiCAM.Pn4000.Common.CS.Hash);
		StringHelper.AddWithCurrentCulture(stringBuilder, format, "PRODUCTION_MACHINE", MachineName, Environment.NewLine);
		StringHelper.AddWithCurrentCulture(stringBuilder, format, "PRODUCTION_USER", UserName, Environment.NewLine);
		string text = (string.IsNullOrEmpty(ChargeNumber) ? string.Empty : ChargeNumber);
		StringHelper.AddWithCurrentCulture(stringBuilder, format, "PRODUCTION_CHARGE", text, Environment.NewLine);
		StringHelper.AddWithCurrentCulture(stringBuilder, format, "PRODUCTION_TIME", TimeStamp.ToString("s"), Environment.NewLine);
		stringBuilder.AppendLine(WiCAM.Pn4000.Common.CS.Hash);
		return stringBuilder.ToString();
	}

	public void UpdateFromPlate(PlateInfo plate)
	{
		if (string.IsNullOrEmpty(ChargeNumber))
		{
			ChargeNumber = plate.PLATE_NAME_IDENT;
		}
		if (Machine == null)
		{
			Machine = Machines.Find((MachineViewInfo x) => x.Number == plate.PLATE_MACHINE_NO);
		}
		if (string.IsNullOrEmpty(UserName))
		{
			UserName = Environment.UserName;
		}
		if (TimeStamp.Date < DateTime.Today)
		{
			TimeStamp = DateTime.Now;
		}
	}
}
