using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;

public interface IPressBrakeInfoDeprecated
{
	ClampPositionOptions ClampPositionOption { get; set; }

	string Manufacturer { get; set; }

	string PP { get; set; }

	ControlerType Type { get; set; }

	BendTableOptions UseGlobalBendSequenceList { get; set; }

	BendTableOptions UseGlobalBendTable { get; set; }

	string Version { get; set; }
}
