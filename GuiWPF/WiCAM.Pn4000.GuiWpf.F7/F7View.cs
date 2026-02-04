using System.Windows;
using System.Windows.Markup;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.F7;

public partial class F7View : Window, IComponentConnector
{
	public F7ViewModel Vm { get; }

	public F7View(F7ViewModel vm)
	{
		Vm = vm;
		vm.OnClosing += Vm_OnClosing;
		base.DataContext = vm;
	}

	public void Init(IDoc3d currentDoc)
	{
		Vm.Init(currentDoc);
		InitializeComponent();
	}

	private void Vm_OnClosing()
	{
		Vm.OnClosing -= Vm_OnClosing;
		Close();
	}
}
