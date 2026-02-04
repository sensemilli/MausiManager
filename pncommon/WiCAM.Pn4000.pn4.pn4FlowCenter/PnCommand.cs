using System.Text;
using pncommon.WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Contracts.PnCommands;

namespace WiCAM.Pn4000.pn4.pn4FlowCenter;

public class PnCommand : IPnCommand
{
	public int Group { get; set; }

	public string Command { get; set; }

	public string DefaultLabel { get; set; }

	public int LabelId { get; set; }

	public int ToolTipId { get; set; }

	public string CurrentLabel { get; set; }

	public string CurrentTooltip { get; set; }

	public int AddValue1 { get; set; }

	public int AddValue2 { get; set; }

	public string AddValue3 { get; set; }

	public int PnColorIndex { get; set; }

	public PnCommand(MFileData item)
	{
		this.Group = item.PnGroup;
		this.Command = item.PnCommand;
		this.DefaultLabel = item.PnText;
		this.LabelId = item.PnTextId;
		this.ToolTipId = item.PnTooltipId;
		this.CurrentLabel = this.DefaultLabel;
	}

	public PnCommand(IRFileRecord item)
	{
		this.Group = item.FunctionGroup;
		this.Command = item.FunctionName;
		this.DefaultLabel = item.DefaultLabel;
		this.LabelId = item.IdLabel;
		this.ToolTipId = item.IdHelp;
		this.CurrentLabel = this.DefaultLabel;
		this.AddValue1 = item.AddValue1;
		this.AddValue2 = item.AddValue2;
		this.AddValue3 = item.AddValue3;
	}

	public PnCommand(int group, string command)
	{
		this.Group = group;
		this.Command = command;
	}

	public PnCommand()
	{
	}

	public PnCommand(int group, string command, string defaultLabel, int idLabel, int idHelp)
	{
		this.Group = group;
		this.Command = command;
		this.DefaultLabel = defaultLabel;
		this.LabelId = idLabel;
		this.ToolTipId = idHelp;
	}

	public string GetPnFormatedString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("C_CALL {0} {1}", this.Group, this.Command);
		return stringBuilder.ToString();
	}

	public string GetPnFormatedStringSubmenuEdition()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("C_CALL 25 {0}", this.Group);
		return stringBuilder.ToString();
	}

	public IRFileRecord GetRFileRecord()
	{
		return new RFileRecord
		{
			FunctionGroup = this.Group,
			FunctionName = this.Command,
			DefaultLabel = this.DefaultLabel,
			IdLabel = this.LabelId,
			IdHelp = this.ToolTipId,
			AddValue1 = this.AddValue1,
			AddValue2 = this.AddValue2,
			AddValue3 = this.AddValue3
		};
	}
}
