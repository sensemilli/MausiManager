using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.GuiWpf.TabBend;

public class EditPanel3DContentBase : ViewModelBase
{
	private double _opacity = 0.6;

	public double Opacity
	{
		get
		{
			return _opacity;
		}
		set
		{
			_opacity = value;
			NotifyPropertyChanged("Opacity");
		}
	}
}
