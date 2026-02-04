using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.ToolCalculation;

public interface IToolCalculationOption
{
	public enum enum1
	{
		PreferTopPriority = 0,
		PreferSameTools = 1
	}

	[Flags]
	public enum enum2
	{
		AddToExistingToolStationsDontModify = 1,
		AddToExistingToolStationsAndExtend = 2,
		AddToExistingToolStationsAndRemove = 4,
		AddToExistingToolStationsAndModify = 6,
		CreateSingleToolStation = 8,
		CreateMultipleToolStations = 0x10,
		CreateNewToolStationForEachBend = 0x20
	}

	public enum enum3
	{
		UsePreferredToolsIfExistsOrFail = 0,
		PreferPreferredTools = 1,
		IgnorePreferredTools = 2
	}

	public enum enum4
	{
		AllToolsInMachine = 0,
		AllowMultipleToolSets = 1,
		TandemMachine = 2,
		IgnoreMachineLength = 3
	}

	public enum ProfileOrders
	{
		OrderByPriority = 0,
		OrderByMostBendAggregation = 1
	}

	public enum WorkingHeightSettings
	{
		IgnoreWorkingHeights = 0,
		ConstTotalWorkingHeight = 1,
		CalculateWorkingHeightForBends = 2
	}

	ICalculationArg CalculationArg { get; set; }

	List<IToolListAvailable> ToolLists { get; set; }

	IToolListManager ToolListManager { get; set; }

	string Description { get; set; }

	int BruteforceBendOrderAmount { get; set; }

	List<IBendSequenceOrder> BendOrderStrategyGuids { get; set; }

	bool CalculateAllBendOrders { get; set; }

	double TpvWeightTotalWithSpace { get; set; }

	double TpvWeightTotalWithoutSpace { get; set; }

	double TpvWeightSumOnlyPresence { get; set; }

	double CollisionExtension { get; set; }

	bool CollisionCalcOverbend { get; set; }

	bool RoundBendLengthToToolLengths { get; set; }

	bool IgnoreToolPieceNumbers { get; set; }

	bool UsePreferredTools { get; set; }

	bool UseSuggestedTools { get; set; }

	IPnBndDoc Doc { get; set; }

	bool AllowToolStationsWithMissingToolCoverage { get; set; }

	bool AllowToolStationsWithCollisions { get; set; }

	double AdapterMaxGapLength { get; set; }

	double AdapterMaxTotalGapsRelative { get; set; }

	double BendCoverageMaxBorderGaps { get; set; }

	double BendCoverageMaxCenterGaps { get; set; }

	double BendCoverageMinTotalLength { get; set; }

	double LegCoverageMaxBorderGapsRel { get; set; }

	double LegCoverageMinTotalLengthRel { get; set; }

	double LegLengthFactor { get; set; }

	bool IgnoreLegCoverage { get; set; }

	bool IgnoreForce { get; set; }

	bool IgnoreToolCollision { get; set; }

	bool IgnoreToolProfile { get; set; }

	bool IgnoreMountTypeInValidation { get; set; }

	double PreferredSpaceBetweenClusters { get; set; }

	double PreferredToolCollisionMargin { get; set; }

	double PreferredToolBendMargin { get; set; }

	double NecessarySpaceBetweenTools { get; set; }

	double MaxPartLength { get; set; }

	double BadBendScoreLimit { get; set; }

	bool SimplifyModel { get; set; }

	bool BreakAfterFirstPartialSolution { get; set; }

	bool CalculatePartialSolutionIfNoCompletePossible { get; set; }

	bool SnapPiecesToOthers { get; set; }

	bool AllowInvalidPositionsForPieces { get; set; }

	bool AllowAdapter { get; set; }

	bool AllowAdapterStacking { get; set; }

	ProfileOrders ProfileOrder { get; set; }

	int OldToolPresenceVectorMergeTries { get; set; }

	ToolPieceSortingStrategies DefaultSortingStrategy { get; set; }

	bool AutoImplementHeelTools { get; set; }

	bool PreferBiggerToolStationsWithLowerAmounts { get; set; }

	int AddSensorTools { get; set; }

	bool PostCalcRemoveUnnecessaryClusters { get; set; }

	bool PostCalcAutoCalcSensorDiscs { get; set; }

	bool PostCalcAutoCalcFinger { get; set; }

	bool PostCalcAutoCalcAcb { get; set; }

	int? ForcedUpperAdapterPieceId { get; set; }

	double ForcedUpperAdapterOffset { get; set; }

	double ForcedUpperAdapterSpace { get; set; }

	int ForcedUpperAdapterAmount { get; set; }

	int? ForcedLowerAdapterPieceId { get; set; }

	double ForcedLowerAdapterOffset { get; set; }

	double ForcedLowerAdapterSpace { get; set; }

	int ForcedLowerAdapterAmount { get; set; }

	double NewStationUpperToolOffset { get; set; }

	double NewStationLowerToolOffset { get; set; }

	double WorkingHeightMaxDelta { get; set; }

	bool TryAddAdditionalBends { get; set; }

	bool SplitClusterToMultipleToolSetups { get; set; }

	double CombinedBendMergeBendIntervals { get; set; }

	bool UseCollisionMemory { get; set; }

	IToolCalculationOption CreateClone();

	void ApplySettingsFrom(IToolCalculationOption source);
}
