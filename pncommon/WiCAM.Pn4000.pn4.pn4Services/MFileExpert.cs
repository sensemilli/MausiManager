using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using pncommon.WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Encodings;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.PKernelFlow.Adapters;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class MFileExpert : IMFileExpert
{
	public List<MFileHead> heads = new List<MFileHead>();

	public List<MFileData> lines = new List<MFileData>();

	public string m_filename;

	public string m_name;

	public string m_tooltip;

	public MFileExpert()
	{
	}

	public MFileExpert(string path_s)
	{
		this.LoadFromFile(path_s);
	}

	private void LoadFromFile(string path_s)
	{
		if (!File.Exists(path_s))
		{
			return;
		}
		string[] array = File.ReadAllLines(path_s, CurrentEncoding.SystemEncoding);
		if (array.Count() < 14 || !array[0].Contains("VERSION 2.00"))
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < array.Count(); i++)
		{
			string text = array[i].Trim();
			if (!(text.Trim() != string.Empty) || text[0] == '#')
			{
				continue;
			}
			if (num > 10)
			{
				text = array[i];
				MFileData mFileData = new MFileData();
				mFileData.PnGroup = Convert.ToInt32(text.Substring(28, 3).Trim());
				mFileData.PnCommand = text.Substring(33, 20).Trim();
				mFileData.PnVisible = Convert.ToInt32(text.Substring(0, 7).Trim()) > 0;
				mFileData.PnColorIndex = Convert.ToInt32(text.Substring(25, 2).Trim());
				mFileData.PnTextId = Convert.ToInt32(text.Substring(54, 8).Trim());
				mFileData.PnTooltipId = Convert.ToInt32(text.Substring(63, 8).Trim());
				if (mFileData.PnTextId == 0)
				{
					mFileData.PnText = text.Substring(7, 18).Trim();
				}
				else
				{
					mFileData.PnText = GeneralSystemComponentsAdapter.GetTextById(mFileData.PnTextId);
					if (mFileData.PnText == string.Empty)
					{
						mFileData.PnText = text.Substring(7, 18).Trim();
					}
				}
				if (mFileData.PnTooltipId > 0)
				{
					mFileData.PnToolTipText = GeneralSystemComponentsAdapter.GetTextById(mFileData.PnTooltipId);
				}
				this.lines.Add(mFileData);
			}
			num++;
		}
	}

	private static int GetMFileTextId(string line)
	{
		if (line.Length < 56)
		{
			return 0;
		}
		if (int.TryParse(line.Substring(54, 8).Trim(), out var result))
		{
			return result;
		}
		return 0;
	}

	private static int GetMFileToolTipId(string line)
	{
		if (line.Length < 65)
		{
			return 0;
		}
		if (int.TryParse(line.Substring(63, 8).Trim(), out var result))
		{
			return result;
		}
		return 0;
	}

	public static MFileExpert CreateByPfs842(int refno, int count)
	{
		MFileExpert mFileExpert = new MFileExpert();
		for (int i = 0; i < count; i++)
		{
			string mFileLine = GeneralSystemComponentsAdapter.GetMFileLine(refno, i + 1);
			switch (i)
			{
			case 0:
				mFileExpert.m_filename = mFileLine.Trim();
				continue;
			case 1:
			{
				MFileHead mFileHead = new MFileHead();
				mFileHead.Name = mFileLine.Trim();
				mFileExpert.heads.Add(mFileHead);
				mFileExpert.m_name = mFileHead.Name;
				continue;
			}
			case 2:
				if (mFileExpert.heads.Count > 0)
				{
					mFileExpert.heads[0].NameId = MFileExpert.GetMFileTextId(mFileLine);
					mFileExpert.heads[0].ToolTipId = MFileExpert.GetMFileToolTipId(mFileLine);
				}
				continue;
			}
			try
			{
				MFileData mFileData = new MFileData();
				string text = MFileExpert.RepairMDATAString(mFileLine);
				mFileData.PnTextId = MFileExpert.GetMFileTextId(text);
				mFileData.PnTooltipId = MFileExpert.GetMFileToolTipId(text);
				mFileData.PnGroup = Convert.ToInt32(text.Substring(28, 3).Trim());
				mFileData.PnCommand = text.Substring(33, 20).Trim();
				if (mFileData.PnCommand == string.Empty)
				{
					mFileData.PnCommand = null;
				}
				mFileData.PnText = text.Substring(7, 18).Trim();
				mFileData.PnVisible = Convert.ToInt32(text.Substring(0, 7).Trim()) > 0;
				mFileData.PnColorIndex = Convert.ToInt32(text.Substring(25, 2).Trim());
				mFileExpert.lines.Add(mFileData);
			}
			catch (Exception)
			{
			}
		}
		return mFileExpert;
	}

	private static string RepairMDATAString(string v)
	{
		if (v == "     1                    0   0                              0        0")
		{
			return v;
		}
		string text = v;
		for (int i = 0; i < 4; i++)
		{
			text = text.Substring(0, text.LastIndexOf(' ')).TrimEnd();
		}
		text = text.Substring(0, text.LastIndexOf(' '));
		int num = v.IndexOf(' ', text.Length + 1) - text.Length - 2;
		int num2 = 25 - text.Length - num;
		for (int j = 0; j < num2; j++)
		{
			v = v.Insert(text.Length, " ");
		}
		return v;
	}
}
