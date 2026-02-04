using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

public partial class FastSelectItemWindow : Window, IComponentConnector
{
	public string Out_data { get; set; }

	public FastSelectItemWindow()
	{
		this.InitializeComponent();
	}

	internal void SetData(Dictionary<string, Action> examplesTab)
	{
		foreach (string key in examplesTab.Keys)
		{
			this.List.Items.Add(key);
		}
	}

	private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (this.List.SelectedIndex >= 0)
		{
			this.Out_data = (string)this.List.SelectedItem;
		}
		base.Close();
	}
}
