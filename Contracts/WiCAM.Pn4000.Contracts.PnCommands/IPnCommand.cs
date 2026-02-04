namespace WiCAM.Pn4000.Contracts.PnCommands;

public interface IPnCommand
{
	int Group { get; set; }

	string Command { get; set; }

	string DefaultLabel { get; set; }

	int LabelId { get; set; }

	int ToolTipId { get; set; }

	string CurrentLabel { get; set; }

	string CurrentTooltip { get; set; }

	int AddValue1 { get; set; }

	int AddValue2 { get; set; }

	string AddValue3 { get; set; }

	int PnColorIndex { get; set; }

	IRFileRecord GetRFileRecord();
}
