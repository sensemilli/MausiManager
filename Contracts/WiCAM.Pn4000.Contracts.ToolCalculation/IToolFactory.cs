using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.ToolCalculation;

public interface IToolFactory
{
	IToolListManager CreateToolListManager(IEnumerable<IToolListAvailable> toolLists);

	IToolsAndBends CreateToolsAndBends();

	IToolsAndBends CreateToolsAndBends(IReadOnlyCollection<ICombinedBendDescriptor> bends);

	IToolsAndBends CreateToolsAndBends(IReadOnlyCollection<ICombinedBendDescriptor> bends, IBendMachine? machine);

	void UpdateCbds(IToolsAndBends? toolsAndBends, IReadOnlyCollection<ICombinedBendDescriptor> bends);

	IBendPositioning CreateBendPositioning(ICombinedBendDescriptor cbd, IEnumerable<IBendDescriptor> allBendDescriptors, IEnumerable<ICombinedBendDescriptor> allPreviousBends);

	IToolSetups CreateToolSetup(IToolsAndBends toolsAndBends, IBendMachineTools bendMachineTools);

	IToolSetups DeserializeToolSetup(IToolsAndBends? toolsAndBends, double upperBeamXStart, double upperBeamXEnd, double lowerBeamXStart, double lowerBeamXEnd, int mountTypeIdUpperBeam, int mountTypeIdLowerBeam, int number);

	IToolSection CreateSection(IToolCluster cluster, bool upper);

	IToolSection CreateSection(IToolSection section);

	IToolCluster CreateSubCluster(IToolCluster parent);

	IToolPiece CreateToolPiece(IToolPieceProfile pieceProfile, IToolSection toolSection);

	IToolPiece CreateToolPiece(IToolPieceProfile pieceProfile, IToolPiece refPiece, int offset);

	void RemoveToolSetup(IToolsAndBends toolsAndBends, IToolSetups setup);

	void RemoveCluster(IToolCluster cluster, IToolsAndBends toolsAndBends);

	void RemoveCluster(IToolCluster cluster);

	void RemoveSection(IToolSection section);

	void RemoveToolPiece(IToolPiece piece);

	IEnumerable<IToolSection> SplitSections(IEnumerable<IToolPiece> pieces);

	IEnumerable<IToolSection> MergeSections(IEnumerable<IToolSection> sections);

	IBendPositioning DeserializeBendPositioning(IToolsAndBends? toolsAndBends, IBend bend, MachinePartInsertionDirection insertionDirection, bool isReversedGeometry);

	IBend DeserializeBend(IEnumerable<(int bendEntryId, int? subBendCount, int? subBendIndex)> faceGroupIdentifiers, IEnumerable<IRange> bendingZones, IEnumerable<(int fgId, double angle, double kFactor, double finalRadius)> bendStates, IEnumerable<IRange>? collisionIntervals, List<(double start, double end, int fgId, double fgOffset)>? startEndOffsetsBends);

	IToolSection CreateDummySection(IToolSection refSection);
}
