using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.PnCommand;

public static class DirtyExtension
{
	public static IDoc3d doc(this IPnCommandArg arg)
	{
		return (IDoc3d)arg.Doc;
	}
}
