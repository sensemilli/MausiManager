using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

namespace WiCAM.Pn4000.GuiWpf.UiBendMachine.SelectMachine;

public partial class SelectMachineView : UserControl, ISelectMachineView, IComponentConnector
{
	public ISelectMachineView Init(ISelectMachineViewModel selectMachineViewModel)
	{
		base.DataContext = selectMachineViewModel;
		InitializeComponent();
		return this;
	}

}
