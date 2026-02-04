using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.CalculationInfo;

internal partial class Status3dCalculationInfoView : UserControl, IStatus3dCalculationInfoView, IComponentConnector
{
	public Status3dCalculationInfoView(Status3dCalculationInfoViewModel vm)
	{
		base.DataContext = vm;
		InitializeComponent();
	}
}
