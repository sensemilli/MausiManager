using System;
using System.Windows;
using System.Windows.Controls;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class PnListView : ListView
{
	private PnGridView _gridView;

	public PnListView()
	{
		base.Initialized += ExListView_Initialized;
	}

	private void ExListView_Initialized(object sender, EventArgs e)
	{
		this._gridView = base.View as PnGridView;
		if (this._gridView != null)
		{
			this._gridView.IsInitialized = true;
			this._gridView.IsolatedStoredFilename = Global.ToMD5(this.GetLogicalTreeString()) + ".txt";
			this._gridView.SetColumnOrder();
			this._gridView.HookColumnWidthsChangedEvents();
		}
	}

	private string GetLogicalTreeString()
	{
		string result = "";
		this.GetLogicalTree(this, ref result);
		return result;
	}

	private void GetLogicalTree(object obj, ref string result)
	{
		if (obj is DependencyObject)
		{
			result = result + obj?.ToString() + "\\";
			this.GetLogicalTree(LogicalTreeHelper.GetParent(obj as DependencyObject), ref result);
		}
	}
}
