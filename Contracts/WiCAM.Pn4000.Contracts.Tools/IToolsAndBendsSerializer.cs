using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolsAndBendsSerializer
{
	string Convert(ISToolSetups setups);

	ISToolSetups ConvertBack(string setups);

	ISToolSetups Convert(IToolSetups setups);

	IToolSetups ConvertBack(ISToolSetups setups, IBendMachineTools bendMachine);

	ISToolsAndBends Convert(IToolsAndBends toolsAndBends);

	IToolsAndBends ConvertBack(ISToolsAndBends toolsAndBends, IBendMachineTools bendMachine);
}
