using System.Windows;
using System.Windows.Controls;
using WiCAM.Pn4000.Machine;

namespace WiCAM.Pn4000.JobManager;

public class MachineTemplateSelector : DataTemplateSelector
{
	public DataTemplate MachineZero { get; set; }

	public DataTemplate Machine { get; set; }

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (item is MachineViewInfo machineViewInfo && machineViewInfo.Machine.Number == 0)
		{
			return MachineZero;
		}
		return Machine;
	}
}
