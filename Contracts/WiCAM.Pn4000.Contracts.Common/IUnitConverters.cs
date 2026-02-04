namespace WiCAM.Pn4000.Contracts.Common;

public interface IUnitConverters
{
	double ConvertInchToMm(double inch);

	double ConvertMmToInch(double mm);

	double? ConvertMmToInch(double? mm);
}
