using System;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

namespace WiCAM.Pn4000.BendDoc.UndoRedo;

internal class Undo3dSave : IUndo3dSave
{
	public string DescriptionLastAction { get; }

	public DateTime Timestamp { get; }

	public object Data { get; }

	public Undo3dSave(string descriptionLastAction, SPnBndDoc data)
	{
		this.DescriptionLastAction = descriptionLastAction;
		this.Data = data;
		this.Timestamp = DateTime.Now;
	}
}
