using System;

namespace WiCAM.Pn4000.PN3D.Interfaces;

public interface ICurrentBusyState
{
	bool IsBlockingRibbon { get; }

	bool IsWaitCursor { get; }

	event Action IsBlockingRibbonChanged;

	event Action IsWaitCursorChanged;
}
