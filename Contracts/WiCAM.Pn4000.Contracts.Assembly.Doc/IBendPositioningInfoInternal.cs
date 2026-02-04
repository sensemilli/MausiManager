using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IBendPositioningInfoInternal : IBendPositioningInfo
{
	bool CalcIsReversedGeometry(MachinePartInsertionDirection dir);

	void UpdateBendOffsets(List<FaceGroup> bends, Model unfoldModel);
}
