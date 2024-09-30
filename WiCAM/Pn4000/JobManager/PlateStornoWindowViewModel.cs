using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public class PlateStornoWindowViewModel : ViewModelBase
{
	private PlateProductionInfo _plate;

	private ICommand _okCommand;

	private ICommand _cancelCommand;

	public PlateProductionInfo Plate
	{
		get
		{
			return _plate;
		}
		set
		{
			_plate = value;
			NotifyPropertyChanged("Plate");
		}
	}

	public ICommand OkCommand
	{
		get
		{
			if (_okCommand == null)
			{
				_okCommand = new RelayCommand(delegate(object x)
				{
					Ok(x);
				}, (object x) => true);
			}
			return _okCommand;
		}
	}

	public ICommand CancelCommand
	{
		get
		{
			if (_cancelCommand == null)
			{
				_cancelCommand = new RelayCommand(delegate(object x)
				{
					Cancel(x);
				}, (object x) => true);
			}
			return _cancelCommand;
		}
	}

	private void Ok(object view)
	{
		if (view is Window window)
		{
			window.DialogResult = true;
			window.Close();
		}
	}

	private void Cancel(object view)
	{
		if (view is Window window)
		{
			window.DialogResult = false;
			window.Close();
		}
	}
}
