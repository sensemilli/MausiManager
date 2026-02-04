using System.Collections.Generic;
using System.Linq;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolAccessor
{
	IEnumerable<(IToolPiece piece, IToolProfile? profile)> GetPunchesWithAdapters(IBendPositioning bend);

	IEnumerable<(IToolPiece piece, IToolProfile? profile)> GetDiesWithAdapters(IBendPositioning bend);

	IEnumerable<(IToolPiece piece, IToolProfile? profile)> GetPunchesWithAdaptersInRange(IToolSetups root, double xStart, double xEnd, double zOffset);

	IEnumerable<(IToolPiece piece, IToolProfile? profile)> GetDiesWithAdaptersInRange(IToolSetups root, double xStart, double xEnd, double iOffset, double zOffset);

	IEnumerable<(IToolPiece piece, IToolProfile? profile)> GetPiecesWithAdaptersInRange(IEnumerable<(IToolPiece, IToolProfile?)> input);

	IEnumerable<(IToolPiece piece, IToolProfile? profile)> GetPunchesInRange(IToolSetups root, double xStart, double xEnd, double zOffset);

	IEnumerable<(IToolPiece piece, IToolProfile? profile)> GetDiesInRange(IToolSetups root, double xStart, double xEnd, double iOffset, double zOffset);

	IEnumerable<(IToolPiece piece, IToolProfile profile)> GetAdapters(IToolPiece piece);

	IOrderedEnumerable<(IToolPiece piece, Dictionary<IToolProfile, List<IBendPositioning>> profilesForBends)> GetAllPunchesAndAdapters(List<IBendPositioning> bends);

	IOrderedEnumerable<(IToolPiece piece, Dictionary<IToolProfile, List<IBendPositioning>> profilesForBends)> GetAllDiesAndAdapters(List<IBendPositioning> bends);

	IOrderedEnumerable<(IToolSection section, Dictionary<IToolProfile, List<IBendPositioning>> profilesForBends)> GetAllPuncheAndAdapterSections(List<IBendPositioning> bends);

	IOrderedEnumerable<(IToolSection section, Dictionary<IToolProfile, List<IBendPositioning>> profilesForBends)> GetAllDieAndAdapterSections(List<IBendPositioning> bends);

	List<IToolHierarchyNode> GetUpperHierarchy(IToolSetups root);

	List<IToolHierarchyNode> GetLowerHierarchy(IToolSetups root);

	List<IToolHierarchyNode> GetUpperHierarchyPunchToBeam(IToolSetups root);

	List<IToolHierarchyNode> GetLowerHierarchyDieToTable(IToolSetups root);
}
