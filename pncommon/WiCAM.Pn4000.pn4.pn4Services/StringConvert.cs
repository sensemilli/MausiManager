using System.Globalization;

namespace WiCAM.Pn4000.pn4.pn4Services;

public static class StringConvert
{
	private static short __shortTemp;

	private static int __intTemp;

	private static long __longTemp;

	private static float __floatTemp;

	private static double __doubleTemp;

	public static short ToShort(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return short.MinValue;
		}
		StringConvert.__shortTemp = short.MinValue;
		short.TryParse(input, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out StringConvert.__shortTemp);
		return StringConvert.__shortTemp;
	}

	public static int ToInt(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return int.MinValue;
		}
		StringConvert.__intTemp = int.MinValue;
		int.TryParse(input, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out StringConvert.__intTemp);
		return StringConvert.__intTemp;
	}

	public static long ToLong(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return long.MinValue;
		}
		StringConvert.__longTemp = long.MinValue;
		long.TryParse(input, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out StringConvert.__longTemp);
		return StringConvert.__longTemp;
	}

	public static double ToDouble(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return double.MinValue;
		}
		StringConvert.__doubleTemp = double.MinValue;
		double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out StringConvert.__doubleTemp);
		return StringConvert.__doubleTemp;
	}

	public static float ToFloat(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return float.MinValue;
		}
		StringConvert.__floatTemp = float.MinValue;
		float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out StringConvert.__floatTemp);
		return StringConvert.__floatTemp;
	}
}
