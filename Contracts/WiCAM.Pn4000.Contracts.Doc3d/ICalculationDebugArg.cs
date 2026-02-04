using System;
using WiCAM.Pn4000.Contracts.ToolCalculation;

namespace WiCAM.Pn4000.Contracts.Doc3d;

public interface ICalculationDebugArg
{
	bool PauseEnabled { get; set; }

	ILogStep Log { get; }

	ILogStep CurrentLog { get; }

	event Action StartWaiting;

	void ContinueCalculation();

	void WaitAndContinue();

	ILogStep CreateSubLog(string message);

	void CloseSubLog(ILogStep? log);
}
