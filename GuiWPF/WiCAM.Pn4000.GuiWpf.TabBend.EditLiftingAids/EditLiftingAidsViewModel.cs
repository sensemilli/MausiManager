using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.GuiWpf.TabBend.BendSequenceList;
using WiCAM.Pn4000.GuiWpf.TabBend.EditTools.Interafces;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Pn4000.ToolCalculationMediator;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditLiftingAids;

internal class EditLiftingAidsViewModel : EditDraggingXViewModel, IEditLiftingAidsViewModel, ISubViewModel, IPopupViewModel
{
	private readonly IBendSequenceListViewModel _sequenceListViewModel;

	public EditLiftingAidsViewModel(IShortcutSettingsCommon shortcutSettingsCommon, IBillboardFactory billboardFactory, IScreen3DMain screen3DMain, IModelDragVisualizerX modelDragVisualizerX, IBendSelection bendSelection, ISnapPointsCalculator snapPointsCalculator, IToolCalculations toolCalculations, IPnBndDoc doc, IntervalOperator<IRange> intervalOperator, IBendSequenceListViewModel sequenceListViewModel)
		: base(shortcutSettingsCommon, billboardFactory, screen3DMain, modelDragVisualizerX, bendSelection, snapPointsCalculator, toolCalculations, doc, intervalOperator)
	{
		_sequenceListViewModel = sequenceListViewModel;
	}

	protected override bool IsValidModel(Model m)
	{
		if (m.PartRole != PartRole.LeftBackLiftingAid && m.PartRole != PartRole.LeftFrontLiftingAid && m.PartRole != PartRole.RightFrontLiftingAid)
		{
			return m.PartRole == PartRole.RightBackLiftingAid;
		}
		return true;
	}

	protected override void CommitDragging(double distance, Vector3d origin)
	{
		ICombinedBendDescriptor combinedBendDescriptor = _bendSelection.CurrentBend?.Bend.CombinedBendDescriptor;
		if (combinedBendDescriptor == null)
		{
			return;
		}
		switch (_draggingModel.PartRole)
		{
		case PartRole.LeftBackLiftingAid:
		{
			ICombinedBendDescriptor combinedBendDescriptor2 = combinedBendDescriptor;
			double? rightFrontLiftingAidHorizontalCoordinar = combinedBendDescriptor2.LeftBackLiftingAidHorizontalCoordinar;
			double valueOrDefault = rightFrontLiftingAidHorizontalCoordinar.GetValueOrDefault();
			if (!rightFrontLiftingAidHorizontalCoordinar.HasValue)
			{
				valueOrDefault = 0.0;
				ICombinedBendDescriptor combinedBendDescriptor6 = combinedBendDescriptor2;
				double? rightFrontLiftingAidHorizontalCoordinar2 = valueOrDefault;
				combinedBendDescriptor6.LeftBackLiftingAidHorizontalCoordinar = rightFrontLiftingAidHorizontalCoordinar2;
			}
			combinedBendDescriptor.LeftBackLiftingAidHorizontalCoordinar += distance;
			break;
		}
		case PartRole.LeftFrontLiftingAid:
		{
			ICombinedBendDescriptor combinedBendDescriptor2 = combinedBendDescriptor;
			double? rightFrontLiftingAidHorizontalCoordinar = combinedBendDescriptor2.LeftFrontLiftingAidHorizontalCoordinar;
			double valueOrDefault = rightFrontLiftingAidHorizontalCoordinar.GetValueOrDefault();
			if (!rightFrontLiftingAidHorizontalCoordinar.HasValue)
			{
				valueOrDefault = 0.0;
				ICombinedBendDescriptor combinedBendDescriptor5 = combinedBendDescriptor2;
				double? rightFrontLiftingAidHorizontalCoordinar2 = valueOrDefault;
				combinedBendDescriptor5.LeftFrontLiftingAidHorizontalCoordinar = rightFrontLiftingAidHorizontalCoordinar2;
			}
			combinedBendDescriptor.LeftFrontLiftingAidHorizontalCoordinar += distance;
			break;
		}
		case PartRole.RightBackLiftingAid:
		{
			ICombinedBendDescriptor combinedBendDescriptor2 = combinedBendDescriptor;
			double? rightFrontLiftingAidHorizontalCoordinar = combinedBendDescriptor2.RightBackLiftingAidHorizontalCoordinar;
			double valueOrDefault = rightFrontLiftingAidHorizontalCoordinar.GetValueOrDefault();
			if (!rightFrontLiftingAidHorizontalCoordinar.HasValue)
			{
				valueOrDefault = 0.0;
				ICombinedBendDescriptor combinedBendDescriptor4 = combinedBendDescriptor2;
				double? rightFrontLiftingAidHorizontalCoordinar2 = valueOrDefault;
				combinedBendDescriptor4.RightBackLiftingAidHorizontalCoordinar = rightFrontLiftingAidHorizontalCoordinar2;
			}
			combinedBendDescriptor.RightBackLiftingAidHorizontalCoordinar += distance;
			break;
		}
		case PartRole.RightFrontLiftingAid:
		{
			ICombinedBendDescriptor combinedBendDescriptor2 = combinedBendDescriptor;
			double? rightFrontLiftingAidHorizontalCoordinar = combinedBendDescriptor2.RightFrontLiftingAidHorizontalCoordinar;
			double valueOrDefault = rightFrontLiftingAidHorizontalCoordinar.GetValueOrDefault();
			if (!rightFrontLiftingAidHorizontalCoordinar.HasValue)
			{
				valueOrDefault = 0.0;
				ICombinedBendDescriptor combinedBendDescriptor3 = combinedBendDescriptor2;
				double? rightFrontLiftingAidHorizontalCoordinar2 = valueOrDefault;
				combinedBendDescriptor3.RightFrontLiftingAidHorizontalCoordinar = rightFrontLiftingAidHorizontalCoordinar2;
			}
			combinedBendDescriptor.RightFrontLiftingAidHorizontalCoordinar += distance;
			break;
		}
		}
		_doc.RecalculateSimulation();
	}

	protected override void CmdActivateClick(IPnInputEventArgs e, IBillboard arg2)
	{
		e.Handle();
		ICombinedBendDescriptor combinedBendDescriptor = _bendSelection.CurrentBend?.Bend.CombinedBendDescriptor;
		if (combinedBendDescriptor != null)
		{
			switch (_draggingModel.PartRole)
			{
			case PartRole.LeftBackLiftingAid:
				combinedBendDescriptor.UseLeftBackLiftingAid = (LiftingAidEnum)((int)(combinedBendDescriptor.UseLeftBackLiftingAid + 1) % 3);
				break;
			case PartRole.LeftFrontLiftingAid:
				combinedBendDescriptor.UseLeftFrontLiftingAid = (LiftingAidEnum)((int)(combinedBendDescriptor.UseLeftFrontLiftingAid + 1) % 3);
				break;
			case PartRole.RightBackLiftingAid:
				combinedBendDescriptor.UseRightBackLiftingAid = (LiftingAidEnum)((int)(combinedBendDescriptor.UseRightBackLiftingAid + 1) % 3);
				break;
			case PartRole.RightFrontLiftingAid:
				combinedBendDescriptor.UseRightFrontLiftingAid = (LiftingAidEnum)((int)(combinedBendDescriptor.UseRightFrontLiftingAid + 1) % 3);
				break;
			}
		}
		_sequenceListViewModel.RefreshAllProps();
		_doc.RecalculateSimulation();
	}
}
