using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Material;

internal partial class Status3dMaterialView : UserControl, IStatus3dMaterialView, IComponentConnector
{
	public Status3dMaterialView(IStatus3dMaterialViewModel vm)
	{
		base.DataContext = vm;
		InitializeComponent();
	}


}
