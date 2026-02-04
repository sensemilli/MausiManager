using BendDataBase.Enums;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.BendSimulation;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Popup.Model;

public class PopupBendParameterModel
{
	public IBendMachineSimulation BendMachine { get; set; }

	public IPnCommandsBend RibbonCommands { get; set; }

	public IDoc3d Doc { get; set; }

	public IGlobals Globals { get; set; }

	public BendParamType P3DBendTableValueShowType { get; set; }

	public PopupBendParameterModel(IGlobals globals, IPnCommandsBend ribbonCommands)
	{
		this.Globals = globals;
		this.RibbonCommands = ribbonCommands;
		this.P3DBendTableValueShowType = (BendParamType)this.Globals.ConfigProvider.InjectOrCreate<General3DConfig>().P3D_BendTableValueShowType;
	}

	public void Init(IDoc3d doc, IBendMachineSimulation bendMachine)
	{
		this.BendMachine = bendMachine;
		this.Doc = doc;
	}
}
