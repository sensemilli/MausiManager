using System;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class SpatialImportProgress
{
	public bool IsAssembly { get; set; }

	public DateTime Timestamp { get; set; }

	public int? TotalParts { get; set; }

	public int? PartId { get; set; }

	public int Faces { get; set; }

	public int Triangles { get; set; }
}
