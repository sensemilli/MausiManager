using System;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class PopupCheckConfigChangedViewModel : ViewModelBase
{
	private ICommand _closeOkCommand;

	private ICommand _closeCancelCommand;

	private double _dialogOpacity;

	private readonly Action<bool> _closeAction;

	public ICommand CloseOkCommand => this._closeOkCommand ?? (this._closeOkCommand = new RelayCommand<Window>(CloseLikeOk));

	public ICommand CloseCancelCommand => this._closeCancelCommand ?? (this._closeCancelCommand = new RelayCommand<Window>(CloseLikeCancel));

	public double DialogOpacity
	{
		get
		{
			return this._dialogOpacity;
		}
		set
		{
			this._dialogOpacity = value;
			base.NotifyPropertyChanged("DialogOpacity");
		}
	}

	public PopupCheckConfigChangedViewModel(IGlobals globals, Action<bool> closeAction)
	{
		this._closeAction = closeAction;
		this.DialogOpacity = globals.ConfigProvider.InjectOrCreate<GeneralUserSettingsConfig>().DialogOpacity;
	}

	private void CloseLikeOk(Window window)
	{
		if (window != null)
		{
			window.Close();
			this._closeAction?.Invoke(obj: true);
		}
	}

	private void CloseLikeCancel(Window window)
	{
		if (window != null)
		{
			window.Close();
			this._closeAction?.Invoke(obj: false);
		}
	}
}
