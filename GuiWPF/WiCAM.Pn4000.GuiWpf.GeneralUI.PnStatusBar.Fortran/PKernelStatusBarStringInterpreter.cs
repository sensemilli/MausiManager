using System;
using System.Windows.Media;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

internal class PKernelStatusBarStringInterpreter
{
	public Brush Brush;

	public string Text;

	public Brush TextBrush;

	private static readonly SolidColorBrush _sbWhite = new SolidColorBrush(Colors.White);

	private static readonly SolidColorBrush _sbBlack = new SolidColorBrush(Colors.Black);

	public PKernelStatusBarStringInterpreter(string str)
	{
		GetInfo(str);
	}

	private static SolidColorBrush GetSolidBrush(Color cl)
	{
		if (cl.R == byte.MaxValue && cl.G == byte.MaxValue && cl.B == byte.MaxValue)
		{
			return _sbWhite;
		}
		if (cl.R == 0 && cl.G == 0 && cl.B == 0)
		{
			return _sbBlack;
		}
		return new SolidColorBrush(cl);
	}

	private void GetInfo(string str)
	{
		string text = str;
		Color cl = Color.FromArgb(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		str = str.TrimStart();
		if (str.Length > 6 && str.Substring(0, 3) == "[B:")
		{
			int num = str.IndexOf(']');
			GetRgbColorFromString(str.Substring(3, num - 3), text + " (point1)", ref cl);
			str = str.Substring(num + 1);
		}
		Brush = GetSolidBrush(cl);
		cl = Colors.Black;
		int num2;
		while ((num2 = str.IndexOf('[')) >= 0)
		{
			int num3 = str.IndexOf(']');
			if (num3 < num2)
			{
				break;
			}
			string text2 = str.Substring(num2 + 1, num3 - num2 - 1);
			if (text2 != string.Empty)
			{
				if (!GetRgbColorFromString(text2, text + " (point2)", ref cl))
				{
					break;
				}
				try
				{
					str = str.Substring(num3 + 1);
				}
				catch
				{
					str = string.Empty;
				}
			}
			if (text2 == string.Empty)
			{
				TextBrush = GetSolidBrush(cl);
				Text = str;
				return;
			}
		}
		TextBrush = GetSolidBrush(cl);
		Text = str;
	}

	private bool GetRgbColorFromString(string txt, string org, ref Color cl)
	{
		try
		{
			txt = txt.TrimStart();
			int num = txt.IndexOf(',');
			if (num < 0)
			{
				return false;
			}
			int num2 = txt.IndexOf(',', num + 1);
			if (num < 0)
			{
				return false;
			}
			byte r = Convert.ToByte(txt.Substring(0, num).Trim());
			byte g = Convert.ToByte(txt.Substring(num + 1, num2 - 1 - num).Trim());
			byte b = Convert.ToByte(txt.Substring(num2 + 1).Trim());
			cl = Color.FromRgb(r, g, b);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
