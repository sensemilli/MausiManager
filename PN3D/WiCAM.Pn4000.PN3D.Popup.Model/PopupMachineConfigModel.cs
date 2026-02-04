using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.pn4.Interfaces;

namespace WiCAM.Pn4000.PN3D.Popup.Model;

public class PopupMachineConfigModel
{
	public IBendMachine BendMachine { get; private set; }

	public IDoc3d Doc { get; private set; }

	public IGlobals Globals { get; set; }

	public IMainWindowDataProvider MainWindowDataProvider { get; set; }

	public IPnCommandsBend RibbonCommands { get; set; }

	public PopupMachineConfigModel(IPnCommandsBend ribbonCommands, IGlobals globals, IMainWindowDataProvider mainWindowDataProvider)
	{
		this.RibbonCommands = ribbonCommands;
		this.Globals = globals;
		this.MainWindowDataProvider = mainWindowDataProvider;
	}

	public void Init(IDoc3d doc, IBendMachine bendMachine)
	{
		this.Doc = doc;
		this.BendMachine = bendMachine;
	}
}
