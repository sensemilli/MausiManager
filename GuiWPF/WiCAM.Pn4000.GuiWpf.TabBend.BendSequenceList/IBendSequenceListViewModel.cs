using System;
using System.Collections.Generic;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiWpf.UiBasic;

namespace WiCAM.Pn4000.GuiWpf.TabBend.BendSequenceList;

internal interface IBendSequenceListViewModel
{
	RadObservableCollection<ComboboxEntry<int>> ToolSetups { get; }

	RadObservableCollection<ComboboxEntry<int?>> StepChangeModes { get; }

	RadObservableCollection<BendSequenceListViewModel.BendViewModel> Bends { get; }

	event Action OpenBendContextMenu;

	event Action RepaintModels;

	void Dispose();

	void ColorModelParts(IPaintTool painter);

	void ChangeRadius(List<BendSequenceListViewModel.BendViewModel> cbd, double newRadius);

	void ChangeDeduction(List<BendSequenceListViewModel.BendViewModel> cbd, double newDeduction);

	void RefreshAllProps();
}
