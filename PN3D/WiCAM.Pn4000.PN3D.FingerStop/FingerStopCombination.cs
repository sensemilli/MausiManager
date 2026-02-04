using System;
using System.Collections.Generic;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine.FingerStop;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.PN3D.FingerStop;

[Obsolete]
public class FingerStopCombination : IFingerStopCombination
{
	private List<string>? _faceNames;

	public StopCombinationType Type { get; init; }

	public List<string> FaceNames => this._faceNames ?? (this._faceNames = this.Type.ToFaceNames());

	public bool IsClamp => this.Type.IsClamp();

	public bool HasCylinder => this.Type.HasCylinder();

	public bool HasSupport => this.Type.HasSupport();

	public bool BoundingBoxPosition => this.Type.IsBoundingBoxPosition();

	public bool NoValidPositionFound => this.Type.HasFlag(StopCombinationType.NoValidPosition);
}
