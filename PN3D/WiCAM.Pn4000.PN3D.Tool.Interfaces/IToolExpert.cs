using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.CommonBend;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Tool.Interfaces;

public interface IToolExpert
{
	void Init(IDoc3d doc);

	IPreferredProfile GetSuggestedTools(ICombinedBendDescriptor cbd);

	IPreferredProfile GetPreferredTools(ICombinedBendDescriptor cbd);

	IPunchGroup GetPunchGroupForFaceGroup(ICombinedBendDescriptorInternal bendDesc, out NoToolsFoundReason reason);

	IDieGroup GetDieGroupForFaceGroup(ICombinedBendDescriptorInternal bendDesc, out NoToolsFoundReason reason);

	IPreferredProfile GetToolGroupsForBend(ICombinedBendDescriptorInternal cbd);

	double GetHemSplitAngle(ICombinedBendDescriptor cbd);
}
