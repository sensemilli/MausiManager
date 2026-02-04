namespace WiCAM.Pn4000.Helpers;

public class WiConvert
{
	public static string ReadString(string value, int start, int length)
	{
		if (value.Length - start < length)
		{
			length = value.Length - start;
		}
		return value.Substring(start, length);
	}

	public static int ReadToInt32(string value, int start, int length)
	{
		bool flag = false;
		int num = 0;
		int num2 = start + length;
		while (value[start] == ' ')
		{
			start++;
		}
		while (start < num2)
		{
			if (value[start] == '-')
			{
				flag = true;
				start++;
			}
			else
			{
				num = 10 * num + (value[start] - 48);
				start++;
			}
		}
		if (flag)
		{
			num = -num;
		}
		return num;
	}

	public static double ReadToDouble(string value, int start, int length)
	{
		bool flag = false;
		int num = start + length;
		double num2 = 0.0;
		while (value[start] == ' ')
		{
			start++;
		}
		while (value[start] != '.')
		{
			if (value[start] == '-')
			{
				flag = true;
				start++;
			}
			else
			{
				num2 = 10.0 * num2 + (double)(value[start] - 48);
				start++;
			}
		}
		start++;
		double num3 = 1.0;
		int num4 = 0;
		while (start < num && value[start] != ' ')
		{
			num4 = 10 * num4 + (value[start] - 48);
			num3 *= 0.1;
			start++;
		}
		num2 += (double)num4 * num3;
		if (flag)
		{
			num2 = 0.0 - num2;
		}
		return num2;
	}

	public static float ReadToFloat(string value, int start, int length)
	{
		bool flag = false;
		int num = start + length;
		float num2 = 0f;
		while (value[start] == ' ')
		{
			start++;
		}
		while (value[start] != '.')
		{
			if (value[start] == '-')
			{
				flag = true;
				start++;
			}
			else
			{
				num2 = 10f * num2 + (float)(value[start] - 48);
				start++;
			}
		}
		start++;
		float num3 = 1f;
		int num4 = 0;
		while (start < num && value[start] != ' ')
		{
			num4 = 10 * num4 + (value[start] - 48);
			num3 *= 0.1f;
			start++;
		}
		num2 += (float)num4 * num3;
		if (flag)
		{
			num2 = 0f - num2;
		}
		return num2;
	}
}
