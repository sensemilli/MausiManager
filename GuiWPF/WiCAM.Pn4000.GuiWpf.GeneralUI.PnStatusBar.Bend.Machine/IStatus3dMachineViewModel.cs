using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Machine;

public interface IStatus3dMachineViewModel : IPnStatusViewModel
{
	IDoc3d? Doc { get; set; }

	IBendMachine? BendMachine { get; }

	string Header { get; set; }

	string PunchName { get; }

	string PunchRadius { get; }

	string DieName { get; }

	string VWidth { get; }

	string VAngle { get; }

	string CornerRadius { get; }

	string DescPressBrakeDataName { get; }

	string DescMachineNo { get; }

	string DescPunchName { get; }

	string DescPunchRadius { get; }

	string DescDieName { get; }

	string DescVWidth { get; }

	string DescVAngle { get; }

	string DescCornerRadius { get; }
}
