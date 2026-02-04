using System;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.ToolCalculation.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools.Interafces;

internal interface IModelDragVisualizerX
{
	bool IsDragging { get; }

	ISnapPoint? SnapPrimary { get; }

	ISnapPoint? SnapSecondary { get; }

	Vector3d ModelOrigin { get; }

	event Action<double> DistanceChanged;

	void Start(Model model, Model referenceSystemModel, ICollection<IRange> blockedIntervals, ICollection<ISnapPoint> snapPoints);

	void SetSnapPoints(ICollection<IRange> blockedIntervals, ICollection<ISnapPoint> snapPoints);

	double Stop();

	void Drag(Vector2f pos);

	void SnapPointVisualization();
}
