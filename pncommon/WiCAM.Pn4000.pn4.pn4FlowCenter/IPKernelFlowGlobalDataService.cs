using System.Windows;

namespace WiCAM.Pn4000.pn4.pn4FlowCenter;

public interface IPKernelFlowGlobalDataService
{
	string LastPipeFunctionName { get; set; }

	Window MainWindow { get; set; }

	object LastModel { get; set; }

	int AllowCenterMenu { get; set; }

	int ActiveProgramPart { get; set; }

	int ActiveBendMachineID { get; set; }

	int PnLanguage { get; set; }

	int PnViewerMode { get; set; }

	int PnInchMode { get; set; }

	int PnMachine { get; set; }
}
