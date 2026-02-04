namespace WiCAM.Pn4000.Contracts.PnCommands;

public interface IPnCommandsBend
{
	F2exeReturnCode SimulationPlay(IPnCommandArg arg);

	F2exeReturnCode SimulationPause(IPnCommandArg arg);

	F2exeReturnCode SimulationNext(IPnCommandArg arg);

	F2exeReturnCode SimulationEnd(IPnCommandArg arg);

	F2exeReturnCode SimulationPrevious(IPnCommandArg arg);

	F2exeReturnCode SimulationStart(IPnCommandArg arg);

	F2exeReturnCode SelectBendMachine(IPnCommandArg arg);

	F2exeReturnCode SetBendMachine(IPnCommandArg arg);

	void CalculateBendOrder(IPnCommandArg arg);

	F2exeReturnCode SetTools(IPnCommandArg arg);

	F2exeReturnCode SetTools(IPnCommandArg arg, int optionId);

	void AssignBendsToSections(IPnCommandArg arg);

	void EditTools3d(IPnCommandArg arg);

	F2exeReturnCode SetFingers(IPnCommandArg arg);

	F2exeReturnCode CalculateSimulation(IPnCommandArg arg);

	F2exeReturnCode ValidateSimulation(IPnCommandArg arg);

	void PlaySimulation(IPnCommandArg arg);

	F2exeReturnCode AutoBend(IPnCommandArg arg);

	void BendMachineConfig(IPnCommandArg arg);

	F2exeReturnCode BendPp(IPnCommandArg arg, bool createNc, bool createReport, string reportFormat);

	F2exeReturnCode BendPpNo(IPnCommandArg arg);

	void BendProgrammableVariables(IPnCommandArg arg);

	void ShowBendParameter(IPnCommandArg arg);

	F2exeReturnCode BendReportProductionPaperConvert(IPnCommandArg arg, string format);

	F2exeReturnCode BendReportProductionPaper(IPnCommandArg arg, string format);

	void BendProductionPaperSettings(IPnCommandArg arg);

	F2exeReturnCode BendPPSend(IPnCommandArg arg, bool nc, bool report);

	void ValidateFlangeLength(IPnCommandArg obj);

	F2exeReturnCode ExportGLB(IPnCommandArg arg);
}
