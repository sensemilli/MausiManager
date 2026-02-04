using System.Collections.Generic;
using BendDataSourceModel.Enums;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SBendAdapterSet
{
	public int Id { get; set; }

	public List<SBendAdapterSection> Sections { get; set; }

	public List<SBendToolGap> Gaps { get; set; }

	public Vector3d InitializeInsertPoint { get; set; }

	public BendAdapterLocationType Type { get; set; }
}
