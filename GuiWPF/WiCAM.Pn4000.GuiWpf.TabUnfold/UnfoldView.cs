using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.TabUnfold;

public partial class UnfoldView : UserControl, IComponentConnector
{
	public UnfoldView(IUnfoldViewModel vm)
	{
		base.DataContext = vm;
		InitializeComponent();
	}

}
