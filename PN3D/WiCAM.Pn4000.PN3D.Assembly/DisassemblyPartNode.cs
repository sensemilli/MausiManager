using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class DisassemblyPartNode
{
	public DisassemblyPart Part { get; set; }

	public Matrix4d Transform { get; set; }

	public Matrix4d WorldMatrix => this.Transform * (this.Parent?.WorldMatrix ?? Matrix4d.Identity);

	public bool HiddenInAssembly { get; set; }

	public bool SuppressedInAssembly { get; set; }

	public DisassemblyPartNode Parent { get; set; }

	public List<DisassemblyPartNode> Children { get; set; }

	public Model ModelLowTesselation { get; set; }

	public string Path()
	{
		if (this.Parent != null)
		{
			return this.Parent.Path() + "\\" + this.Part?.OriginalAssemblyName;
		}
		return this.Part?.OriginalAssemblyName;
	}
}
