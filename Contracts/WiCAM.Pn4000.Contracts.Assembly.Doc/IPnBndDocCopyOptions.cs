using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IPnBndDocCopyOptions
{
	ModelCopyMode EntryModelCopyMode { get; set; }

	ModelCopyMode UnfoldModelCopyMode { get; set; }

	ModelCopyMode BendModelCopyMode { get; set; }

	ModelCopyMode MachineModelCopyMode { get; set; }
}
