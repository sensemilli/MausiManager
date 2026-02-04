using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.Model;

public class EditToolsAtSelectedBendModel
{
	public ICombinedBendDescriptorInternal SelectedCommonBend { get; }

	public IDoc3d Doc { get; }

	public EditToolsAtSelectedBendModel(IDoc3d doc, ICombinedBendDescriptorInternal selectedCommonBend)
	{
		this.Doc = doc;
		this.SelectedCommonBend = selectedCommonBend;
	}
}
