using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public partial class FingerView : UserControl, IComponentConnector, IStyleConnector
{
	public FingerView()
	{
		InitializeComponent();
	}

	private void EventSetter_MouseEnter(object sender, MouseEventArgs e)
	{
		if (base.DataContext is FingerViewModel fingerViewModel && sender is FrameworkElement { DataContext: StopCombinationViewModel dataContext })
		{
			fingerViewModel.HoveredCombination = dataContext;
		}
	}

	private void EventSetter_MouseLeave(object sender, MouseEventArgs e)
	{
		if (base.DataContext is FingerViewModel fingerViewModel)
		{
			fingerViewModel.HoveredCombination = null;
		}
	}

	public void AlignFace_Click(object sender, RoutedEventArgs e)
	{
		if (base.DataContext is FingerViewModel fingerViewModel)
		{
			fingerViewModel.AlignFaceY();
		}
	}
}
