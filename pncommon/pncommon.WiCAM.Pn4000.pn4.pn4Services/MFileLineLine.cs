using System;

namespace pncommon.WiCAM.Pn4000.pn4.pn4Services;

public class MFileLineLine
{
	public int group { get; set; }

	public string fname { get; set; }

	public string default_label { get; set; }

	public int lang_id1 { get; set; }

	public int lang_id2 { get; set; }

	public MFileLineLine(string str)
	{
		try
		{
			this.group = Convert.ToInt32(str.Substring(29, 3).Trim());
			this.fname = str.Substring(33, 15).Trim();
			this.default_label = str.Substring(7, 17).Trim();
			this.lang_id1 = Convert.ToInt32(str.Substring(54, 8).Trim());
			this.lang_id2 = Convert.ToInt32(str.Substring(63, 8).Trim());
		}
		catch
		{
		}
	}
}
