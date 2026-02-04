using System;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class MeasurementSettingsViewModel : ViewModelBase
{
	private ChangedConfigType _changed;

	private double _maxDieHeight;

	private double _maxVWidth;

	private double _minDieHeight;

	private double _minLegLength;

	public Action<ChangedConfigType> DataChanged;

	public double MaxDieHeight
	{
		get
		{
			return _maxDieHeight;
		}
		set
		{
			_maxDieHeight = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("MaxDieHeight");
		}
	}

	public double MaxVWidth
	{
		get
		{
			return _maxVWidth;
		}
		set
		{
			_maxVWidth = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("MaxVWidth");
		}
	}

	public double MinDieHeight
	{
		get
		{
			return _minDieHeight;
		}
		set
		{
			_minDieHeight = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("MinDieHeight");
		}
	}

	public double MinLegLength
	{
		get
		{
			return _minLegLength;
		}
		set
		{
			_minLegLength = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("MinLegLength");
		}
	}

	public void Init(IAngleMeasurementSettings settings)
	{
		_maxDieHeight = settings.MaxDieHeight;
		_maxVWidth = settings.MaxVWidth;
		_minDieHeight = settings.MinDieHeight;
		_minLegLength = settings.MinLegLength;
		_changed = ChangedConfigType.NoChanges;
	}

	public void Save(IAngleMeasurementSettings saveTarget)
	{
		saveTarget.MaxDieHeight = _maxDieHeight;
		saveTarget.MaxVWidth = _maxVWidth;
		saveTarget.MinDieHeight = _minDieHeight;
		saveTarget.MinLegLength = _minLegLength;
		DataChanged?.Invoke(_changed);
	}

	public void Dispose()
	{
	}
}
