using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolsToMachineModel
{
	void RemoveTools(IPnBndDoc doc);

	void AddTools(IToolCluster toolSetups, IPnBndDoc doc);

	void PositionPart(IBendPositioning bend, IPnBndDoc doc);

	Model AddToolPiece(IToolPiece toolPiece, IPnBndDoc doc);

	Model AddToolPiece(IToolPiece toolPiece, IBendMachine bendMachine);

	Model GetToolSystemModel(IPnBndDoc doc, bool upper);

	Model CreateExtrudedProfile(IPnBndDoc doc, IMultiToolProfile profile, double length);

	Model CreateExtrudedProfile(string fullFilename, double length, List<double>? heights = null);

	Model CreateExtrudedProfile(string fullFilename, double length, double zMin, double zMax);

	IEnumerable<(MachineReferenceTypes refSys, ProfileShell model, string frameId)> CreateExtrudedMachineProfiles(IBendMachine bendMachine, double extrudedLength);

	void VisualizeModelTemp(Model model, bool visible);

	Model GetModelForProfile(IBendMachine bendMachine, IToolPieceProfile pieceProfile);
}
