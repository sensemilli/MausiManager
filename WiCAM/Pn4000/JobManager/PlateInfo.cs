using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;
using WiCAM.Pn4000.JobManager.Classes;

namespace WiCAM.Pn4000.JobManager;

[Serializable]
public class PlateInfo : ViewModelBase, IDatItem
{
	[Browsable(false)]
	public string Path;

	private int _PLATE_STATUS;

	private int _PLATE_PRODUCED;

	private int _PLATE_STORNO;

	[XmlIgnore]
	public JobInfo JobReference;

	[Obligatory]
	public int PLATE_NUMBER { get; set; }

	[Obligatory]
	[InchConversion(true)]
	public double PLATE_SIZE_X { get; set; }

	[Obligatory]
	[InchConversion(true)]
	public double PLATE_SIZE_Y { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_ORIENTATION { get; set; }

	[Obligatory]
	[XmlElement(IsNullable = false)]
	public string PLATE_NAME { get; set; }

	[Obligatory]
	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_IDENT { get; set; }

	[Obligatory]
	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_DESCRIP { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_TYPE { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_1 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_2 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_3 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_4 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_5 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_6 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_7 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_8 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_9 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_10 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_11 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_12 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_13 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_14 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_15 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_16 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_17 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_18 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_19 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REMARK_20 { get; set; }

	[Obligatory]
	[XmlElement(IsNullable = false)]
	public string PLATE_NAME_REST { get; set; }

	[Obligatory]
	public int PLATE_STORAGE_NO { get; set; }

	[Obligatory]
	public string REST_PLATE_REF_NO { get; set; }

	public int NUMBER_OF_PLATES { get; set; }

	[Obligatory]
	public int PLATE_STATUS
	{
		get
		{
			return _PLATE_STATUS;
		}
		set
		{
			_PLATE_STATUS = value;
			NotifyPropertyChanged("PLATE_STATUS");
		}
	}

	[Obligatory]
	public int PLATE_PRODUCED
	{
		get
		{
			return _PLATE_PRODUCED;
		}
		set
		{
			_PLATE_PRODUCED = value;
			NotifyPropertyChanged("PLATE_PRODUCED");
		}
	}

	public int PLATE_PRODUCED_ACT { get; set; }

	[Obligatory]
	public int PLATE_STORNO
	{
		get
		{
			return _PLATE_STORNO;
		}
		set
		{
			_PLATE_STORNO = value;
			NotifyPropertyChanged("PLATE_STORNO");
		}
	}

	public DateTime PLATE_ORDER_DATE_MIN { get; set; }

	public DateTime PLATE_ORDER_DATE_MAX { get; set; }

	public int PLATE_PRIORITY_MIN { get; set; }

	public int PLATE_PRIORITY_MAX { get; set; }

	public double PLATE_SIZE_X_MIN { get; set; }

	public double PLATE_SIZE_Y_MIN { get; set; }

	public double PLATE_SIZE_X_MIN_LLC { get; set; }

	public double PLATE_SIZE_Y_MIN_LLC { get; set; }

	[Obligatory]
	public double PLATE_TIME_WORK { get; set; }

	public double PLATE_TIME_REAL { get; set; }

	public double PLATE_TIME_1 { get; set; }

	public double PLATE_TIME_2 { get; set; }

	[Obligatory]
	public double PLATE_TIME_TOTAL { get; set; }

	public double PLATE_TIME_TOTAL_NP { get; set; }

	public double PLATE_TIME_CLEANUP { get; set; }

	public double PLATE_TIME_WORK_T { get; set; }

	public double PLATE_TIME_1_T { get; set; }

	public double PLATE_TIME_2_T { get; set; }

	public double PLATE_TIME_CLEANUP_T { get; set; }

	public double PLATE_COST_PUNCHING { get; set; }

	public double PLATE_COST_NIBBLING { get; set; }

	public double PLATE_COST_PIERCING { get; set; }

	public double PLATE_COST_MAIN_TIME { get; set; }

	public double PLATE_COST_ADD_TIME { get; set; }

	public double PLATE_COST_MATERIAL { get; set; }

	public double PLATE_COST_PROGRAM { get; set; }

	public double PLATE_COST_MANUFACT { get; set; }

	[DatKey("PLATE_COST_ADD_%_01")]
	public double PLATE_COST_ADD_PROC_01 { get; set; }

	[DatKey("PLATE_COST_ADD_%_02")]
	public double PLATE_COST_ADD_PROC_02 { get; set; }

	[DatKey("PLATE_COST_ADD_%_03")]
	public double PLATE_COST_ADD_PROC_03 { get; set; }

	[DatKey("PLATE_COST_ADD_%_04")]
	public double PLATE_COST_ADD_PROC_04 { get; set; }

	[DatKey("PLATE_COST_ADD_%_05")]
	public double PLATE_COST_ADD_PROC_05 { get; set; }

	[DatKey("PLATE_COST_ADD_%_06")]
	public double PLATE_COST_ADD_PROC_06 { get; set; }

	[DatKey("PLATE_COST_ADD_%_07")]
	public double PLATE_COST_ADD_PROC_07 { get; set; }

	[DatKey("PLATE_COST_ADD_%_08")]
	public double PLATE_COST_ADD_PROC_08 { get; set; }

	public double PLATE_COST_TOTAL { get; set; }

	public double PLATE_LASER_LENGTH { get; set; }

	public double PLATE_PIERCING_PNT { get; set; }

	public double PLATE_PATH_CORR { get; set; }

	public double PLATE_AREA_TOTAL { get; set; }

	public double PLATE_AREA_USED { get; set; }

	public double PLATE_AREA_PARTS { get; set; }

	public double PLATE_AREA_WASTE { get; set; }

	public double PLATE_AREA_REST { get; set; }

	public double PLATE_AREA_TOTAL_T { get; set; }

	public double PLATE_AREA_USED_T { get; set; }

	public double PLATE_AREA_PARTS_T { get; set; }

	public double PLATE_AREA_WASTE_T { get; set; }

	public double PLATE_AREA_REST_T { get; set; }

	public double PLATE_AREA_JOINT_L { get; set; }

	[DatKey("PLATE_AREA_WASTE_-")]
	public double PLATE_AREA_WASTE_MINUS { get; set; }

	[DatKey("PLATE_AREA_PARTS_%")]
	public double PLATE_AREA_PARTS_PROC { get; set; }

	[DatKey("PLATE_AREA_WASTE_%")]
	public double PLATE_AREA_WASTE_PROC { get; set; }

	public double PLATE_WEIGHT_TOTAL { get; set; }

	public double PLATE_WEIGHT_USED { get; set; }

	public double PLATE_WEIGHT_PARTS { get; set; }

	public double PLATE_WEIGHT_WASTE { get; set; }

	public double PLATE_WEIGHT_REST { get; set; }

	public double PLATE_WEIGHT_TOTAL_T { get; set; }

	public double PLATE_WEIGHT_USED_T { get; set; }

	public double PLATE_WEIGHT_PARTS_T { get; set; }

	public double PLATE_WEIGHT_WASTE_T { get; set; }

	public double PLATE_WEIGHT_REST_T { get; set; }

	public double PLATE_WEIGHT_JOINT_L { get; set; }

	[DatKey("PLATE_WEIGHT_WASTE_-")]
	public double PLATE_WEIGHT_WASTE_MINUS { get; set; }

	public double PLATE_AREA_RE_TOTAL { get; set; }

	public double PLATE_AREA_RE_USED { get; set; }

	public double PLATE_AREA_RE_PARTS { get; set; }

	public double PLATE_AREA_RE_WASTE { get; set; }

	public double PLATE_AREA_RE_REST { get; set; }

	public double PLATE_AR_RE_TOTAL_T { get; set; }

	public double PLATE_AR_RE_USED_T { get; set; }

	public double PLATE_AR_RE_PARTS_T { get; set; }

	public double PLATE_AR_RE_WASTE_T { get; set; }

	public double PLATE_AR_RE_REST_T { get; set; }

	public double PLATE_WGHT_RE_TOTAL { get; set; }

	public double PLATE_WGHT_RE_USED { get; set; }

	public double PLATE_WGHT_RE_PARTS { get; set; }

	public double PLATE_WGHT_RE_WASTE { get; set; }

	public double PLATE_WGHT_RE_REST { get; set; }

	public double PLATE_WG_RE_TOTAL_T { get; set; }

	public double PLATE_WG_RE_USED_T { get; set; }

	public double PLATE_WG_RE_PARTS_T { get; set; }

	public double PLATE_WG_RE_WASTE_T { get; set; }

	public double PLATE_WG_RE_REST_T { get; set; }

	public double PLATE_AREA_CN_TOTAL { get; set; }

	public double PLATE_AREA_CN_USED { get; set; }

	public double PLATE_AREA_CN_PARTS { get; set; }

	public double PLATE_AREA_CN_WASTE { get; set; }

	public double PLATE_AREA_CN_REST { get; set; }

	public double PLATE_AR_CN_TOTAL_T { get; set; }

	public double PLATE_AR_CN_USED_T { get; set; }

	public double PLATE_AR_CN_PARTS_T { get; set; }

	public double PLATE_AR_CN_WASTE_T { get; set; }

	public double PLATE_AR_CN_REST_T { get; set; }

	public double PLATE_WGHT_CN_TOTAL { get; set; }

	public double PLATE_WGHT_CN_USED { get; set; }

	public double PLATE_WGHT_CN_PARTS { get; set; }

	public double PLATE_WGHT_CN_WASTE { get; set; }

	public double PLATE_WGHT_CN_REST { get; set; }

	public double PLATE_WG_CN_TOTAL_T { get; set; }

	public double PLATE_WG_CN_USED_T { get; set; }

	public double PLATE_WG_CN_PARTS_T { get; set; }

	public double PLATE_WG_CN_WASTE_T { get; set; }

	public double PLATE_WG_CN_REST_T { get; set; }

	[Obligatory]
	[Browsable(false)]
	public int PLATE_MACHINE_NO { get; set; }

	[Obligatory]
	[Browsable(false)]
	public string PLATE_MACHINE_NAME { get; set; }

	public string PLATE_MACHINE_REMARK { get; set; }

	public int PLATE_MATERIAL_NO { get; set; }

	public string PLATE_MATERIAL_NAME { get; set; }

	public int PLATE_MAT_GRP_NO { get; set; }

	public string PLATE_MAT_GRP_NAME { get; set; }

	public double PLATE_DENSITY { get; set; }

	[InchConversion(true)]
	public double PLATE_THICKNESS { get; set; }

	[Obligatory]
	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_1 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_2 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_3 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_4 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_5 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_6 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_7 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_8 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_9 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_10 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_11 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_12 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_13 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_14 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_15 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_16 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_17 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_18 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_19 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_20 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_21 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_22 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_23 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_24 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_25 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_26 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_27 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_28 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_29 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_30 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_31 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_32 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_33 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_34 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_35 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_36 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_37 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_38 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_39 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_HEADER_TXT_40 { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_GEO_VIEW { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_GTX_VIEW { get; set; }

	[Obligatory]
	[Browsable(false)]
	[XmlElement(IsNullable = false)]
	public string PLATE_PIXEL_VIEW { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_TOOL_LIST { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_PALETTE_VIEW { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_SEND_NCDRUK { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_SEND_NCTAPE { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_SEND_NCZEIT { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_SEND_NCHUB { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_SEND_WPLAN { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_SEND_SPLAN { get; set; }

	[XmlElement(IsNullable = false)]
	public string PLATE_SEND_CADGEO { get; set; }

	public int PLATE_DIFF_PARTS { get; set; }

	public int PLATE_TOTAL_PARTS { get; set; }

	public int REST_PLATES_TOTAL { get; set; }

	public List<PlatePartInfo> PlateParts { get; set; } = new List<PlatePartInfo>();


	public PlateInfo()
	{
		new TypeInitializer().Initialize(this);
	}

	public int RestOfProduction()
	{
		return NUMBER_OF_PLATES - PLATE_PRODUCED - PLATE_STORNO;
	}

	public void UpdateFrom(PlateInfo plate)
	{
		UpdateItem(this, plate);
	}

	public static void UpdateItem(PlateInfo destinationItem, PlateInfo sourceItem)
	{
		PropertyInfo[] properties = typeof(PlateInfo).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanWrite)
			{
				propertyInfo.SetValue(destinationItem, propertyInfo.GetValue(sourceItem, null), null);
			}
		}
	}

	public string HtmlPath()
	{
		return FindPath(this, "PLATE_" + PLATE_NUMBER + ".HTM");
	}

	public string PixelViewPath()
	{
		return FindPath(this, PLATE_PIXEL_VIEW);
	}

	private static string FindPath(PlateInfo plate, string fileName)
	{
		string result = string.Empty;
		if (plate != null && !string.IsNullOrEmpty(plate.Path))
		{
			string directoryName = System.IO.Path.GetDirectoryName(plate.Path);
			if (!string.IsNullOrEmpty(directoryName))
			{
				result = System.IO.Path.Combine(directoryName, fileName);
			}
		}
		return result;
	}
}
