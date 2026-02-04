using System;
using WiCAM.Pn4000.Common.Wpf.UnitConversion;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class VisualBendInfoItems
{
	public ICombinedBendDescriptorInternal CommonBendFace { get; }

	public int UiOrder { get; }

	public double AngleDeg { get; }

	public InchConversion OriginalRadius { get; }

	public InchConversion Radius { get; }

	public InchConversion BendDeduction { get; }

	public InchConversion DinLength { get; }

	public InchConversion BendingAllowance { get; }

	public double KFactor { get; }

	public string KFactorAlgorithmTranslated { get; }

	public BendTableReturnValues KFactorAlgorithmEnum { get; set; }

	public bool IsNotExactKFactor { get; }

	public InchConversion VWidth { get; }

	public double VAngle { get; }

	public InchConversion CornerRadius { get; }

	public InchConversion PunchRadius { get; }

	public string PunchGroupId { get; }

	public string PunchGroupName { get; }

	public string DieGroupId { get; }

	public string DieGroupName { get; }

	public string ToolSelectionAlgorithmTranslated { get; }

	public VisualBendInfoItems(ICombinedBendDescriptorInternal commonBendFace, IToolProfile? upperTool, IToolProfile? lowerTool, ToolSelectionType? tst)
	{
		this.CommonBendFace = commonBendFace;
		this.UiOrder = this.CommonBendFace.Order + 1;
		this.AngleDeg = this.CommonBendFace[0].BendParams.Angle * 180.0 / Math.PI;
		this.Radius = new InchConversion(this.CommonBendFace[0].BendParams.FinalRadius);
		this.OriginalRadius = new InchConversion(this.CommonBendFace[0].BendParams.OriginalRadius);
		this.BendingAllowance = new InchConversion(this.CommonBendFace[0].BendParams.BendingAllowance);
		this.BendDeduction = new InchConversion(this.CommonBendFace[0].BendParams.FinalBendDeduction);
		this.DinLength = new InchConversion(this.CommonBendFace[0].BendParams.DinLength);
		this.KFactor = this.CommonBendFace[0].BendParams.KFactor;
		this.KFactorAlgorithmEnum = this.CommonBendFace[0].BendParams.KFactorAlgorithm;
		this.IsNotExactKFactor = this.KFactorAlgorithmEnum == BendTableReturnValues.INTERPOLATED || this.KFactorAlgorithmEnum == BendTableReturnValues.NO_VALUE_FOUND;
		this.KFactorAlgorithmTranslated = KFactorAlgorithmTranslation.GetTranslation(this.KFactorAlgorithmEnum);
		this.ToolSelectionAlgorithmTranslated = ToolSelectionAlgorithmTranslation.GetTranslation(tst.GetValueOrDefault());
		IProfileGroup profileGroup = upperTool?.Group;
		IProfileGroup profileGroup2 = lowerTool?.Group;
		this.PunchGroupId = profileGroup?.ID.ToString() ?? string.Empty;
		this.PunchGroupName = profileGroup?.Name ?? string.Empty;
		this.DieGroupId = profileGroup2?.ID.ToString() ?? string.Empty;
		this.DieGroupName = profileGroup2?.Name ?? string.Empty;
		this.VWidth = new InchConversion((profileGroup2 as IDieGroup)?.VWidth ?? 0.0);
		this.VAngle = (profileGroup2 as IDieGroup)?.VAngle ?? 0.0;
		this.CornerRadius = new InchConversion((profileGroup2 as IDieGroup)?.Radius ?? 0.0);
		this.PunchRadius = new InchConversion((profileGroup as IPunchGroup)?.Radius ?? 0.0);
	}
}
