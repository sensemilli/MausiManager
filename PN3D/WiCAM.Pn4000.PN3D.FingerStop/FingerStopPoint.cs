using System;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.FingerStops;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.PN3D.FingerStop;

[Serializable]
[Obsolete]
public class FingerStopPoint : IFingerStopPointInternal, IFingerStopPoint
{
	public Vector3d StopPoint { get; set; }

	public Vector3d StopPointRelativeToPart { get; set; }

	public IFingerStopCombination StopCombination { get; set; }

	public PartRole Finger { get; }

	public Vector3d BackEdgeCenterPoint { get; set; }

	public double RelativeEdgePosition { get; set; }

	public bool SavetyDistanceOverToolsUsed { get; set; }

	public int Rating { get; set; }

	public FingerPosition FingerPosition { get; set; }

	public FingerStopPoint(Vector3d stopPoint, Vector3d stopPointRelativeToPart, PartRole finger, IFingerStopCombination stopCombinationCombination, FingerPosition fingerPosition, int rating)
	{
		this.StopPoint = stopPoint;
		this.StopPointRelativeToPart = stopPointRelativeToPart;
		this.StopCombination = stopCombinationCombination;
		this.Finger = finger;
		this.Rating = rating;
		this.FingerPosition = fingerPosition;
	//	base._002Ector();
	}
}
