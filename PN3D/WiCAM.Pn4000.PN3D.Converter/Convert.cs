namespace WiCAM.Pn4000.PN3D.Converter;

public static class Convert
{
	public static double MmToInch(double? value)
	{
		if (value.HasValue)
		{
			return value.Value / 25.4;
		}
		return 0.0;
	}

	public static double InchToMm(double? value)
	{
		if (value.HasValue)
		{
			return value.Value * 25.4;
		}
		return 0.0;
	}
}
