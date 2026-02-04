using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.PnCommands;

public interface IRFileRecord
{
	RFileType Type { get; set; }

	int VisualGroup { get; set; }

	string DefaultLabel { get; set; }

	int FunctionGroup { get; set; }

	string FunctionName { get; set; }

	int IconSize { get; set; }

	int ShowLabel { get; set; }

	string LetterShortcut { get; set; }

	int IdLabel { get; set; }

	int IdHelp { get; set; }

	int AddValue1 { get; set; }

	int AddValue2 { get; set; }

	string AddValue3 { get; set; }

	string cfg { get; set; }

	object tmp_object { get; set; }

	object tmp_object2 { get; set; }

	List<IRFileRecord> SubRecords { get; set; }

	bool DrawArrow { get; set; }

	string GetOutputText();
}
