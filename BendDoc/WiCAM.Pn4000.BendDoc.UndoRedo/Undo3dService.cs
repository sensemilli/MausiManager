using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendServices;

namespace WiCAM.Pn4000.BendDoc.UndoRedo;

internal class Undo3dService : IUndo3dService
{
	public void Reset(IPnBndDoc doc)
	{
		doc.Factorio.Resolve<IUndo3dDocService>().Reset();
	}

	public void Save(IPnBndDoc doc, string messageLastAction)
	{
		doc.Factorio.Resolve<IUndo3dDocService>().Save(messageLastAction);
	}

	public bool Undo(IPnBndDoc doc)
	{
		return doc.Factorio.Resolve<IUndo3dDocService>().Undo();
	}

	public bool Redo(IPnBndDoc doc)
	{
		return doc.Factorio.Resolve<IUndo3dDocService>().Redo();
	}

	public IEnumerable<IUndo3dSave> GetSavesUndo(IPnBndDoc doc)
	{
		return doc.Factorio.Resolve<IUndo3dDocService>().GetSavesUndo();
	}

	public IEnumerable<IUndo3dSave> GetSavesRedo(IPnBndDoc doc)
	{
		return doc.Factorio.Resolve<IUndo3dDocService>().GetSavesRedo();
	}
}
