using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.PN3D.Converter;

public class UnitConverters : IUnitConverters
{
	public double ConvertInchToMm(double inch)
	{
		return Convert.InchToMm(inch);
	}

	public double ConvertMmToInch(double mm)
	{
		return Convert.MmToInch(mm);
	}

	public double? ConvertMmToInch(double? mm)
	{
		if (!mm.HasValue)
		{
			return null;
		}
		return Convert.MmToInch(mm.Value);
	}
}
