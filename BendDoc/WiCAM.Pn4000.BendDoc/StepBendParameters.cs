using System;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;

namespace WiCAM.Pn4000.BendDoc;

internal class StepBendParameters : BendParametersBase
{
	private double _originalRadius;

	public override FaceGroup ModifiedEntryFaceGroup
	{
		get
		{
			base._doc._modifiedModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, this.StepIndex), out (FaceGroup, Model) value);
			return value.Item1;
		}
	}

	public override FaceGroup UnfoldFaceGroup
	{
		get
		{
			base._doc._unfoldModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, this.StepIndex), out (FaceGroup, Model) value);
			return value.Item1;
		}
	}

	public override FaceGroup BendFaceGroup
	{
		get
		{
			base._doc._bendModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, this.StepIndex), out (FaceGroup, Model) value);
			return value.Item1;
		}
	}

	public override Model ModifiedEntryFaceGroupModel
	{
		get
		{
			base._doc._modifiedModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, this.StepIndex), out (FaceGroup, Model) value);
			return value.Item2;
		}
	}

	public override Model UnfoldFaceGroupModel
	{
		get
		{
			base._doc._unfoldModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, this.StepIndex), out (FaceGroup, Model) value);
			return value.Item2;
		}
	}

	public override Model BendFaceGroupModel
	{
		get
		{
			base._doc._bendModelLookup.TryGetValue(new Tuple<int, int>(base.EntryFaceGroup.ID, this.StepIndex), out (FaceGroup, Model) value);
			return value.Item2;
		}
	}

	public override double OriginalRadius => this._originalRadius;

	public override double AngleAbs => this.UnfoldFaceGroup.ConvexAxis.OpeningAngle;

	public override double Length
	{
		get
		{
			if (this.UnfoldFaceGroup == null)
			{
				return base.EntryFaceGroup.ConcaveAxis.MaxParameter - base.EntryFaceGroup.ConcaveAxis.MinParameter;
			}
			return this.UnfoldFaceGroup.ConcaveAxis.MaxParameter - this.UnfoldFaceGroup.ConcaveAxis.MinParameter;
		}
	}

	public int StepIndex { get; }

	public override bool IsStepBend => true;

	public StepBendParameters(FaceGroup entryFaceGroup, int stepIndex, double radius, Doc3d doc)
		: base(entryFaceGroup, doc)
	{
		this.StepIndex = stepIndex;
		this._originalRadius = radius;
	}

	internal override BendParametersBase Copy(Doc3d doc)
	{
		return new StepBendParameters(doc.EntryModel3D.GetFaceGroupById(base.EntryFaceGroup.ID), this.StepIndex, this.OriginalRadius, doc);
	}
}
