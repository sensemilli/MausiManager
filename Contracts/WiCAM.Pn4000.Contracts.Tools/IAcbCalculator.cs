using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IAcbCalculator
{
	void AutoSelectAcbDisks(List<IBendPositioning> bends, IAcbPunchPiece piece, Model modelUnfold, IMaterialArt material, IBendMachineTools? toolConfig);

	void AutoSelectAcbDisks(List<IBendPositioning> bends, IEnumerable<IAcbPunchPiece> pieces, Model modelUnfold, IMaterialArt material, IBendMachineTools? toolConfig);
}
