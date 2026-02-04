using System;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Unfold;

public interface IModelForPnStdPreparationWrapper
{
	event Action OnTransfer2d;

	F2exeReturnCode Apply2D<T>(IDoc3d doc, Model model, Face face, Model faceModel, bool removeProjectionHoles, bool includeZeroBorders) where T : IModelForPnStdPreparation;
}
