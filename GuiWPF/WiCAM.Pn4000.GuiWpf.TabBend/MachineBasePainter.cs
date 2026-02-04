using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.GuiWpf.TabBend;

internal class MachineBasePainter : IMachineBasePainter
{
	private readonly IEditToolsSelection _editToolsSelection;

	private readonly IBendSelection _bendSelection;

	private static Color ColorFingerUser { get; } = new Color(0f, 0.69803923f, 1f, 1f);

	public MachineBasePainter(IEditToolsSelection editToolsSelection, IBendSelection bendSelection)
	{
		_editToolsSelection = editToolsSelection;
		_bendSelection = bendSelection;
	}

	public void PaintMachineBasic(IPaintTool paintTool)
	{
		float num = 3f;
		IBendMachineGeometry bendMachineGeometry = _editToolsSelection.CurrentMachine?.Geometry;
		Model model = bendMachineGeometry?.AngleMeasurmentSystem?.AngleMeasurmentModel;
		IBendPositioning currentBend = _bendSelection.CurrentBend;
		if (currentBend == null)
		{
			return;
		}
		ICombinedBendDescriptorInternal cbd = _bendSelection.GetCbd(currentBend);
		if (cbd != null)
		{
			Color value = ((!cbd.UseAngleMeasurement) ? Color.Red : Color.Green);
			paintTool.SetModelEdgeColor(model, value, num);
			SetLiftingColor(cbd.UseLeftFrontLiftingAid, bendMachineGeometry?.LeftFrontLiftingAid, num, paintTool);
			SetLiftingColor(cbd.UseLeftBackLiftingAid, bendMachineGeometry?.LeftBackLiftingAid, num, paintTool);
			SetLiftingColor(cbd.UseRightFrontLiftingAid, bendMachineGeometry?.RightFrontLiftingAid, num, paintTool);
			SetLiftingColor(cbd.UseRightBackLiftingAid, bendMachineGeometry?.RightBackLiftingAid, num, paintTool);
			value = cbd.FingerPositioningMode switch
			{
				FingerPositioningMode.User => ColorFingerUser, 
				FingerPositioningMode.Auto => cbd.FingerStability switch
				{
					FingerStability.Unstable => Color.Red, 
					FingerStability.SemiStable => Color.Yellow, 
					FingerStability.Stable => Color.Lime, 
					_ => Color.Grey, 
				}, 
				FingerPositioningMode.None => Color.Grey, 
				_ => Color.Grey, 
			};
			if (cbd.FingerPositioningMode != 0)
			{
				paintTool.SetModelEdgeColor(bendMachineGeometry?.LeftFinger?.FingerModel, value, 2f, applyToSubModels: true);
				paintTool.SetModelEdgeColor(bendMachineGeometry?.RightFinger?.FingerModel, value, 2f, applyToSubModels: true);
			}
		}
	}

	private void SetLiftingColor(LiftingAidEnum useLiftingAid, ILiftingAid? liftingAid, float lw, IPaintTool paintTool)
	{
		if (liftingAid?.LiftingAidBaseModel != null)
		{
			Color value = useLiftingAid switch
			{
				LiftingAidEnum.NoLiftingAid => Color.Red, 
				LiftingAidEnum.UseActive => Color.Green, 
				LiftingAidEnum.UseSupportOnly => Color.Blue, 
				_ => Color.Grey, 
			};
			paintTool.SetModelEdgeColor(liftingAid.LiftingAidBaseModel, value, lw, applyToSubModels: true);
		}
	}
}
