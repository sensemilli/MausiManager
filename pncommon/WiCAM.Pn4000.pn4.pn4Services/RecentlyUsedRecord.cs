using System;
using System.Xml.Serialization;
using WiCAM.Pn4000.pn4.pn4Services.CADGEO;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class RecentlyUsedRecord
{
	public string FileName { get; set; }

	public string FullPath { get; set; }

	public string Type { get; set; }

	public int ArchiveID { get; set; }

	[XmlIgnore]
	public GadGeoDrawMultiOutput Drawer { get; set; }

	[XmlIgnore]
	public bool NeedPreview { get; set; }

	[XmlIgnore]
	public DateTime PreviewDate { get; set; }
}
