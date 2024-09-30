using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public class PlateProductionControlViewModel : ViewModelBase, IDataErrorInfo
{
	private readonly IJobManagerSettings _settings;

	private PlateProductionInfo _plate;

	private double _fontSize;

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

	public List<string> Users { get; set; }

	public string SelectedUser
	{
		get
		{
			return _settings.LastSelectedUser;
		}
		set
		{
			_settings.LastSelectedUser = value;
			_plate.ProductionData.UserName = value;
			NotifyPropertyChanged("SelectedUser");
		}
	}

	public string ChargeNumber
	{
		get
		{
			return _plate.ProductionData.ChargeNumber;
		}
		set
		{
			_plate.ProductionData.ChargeNumber = value;
			NotifyPropertyChanged("ChargeNumber");
		}
	}

	public double FontSize
	{
		get
		{
			return _fontSize;
		}
		set
		{
			_fontSize = value;
			NotifyPropertyChanged("FontSize");
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
				}, (object x) => CanOk());
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

	public string this[string columnName]
	{
		get
		{
			if (columnName.Equals("SelectedUser", StringComparison.CurrentCultureIgnoreCase))
			{
				if (_settings.IsUserObligatory && string.IsNullOrWhiteSpace(SelectedUser))
				{
					return "User is not selected!";
				}
			}
			else if (columnName.Equals("ChargeNumber", StringComparison.CurrentCultureIgnoreCase) && _settings.IsChargeObligatory && string.IsNullOrWhiteSpace(ChargeNumber))
			{
				return "Charge is not defined!";
			}
			return string.Empty;
		}
	}

	public string Error { get; }

	private void Ok(object view)
	{
		_plate.ProductionData.UserName = _settings.LastSelectedUser;
		if (view is Window window)
		{
			window.DialogResult = true;
			window.Close();
		}
	}

	private bool CanOk()
	{
		if (_settings.IsUserObligatory && string.IsNullOrEmpty(SelectedUser))
		{
			return false;
		}
		if (_settings.IsChargeObligatory && string.IsNullOrEmpty(Plate.ProductionData.ChargeNumber))
		{
			return false;
		}
		return true;
	}

	private void Cancel(object view)
	{
		if (view is Window window)
		{
			window.DialogResult = false;
			window.Close();
		}
	}

	public PlateProductionControlViewModel(IJobManagerServiceProvider provider)
	{
		_settings = provider.FindService<IJobManagerSettings>();
		Users = _settings.UserNames;
		if (_settings.IsUserObligatory)
		{
			_settings.LastSelectedUser = string.Empty;
		}
	}
}
