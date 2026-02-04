using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IPressBrakeInfo
{
	ClampPositionOptions ClampPositionOption { get; set; }

	VWidthTypes DefaultVWidthType { get; set; }

	string PP { get; set; }

	ControlerType Type { get; set; }

	BendTableOptions UseGlobalBendSequenceList { get; set; }

	BendTableOptions UseGlobalBendTable { get; set; }

	string Version { get; set; }
}
