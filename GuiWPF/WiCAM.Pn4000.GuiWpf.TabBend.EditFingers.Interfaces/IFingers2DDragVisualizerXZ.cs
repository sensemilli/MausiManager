using System;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.PaintTools;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers.Interfaces;

internal interface IFingers2DDragVisualizerXZ
{
	bool IsDragging { get; }

	event Action RaiseColorModelParts;

	void ColorModelParts(IPaintTool painter);

	void MouseMove(object sender, MouseEventArgs e);

	void Start(PartRole selectedFinger);

	(double distanceOnPrimaryDir, double distanceOnSecondaryDir, Vector3d? otherFinger) Stop();
}
