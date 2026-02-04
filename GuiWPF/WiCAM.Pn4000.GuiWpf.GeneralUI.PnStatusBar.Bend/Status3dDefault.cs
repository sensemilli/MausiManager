using WiCAM.Pn4000.GuiContracts.PnStatusBar;
using WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.CalculationInfo;
using WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.DocInfo;
using WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Machine;
using WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Material;
using WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Model;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend;

internal class Status3dDefault : IStatus3dDefault
{
	private readonly IPnStatusBarHelper _statusBarHelper;

	private readonly IUiDocManager _docManager;

	private readonly IStatus3dDocInfoViewModel _statusDocInfoVm;

	private readonly IStatus3dMaterialViewModel _statusMaterialVm;

	private readonly IStatus3dModelInfoViewModel _statusModelInfoVm;

	private readonly IStatus3dMachineViewModel _statusMachineVm;

	public Status3dDefault(IPnStatusBarHelper statusBarHelper, IUiDocManager docManager, IStatus3dDocInfoViewModel statusDocInfoVm, IStatus3dMachineViewModel statusMachineVm, IStatus3dMaterialViewModel statusMaterialVm, IStatus3dModelInfoViewModel statusModelInfoVm)
	{
		_statusBarHelper = statusBarHelper;
		_docManager = docManager;
		_statusDocInfoVm = statusDocInfoVm;
		_statusMachineVm = statusMachineVm;
		_statusMaterialVm = statusMaterialVm;
		_statusModelInfoVm = statusModelInfoVm;
		_docManager.CurrentDocChanged += _docManager_CurrentDocChanged;
	}

	private void _docManager_CurrentDocChanged(IDoc3d docOld, IDoc3d docNew)
	{
		if (docOld != docNew)
		{
			_statusDocInfoVm.Doc = docNew;
			_statusMachineVm.Doc = docNew;
			_statusMaterialVm.Doc = docNew;
			_statusModelInfoVm.Doc = docNew;
		}
	}

	public void ShowDefaultStatusBars()
	{
		_statusBarHelper.SetSlotControl<IPnStatusBarPKernelSlot0Control>(0);
		_statusBarHelper.SetSlotControl<IStatus3dCalculationInfoView>(1);
		_statusBarHelper.SetSlotControl<IStatus3dDocInfoView>(2);
		_statusBarHelper.SetSlotControl<IStatus3dMachineView>(3);
		_statusBarHelper.SetSlotControl<IStatus3dMaterialView>(4);
		_statusBarHelper.SetSlotControl<IStatus3dModelDimensionView>(5);
		_statusBarHelper.SetSlotControl<IStatus3dModelTypeView>(6);
		_statusBarHelper.SetSlotControl<IStatus3dModelComponentsView>(7);
		_statusBarHelper.SetSlotControl<IPnStatusBarPKernelSlot8Control>(8);
		_statusDocInfoVm.Doc = _docManager.CurrentDoc;
		_statusMachineVm.Doc = _docManager.CurrentDoc;
		_statusMaterialVm.Doc = _docManager.CurrentDoc;
		_statusModelInfoVm.Doc = _docManager.CurrentDoc;
	}
}
