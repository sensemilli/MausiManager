using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.DocInfo;

internal partial class Status3dDocInfoView : UserControl, IStatus3dDocInfoView, IComponentConnector
{
	public Status3dDocInfoView(IStatus3dDocInfoViewModel vm)
	{
		base.DataContext = vm;
		InitializeComponent();
	}

	
}
