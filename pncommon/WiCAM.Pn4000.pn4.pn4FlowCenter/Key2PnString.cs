using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace WiCAM.Pn4000.pn4.pn4FlowCenter;

public static class Key2PnString
{
	public static int FromWPFShortCutKey(global::System.Windows.Input.KeyEventArgs e)
	{
		int result = -1;
		if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
		{
			switch (e.Key)
			{
			case Key.A:
				result = 65;
				break;
			case Key.B:
				result = 66;
				break;
			case Key.C:
				result = 67;
				break;
			case Key.D:
				result = 68;
				break;
			case Key.E:
				result = 69;
				break;
			case Key.F:
				result = 70;
				break;
			case Key.G:
				result = 71;
				break;
			case Key.H:
				result = 72;
				break;
			case Key.I:
				result = 73;
				break;
			case Key.J:
				result = 74;
				break;
			case Key.K:
				result = 75;
				break;
			case Key.L:
				result = 76;
				break;
			case Key.M:
				result = 77;
				break;
			case Key.N:
				result = 78;
				break;
			case Key.O:
				result = 79;
				break;
			case Key.P:
				result = 80;
				break;
			case Key.Q:
				result = 81;
				break;
			case Key.R:
				result = 82;
				break;
			case Key.S:
				result = 83;
				break;
			case Key.T:
				result = 84;
				break;
			case Key.U:
				result = 85;
				break;
			case Key.V:
				result = 86;
				break;
			case Key.W:
				result = 87;
				break;
			case Key.X:
				result = 88;
				break;
			case Key.Y:
				result = 89;
				break;
			case Key.Z:
				result = 90;
				break;
			case Key.D0:
				result = 48;
				break;
			case Key.D1:
				result = 49;
				break;
			case Key.D2:
				result = 50;
				break;
			case Key.D3:
				result = 51;
				break;
			case Key.D4:
				result = 52;
				break;
			case Key.D5:
				result = 53;
				break;
			case Key.D6:
				result = 54;
				break;
			case Key.D7:
				result = 55;
				break;
			case Key.D8:
				result = 56;
				break;
			case Key.D9:
				result = 57;
				break;
			}
		}
		return result;
	}

	public static string FromWPF(global::System.Windows.Input.KeyEventArgs e)
	{
		string result = string.Empty;
		switch (e.Key)
		{
		case Key.Up:
			result = "38 1 328";
			break;
		case Key.Down:
			result = "40 1 336";
			break;
		case Key.Left:
			result = "37 1 331";
			break;
		case Key.Right:
			result = "39 1 333";
			break;
		case Key.Escape:
			result = "27 1 1";
			break;
		case Key.Return:
			result = "13 1 28";
			break;
		case Key.Back:
			result = "8 1 14";
			break;
		case Key.Insert:
			result = "45 1 338";
			break;
		case Key.Home:
			result = "36 1 327";
			break;
		case Key.Delete:
			result = "46 1 339";
			break;
		case Key.Prior:
			result = "33 1 329";
			break;
		case Key.Next:
			result = "34 1 337";
			break;
		case Key.End:
			result = "35 1 335";
			break;
		case Key.F1:
			result = "112 1 59";
			break;
		case Key.F2:
			result = "113 1 60";
			break;
		case Key.F3:
			result = "114 1 61";
			break;
		case Key.F4:
			result = "115 1 62";
			break;
		case Key.F5:
			result = "116 1 63";
			break;
		case Key.F6:
			result = "117 1 64";
			break;
		case Key.F7:
			result = "118 1 65";
			break;
		case Key.F8:
			result = "119 1 66";
			break;
		case Key.F9:
			result = "120 1 67";
			break;
		case Key.F12:
			result = "123 1 88";
			break;
		case Key.X:
			result = "120 1 16429";
			break;
		case Key.Y:
			result = "121 1 16405";
			break;
		case Key.A:
			result = "97 1 30";
			break;
		case Key.B:
			result = "98 1 48";
			break;
		case Key.C:
			result = "99 1 46";
			break;
		case Key.D:
			result = "100 1 32";
			break;
		case Key.E:
			result = "101 1 18";
			break;
		case Key.F:
			result = "102 1 33";
			break;
		case Key.G:
			result = "103 1 34";
			break;
		case Key.H:
			result = "104 1 35";
			break;
		case Key.I:
			result = "105 1 23";
			break;
		case Key.J:
			result = "106 1 36";
			break;
		case Key.K:
			result = "107 1 37";
			break;
		case Key.L:
			result = "108 1 38";
			break;
		case Key.M:
			result = "109 1 50";
			break;
		case Key.N:
			result = "110 1 49";
			break;
		case Key.O:
			result = "111 1 24";
			break;
		case Key.P:
			result = "112 1 25";
			break;
		case Key.Q:
			result = "113 1 16";
			break;
		case Key.R:
			result = "114 1 19";
			break;
		case Key.S:
			result = "115 1 31";
			break;
		case Key.T:
			result = "116 1 20";
			break;
		case Key.U:
			result = "117 1 22";
			break;
		case Key.V:
			result = "118 1 47";
			break;
		case Key.W:
			result = "119 1 17";
			break;
		case Key.Z:
			result = "122 1 44";
			break;
		case Key.Add:
			result = "43 1 78";
			break;
		case Key.Subtract:
			result = "45 1 74";
			break;
		case Key.Space:
			result = "32 1 32";
			break;
		}
		return result;
	}

	public static int FromFormShortCutKey(global::System.Windows.Forms.KeyEventArgs e)
	{
		int result = -1;
		if (((e.KeyValue >= 65 && e.KeyValue <= 90) || (e.KeyValue >= 49 && e.KeyValue <= 57)) && (Control.ModifierKeys & Keys.Control) == Keys.Control)
		{
			result = e.KeyValue;
		}
		return result;
	}

	public static string FromForms(global::System.Windows.Forms.KeyEventArgs e)
	{
		string result = string.Empty;
		char asciiCharacter = Key2PnString.GetAsciiCharacter(e.KeyValue, 0);
		switch (e.KeyCode)
		{
		case Keys.Oemplus:
			if (asciiCharacter == '+')
			{
				result = "43 1 78";
			}
			break;
		case Keys.OemMinus:
			if (asciiCharacter == '-')
			{
				result = "45 1 74";
			}
			break;
		}
		switch (e.KeyValue)
		{
		case 107:
			result = "43 1 78";
			break;
		case 109:
			result = "45 1 74";
			break;
		}
		switch (e.KeyCode)
		{
		case Keys.Up:
			result = "38 1 328";
			break;
		case Keys.Down:
			result = "40 1 336";
			break;
		case Keys.Left:
			result = "37 1 331";
			break;
		case Keys.Right:
			result = "39 1 333";
			break;
		case Keys.Escape:
			result = "27 1 1";
			break;
		case Keys.Return:
			result = "13 1 28";
			break;
		case Keys.Back:
			result = "8 1 14";
			break;
		case Keys.Insert:
			result = "45 1 338";
			break;
		case Keys.Home:
			result = "36 1 327";
			break;
		case Keys.Delete:
			result = "46 1 339";
			break;
		case Keys.Prior:
			result = "33 1 329";
			break;
		case Keys.Next:
			result = "34 1 337";
			break;
		case Keys.End:
			result = "35 1 335";
			break;
		case Keys.F1:
			result = "112 1 59";
			break;
		case Keys.F2:
			result = "113 1 60";
			break;
		case Keys.F3:
			result = "114 1 61";
			break;
		case Keys.F4:
			result = "115 1 62";
			break;
		case Keys.F5:
			result = "116 1 63";
			break;
		case Keys.F6:
			result = "117 1 64";
			break;
		case Keys.F7:
			result = "118 1 65";
			break;
		case Keys.F8:
			result = "119 1 66";
			break;
		case Keys.F9:
			result = "120 1 67";
			break;
		case Keys.F12:
			result = "123 1 88";
			break;
		case Keys.X:
			result = "120 1 16429";
			break;
		case Keys.Y:
			result = "121 1 16405";
			break;
		case Keys.A:
			result = "97 1 30";
			break;
		case Keys.B:
			result = "98 1 48";
			break;
		case Keys.C:
			result = "99 1 46";
			break;
		case Keys.D:
			result = "100 1 32";
			break;
		case Keys.E:
			result = "101 1 18";
			break;
		case Keys.F:
			result = "102 1 33";
			break;
		case Keys.G:
			result = "103 1 34";
			break;
		case Keys.H:
			result = "104 1 35";
			break;
		case Keys.I:
			result = "105 1 23";
			break;
		case Keys.J:
			result = "106 1 36";
			break;
		case Keys.K:
			result = "107 1 37";
			break;
		case Keys.L:
			result = "108 1 38";
			break;
		case Keys.M:
			result = "109 1 50";
			break;
		case Keys.N:
			result = "110 1 49";
			break;
		case Keys.O:
			result = "111 1 24";
			break;
		case Keys.P:
			result = "112 1 25";
			break;
		case Keys.Q:
			result = "113 1 16";
			break;
		case Keys.R:
			result = "114 1 19";
			break;
		case Keys.S:
			result = "115 1 31";
			break;
		case Keys.T:
			result = "116 1 20";
			break;
		case Keys.U:
			result = "117 1 22";
			break;
		case Keys.V:
			result = "118 1 47";
			break;
		case Keys.W:
			result = "119 1 17";
			break;
		case Keys.Z:
			result = "122 1 44";
			break;
		case Keys.Space:
			result = "32 1 32";
			break;
		}
		return result;
	}

	[DllImport("User32.dll")]
	public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpChar, int uFlags);

	[DllImport("User32.dll")]
	public static extern int GetKeyboardState(byte[] pbKeyState);

	public static char GetAsciiCharacter(int uVirtKey, int uScanCode)
	{
		byte[] array = new byte[256];
		Key2PnString.GetKeyboardState(array);
		byte[] array2 = new byte[2];
		if (Key2PnString.ToAscii(uVirtKey, uScanCode, array, array2, 0) == 1)
		{
			return (char)array2[0];
		}
		return '\0';
	}
}
