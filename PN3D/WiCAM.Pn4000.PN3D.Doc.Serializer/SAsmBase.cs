using System;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer;

public class SAsmBase
{
	public int MajorVersion { get; set; }

	public int MinorVersion { get; set; }

	public virtual SAsmBase GetUpdatedAssembly()
	{
		throw new NotImplementedException();
	}
}
