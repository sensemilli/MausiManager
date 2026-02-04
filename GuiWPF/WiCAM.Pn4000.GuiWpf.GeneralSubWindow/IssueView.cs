using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.GeneralSubWindow;

public partial class IssueView : UserControl, IIssueView, IComponentConnector
{
	public IssueView()
	{
		InitializeComponent();
	}
}
