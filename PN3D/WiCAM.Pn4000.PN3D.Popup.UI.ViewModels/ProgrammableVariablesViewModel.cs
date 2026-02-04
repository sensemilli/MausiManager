using System;
using System.Collections.ObjectModel;
using BendDataSourceModel.Processes;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.PN3D.Popup.UI.Information;
using WiCAM.Pn4000.PN3D.Popup.UI.Models;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class ProgrammableVariablesViewModel : PopupViewModelBase
{
	private readonly Action _closeAction;

	private ProgrammableVariablesModel _programmableVariablesModel;

	public ObservableCollection<BdsmBendProcess> DB { get; set; }

	public ProgramableVariablesVisibility VB { get; set; }

	public IConfigProvider ConfigProvider { get; }

	public ProgrammableVariablesViewModel(ProgrammableVariablesModel programmableVariablesModel, IConfigProvider configProvider, Action closeAction = null)
	{
		this.ConfigProvider = configProvider;
		this.DB = programmableVariablesModel.DB;
		this.VB = programmableVariablesModel.Visibility;
		this._closeAction = closeAction;
		this._programmableVariablesModel = programmableVariablesModel;
		this.Initialize();
	}

	private void Initialize()
	{
		base.Button5_CancelClick = new RelayCommand<object>(CancelClick, CanCancelClick);
		base.Button16_OkClick = new RelayCommand<object>(OkClick, CanOkClick);
	}

	private bool CanOkClick(object obj)
	{
		return true;
	}

	private void OkClick(object obj)
	{
		this._programmableVariablesModel.isViewOkResult = true;
		if (this._closeAction != null)
		{
			this._closeAction();
		}
	}

	private bool CanCancelClick(object obj)
	{
		return true;
	}

	private void CancelClick(object obj)
	{
		this._programmableVariablesModel.isViewOkResult = false;
		if (this._closeAction != null)
		{
			this._closeAction();
		}
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
	}
}
