using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.Contracts.BendServices;

public interface IUndo3dService
{
	void Reset(IPnBndDoc doc);

	void Save(IPnBndDoc doc, string messageLastAction);

	bool Undo(IPnBndDoc doc);

	bool Redo(IPnBndDoc doc);

	IEnumerable<IUndo3dSave> GetSavesUndo(IPnBndDoc doc);

	IEnumerable<IUndo3dSave> GetSavesRedo(IPnBndDoc doc);
}
