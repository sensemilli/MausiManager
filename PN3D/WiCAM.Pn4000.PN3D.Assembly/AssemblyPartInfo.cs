using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class AssemblyPartInfo
{
	public int ID { get; set; }

	public GeometryType GeometryType { get; set; }

	public int Count { get; set; }

	public string Name { get; set; }

	public string AssemblyName { get; set; }

	public Model PartModel { get; set; }

	public int Status { get; set; }

	public string MaterialName { get; set; }

	public string OriginalMaterialName { get; set; }

	public List<string> Instances { get; set; } = new List<string>();

	public List<Matrix4d> Matrixes { get; set; } = new List<Matrix4d>();
}
