using System;
using WiCAM.Pn4000.Contracts.LogCenterServices;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class PipLstEntry : IPipLstEntry
{
	public int CommandGroup { get; set; }

	public string CommandName { get; set; }

	public int Id1 { get; set; }

	public int Id2 { get; set; }

	public string DefaultText { get; set; }

	public string CurrentText { get; set; }

	public PipLstEntry(string key, ILogCenterService logCenterService)
	{
		try
		{
			string[] array = key.Split('|');
			this.CommandGroup = Convert.ToInt32(array[0].Trim());
			this.CommandName = array[1].Trim();
			this.Id1 = Convert.ToInt32(array[2].Trim());
			this.Id2 = Convert.ToInt32(array[3].Trim());
			this.DefaultText = array[4].Trim();
			this.CurrentText = this.DefaultText;
		}
		catch (Exception e)
		{
			logCenterService.CatchRaport(e);
		}
	}
}
