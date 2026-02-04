using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IAcbActivator
{
	void TryActivate(IToolsAndBends toolsAndBends, Model unfoldModel, IMaterialArt material);

	void TryActivate(IToolSetups root, IBendPositioning bend, Model unfoldModel, IMaterialArt material);

	void TryActivate(IAcbPunchPiece acbTool, IToolsAndBends? toolsAndBends, Model unfoldModel, IMaterialArt material);

	AcbActivationResult CanActivate(IAcbPunchPiece acbTool, IBendPositioning bendPos, Model unfoldModel, IMaterialArt material);

	Matrix4d GetPartTransform(FaceGroup g, Model gModel, Model part, IBendPositioning bendPos);
}
