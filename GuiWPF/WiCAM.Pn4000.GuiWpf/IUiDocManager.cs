using System;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf;

public interface IUiDocManager
{
	IDoc3d CurrentDoc { get; set; }

	event Action<IDoc3d, IDoc3d> CurrentDocChanged;
}
