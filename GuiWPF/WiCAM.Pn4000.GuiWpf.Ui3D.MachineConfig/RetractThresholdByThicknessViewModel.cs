using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class RetractThresholdByThicknessViewModel : INotifyPropertyChanged
{
	private double _thickness;

	private double _retractThreshold;

	public double Thickness
	{
		get
		{
			return _thickness;
		}
		set
		{
			_thickness = value;
		}
	}

	public double RetractThreshold
	{
		get
		{
			return _retractThreshold;
		}
		set
		{
			_retractThreshold = value;
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
