using System;
using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.BendServices;

public interface IUndo3dDocService
{
	event Action SavesChanged;

	void Reset();

	void Save(string messageLastAction);

	bool Undo();

	bool Redo();

	bool Goto(IUndo3dSave save);

	IEnumerable<IUndo3dSave> GetSavesUndo();

	IEnumerable<IUndo3dSave> GetSavesRedo();
}
