using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.Serialization.SerializationStructure.FingerStops;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SFingerStopPoint
{
	public Vector3d StopPoint { get; set; }

	public Vector3d StopPointRelativeToPart { get; set; }

	public SFingerStopCombination StopCombination { get; set; }

	public PartRole Finger { get; set; }

	public Vector3d BackEdgeCenterPoint { get; set; }

	public double RelativeEdgePosition { get; set; }

	public int Rating { get; set; }

	public SFingerPosition FingerPosition { get; set; }
}
