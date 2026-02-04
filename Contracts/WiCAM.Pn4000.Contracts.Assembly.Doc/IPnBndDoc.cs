using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.Validations;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IPnBndDoc : IPnBndDocExt
{
	bool IsAssemblyLoading { get; set; }

	bool IsDevImport { get; }

	bool FreezeCombinedBendDescriptors { get; set; }

	new IReadOnlyList<ICombinedBendDescriptorInternal> CombinedBendDescriptors { get; }

	IReadOnlyList<ICombinedBendDescriptor> IPnBndDocExt.CombinedBendDescriptors => this.CombinedBendDescriptors.Cast<ICombinedBendDescriptor>().ToList();

	IBendMachineSimulationBasic? BendMachineConfig { get; }

	bool ToolCalculationEnabled { get; set; }

	IPreferredProfileStore PreferredProfileStore { get; }

	ISimulationThread BendSimulation { get; set; }

	bool IsReconstructed { get; }

	bool SimulationValidated { get; }

	bool NcFileGenerated { get; set; }

	bool ReportGenerated { get; set; }

	bool NcDataSend { get; set; }

	List<ValidationResult> ValidationResults { get; set; }

	new string SavedFileName { get; set; }

	new int SavedArchiveNumber { get; set; }

	bool MachinePartiallyLoadedForUnfold { get; }

	bool MachineFullyLoaded { get; set; }

	string? MachinePath { get; set; }

	bool IsSerialized { get; set; }

	bool HasModel { get; }

	bool HasRadiusChangeErrors { get; }

	bool HasToolSetups { get; }

	bool HasFingers { get; }

	bool HasAnyFingers { get; }

	bool HasAllFingers { get; }

	bool HasBendStepsCalculated { get; set; }

	bool HasSimulationCalculated { get; set; }

	IReadOnlyList<string> NamesPpBase { get; }

	string NamePPBase { get; set; }

	string NamePPSuffix { get; set; }

	SetNcTimestampTypes NamePpTimestamps { get; set; }

	IMessageLogDoc MessageDisplay { get; }

	DocState State { get; }

	IToolManager? ToolManager { get; }

	ToolSelectionType ToolSelectionType { get; set; }

	string CreationUserName { get; set; }

	string LastModifiedUserName { get; set; }

	DateTime CreationDate { get; set; }

	DateTime LastModified { get; set; }

	bool SafeModeUnfold { get; set; }

	List<int> SafeModeUnfoldErrorBendOrder { get; set; }

	[Obsolete("get by di")]
	IConfigProvider ConfigProvider { get; }

	List<SimulationInstance> SimulationInstancesAdditionalParts { get; set; }

	SimulationInstance? CurrentSimulationInstancesAdditionalPart { get; }

	bool KFactorWarningsError { get; }

	bool KFactorWarningsAcceptedByUser { get; set; }

	bool PnMaterialByUser { get; set; }

	IScopedFactorio Factorio { get; }

	event Action<IPnBndDoc> Closed;

	event Action<Model, Model> EntryModel3DChanged;

	event Action<Model, Model> ModifiedEntryModel3DChanged;

	event Action<Model, Model> UnfoldModel3DChanged;

	event Action<Model, Model> BendModel3DChanged;

	event Action<Model, Model> View3DModelChanged;

	event Action<ISimulationThread, ISimulationThread> BendSimulationChanged;

	event Action BendMachineChanged;

	event Action CombinedBendDescriptorsChanged;

	event Action<IToolsAndBends, IToolsAndBends> ToolsAndBendsChanged;

	event Action<IPnBndDoc> FingerChanged;

	event Action<IPnBndDoc> RefreshSimulation;

	void OnToolsChanged();

	void OnBendDataChanged();

	void ResetTools();

	void CreateToolsAndBends();

	void ResetFingers();

	void ResetLiftingAids();

	void CombinedBendsAutoSort(IBendSequenceOrder strategy);

	void CombinedBendsAutoSortBruteforce(int bruteforceIndex);

	bool ReconstructFromFace(Face entryFace, Model entryFaceModel);

	bool SetTopFace(Model model, Face face, Model faceModel);

	bool ApplyBendOrder(IReadOnlyList<ICombinedBendDescriptorInternal> order, bool autoCombine = true);

	bool ApplyBendSequence(IPnBndDoc doc);

	bool SplitBend(int bendIndex, double splitValue);

	bool MergeSplitBends(List<int> bendIndices);

	bool CanSplitBendsMerge(List<int> bendIndices);

	bool ChangeSplitValue(int bendIndex, double splitValue);

	bool ChangeManualRadius(int bendIndex, double radius);

	bool ChangeManualRadius(List<int> bendIndex, double radius);

	bool ChangeManualBendDeduction(int bendIndex, double bd);

	bool ChangeManualBendDeduction(List<int> bendIndex, double bd);

	bool ConvertBendToStepBend(int bendIndex);

	bool ConvertStepBendToBend(int bendIndex);

	bool ChangeStepBendProperties(int bendIndex, int numSteps, double radius, double? bendDeductionMiddle, double? bendDeductionStartEnd);

	bool MergeCombinedBendsInUnfoldModel(int bendIdx0, int bendIdx1);

	bool MergeCombinedBendsInBendModel(int bendIdx0, int bendIdx1);

	bool SplitCombinedBends(int bendIdx0, int splitIndex);

	bool SplitCombinedBends(int bendIdx0, List<int> splitIndices, bool update);

	bool SplitCombinedBends();

	void SetPartInsertionDirectionForBend(int bendIdx, MachinePartInsertionDirection dir);

	void DisableSimulationCalculated();

	bool UpdateDoc();

	void AssignSuggestedTools();

	void AssignPreferredTools();

	bool ValidateTools(out string message, out bool maxForceOk, out bool toolOverlapOk, out bool toolSavetyDistOk, out bool toolsFitInMachine);

	void UpdateGeneralInfo();

	void UpdateBendMachineInfo();

	IBendTable GetApplicableBendTable(out bool isMachineSpecific);

	bool CheckSelfCollisionUnfoldModel();

	void Save(string filename);

	void SetDefautMachine(bool viewStyle);

	void CenterModel(Model model);

	void ChangeStartFaceGroupAndSide(int newStartFaceGroupId, int side);

	void SetModelDefaultColors();

	void SetModelDefaultColors(Model model);

	void ColorPurchasedParts(Model model, Color colPp);

	UiModelType GetModelType(Model model);

	F2exeReturnCode ReconstructIrregularBends(bool experimental = false);

	void ReApplyAdditionalParts();

	void SetSmallPartType(bool isSmallPart);

	F2exeReturnCode CheckWarningKFactorMaterialByUser();

	bool HasRequiredUserComments();

	void CalculateFingers();

	void CalculateFingers(List<int> bendOrders);

	void FingerChangedInvoke();

	void RecalculateSimulation();

	bool IsUpdateDocNeeded();

	string GetAutoPpName(bool multi, int subNo, int? numberPp = null);

	IEnumerable<(int subNo, List<ICombinedBendDescriptor> subBends)> BendsPerPp(IEnumerable<ICombinedBendDescriptor> bends);

	IPnBndDoc Copy(ModelCopyMode entry, ModelCopyMode unfold, ModelCopyMode bend, ModelCopyMode machine);

	void Init(string filename, bool isAssemblyLoading = false, string? assemblyGuid = null, bool isDevImport = false);

	List<List<ICombinedBendDescriptorInternal>> GroupCompatibleBends(int step);

	bool TryMergeWithNext(ICombinedBendDescriptorInternal cbd);

	bool CanMergeWithNext(ICombinedBendDescriptorInternal cbd);
}
