using System;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

[Flags]
public enum ChangesResult : uint
{
	NoChanges = 0u,
	RecalculateFingers = 1u,
	RecalculateTools = 2u,
	ReloadMachine = 4u,
	ReunfoldModel = 8u
}
