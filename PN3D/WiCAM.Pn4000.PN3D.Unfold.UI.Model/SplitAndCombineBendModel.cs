using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.Model;

public class SplitAndCombineBendModel
{
	public ICombinedBendDescriptorInternal SelectedCommonBend { get; set; }

	public SplitAndCombineBendModel(ICombinedBendDescriptorInternal selectedCommonBend)
	{
		this.SelectedCommonBend = selectedCommonBend;
	}
}
