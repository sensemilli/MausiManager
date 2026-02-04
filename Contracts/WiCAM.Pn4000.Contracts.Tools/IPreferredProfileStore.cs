using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IPreferredProfileStore
{
	ToolSelectionType AssignPreferredProfileToCommonBend(IPreferredProfile preferredProfile, int entryFaceGroupId, int subBendIndex, ToolSelectionType toolSelectionType);

	IPreferredProfile? GetPreferredProfileForCommonBend(ICombinedBendDescriptor cbd, out ToolSelectionType tst);

	IPreferredProfile GetPreferredProfileForFaceGroup(int entryFaceGroupId, int subBendIndex, out ToolSelectionType toolSelectionType);

	(IToolProfile? upperTool, IToolProfile? lowerTool, ToolSelectionType tst) GetBestToolProfiles(ICombinedBendDescriptor cbd);

	(IPunchProfile? upperTool, IDieProfile? lowerTool, ToolSelectionType tst) GetBestPunchDieProfiles(ICombinedBendDescriptor cbd);

	List<IBendProfile> GetSuggestedTools(ICombinedBendDescriptor cbd);

	Dictionary<(int, int), Pair<IPreferredProfile, ToolSelectionType>> GetAllEntries();

	void Clear();

	bool IsEmpty();

	void CopyFrom(IPreferredProfileStore source);
}
