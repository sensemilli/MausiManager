using System;

namespace WiCAM.Pn4000.Contracts.PnCommands;

public interface IPnCommandRegister
{
	event Action<IPnCommand> OnCommandCalledBegin;

	event Action<IPnCommand> OnCommandCalledEnded;

	void RegisterDynamicCommand(int group, string cmdStr, bool overwrite, Func<IPnCommandArg, F2exeReturnCode> cmd);

	void RegisterDynamicCommand(int group, string cmdStr, bool overwrite, Action<IPnCommandArg> cmd);
}
