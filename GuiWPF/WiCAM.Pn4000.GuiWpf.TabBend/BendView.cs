using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.TabBend;

public partial class BendView : UserControl, IComponentConnector
{
	public BendView(IBendViewModel vm)
	{
		InitializeComponent();
		base.DataContext = vm;
	}


}
