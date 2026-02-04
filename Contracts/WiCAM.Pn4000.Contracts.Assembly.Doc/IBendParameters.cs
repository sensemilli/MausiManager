using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.BendDataBase;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IBendParameters
{
	int FaceGroupId { get; }

	FaceGroup ModifiedEntryFaceGroup { get; }

	FaceGroup EntryFaceGroup { get; }

	FaceGroup UnfoldFaceGroup { get; }

	FaceGroup BendFaceGroup { get; }

	Model ModifiedEntryFaceGroupModel { get; }

	Model UnfoldFaceGroupModel { get; }

	Model BendFaceGroupModel { get; }

	double OriginalRadius { get; }

	double? ManualRadius { get; }

	double? ToolRadius { get; }

	double FinalRadius { get; }

	double AngleAbs { get; }

	int AngleSign { get; }

	double Angle { get; }

	double Length { get; }

	double DinLength { get; }

	double? ManualBendDeduction { get; }

	double FinalBendDeduction { get; }

	double BendingAllowance { get; }

	double KFactor { get; }

	double? SpringBack { get; }

	BendTableReturnValues KFactorAlgorithm { get; }

	Line BendLineUnfoldModel { get; }

	Line BendLineBendModel { get; }

	bool IsStepBend { get; }

	bool IsHemBend { get; }

	bool IsAuxillaryBend { get; }

	(FaceGroup fg, Model model) ModelFaceGroup(UiModelType uiModelType);
}
