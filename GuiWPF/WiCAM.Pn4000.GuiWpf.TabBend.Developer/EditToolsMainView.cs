using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.TabBend.Developer;

internal partial class EditToolsMainView : UserControl, IEditToolsMainView, IComponentConnector
{
	public IEditToolsMainViewModel Vm { get; }

	public EditToolsMainView(IEditToolsMainViewModel vm)
	{
		Vm = vm;
		base.DataContext = vm;
		InitializeComponent();
	}


}
