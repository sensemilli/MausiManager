using System.Windows;

namespace WiCAM.Pn4000.pn4.pn4FlowCenter;

public class PKernelFlowGlobalDataService : IPKernelFlowGlobalDataService
{
	public string LastPipeFunctionName { get; set; } = string.Empty;

	public Window MainWindow { get; set; }

	public object LastModel { get; set; }

	public int AllowCenterMenu { get; set; } = 1;

	public int ActiveProgramPart { get; set; } = -1;

	public int ActiveBendMachineID { get; set; } = -1;

	public int PnLanguage { get; set; } = -1;

	public int PnViewerMode { get; set; } = -1;

	public int PnInchMode { get; set; } = -1;

	public int PnMachine { get; set; } = -1;
}
