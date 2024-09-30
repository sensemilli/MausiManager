using System;
using System.Collections.Generic;
using System.Text;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.Lop;

internal class LopTemplateReader : ILopTemplateReader
{
	private static readonly string TypeOfFeedback = "FEEDBACK_TYPE";

	private static readonly string __templateBegin = "TEMPLATE_BEGIN";

	private static readonly string __templateEnd = "TEMPLATE_END";

	private static readonly string __settingsDivider = "#-------------";

	private static readonly string __settings = "SETTINGS";

	private static readonly string __path = "PATH";

	private static readonly string __nameLength = "NAME_LENGHT";

	private static readonly string __nameFormat = "NAME_FORMAT";

	public int ReadTemplate(LopTemplateInfo template, string content, int begin)
	{
		int num = content.IndexOf(__templateBegin, begin, StringComparison.OrdinalIgnoreCase);
		if (!StringHelper.ReadLine(content, __templateBegin, begin).IsComment())
		{
			if (num > 0)
			{
				num += __templateBegin.Length;
				int num2 = content.IndexOf(__templateEnd, num, StringComparison.OrdinalIgnoreCase);
				ReadTemplateData(template, content.Substring(num, num2 - num));
				begin = num2;
			}
			else
			{
				begin = -1;
			}
		}
		else
		{
			begin = num + __templateBegin.Length;
		}
		return begin;
	}

	private void ReadTemplateData(LopTemplateInfo template, string templateContent)
	{
		string[] lines = templateContent.SplitToLines();
		int current = ReadSettings(template, lines);
		if (template.TypeOfFeedback == FeedbackFileType.LOP)
		{
			template.Content = ReadLopTemplate(template, lines, current);
		}
		if (template.TypeOfFeedback == FeedbackFileType.CSV)
		{
			template.Content = ReadCsvTemplate(template, lines, current);
		}
	}

	private string ReadCsvTemplate(LopTemplateInfo template, IReadOnlyList<string> lines, int current)
	{
		StringBuilder stringBuilder = new StringBuilder(50000);
		while (current < lines.Count)
		{
			if (!lines[current].IsComment())
			{
				if (lines[current].ContainsString("$"))
				{
					string[] array = lines[current].Split(';');
					foreach (string text in array)
					{
						template.Mapping.Add(text.Trim(), null);
					}
				}
				stringBuilder.AppendLine(lines[current]);
			}
			current++;
		}
		return stringBuilder.ToString();
	}

	private string ReadLopTemplate(LopTemplateInfo template, IReadOnlyList<string> lines, int current)
	{
		StringBuilder stringBuilder = new StringBuilder(50000);
		while (current < lines.Count)
		{
			if (lines[current].ContainsString("$"))
			{
				string key = lines[current].ReadValue();
				template.Mapping.Add(key, null);
			}
			stringBuilder.AppendLine(lines[current++]);
		}
		return stringBuilder.ToString();
	}

	private int ReadSettings(LopTemplateInfo template, IReadOnlyList<string> lines)
	{
		int num = 0;
		int num2 = 0;
		while (num2 < 2)
		{
			if (lines[num].ContainsString(__settingsDivider))
			{
				num2++;
				num++;
				continue;
			}
			if (num2 == 1)
			{
				if (lines[num].ContainsString(__settings))
				{
					string[] array = lines[num].Split(';');
					template.Type = array[1].Trim();
					template.Company = array[2].Trim();
					template.LopType = array[3].Trim();
				}
				else if (lines[num].ContainsString(__path))
				{
					string input = lines[num].ReadValue();
					template.DestinationPath = PnPathBuilder.CheckPnDriveKey(input);
				}
				else if (lines[num].ContainsString(__nameLength))
				{
					string input2 = lines[num].ReadValue();
					template.NameLength = StringHelper.ToInt(input2);
				}
				else if (lines[num].ContainsString(__nameFormat))
				{
					template.NameFormat = lines[num].ReadValue();
				}
				else if (lines[num].ContainsString(TypeOfFeedback))
				{
					if (lines[num].ReadValue().Equals("CSV", StringComparison.OrdinalIgnoreCase))
					{
						template.TypeOfFeedback = FeedbackFileType.CSV;
					}
				}
				else if (lines[num].ContainsString("HEADER"))
				{
					template.Header = lines[num].ReadValue();
				}
			}
			num++;
		}
		return num;
	}
}
