using System;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class CrowningItemViewModel : ViewModelBase
{
	public int MatGrpId { get; set; }

	public double Thickness { get; set; }

	public double[] Values { get; set; } = Array.Empty<double>();
}
