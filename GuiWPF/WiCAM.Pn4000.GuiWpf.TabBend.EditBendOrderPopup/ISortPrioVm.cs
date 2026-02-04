using System.ComponentModel;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditBendOrderPopup;

internal interface ISortPrioVm
{
	BendSequenceSorts? SortType { get; set; }

	string Description { get; }

	string DescriptionLong { get; }

	event PropertyChangedEventHandler PropertyChanged;
}
