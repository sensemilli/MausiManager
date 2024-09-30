using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public class JobProductionStrategySelectViewModel : ViewModelBase, IDataErrorInfo
{
	private readonly IJobManagerSettings _settings;

	private ProductionInfo _data;

	private ICommand _okCommand;

	private ICommand _cancelCommand;

	public string Error { get; }

	public ProductionInfo Data
	{
		get
		{
			return _data;
		}
		set
		{
			_data = value;
			NotifyPropertyChanged("Data");
		}
	}

	public string SelectedUser
	{
		get
		{
			return _settings.LastSelectedUser;
		}
		set
		{
			_settings.LastSelectedUser = value;
			Data.UserName = value;
			NotifyPropertyChanged("SelectedUser");
		}
	}

	public string ChargeNumber
	{
		get
		{
			return Data.ChargeNumber;
		}
		set
		{
			Data.ChargeNumber = value;
			NotifyPropertyChanged("ChargeNumber");
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

	private void Ok(object view)
	{
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
		if (_settings.IsChargeObligatory && string.IsNullOrEmpty(Data.ChargeNumber))
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

	public JobProductionStrategySelectViewModel(IJobManagerServiceProvider provider, ProductionInfo data)
	{
		_settings = provider.FindService<IJobManagerSettings>();
		_data = data;
	}
}
