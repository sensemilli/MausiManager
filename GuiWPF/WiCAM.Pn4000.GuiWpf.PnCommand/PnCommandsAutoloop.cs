using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Pipes;

namespace WiCAM.Pn4000.GuiWpf.PnCommand;

internal class PnCommandsAutoloop : IPnCommandsAutoloop
{
	private readonly PN3DRootPipe _rootPipe;

	private IPnCommandBasics Cmd { get; }

	public PnCommandsAutoloop(IPnCommandBasics cmd, PN3DRootPipe rootPipe)
	{
		_rootPipe = rootPipe;
		Cmd = cmd;
	}

	private IPnCommandArg CreateArg()
	{
		return new PnCommandArg(null, null)
		{
			IsReadOnly = false
		};
	}
}
