using System.Collections.Generic;
using System.Windows.Controls;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.pn4.uicontrols.Buttons;

namespace WiCAM.Pn4000.pn4.pn4UILib.Ribbon;

public class TabInformation
{
	public PnRibbonTabButton Tab;

	public IPnRibbonNode Node;

	public StackPanel Panel;

	public Dictionary<object, IPnRibbonNode> ElementNodeDictionary;

	public Dictionary<PnRibbonButton, IPnCommand> ButtonCommandConnection;

	public bool IsValid;

	public TabInformation Before;

	public TabType Type;
}
