using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class BendSequenceItem : ViewModelBase
{
	public readonly ICombinedBendDescriptorInternal CombinedBendDescriptor;

	private bool _hovered;

	public bool IsExcluded => !this.CombinedBendDescriptor.IsIncluded;

	public int NewIndex => this.CombinedBendDescriptor.Order;

	public BendSequenceItem(ICombinedBendDescriptorInternal combinedBendDescriptor)
	{
		this.CombinedBendDescriptor = combinedBendDescriptor;
	}
}
