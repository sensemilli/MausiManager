namespace WiCAM.Pn4000.Contracts.PnCommands;

public interface IPnCommandsUnfold
{
	void PipetteFaceColorForVisibleFace(IPnCommandArg arg);

	void ReconstructFromFaceSelectFace(IPnCommandArg arg);

	void ReconstructIrregularBends(IPnCommandArg arg);

	void ReconstructBendsExperimental(IPnCommandArg arg);

	void ValidateGeometry(IPnCommandArg arg);

	void ValidateGeometryReset(IPnCommandArg arg);

	void UnfoldFromFaceSelectFace(IPnCommandArg arg);

	F2exeReturnCode UnfoldTube(IPnCommandArg arg);

	void ViewEntryModel(IPnCommandArg arg);

	void ViewModifiedModel(IPnCommandArg arg);

	void ViewUnfoldModel(IPnCommandArg arg);

	F2exeReturnCode UnfoldWithMessage(IPnCommandArg arg);

	void SetMaterial(IPnCommandArg arg);

	F2exeReturnCode Transfer2D(IPnCommandArg arg);

	void Transfer2DFromModifiedFace(IPnCommandArg arg, bool removeProjectionHoles);

	void ViewInputModel(IPnCommandArg arg);
}
