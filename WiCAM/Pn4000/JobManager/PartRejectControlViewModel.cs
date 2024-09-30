using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public class PartRejectControlViewModel : ViewModelBase
{
	private PlatePartProductionInfo _part;

	private ICommand _okCommand;

	private ICommand _cancelCommand;

	public PlatePartProductionInfo Part
	{
		get
		{
			return _part;
		}
		set
		{
			_part = value;
			NotifyPropertyChanged("Part");
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
		int amountToReject = Part.AmountToReject;
		Part.AmountToReject = amountToReject;
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
