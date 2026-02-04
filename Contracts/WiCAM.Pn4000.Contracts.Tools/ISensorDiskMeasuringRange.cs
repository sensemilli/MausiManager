namespace WiCAM.Pn4000.Contracts.Tools;

public interface ISensorDiskMeasuringRange
{
	double MaxTensileStrength { get; set; }

	double MinThickness { get; set; }

	double MaxThickness { get; set; }

	double MinAngle { get; set; }

	double MaxAngle { get; set; }
}
