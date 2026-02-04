using System;
using System.Collections.Generic;
using System.IO;
using WiCAM.Pn4000.Encodings;

namespace pncommon.WiCAM.Pn4000.pn4.pn4Services;

public class MFile
{
	public List<MFileLineLine> Lines = new List<MFileLineLine>();

	public MFile(string filename)
	{
		string[] array;
		try
		{
			array = File.ReadAllLines(filename, CurrentEncoding.SystemEncoding);
		}
		catch
		{
			return;
		}
		for (int i = 13; i < array.Length; i++)
		{
			if (array[i] != string.Empty)
			{
				this.Lines.Add(new MFileLineLine(array[i]));
			}
		}
	}

	internal void RemoveSeparators()
	{
		for (int num = this.Lines.Count - 1; num >= 0; num--)
		{
			try
			{
				if (this.Lines[num].fname[0] == '.' && this.Lines[num].group == 0)
				{
					this.Lines.RemoveAt(num);
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
