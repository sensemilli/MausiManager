using System;

namespace WiCAM.Pn4000.Contracts.BendServices;

public interface IUndo3dSave
{
	string DescriptionLastAction { get; }

	DateTime Timestamp { get; }

	object Data { get; }
}
