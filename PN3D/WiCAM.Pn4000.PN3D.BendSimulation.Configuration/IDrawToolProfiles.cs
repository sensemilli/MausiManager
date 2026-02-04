using System.Windows.Controls;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Adapter;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Holder;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.PN3D.BendSimulation.Configuration;

public interface IDrawToolProfiles
{
	void LoadPreview3D(string geometryFile, IScreen3D image);

	void LoadPreview2D(string? geometryFile, double workingHeightMinY, double workingHeightMaxY, double offsetX, Canvas image, bool showWorkingHeight = true);

	void LoadPreview2D(CadGeoInfo? cadGeoInfo, double workingHeightMinY, double workingHeightMaxY, double offsetX, Canvas image, bool showWorkingHeight = true);

	void LoadPunchPreview2D(IPunchProfile profile, Canvas image, IBendMachineTools machine, bool showWorkingHeight = true);

	void LoadPunchPreview2D(string geometryFile, double workingHeight, Canvas image, bool showWorkingHeight = true);

	void LoadPunchPreview2D(PunchProfile profile, Canvas image, BendMachine machine, bool showWorkingHeight = true);

	void LoadPunchPreview3D(PunchProfile profile, IScreen3D image, BendMachine machine);

	void LoadDiePreview2D(IDieProfile profile, Canvas image, IBendMachineTools machine, bool showWorkingHeight = true);

	void LoadDiePreview2D(string geometryFile, double workingHeight, Canvas image, bool showWorkingHeight = true);

	void LoadDiePreview2D(DieProfile profile, Canvas image, BendMachine machine, bool showWorkingHeight = true);

	void LoadDiePreview3D(DieProfile profile, IScreen3D image, BendMachine machine);

	void LoadDiePreview2D(HemProfile profile, Canvas image, BendMachine machine, bool showWorkingHeight = true);

	void LoadDiePreview3D(HemProfile profile, IScreen3D image, BendMachine machine);

	void LoadAdapterPreview2D(AdapterProfile profile, Canvas image, BendMachine machine, bool showWorkingHeight = true);

	void LoadAdapterPreview3D(AdapterProfile profile, IScreen3D image, BendMachine machine);

	void LoadHolderPreview2D(HolderProfile profile, Canvas image, BendMachine machine, bool showWorkingHeight = true);

	void LoadHolderPreview3D(HolderProfile profile, IScreen3D image, BendMachine machine);
}
