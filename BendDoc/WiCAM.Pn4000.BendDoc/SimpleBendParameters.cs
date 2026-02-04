using System;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;

namespace WiCAM.Pn4000.BendDoc;

internal class SimpleBendParameters : BendParametersBase
{
	public override FaceGroup ModifiedEntryFaceGroup
	{
		get
		{
			base._doc._modifiedModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, 0), out (FaceGroup, Model) value);
			return value.Item1;
		}
	}

	public override FaceGroup UnfoldFaceGroup
	{
		get
		{
			base._doc._unfoldModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, 0), out (FaceGroup, Model) value);
			return value.Item1;
		}
	}

	public override FaceGroup BendFaceGroup
	{
		get
		{
			base._doc._bendModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, 0), out (FaceGroup, Model) value);
			return value.Item1;
		}
	}

	public override Model ModifiedEntryFaceGroupModel
	{
		get
		{
			base._doc._modifiedModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, 0), out (FaceGroup, Model) value);
			return value.Item2;
		}
	}

	public override Model UnfoldFaceGroupModel
	{
		get
		{
			base._doc._unfoldModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, 0), out (FaceGroup, Model) value);
			return value.Item2;
		}
	}

	public override Model BendFaceGroupModel
	{
		get
		{
			base._doc._bendModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, 0), out (FaceGroup, Model) value);
			return value.Item2;
		}
	}

	public override double OriginalRadius => base.EntryFaceGroup.ConcaveAxis.Radius;

	public override double AngleAbs => base.EntryFaceGroup.ConvexAxis.OpeningAngle;

	public override double Length => base.EntryFaceGroup.ConcaveAxis.MaxParameter - base.EntryFaceGroup.ConcaveAxis.MinParameter;

	public SimpleBendParameters(FaceGroup entryFaceGroup, Doc3d doc)
		: base(entryFaceGroup, doc)
	{
	}

	internal override BendParametersBase Copy(Doc3d doc)
	{
		return new SimpleBendParameters(doc.EntryModel3D.GetFaceGroupById(base.EntryFaceGroup.ID), doc)
		{
			AngleSign = base.AngleSign,
			ManualBendDeduction = base.ManualBendDeduction,
			ManualRadius = base.ManualRadius,
			ToolRadius = base.ToolRadius
		};
	}
}
