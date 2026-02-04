using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.PN3D.Popup.View;

public partial class PopupValidationPending : Window, IComponentConnector
{
	private PopupValidationPendingViewModel Vm;

	public bool ValidationEnded;

	private bool BlockClose = true;

	private Cursor _cursorOrg;

	public Action Aborting { get; set; }

	public PopupValidationPending(PopupValidationPendingViewModel vm)
	{
		this._cursorOrg = Mouse.OverrideCursor;
		Mouse.OverrideCursor = null;
		this.Vm = vm;
		base.DataContext = vm;
		this.InitializeComponent();
	}

	public void StopEvent(ISimulationThread obj)
	{
		Application.Current.Dispatcher.BeginInvoke(new Action(StopEventMainThread));
	}

	private void StopEventMainThread()
	{
		this.ValidationEnded = true;
		this.btnExit.Content = this.Vm.Translator.Translate("l_popup.PopupDisassembly.Ok");
		this.BlockClose = false;
		if (this.Vm.AutoContinueIfOk && !this.Vm.HasErrors)
		{
			base.Close();
		}
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		if (this.ValidationEnded)
		{
			base.Close();
		}
		else
		{
			this.Aborting?.Invoke();
		}
	}

	public void PauseEvent(ISimulationThread obj)
	{
		this.BlockClose = false;
		base.Close();
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (this.BlockClose)
		{
			e.Cancel = true;
		}
		else
		{
			Mouse.OverrideCursor = this._cursorOrg;
		}
	}
}
