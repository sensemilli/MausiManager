using System;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Contracts.LogCenterServices;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup.Information;

public class QuickTableColumnInfo
{
	public int Type { get; set; }

	public string Label { get; set; }

	public bool Sum { get; set; }

	public string Name { get; set; }

	public int FormatModification { get; set; } = -1;

	public int Round { get; set; }

	public DataGridTextColumn Column { get; set; }

	public GridViewDataColumn RadColumn { get; set; }

	public QuickTableColumnInfo(string code, ILogCenterService logCenterService)
	{
		int num = code.IndexOf('[');
		int num2 = code.IndexOf(']');
		if (num < 0 || num2 < 0 || num2 - num < 2)
		{
			this.Type = 1;
			this.Sum = false;
			this.Label = code;
			return;
		}
		int num3 = 1;
		this.Sum = code[num + 1] == '+';
		if (this.Sum)
		{
			num3++;
		}
		this.Name = code.Substring(0, num);
		this.Label = this.Name;
		try
		{
			this.Type = Convert.ToInt32($"{code[num + num3]}");
			int num4 = code.IndexOf('.', num);
			if (num4 > num)
			{
				this.FormatModification = Convert.ToInt32(code.Substring(num4 + 1, num2 - num4 - 1));
			}
		}
		catch (Exception e)
		{
			logCenterService.CatchRaport(e);
		}
	}

	internal Type GetColumnType()
	{
		return this.Type switch
		{
			1 => typeof(string), 
			2 => typeof(int), 
			3 => typeof(double), 
			4 => typeof(DateTime), 
			5 => typeof(TimeSpan), 
			_ => typeof(object), 
		};
	}
}
