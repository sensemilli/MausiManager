using System;
using System.IO;
using System.Text;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.LabelPrinter;

internal class PartLabelContentModifier
{
	public string Modify(LabelModifyReference reference, string jobPath)
	{
		string path = $"PRTLB_{reference.Part.Part.PART_NUMBER}.LAB";
		string[] array = IOHelper.FileReadAllLines(Path.Combine(jobPath, path));
		StringBuilder stringBuilder = new StringBuilder(50000);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.IndexOf("LBL_SETTINGS", StringComparison.CurrentCultureIgnoreCase) > -1)
			{
				ModifyLine(stringBuilder, text, reference.Amount);
			}
			else
			{
				stringBuilder.AppendLine(text);
			}
		}
		return stringBuilder.ToString();
	}

	private void ModifyLine(StringBuilder sb, string line, int amount)
	{
		string[] array = line.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		int num = 0;
		while (num < 3)
		{
			sb.Append(array[num++]);
			sb.Append(";");
		}
		sb.Append(amount);
		sb.AppendLine();
	}
}
