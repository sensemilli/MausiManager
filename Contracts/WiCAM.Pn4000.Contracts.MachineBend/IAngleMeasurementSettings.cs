namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IAngleMeasurementSettings
{
	double MaxDieHeight { get; set; }

	double MaxVWidth { get; set; }

	double MinDieHeight { get; set; }

	double MinLegLength { get; set; }
}
