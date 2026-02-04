using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.pn4.uicontrols.Buttons;

namespace WiCAM.Pn4000.pn4.pn4UILib.Ribbon;

public class SplitData
{
	public PnRibbonButton BtnMain;

	public PnRibbonButton BtnExtender;

	public IPnCommand SplitCommand;

	public IPnCommand CurrentCommand;
}
