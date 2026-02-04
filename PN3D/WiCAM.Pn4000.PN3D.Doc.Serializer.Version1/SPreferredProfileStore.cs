using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SPreferredProfileStore
{
	public List<Pair<Pair<int, int>, Pair<SPreferredProfile, ToolSelectionType>>> PreferredProfilesPerFG;
}
