using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IBendSelection
{
	IBendPositioning? CurrentBendHovering { get; set; }

	IBendPositioning? CurrentBend { get; set; }

	IEnumerable<IBendPositioning> SelectedBends { get; }

	IPnBndDoc CurrentDoc { get; }

	IToolsAndBends ToolsAndBends { get; }

	event Action<IBendPositioning?>? CurrentBendChanged;

	event Action<IBendPositioning?>? CurrentBendHoveringChanged;

	event Action? SelectionChanged;

	event Action? DataChanged;

	ICombinedBendDescriptorInternal GetCbd(IBendPositioning bend);

	IBendPositioning? GetBendPositioning(ICombinedBendDescriptor cbd);

	void SetData(IPnBndDoc doc, IToolsAndBends toolsAndBends);

	void SetSelection(IBendPositioning bend, bool isSelected);

	void ToggleSelection(IBendPositioning bend);

	void UnselectAll();

	bool IsSelected(IBendPositioning bend);

	void SetCurrentBendBySimulation(int? newOrder);

	void SetCurrentBendHoveredByOrder(int? newOrder);

	void RefreshCurrentBend();

	void RefreshSimulation();
}
