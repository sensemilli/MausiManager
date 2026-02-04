using System;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabUnfold;

public interface IUnfoldViewModel : ITab
{
	bool IsActive { get; }

	ModelViewMode ModelTypeActive { get; }

	void ShowModel(ModelViewMode mode, bool setView = true, bool zoomExtend = true, bool animate = false);

	void UnfoldAnimation(Action finishedCallback);

	void Transfer2DFromSelectModifiedFace(bool removeProjectionHoles);

	void UnfoldFromSelectFace();

	void ReconstructFromSelectFace();

	void PipetteFaceColorForVisibleFace();

	void Dispose();
}
