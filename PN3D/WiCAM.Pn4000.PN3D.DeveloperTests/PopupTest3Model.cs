using System.Collections.Generic;
using System.Collections.ObjectModel;
using WiCAM.Pn4000.Popup;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

public class PopupTest3Model
{
	public List<View3DControlPart> Parts { get; set; }

	public ObservableCollection<DisassemblyPart> DisassemblyParts { get; set; }
}
