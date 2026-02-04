using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.Contracts.PaintTools;

public interface IMachinePainter
{
	void SetOpacities(IPaintTool paintTool, IBendMachineGeometry? machine, Dictionary<PartRole, double> opacities);

	void SetOpacities(IPaintTool paintTool, IBendMachineGeometry? machine, Dictionary<MachineParts, double> opacities);
}
