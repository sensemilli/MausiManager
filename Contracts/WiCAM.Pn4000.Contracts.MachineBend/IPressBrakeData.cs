namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IPressBrakeData
{
	double AbsolutClampPosition { get; set; }

	string PpNameFormatSingle { get; set; }

	string PpNameFormatMulti { get; set; }

	bool PpNameAuto { get; set; }

	int AddDateTimePPName { get; set; }

	int AddGeometryToNcFile { get; set; }

	int AntiDeflectionsystem { get; set; }

	int BackLiftingAid { get; set; }

	double DefaultClampingPositionFactor { get; set; }

	double DefaultMachineSpeedChangePosition { get; set; }

	string FingerStopSystemType { get; set; }

	int FootToggleCount { get; set; }

	int FootToggleDefault { get; set; }

	int StepChangeDefault { get; set; }

	int FrontLiftingAid { get; set; }

	int Length { get; set; }

	string MachineNameOrNumberOnController { get; set; }

	string Manufacturer { get; set; }

	int MeasurementAngle { get; set; }

	int MinBeamOpeningHeight { get; set; }

	string Name { get; set; }

	string NCDirectory { get; set; }

	bool NCDirectoryUserSelect { get; set; }

	int OpenNcFileAfterCreation { get; set; }

	string OutputNCFileExtension { get; set; }

	string ReportDirectory { get; set; }

	int SafetyOffsetBeamOpeningHeight { get; set; }

	int TotalHeight { get; set; }

	int TotalLength { get; set; }

	string Type { get; set; }

	double PressForceMin { get; set; }

	double PressForceMax { get; set; }

	bool LimitPressForceToMaxPressForceOfTools { get; set; }
}
