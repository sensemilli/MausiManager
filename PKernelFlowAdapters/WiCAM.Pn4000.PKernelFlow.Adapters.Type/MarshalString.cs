using System.Runtime.InteropServices;
using WiCAM.Pn4000.Encodings;

namespace WiCAM.Pn4000.PKernelFlow.Adapters.Type;

public class MarshalString
{
	public static void ToIntPtrAtFortranStyle(string text, nint pointer, int textLength, int offset = 0)
	{
		if (text == null)
		{
			text = string.Empty;
		}
		byte[] bytes = CurrentEncoding.SystemEncoding.GetBytes(text);
		int num = bytes.Length;
		if (num > textLength)
		{
			num = textLength;
		}
		int i;
		for (i = 0; i < num; i++)
		{
			Marshal.WriteByte(pointer, i + offset, bytes[i]);
		}
		for (int j = i; j < textLength; j++)
		{
			Marshal.WriteByte(pointer, j + offset, 32);
		}
	}

	public static void ToIntPtrAtCStyle(string text, nint pointer, int textLength)
	{
		if (text == null)
		{
			text = string.Empty;
		}
		byte[] bytes = CurrentEncoding.SystemEncoding.GetBytes(text);
		for (int i = 0; i < bytes.Length; i++)
		{
			Marshal.WriteByte(pointer, i, bytes[i]);
		}
		Marshal.WriteByte(pointer, bytes.Length, 0);
	}

	public static string IntPtrToString(nint str, int len)
	{
		byte[] array = new byte[len];
		for (int i = 0; i < len; i++)
		{
			array[i] = Marshal.ReadByte(str, i);
		}
		return MarshalString.FString2String(array);
	}

	public static string FString2String(byte[] buf)
	{
		return CurrentEncoding.SystemEncoding.GetString(buf).TrimEnd();
	}

	public static byte[] CreateFString(int textLength)
	{
		byte[] array = new byte[textLength];
		for (int i = 0; i < textLength; i++)
		{
			array[i] = 32;
		}
		return array;
	}

	public static byte[] CreateFString(string text, int textLength)
	{
		byte[] bytes = CurrentEncoding.SystemEncoding.GetBytes(text);
		byte[] array = new byte[textLength];
		int i;
		for (i = 0; i < bytes.Length; i++)
		{
			array[i] = bytes[i];
		}
		for (int j = i; j < textLength; j++)
		{
			array[j] = 32;
		}
		return array;
	}

	public static string PtrToString(nint data, int len)
	{
		string text = Marshal.PtrToStringAnsi(data, len).TrimEnd();
		if (text[text.Length - 1] == '\0')
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text;
	}
}
