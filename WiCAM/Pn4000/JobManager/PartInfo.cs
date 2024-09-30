using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;
using WiCAM.Pn4000.JobManager.Classes;

namespace WiCAM.Pn4000.JobManager;

[Serializable]
[DebuggerDisplay("Name={PART_NAME}  Order={PART_ORDER} Position={PART_POSITION} Remark={PART_REMARK}")]
public class PartInfo : ViewModelBase, IDatItem
{
	[Browsable(false)]
	public string Path;

	private int _PART_PRODUCED;

	private int _PART_STORNO;

	private int _PART_REJECT;

	[XmlIgnore]
	public JobInfo JobReference;

	[Obligatory]
	public int PART_NUMBER { get; set; }

	[Obligatory]
	public string PART_NAME { get; set; }

	public int PART_ARCHIV_NO { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_ARCHIV_NAME { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_ARCHIV_TREE_C2D { get; set; }

	[Obligatory]
	[XmlElement(IsNullable = false)]
	public string PART_ORDER { get; set; }

	[Obligatory]
	[XmlElement(IsNullable = false)]
	public string PART_POSITION { get; set; }

	[Obligatory]
	[XmlElement(IsNullable = false)]
	public string PART_REMARK { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_1 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_2 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_3 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_4 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_5 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_6 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_7 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_8 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_9 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_10 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_11 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_12 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_13 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_14 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_15 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_16 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_17 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_18 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_19 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_REMARK_20 { get; set; }

	public DateTime PART_DATE { get; set; }

	public int PART_PRIO { get; set; }

	public int PART_COM_ID { get; set; }

	public int PART_COL { get; set; }

	public int PART_URGENT { get; set; }

	[Obligatory]
	public int PART_ACOUNT { get; set; }

	public int PART_ACOUNT_MUST { get; set; }

	public int PART_ACOUNT_MAX { get; set; }

	public int PART_ACOUNT_DIFF { get; set; }

	[Obligatory]
	public int PART_PRODUCED
	{
		get
		{
			return _PART_PRODUCED;
		}
		set
		{
			_PART_PRODUCED = value;
			NotifyPropertyChanged("PART_PRODUCED");
		}
	}

	[Obligatory]
	public int PART_STORNO
	{
		get
		{
			return _PART_STORNO;
		}
		set
		{
			_PART_STORNO = value;
			NotifyPropertyChanged("PART_STORNO");
		}
	}

	[Obligatory]
	public int PART_REJECT
	{
		get
		{
			return _PART_REJECT;
		}
		set
		{
			_PART_REJECT = value;
			NotifyPropertyChanged("PART_REJECT");
		}
	}

	[InchConversion(true)]
	public double PART_SIZE_X { get; set; }

	[InchConversion(true)]
	public double PART_SIZE_Y { get; set; }

	[InchConversion(true)]
	public double PART_3D_SIZE_X { get; set; }

	[InchConversion(true)]
	public double PART_3D_SIZE_Y { get; set; }

	[InchConversion(true)]
	public double PART_3D_SIZE_Z { get; set; }

	public int PART_MACHINE_NO { get; set; }

	public string PART_MACHINE_NAME { get; set; }

	public int PART_MACH_NO_ORIG { get; set; }

	public string PART_MACH_NAME_ORIG { get; set; }

	public int PART_MATERIAL_NO { get; set; }

	public string PART_MATERIAL_NAME { get; set; }

	public int PART_MAT_NO_ORIG { get; set; }

	public string PART_MAT_NAME_ORIG { get; set; }

	public double PART_DENSITY { get; set; }

	[InchConversion(true)]
	public double PART_THICKNESS { get; set; }

	[InchConversion(true)]
	public double PART_THICKNESS_ORIG { get; set; }

	public double PART_TIME { get; set; }

	public double PART_TIME_NEST { get; set; }

	[DatKey("PART_TIME_NEST_+")]
	public double PART_TIME_NEST_PLUS { get; set; }

	public double PART_TIME_CLEANUP { get; set; }

	public double PART_TIME_BENDING { get; set; }

	public double PART_TIME_JOB { get; set; }

	public double PART_TIME_NEST_JOB { get; set; }

	[DatKey("PART_TIME_NEST_+_JOB")]
	public double PART_TIME_NEST_PLUS_JOB { get; set; }

	public double PART_TIME_BEND_JOB { get; set; }

	public double PART_COST_PUNCHING { get; set; }

	public double PART_COST_NIBBLING { get; set; }

	public double PART_COST_PIERCING { get; set; }

	public double PART_COST_MAIN_TIME { get; set; }

	public double PART_COST_ADD_TIME { get; set; }

	public double PART_COST_MATERIAL { get; set; }

	public double PART_COST_PROGRAM { get; set; }

	public double PART_COST_MANUFACT { get; set; }

	[DatKey("PART_COST_ADD_%_01 ")]
	public double PART_COST_ADD_PROC_01 { get; set; }

	[DatKey("PART_COST_ADD_%_02 ")]
	public double PART_COST_ADD_PROC_02 { get; set; }

	[DatKey("PART_COST_ADD_%_03 ")]
	public double PART_COST_ADD_PROC_03 { get; set; }

	[DatKey("PART_COST_ADD_%_04")]
	public double PART_COST_ADD_PROC_04 { get; set; }

	[DatKey("PART_COST_ADD_%_05")]
	public double PART_COST_ADD_PROC_05 { get; set; }

	[DatKey("PART_COST_ADD_%_06 ")]
	public double PART_COST_ADD_PROC_06 { get; set; }

	[DatKey("PART_COST_ADD_%_07")]
	public double PART_COST_ADD_PROC_07 { get; set; }

	[DatKey("PART_COST_ADD_%_08 ")]
	public double PART_COST_ADD_PROC_08 { get; set; }

	public double PART_COST_TOTAL { get; set; }

	public double PART_COST_PUN_NEST { get; set; }

	public double PART_COST_NIBB_NEST { get; set; }

	public double PART_COST_PIE_NEST { get; set; }

	public double PART_COST_MAIN_NEST { get; set; }

	public double PART_COST_ADD_NEST { get; set; }

	public double PART_COST_MAT_NEST { get; set; }

	public double PART_COST_PROG_NEST { get; set; }

	public double PART_COST_MANU_NEST { get; set; }

	[DatKey("PART_COST_A%01_NEST")]
	public double PART_COST_APROC01_NEST { get; set; }

	[DatKey("PART_COST_A%02_NEST")]
	public double PART_COST_APROC02_NEST { get; set; }

	[DatKey("PART_COST_A%03_NEST")]
	public double PART_COST_APROC03_NEST { get; set; }

	[DatKey("PART_COST_A%04_NEST")]
	public double PART_COST_APROC04_NEST { get; set; }

	[DatKey("PART_COST_A%05_NEST")]
	public double PART_COST_APROC05_NEST { get; set; }

	[DatKey("PART_COST_A%06_NEST")]
	public double PART_COST_APROC06_NEST { get; set; }

	[DatKey("PART_COST_A%07_NEST")]
	public double PART_COST_APROC07_NEST { get; set; }

	[DatKey("PART_COST_A%08_NEST")]
	public double PART_COST_APROC08_NEST { get; set; }

	public double PART_COST_TOT_NEST { get; set; }

	public double PART_LASER_LENGTH { get; set; }

	public double PART_PIERCING_PNT { get; set; }

	public double PART_PATH_CORR { get; set; }

	public double PART_ROT_INCREMENT { get; set; }

	public double PART_BEND_LINES { get; set; }

	public double PART_AREA_SQ_MM { get; set; }

	public double PART_AREA { get; set; }

	[DatKey("PART_AREA_NEST_+")]
	public double PART_AREA_NEST_PLUS { get; set; }

	public double PART_AREA_CN_SQ_MM { get; set; }

	public double PART_AREA_CN { get; set; }

	[DatKey("PART_AREA_CN_NEST_+")]
	public double PART_AREA_CN_NEST_PLUS { get; set; }

	public double PART_WGHT_CN { get; set; }

	[DatKey("PART_WGHT_CN_NEST_+")]
	public double PART_WGHT_CN_NEST_PLUS { get; set; }

	public double PART_WEIGHT { get; set; }

	[DatKey("PART_WEIGHT_NEST_+")]
	public double PART_WEIGHT_NEST_PLUS { get; set; }

	public double PART_AREA_RE_SQ_MM { get; set; }

	public double PART_AREA_RE { get; set; }

	[DatKey("PART_AREA_RE_NEST_+")]
	public double PART_AREA_RE_NEST_PLUS { get; set; }

	public double PART_WGHT_RE { get; set; }

	[DatKey("PART_WGHT_RE_NEST_+")]
	public double PART_WGHT_RE_NEST_PLUS { get; set; }

	public string PART_GEO_VIEW { get; set; }

	[Obligatory]
	[Browsable(false)]
	public string PART_PIXEL_VIEW_BG { get; set; }

	[Obligatory]
	[Browsable(false)]
	[XmlElement(IsNullable = false)]
	public string PART_PIXEL_VIEW_SL { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_1 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_2 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_3 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_4 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_5 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_6 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_7 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_8 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_9 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_10 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_11 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_12 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_13 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_14 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_15 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_16 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_17 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_18 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_19 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_20 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_21 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_22 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_23 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_24 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_25 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_26 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_27 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_28 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_29 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_30 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_31 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_32 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_33 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_34 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_35 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_36 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_37 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_38 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_39 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PART_HEADER_TXT_40 { get; set; }

	public int PART_TOOL_USED { get; set; }

	[Browsable(false)]
	public int Status
	{
		get
		{
			if (PART_ACOUNT <= PART_STORNO)
			{
				return 1;
			}
			if (PART_ACOUNT <= PART_PRODUCED)
			{
				return 3;
			}
			if (PART_ACOUNT <= PART_PRODUCED + PART_STORNO)
			{
				return 3;
			}
			return 2;
		}
	}

	public int PartAmountOnPlate { get; set; }

	public PartInfo()
	{
		new TypeInitializer().Initialize(this);
	}

	public void UpdateFrom(PartInfo part)
	{
		EnumerableHelper.UpdateItem(this, part);
	}

	public string HtmlPath()
	{
		return FindPath(this, "PART_" + PART_NUMBER + ".HTM");
	}

	public string PixelViewPath()
	{
		return FindPath(this, PART_PIXEL_VIEW_BG);
	}

	private static string FindPath(PartInfo part, string fileName)
	{
		string result = string.Empty;
		if (part != null && !string.IsNullOrEmpty(part.Path))
		{
			string directoryName = System.IO.Path.GetDirectoryName(part.Path);
			if (!string.IsNullOrEmpty(directoryName))
			{
				result = System.IO.Path.Combine(directoryName, fileName);
			}
		}
		return result;
	}
}
