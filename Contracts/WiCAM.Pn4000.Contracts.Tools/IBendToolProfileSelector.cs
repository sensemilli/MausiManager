using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IBendToolProfileSelector
{
	(IToolProfile? upperTool, IToolProfile? lowerTool) GetFirstToolProfiles(IBendPositioning bend);

	IBendProfiles GetToolProfiles(IBendPositioning bend);

	IBendProfiles GetToolProfiles(IBendPositioning bend, bool addPreferredTools, bool addSuggestedTools, Dictionary<IAliasMultiToolProfile, bool>? piecesAvailable);

	IEnumerable<IPreferredProfile> GetAllPreferredProfiles();

	void GetPreferedGroupsBasic(IBendMachineTools bendMachine, int matGroupId, double thickness, out HashSet<int> punchGroupIds, out HashSet<int> dieGroupIds);
}
