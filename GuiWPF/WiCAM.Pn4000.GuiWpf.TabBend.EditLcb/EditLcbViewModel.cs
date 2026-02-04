using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.GuiWpf.TabBend.EditLiftingAids;
using WiCAM.Pn4000.GuiWpf.TabBend.EditTools.Interafces;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Pn4000.ToolCalculationMediator;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditLcb;

internal class EditLcbViewModel : EditDraggingXViewModel, IEditLcbViewModel, ISubViewModel, IPopupViewModel
{
	public EditLcbViewModel(IShortcutSettingsCommon shortcutSettingsCommon, IBillboardFactory billboardFactory, IScreen3DMain screen3DMain, IModelDragVisualizerX modelDragVisualizerX, IBendSelection bendSelection, ISnapPointsCalculator snapPointsCalculator, IToolCalculations toolCalculations, IPnBndDoc doc, IntervalOperator<IRange> intervalOperator)
		: base(shortcutSettingsCommon, billboardFactory, screen3DMain, modelDragVisualizerX, bendSelection, snapPointsCalculator, toolCalculations, doc, intervalOperator)
	{
	}

	protected override bool IsValidModel(Model m)
	{
		return m.PartRole == PartRole.AngleMeasurement;
	}

	protected override void CommitDragging(double distance, Vector3d origin)
	{
		ICombinedBendDescriptor combinedBendDescriptor = _bendSelection.CurrentBend?.Bend.CombinedBendDescriptor;
		if (combinedBendDescriptor != null)
		{
			if (_draggingModel.PartRole == PartRole.AngleMeasurement)
			{
				combinedBendDescriptor.AngleMeasurementPositionRel = origin.X - (_bendSelection.CurrentBend?.OffsetWorldX ?? 0.0) + distance;
			}
			_doc.RecalculateSimulation();
		}
	}

	protected override void CmdActivateClick(IPnInputEventArgs e, IBillboard arg2)
	{
		e.Handle();
		ICombinedBendDescriptor combinedBendDescriptor = _bendSelection.CurrentBend?.Bend.CombinedBendDescriptor;
		if (combinedBendDescriptor != null && _draggingModel.PartRole == PartRole.AngleMeasurement)
		{
			combinedBendDescriptor.ActivateAndAutoPositionAngleMeasurementSystem(!combinedBendDescriptor.UseAngleMeasurement, recalcSim: true);
		}
	}
}
