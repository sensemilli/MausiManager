using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;
using WiCAM.Pn4000.JobManager.Classes;
using WiCAM.Pn4000.JobManager.Helpers;

namespace WiCAM.Pn4000.JobManager;

[Serializable]
public class JobInfo : ViewModelBase, IDatItem
{
	private static string _wrongStatusFormat;

	[Browsable(false)]
	public string Path;

	private double _JOB_PROGRESS;

	private int _Status;

	private double _ProductionProgress;

	private double _StornoProgress;

	private string _jobStatusMessage;

	[Obligatory]
	[XmlElement(IsNullable = false)]
	public string JOB_DATA_1 { get; set; }

	public double JOB_PROGRESS
	{
		get
		{
			return _JOB_PROGRESS;
		}
		set
		{
			_JOB_PROGRESS = value;
			NotifyPropertyChanged("JOB_PROGRESS");
		}
	}

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_2 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_3 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_4 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_5 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_6 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_7 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_8 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_9 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_10 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_11 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_12 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_13 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_14 { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_DATA_15 { get; set; }

	[Obligatory]
	public int JOB_MACHINE_NO { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_MACHINE_NAME { get; set; }

	[Obligatory]
	public int JOB_MATERIAL_NO { get; set; }

	[XmlElement(IsNullable = false)]
	public string JOB_MATERIAL_NAME { get; set; }

	public double JOB_DENSITY { get; set; }

	[InchConversion(true)]
	public double JOB_THICKNESS { get; set; }

	[Obligatory]
	[DatKey(" DATE ")]
	public DateTime DATE { get; set; }

	[Obligatory]
	[DatKey(" TIME ")]
	public TimeSpan TIME { get; set; }

	[XmlElement(IsNullable = false)]
	public string USER { get; set; }

	[XmlElement(IsNullable = false)]
	public string USER_LOGIN { get; set; }

	public int AMOUNT_DIFF_PLATES { get; set; }

	public int AMOUNT_DIFF_PARTS { get; set; }

	public int TOTAL_AMOUNT_PLATES { get; set; }

	public int TOTAL_AMOUNT_PARTS { get; set; }

	public DateTime JOB_ORDER_DATE_MIN { get; set; }

	public DateTime JOB_ORDER_DATE_MAX { get; set; }

	[Obligatory]
	public double JOB_TIME_WORK { get; set; }

	[Obligatory]
	public double JOB_TIME_TOTAL { get; set; }

	public double JOB_AREA_TOTAL { get; set; }

	public double JOB_AREA_USED { get; set; }

	public double JOB_AREA_PARTS { get; set; }

	public double JOB_AREA_WASTE { get; set; }

	public double JOB_AREA_REST { get; set; }

	[DatKey("JOB_AREA_PARTS_%")]
	public double JOB_AREA_PARTS_PROC { get; set; }

	[DatKey("JOB_AREA_WASTE_%")]
	public double JOB_AREA_WASTE_PROC { get; set; }

	public double JOB_WEIGHT_TOTAL { get; set; }

	public double JOB_WEIGHT_USED { get; set; }

	public double JOB_WEIGHT_PARTS { get; set; }

	public double JOB_WEIGHT_WASTE { get; set; }

	public double JOB_WEIGHT_REST { get; set; }

	public double JOB_AREA_RE_TOTAL { get; set; }

	public double JOB_AREA_RE_USED { get; set; }

	public double JOB_AREA_RE_PARTS { get; set; }

	public double JOB_AREA_RE_WASTE { get; set; }

	public double JOB_AREA_RE_REST { get; set; }

	[DatKey("JOB_AREA_RE_PARTS_%")]
	public double JOB_AREA_RE_PARTS_PROC { get; set; }

	[DatKey("JOB_AREA_RE_WASTE_%")]
	public double JOB_AREA_RE_WASTE_PROC { get; set; }

	public double JOB_WGHT_RE_TOTAL { get; set; }

	public double JOB_WGHT_RE_USED { get; set; }

	public double JOB_WGHT_RE_PARTS { get; set; }

	public double JOB_WGHT_RE_WASTE { get; set; }

	public double JOB_WGHT_RE_REST { get; set; }

	public double JOB_COST_PUNCHING { get; set; }

	public double JOB_COST_NIBBLING { get; set; }

	public double JOB_COST_PIERCING { get; set; }

	public double JOB_COST_MAIN_TIME { get; set; }

	public double JOB_COST_ADD_TIME { get; set; }

	public double JOB_COST_MATERIAL { get; set; }

	public double JOB_COST_PROGRAM { get; set; }

	public double JOB_COST_MANUFACT { get; set; }

	[DatKey("JOB_COST_ADD_%_01")]
	public double JOB_COST_ADD_PROC_01 { get; set; }

	[DatKey("JOB_COST_ADD_%_02")]
	public double JOB_COST_ADD_PROC_02 { get; set; }

	[DatKey("JOB_COST_ADD_%_03")]
	public double JOB_COST_ADD_PROC_03 { get; set; }

	[DatKey("JOB_COST_ADD_%_04")]
	public double JOB_COST_ADD_PROC_04 { get; set; }

	[DatKey("JOB_COST_ADD_%_05")]
	public double JOB_COST_ADD_PROC_05 { get; set; }

	[DatKey("JOB_COST_ADD_%_06")]
	public double JOB_COST_ADD_PROC_06 { get; set; }

	[DatKey("JOB_COST_ADD_%_07")]
	public double JOB_COST_ADD_PROC_07 { get; set; }

	[DatKey("JOB_COST_ADD_%_08")]
	public double JOB_COST_ADD_PROC_08 { get; set; }

	public double JOB_COST_TOTAL { get; set; }

	public double JOB_LASER_LENGTH { get; set; }

	public int JOB_PIERCING_PNT { get; set; }

	public int JOB_PATH_CORR { get; set; }

	public int AMOUNT_DIFF_PLT_SIZE { get; set; }

	[Browsable(false)]
	public int Status
	{
		get
		{
			return _Status;
		}
		set
		{
			_Status = value;
			NotifyPropertyChanged("Status");
		}
	}

	[Browsable(false)]
	public double ProductionProgress
	{
		get
		{
			return _ProductionProgress;
		}
		set
		{
			_ProductionProgress = value;
			NotifyPropertyChanged("ProductionProgress");
		}
	}

	[Browsable(false)]
	public double StornoProgress
	{
		get
		{
			return _StornoProgress;
		}
		set
		{
			_StornoProgress = value;
			NotifyPropertyChanged("StornoProgress");
		}
	}

	[Browsable(false)]
	public string JobStatusMessage
	{
		get
		{
			return _jobStatusMessage;
		}
		set
		{
			_jobStatusMessage = value;
			NotifyPropertyChanged("JobStatusMessage");
		}
	}

	[Browsable(false)]
	public List<PlateInfo> Plates { get; set; } = new List<PlateInfo>();


	[Browsable(false)]
	public List<PartInfo> Parts { get; set; } = new List<PartInfo>();


	public JobInfo()
	{
		_wrongStatusFormat = PlateWrongStatusConverter.BuildShortFormat(FindStringResource("MsgWrongPlateStatus"));
		new TypeInitializer().Initialize(this);
	}

	private string FindStringResource(string key)
	{
		object obj = Application.Current.FindResource(key);
		if (obj != null)
		{
			return obj.ToString();
		}
		return key;
	}

	public void CheckStatus()
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (PlateInfo plate in Plates)
		{
			if (plate.PLATE_TIME_WORK <= 0.0)
			{
				plate.PLATE_TIME_WORK = 1.0;
			}
			num += (double)plate.NUMBER_OF_PLATES * plate.PLATE_TIME_WORK;
			num2 += (double)plate.PLATE_PRODUCED * plate.PLATE_TIME_WORK;
			num3 += (double)plate.PLATE_STORNO * plate.PLATE_TIME_WORK;
			if (plate.PLATE_STATUS == 0)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, _wrongStatusFormat, plate.PLATE_HEADER_TXT_1);
				stringBuilder.AppendLine();
			}
		}
		JobStatusMessage = stringBuilder.ToString();
		JOB_PROGRESS = Math.Round((num2 + num3) / num * 100.0, 0);
		ProductionProgress = num2 / num;
		StornoProgress = num3 / num;
		if (Math.Abs(num2 - num) < 0.1)
		{
			Status = 3;
		}
		else if (Math.Abs(num3 - num) < 0.1)
		{
			Status = 1;
		}
		else if (Math.Abs(num3 + num2 - num) < 0.1)
		{
			Status = 3;
		}
		else
		{
			Status = 2;
		}
	}

	public PlateInfo FirstWithPart(int partNumber)
	{
		return Plates.Find((PlateInfo x) => x.PlateParts.Find((PlatePartInfo y) => y.PLATE_PART_NUMBER == partNumber) != null);
	}
}
