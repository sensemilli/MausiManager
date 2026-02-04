using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;

namespace WiCAM.Pn4000.BendDoc;

internal class AuxiliaryBendParameters : BendParametersBase
{
	public override FaceGroup ModifiedEntryFaceGroup { get; }

	public override FaceGroup UnfoldFaceGroup { get; }

	public override FaceGroup BendFaceGroup { get; }

	public override Model ModifiedEntryFaceGroupModel { get; }

	public override Model UnfoldFaceGroupModel { get; }

	public override Model BendFaceGroupModel { get; }

	public override double OriginalRadius { get; }

	public override double AngleAbs { get; }

	public override double Length { get; }

	public override bool IsAuxillaryBend => true;

	public AuxiliaryBendParameters(FaceGroup entryFlatFaceGroup, Doc3d doc)
		: base(entryFlatFaceGroup, doc)
	{
	}

	internal override BendParametersBase Copy(Doc3d doc)
	{
		return new AuxiliaryBendParameters(doc.EntryModel3D.GetFaceGroupById(base.EntryFaceGroup.ID), doc);
	}
}
