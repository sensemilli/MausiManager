using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.BendDoc;

internal class PnBndDocCopyOptions : IPnBndDocCopyOptions
{
	public ModelCopyMode EntryModelCopyMode { get; set; }

	public ModelCopyMode UnfoldModelCopyMode { get; set; }

	public ModelCopyMode BendModelCopyMode { get; set; }

	public ModelCopyMode MachineModelCopyMode { get; set; }
}
