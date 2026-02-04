using System.Collections.Generic;
using BendDataSourceModel.Enums;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SBendToolSection
{
	public int Id { get; set; }

	public List<SBendTool> Tools { get; set; }

	public Vector3d InitializeInsertPoint { get; set; }

	public double CalculatedLength { get; set; }

	public BendToolLocationType Type { get; set; }
}
