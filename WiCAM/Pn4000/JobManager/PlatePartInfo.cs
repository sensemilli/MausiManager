using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;
using WiCAM.Pn4000.JobManager.Classes;

namespace WiCAM.Pn4000.JobManager;

[Serializable]
public class PlatePartInfo : IDatItem
{
	[XmlIgnore]
	public PlateInfo Plate;

	[Browsable(false)]
	public List<PlatePartCollectiveOrder> CollectiveOrders = new List<PlatePartCollectiveOrder>();

	[Obligatory]
	public int PLATE_PART_NUMBER { get; set; }

	public int PLATE_PART_URGENT { get; set; }

	[Obligatory]
	public int PLATE_PART_AMOUNT { get; set; }

	public int PLATE_PART_AMOUNT_PL { get; set; }

	public int PLATE_PART_AMOUNT_AL { get; set; }

	public int PLATE_PART_AMOUNT_MI { get; set; }

	public int PLATE_PART_AMOUNT_MA { get; set; }

	public int PLATE_PART_AMOUNT_DI { get; set; }

	public int PLATE_PART_AMOUNT_SC { get; set; }

	[DatKey("PLATE_PART_AMOUNT_-")]
	public int PLATE_PART_AMOUNT_MINUS { get; set; }

	[DatKey("PLATE_PART_AMOUNT_+")]
	public int PLATE_PART_AMOUNT_PLUS { get; set; }

	[Obligatory]
	public int PLATE_PART_AMOUNT_RE { get; set; }

	public int PLATE_PART_PLT_FIRST { get; set; }

	public int PLATE_PART_PLT_LAST { get; set; }

	public double PLATE_PART_SIZE_X { get; set; }

	public double PLATE_PART_SIZE_Y { get; set; }

	public double PLATE_PART_3D_SIZE_X { get; set; }

	public double PLATE_PART_3D_SIZE_Y { get; set; }

	public double PLATE_PART_3D_SIZE_Z { get; set; }

	[Obligatory]
	public double PLATE_PART_TIME { get; set; }

	[Obligatory]
	[DatKey("PLATE_PART_TIME_+")]
	public double PLATE_PART_TIME_PLUS { get; set; }

	[Obligatory]
	public double PLATE_PART_TIME_REAL { get; set; }

	public double PLATE_PART_AREA { get; set; }

	[Obligatory]
	[DatKey("PLATE_PART_AREA_+")]
	public double PLATE_PART_AREA_PLUS { get; set; }

	public double PLATE_PART_AREA_P { get; set; }

	[DatKey("PLATE_PART_AREA_P+")]
	public double PLATE_PART_AREA_PPLUS { get; set; }

	public double PLATE_PART_AREA_T { get; set; }

	[DatKey("PLATE_PART_AREA_T+")]
	public double PLATE_PART_AREA_TPLUS { get; set; }

	public double PLATE_PART_WEIGHT { get; set; }

	[DatKey("PLATE_PART_WEIGHT_+")]
	public double PLATE_PART_WEIGHT_PLUS { get; set; }

	public double PLATE_PART_WEIGHT_P { get; set; }

	[DatKey("PLATE_PART_WEIGHT_P+")]
	public double PLATE_PART_WEIGHT_PPLUS { get; set; }

	public double PLATE_PART_WEIGHT_T { get; set; }

	[DatKey("PLATE_PART_WEIGHT_T+")]
	public double PLATE_PART_WEIGHT_TPLUS { get; set; }

	public double PLATE_PART_AREA_CN { get; set; }

	[DatKey("PLATE_PART_AREA_CN_+")]
	public double PLATE_PART_AREA_CN_PLUS { get; set; }

	public double PLATE_PART_WGHT_CN { get; set; }

	[DatKey("PLATE_PART_WGHT_CN_+")]
	public double PLATE_PART_WGHT_CN_PLUS { get; set; }

	public double PLATE_PART_AREA_RE { get; set; }

	[DatKey("PLATE_PART_AREA_RE_+")]
	public double PLATE_PART_AREA_RE_PLUS { get; set; }

	public double PLATE_PART_WGHT_RE { get; set; }

	[DatKey("PLATE_PART_WGHT_RE_+")]
	public double PLATE_PART_WGHT_RE_PLUS { get; set; }

	public double PLATE_PART_COST_PUN { get; set; }

	public double PLATE_PART_COST_NIBB { get; set; }

	public double PLATE_PART_COST_PIE { get; set; }

	public double PLATE_PART_COST_MAIN { get; set; }

	public double PLATE_PART_COST_ADD { get; set; }

	public double PLATE_PART_COST_MAT { get; set; }

	public double PLATE_PART_COST_PROG { get; set; }

	public double PLATE_PART_COST_MANU { get; set; }

	[DatKey("PLATE_PART_COST_A%01")]
	public double PLATE_PART_COST_APROC01 { get; set; }

	[DatKey("PLATE_PART_COST_A%02")]
	public double PLATE_PART_COST_APROC02 { get; set; }

	[DatKey("PLATE_PART_COST_A%03")]
	public double PLATE_PART_COST_APROC03 { get; set; }

	[DatKey("PLATE_PART_COST_A%04")]
	public double PLATE_PART_COST_APROC04 { get; set; }

	[DatKey("PLATE_PART_COST_A%05")]
	public double PLATE_PART_COST_APROC05 { get; set; }

	[DatKey("PLATE_PART_COST_A%06")]
	public double PLATE_PART_COST_APROC06 { get; set; }

	[DatKey("PLATE_PART_COST_A%07")]
	public double PLATE_PART_COST_APROC07 { get; set; }

	[DatKey("PLATE_PART_COST_A%08")]
	public double PLATE_PART_COST_APROC08 { get; set; }

	public double PLATE_PART_COST_TOT { get; set; }

	public double PLATE_PART_LASER_CN { get; set; }

	public int PLATE_PART_DISP_STOP { get; set; }

	public int PLATE_PART_DISP_1 { get; set; }

	public int PLATE_PART_DISP_2 { get; set; }

	public int PLATE_PART_DISP_LIFT { get; set; }

	[Browsable(false)]
	[XmlIgnore]
	public PartInfo Part { get; set; }

	[XmlIgnore]
	[DatKey("PLATE_PART_PRODUCED")]
	public int PartsProducedTotal { get; set; }

	[XmlIgnore]
	[DatKey("PLATE_PART_PRODUCED_GOOD")]
	public int PartsProducedGood { get; set; }

	[XmlIgnore]
	[DatKey("PLATE_PART_PRODUCED_BAD")]
	public int PartsProducedBad { get; set; }

	public PlatePartInfo()
	{
		new TypeInitializer().Initialize(this);
	}

	public void UpdateFrom(PlatePartInfo platePart)
	{
		EnumerableHelper.UpdateItem(this, platePart);
	}
}
