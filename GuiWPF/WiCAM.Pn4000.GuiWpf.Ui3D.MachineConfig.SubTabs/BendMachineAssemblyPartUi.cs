using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubTabs;

public class BendMachineAssemblyPartUi : ViewModelBase
{
	private string _geoFileName3D;

	private PartRole _partRole;

	public string GeoFileName3D
	{
		get
		{
			return _geoFileName3D;
		}
		set
		{
			_geoFileName3D = value;
			NotifyPropertyChanged("GeoFileName3D");
		}
	}

	public PartRole PartRole
	{
		get
		{
			return _partRole;
		}
		set
		{
			_partRole = value;
			NotifyPropertyChanged("PartRole");
		}
	}
}
