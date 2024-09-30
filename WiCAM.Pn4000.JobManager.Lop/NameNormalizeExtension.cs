using System.IO;
using System.Text;

namespace WiCAM.Pn4000.JobManager.Lop;

internal static class NameNormalizeExtension
{
	public static string Normalize(this string input)
	{
		if (input.Length > 80)
		{
			string extension = Path.GetExtension(input);
			input = Path.ChangeExtension(Path.GetFileNameWithoutExtension(input).Substring(0, 76), extension);
		}
		char[] array = "<>:/|?\\*\"".ToCharArray();
		foreach (char oldChar in array)
		{
			input = input.Replace(oldChar, '_');
		}
		StringBuilder stringBuilder = new StringBuilder(200);
		string text = input;
		foreach (char c in text)
		{
			if (c > ' ' && c < 'z')
			{
				stringBuilder.Append(c);
			}
			else
			{
				stringBuilder.Append("_");
			}
		}
		return stringBuilder.ToString();
	}
}
