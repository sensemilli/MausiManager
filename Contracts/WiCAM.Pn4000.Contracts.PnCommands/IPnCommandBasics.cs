using System;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.Contracts.PnCommands;

public interface IPnCommandBasics
{
	void CmdNotAvailable(IPnCommandArg arg);

	void DoCmd(IPnCommandArg arg, Action<IPnCommandArg> method, bool showWaitCursor = true, bool blockRibbon = true, bool mainThread = true, [CallerMemberName] string CommandKey = "", [CallerFilePath] string CommandFilePath = "")
	{
		this.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			method(arg);
			return F2exeReturnCode.Undefined;
		}, showWaitCursor, blockRibbon, mainThread, CommandKey, CommandFilePath);
	}

	F2exeReturnCode DoCmd(IPnCommandArg arg, Func<IPnCommandArg, F2exeReturnCode> func, bool showWaitCursor = true, bool blockRibbon = true, bool mainThread = true, [CallerMemberName] string CommandKey = "", [CallerFilePath] string CommandFilePath = "");

	IPnCommandArg CreateCommandArg(IPnBndDoc doc, string commandStr = "");
}
