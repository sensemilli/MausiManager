using System;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendTable;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Unfold;

namespace WiCAM.Pn4000.BendDoc;

internal abstract class BendParametersBase : IBendParameters
{
	protected readonly FaceGroup _entryFaceGroup;

	protected readonly Doc3d _doc;

	protected readonly General3DConfig _general3DConfig;

	public int FaceGroupId => this.EntryFaceGroup.ID;

	public FaceGroup EntryFaceGroup { get; }

	public abstract FaceGroup ModifiedEntryFaceGroup { get; }

	public abstract FaceGroup UnfoldFaceGroup { get; }

	public abstract FaceGroup BendFaceGroup { get; }

	public abstract Model ModifiedEntryFaceGroupModel { get; }

	public abstract Model UnfoldFaceGroupModel { get; }

	public abstract Model BendFaceGroupModel { get; }

	public abstract double OriginalRadius { get; }

	public double? ManualRadius { get; set; }

	public double? ToolRadius { get; set; }

	public double FinalRadius => this.ToolRadius ?? this.ManualRadius ?? this.OriginalRadius;

	public abstract double AngleAbs { get; }

	public int AngleSign { get; set; } = 1;

	public double Angle => this.AngleAbs * (double)this.AngleSign;

	public abstract double Length { get; }

	public double DinLength => BendDataCalculator.DinLengthFromKFactor(this._doc.Thickness, this.AngleAbs, this.FinalRadius, this.KFactor);

	public double? ManualBendDeduction { get; set; }

	public double FinalBendDeduction => this.ManualBendDeduction ?? BendDataCalculator.BendDeductionFromKFactor(this._doc.Thickness, this.AngleAbs, this.FinalRadius, this.KFactor);

	public double BendingAllowance => BendDataCalculator.BendAllowanceFromKFactor(this._doc.Thickness, this.AngleAbs, this.FinalRadius, this.KFactor);

	public double KFactor { get; set; }

	public double? SpringBack { get; private set; }

	public BendTableReturnValues KFactorAlgorithm { get; set; }

	public Line BendLineUnfoldModel
	{
		get
		{
			FaceGroup unfoldFaceGroup = this.UnfoldFaceGroup;
			Matrix4d worldMatrix = this.UnfoldFaceGroupModel.WorldMatrix;
			Vector3d v = unfoldFaceGroup.ConcaveAxis.Origin;
			Vector3d v2 = unfoldFaceGroup.ConcaveAxis.Direction;
			worldMatrix.TransformInPlace(ref v);
			worldMatrix.TransformNormalInPlace(ref v2);
			return new Line(v + v2 * unfoldFaceGroup.ConcaveAxis.MinParameter, v2 * (unfoldFaceGroup.ConcaveAxis.MaxParameter - unfoldFaceGroup.ConcaveAxis.MinParameter));
		}
	}

	public Line BendLineBendModel
	{
		get
		{
			FaceGroup bendFaceGroup = this.BendFaceGroup;
			Matrix4d worldMatrix = this.BendFaceGroupModel.WorldMatrix;
			Vector3d v = bendFaceGroup.ConcaveAxis.Origin;
			Vector3d v2 = bendFaceGroup.ConcaveAxis.Direction;
			worldMatrix.TransformInPlace(ref v);
			worldMatrix.TransformNormalInPlace(ref v2);
			return new Line(v + v2 * bendFaceGroup.ConcaveAxis.MinParameter, v2 * (bendFaceGroup.ConcaveAxis.MaxParameter - bendFaceGroup.ConcaveAxis.MinParameter));
		}
	}

	public virtual bool IsStepBend => false;

	public virtual bool IsHemBend { get; set; }

	public virtual bool IsAuxillaryBend => false;

	protected BendParametersBase(FaceGroup entryFaceGroup, Doc3d doc)
	{
		this._doc = doc;
		this._general3DConfig = doc.ConfigProvider.InjectOrCreate<General3DConfig>();
		this.EntryFaceGroup = entryFaceGroup;
	}

	public (FaceGroup fg, Model model) ModelFaceGroup(UiModelType uiModelType)
	{
		return uiModelType switch
		{
			UiModelType.Unfold => (fg: this.UnfoldFaceGroup, model: this.UnfoldFaceGroupModel), 
			UiModelType.Bend => (fg: this.BendFaceGroup, model: this.BendFaceGroupModel), 
			UiModelType.Entry => (fg: this.EntryFaceGroup, model: this._doc.EntryModel3D), 
			UiModelType.ModifiedEntry => (fg: this.ModifiedEntryFaceGroup, model: this.ModifiedEntryFaceGroupModel), 
			_ => throw new Exception("Invalid Model"), 
		};
	}

	public void UpdateData(IToolProfile? upperTool, IToolProfile? lowerTool)
	{
		(double KFactor, double? SpringBack, BendTableReturnValues KFactorAlgorithm, double? ToolRadius) tuple = this.CalculateData(upperTool, lowerTool);
		double item = tuple.KFactor;
		double? item2 = tuple.SpringBack;
		BendTableReturnValues item3 = tuple.KFactorAlgorithm;
		double? item4 = tuple.ToolRadius;
		this.KFactor = item;
		this.SpringBack = item2;
		this.KFactorAlgorithm = item3;
		this.ToolRadius = item4;
	}

	public (double KFactor, double? SpringBack, BendTableReturnValues KFactorAlgorithm, double? ToolRadius) CalculateData(IToolProfile? upperTool, IToolProfile? lowerTool)
	{
		double? item = null;
		double item2;
		double? item3;
		BendTableReturnValues item4;
		if (this.ManualBendDeduction.HasValue)
		{
			item2 = BendDataCalculator.KFactorFromBendDeduction(this._doc.Thickness, this.AngleAbs, this.FinalRadius, this.ManualBendDeduction.Value);
			item3 = null;
			item4 = BendTableReturnValues.USER_DEFINED;
		}
		else if (this._doc.EntryModel3D.PartInfo.PartType.HasFlag(PartType.Tube))
		{
			item2 = this._doc.ConfigProvider.InjectOrCreate<General3DConfig>().P3D_Default_Tube_KFactor;
			item4 = BendTableReturnValues.EXACT_VALUE;
			item3 = null;
		}
		else
		{
			bool isMachineSpecific;
			IBendTable applicableBendTable = this._doc.GetApplicableBendTable(out isMachineSpecific);
			bool ignoreBendTable = applicableBendTable.GetEntryCount() == 0;
			if (isMachineSpecific)
			{
				string algorithm;
				double resultRadius;
				BendTableReturnValues result;
				double? springBack;
				double kFactorByTools = applicableBendTable.GetKFactorByTools(this._doc.Material, this._doc.Thickness, this.AngleAbs * 180.0 / Math.PI, this.ManualRadius ?? this.OriginalRadius, upperTool, lowerTool, ignoreBendTable, (double t, double r) => Unfold.GetRadiusTolerance(t, r, this._general3DConfig), out algorithm, out resultRadius, out result, out springBack);
				item3 = springBack;
				item4 = result;
				item = resultRadius;
				item2 = kFactorByTools;
			}
			else
			{
				string algorithm2;
				double resultRadius2;
				BendTableReturnValues result2;
				double? springBack2;
				double kFactorByRadius = applicableBendTable.GetKFactorByRadius(this._doc.Material, this._doc.Thickness, this.AngleAbs * 180.0 / Math.PI, this.ManualRadius ?? this.OriginalRadius, this.Length, ignoreBendTable, (double t, double r) => Unfold.GetRadiusTolerance(t, r, this._general3DConfig), out algorithm2, out resultRadius2, out result2, out springBack2);
				item4 = result2;
				item = resultRadius2;
				item2 = kFactorByRadius;
				item3 = springBack2;
			}
		}
		return (KFactor: item2, SpringBack: item3, KFactorAlgorithm: item4, ToolRadius: item);
	}

	internal abstract BendParametersBase Copy(Doc3d doc);
}
