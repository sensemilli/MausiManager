using System;
using System.ComponentModel;
using System.Windows.Markup;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.View;

public partial class PopupSelectMaterialView : PopupBase, IComponentConnector
{
	private PopupSelectMaterialViewModel _viewModel;

	public PopupSelectMaterialView(PopupSelectMaterialViewModel viewModel, IPKernelFlowGlobalDataService pKernelFlowGlobalDataService, ILogCenterService logCenterService, IConfigProvider configProvider, IShowPopupService popupService)
		: base(logCenterService, configProvider, popupService, pKernelFlowGlobalDataService, "SelectMaterial")
	{
		this._viewModel = viewModel;
		base.DataContext = viewModel;
		base.Owner = pKernelFlowGlobalDataService.MainWindow;
	}

	public void Init(IDoc3d doc, Action<PopupSelectMaterialViewModel> closeAction)
	{
		this._viewModel.Init(doc, closeAction);
		this.InitializeComponent();
		this.ColumnThicknessMin.ColumnFilterDescriptor.FieldFilter.Filter1.Operator = FilterOperator.IsLessThanOrEqualTo;
		this.ColumnThicknessMin.ColumnFilterDescriptor.FieldFilter.Filter1.Value = this._viewModel.ThicknessFilterDouble;
		this.ColumnThicknessMax.ColumnFilterDescriptor.FieldFilter.Filter1.Operator = FilterOperator.IsGreaterThanOrEqualTo;
		this.ColumnThicknessMax.ColumnFilterDescriptor.FieldFilter.Filter1.Value = this._viewModel.ThicknessFilterDouble;
		this.ColumnThicknessMax.ColumnFilterDescriptor.FieldFilter.LogicalOperator = FilterCompositionLogicalOperator.Or;
		this.ColumnThicknessMax.ColumnFilterDescriptor.FieldFilter.Filter2.Operator = FilterOperator.IsNull;
		this.MaterialTable.SortDescriptors.Add(new ColumnSortDescriptor
		{
			Column = this.ColumnNumber,
			SortDirection = ListSortDirection.Ascending
		});
		if (this._viewModel.SelectedMaterial != null)
		{
			this.MaterialTable.ScrollIntoView(this._viewModel.SelectedMaterial);
		}
		base.OnClosingAction = (Action<EPopupCloseReason>)Delegate.Combine(base.OnClosingAction, new Action<EPopupCloseReason>(this._viewModel.ViewCloseAction));
	}
}
