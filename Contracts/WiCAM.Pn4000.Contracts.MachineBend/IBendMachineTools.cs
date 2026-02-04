using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IBendMachineTools
{
	IToolManager ToolManager { get; }

	IBendToolProfileSelector ProfileSelector { get; }

	int Number { get; }

	IEnumerable<IMultiToolProfile> MultiToolProfiles { get; }

	IEnumerable<IToolProfile> AllToolProfiles { get; }

	IEnumerable<IPunchProfile> UpperTools { get; }

	IEnumerable<IDieProfile> LowerTools { get; }

	IEnumerable<IPunchGroup> UpperGroups { get; }

	IEnumerable<IDieGroup> LowerGroups { get; }

	IEnumerable<IToolProfile> UpperAdapterProfiles { get; }

	IEnumerable<IToolProfile> LowerAdapterProfiles { get; }

	List<IDieFoldExtentionProfile> DieExtentions { get; }

	IEnumerable<IToolPieceProfile> AllToolPieceProfiles { get; }

	IEnumerable<ISensorDisk> SensorDisks { get; }

	IEnumerable<ISensorDiskProfile> SensorDiskProfiles { get; }

	int MountTypeIdUpperBeam { get; set; }

	int MountTypeIdLowerBeam { get; set; }

	double BeamGapMin { get; }

	double BeamGapMax { get; }

	double LowerBeamXStart { get; }

	double LowerBeamXEnd { get; }

	double UpperBeamXStart { get; }

	double UpperBeamXEnd { get; }

	string MachineProfileGeometry { get; set; }

	List<string> MachineFrameGeometries { get; set; }

	List<string> MachineUpperGeometries { get; set; }

	List<string> MachineLowerGeometries { get; set; }

	IToolPieceProfile GetPieceProfileById(int id);

	IToolPieceProfile? TryGetPieceProfileById(int? id);
}
