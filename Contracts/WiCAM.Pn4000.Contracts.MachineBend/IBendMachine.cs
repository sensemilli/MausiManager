using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.BendPP;
using WiCAM.Pn4000.Contracts.ManualCameraStateView;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IBendMachine
{
	IBendMachineTools ToolConfig { get; }

	IBendMachineGeometry Geometry { get; }

	IPreferredProfileList PreferredProfileList { get; set; }

	IReadOnlyCollection<IPreferredProfile> PreferredProfiles { get; }

	IFingerStopSettings FingerStopSettings { get; }

	IFingerStopPriorities FingerStopPriorities { get; }

	IStopCombinations StopCombinationsLeftFinger { get; }

	IStopCombinations StopCombinationsRightFinger { get; }

	IFingerStopConfig FingerStopConfig { get; }

	IAngleMeasurementSettings AngleMeasurementSettings { get; }

	ILowerToolSystem LowerToolSystem { get; }

	IUpperToolSystem UpperToolSystem { get; }

	IPressBrakeData PressBrakeData { get; }

	IPressBrakeInfo PressBrakeInfo { get; }

	ICrowningTable CrowningTable { get; }

	ICrowningTable PressForceHemTable { get; }

	ICrowningTable PressForceFoldTable { get; }

	IUnfoldConfig UnfoldConfig { get; }

	IBendTable BendTable { get; set; }

	IPostProcessor? PostProcessor { get; }

	IPpMappings PpMappings { get; }

	IToolCalculationSettings ToolCalculationSettings { get; }

	IEnumerable<IToolListAvailable> ToolLists { get; }

	IPpScreenshotConfig PpScreenshotConfig { get; set; }

	string? PpSendBatch { get; }

	string Name { get; }

	string NCName { get; set; }

	string NCName2 { get; set; }

	string NCMachineType { get; set; }

	string NCMachineType2 { get; set; }

	int MachineNo { get; }

	string MachinePath { get; }

	string Remark1 { get; set; }

	string Remark2 { get; set; }

	IMigrationToolIdMapping MigrationToolMapping { get; }

	IBendMachine CopyStructure();

	void SetToolLists(IEnumerable<int> newToolListIds);

	string GetMappedMultiToolProfileName(IMultiToolProfile profile);

	string GetMappedToolProfileName(IToolProfile profile);

	int? GeneratePpNumber();
}
