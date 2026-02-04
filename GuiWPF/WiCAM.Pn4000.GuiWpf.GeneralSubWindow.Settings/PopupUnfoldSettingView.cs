using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.PN3D.Popup.View;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.Popup;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.GeneralSubWindow.Settings;

public partial class PopupUnfoldSettingView : PopupBase, IPopupUnfoldSettingView, IComponentConnector
{
	private List<GridViewDataColumn> DynamicPrefabricatedColumns = new List<GridViewDataColumn>();

	public PopupUnfoldSettingView(ILogCenterService logCenterService, IConfigProvider configProvider, IShowPopupService popupService, IPKernelFlowGlobalDataService globalDataService)
		: base(logCenterService, configProvider, popupService, globalDataService, "PopupUnfoldSetting")
	{
		base.DataContextChanged += OnDataContextChanged;
		InitializeComponent();
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue != e.OldValue)
		{
			if (e.OldValue is PopupUnfoldSettingViewModel popupUnfoldSettingViewModel)
			{
				popupUnfoldSettingViewModel.ReloadPrefabricatedDynColumns -= Vm_ReloadPrefabricatedDynColumns;
			}
			if (e.NewValue is PopupUnfoldSettingViewModel popupUnfoldSettingViewModel2)
			{
				popupUnfoldSettingViewModel2.ReloadPrefabricatedDynColumns += Vm_ReloadPrefabricatedDynColumns;
				Vm_ReloadPrefabricatedDynColumns(popupUnfoldSettingViewModel2.AdditionalHeaders);
			}
			else
			{
				Vm_ReloadPrefabricatedDynColumns(new List<string>());
			}
		}
	}

	private void Vm_ReloadPrefabricatedDynColumns(List<string> obj)
	{
		GridPrefabricatedParts.Columns.RemoveItems(DynamicPrefabricatedColumns);
		int num = 0;
		DynamicPrefabricatedColumns.Clear();
		foreach (string item in obj)
		{
			DynamicPrefabricatedColumns.Add(new GridViewDataColumn
			{
				Header = item,
				DataMemberBinding = new Binding($"AdditionalValues[{num++}]")
			});
		}
		GridPrefabricatedParts.Columns.AddRange(DynamicPrefabricatedColumns);
	}

	public void GridView_PreparedCellForEdit(object sender, GridViewPreparingCellForEditEventArgs e)
	{
		if (!(e.EditingElement is RadComboBox radComboBox))
		{
			return;
		}
		radComboBox.DropDownClosed += delegate(object? s, EventArgs a)
		{
			if (s is RadComboBox radComboBox2)
			{
				ParentOfTypeExtensions.ParentOfType<GridViewCell>((DependencyObject)radComboBox2).CommitEdit();
			}
		};
	}


}
