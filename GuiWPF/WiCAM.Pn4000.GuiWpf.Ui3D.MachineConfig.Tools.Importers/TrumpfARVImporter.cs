using System.Collections.Generic;
using System.Windows;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.TrumpfConfig;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Modifiers;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class TrumpfARVImporter : TrumpfImporter, IHemImporter, IToolImporter, IHolderImporter, IAdapterImporter
{
	public TrumpfARVImporter(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, IModelFactory modelFactory, ImportHelper importHelper, IConfigProvider configProvider)
		: base(unitConverter, toolStorage, modelFactory, importHelper, configProvider)
	{
	}

	public string GetFilterString()
	{
		return Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.TrumpfArv") as string;
	}

	public void ImportDies(string filePath)
	{
		List<TrumpfToolProfile> lowerToolProfiles = new Arv(filePath).GetLowerToolProfiles();
		ImportDies(lowerToolProfiles);
	}

	public void ImportPunches(string filePath)
	{
		List<TrumpfToolProfile> upperToolProfiles = new Arv(filePath).GetUpperToolProfiles();
		ImportPunches(upperToolProfiles);
	}

	public void ImportHems(string filePath)
	{
		List<TrumpfToolProfile> hemProfiles = new Arv(filePath).GetHemProfiles();
		ImportHems(hemProfiles);
	}

	public void ImportLowerHolders(string filePath)
	{
		List<TrumpfHolderProfile> holderProfiles = new Arv(filePath).GetHolderProfiles();
		ImportLowerHolders(holderProfiles);
	}

	public void ImportUpperHolders(string filePath)
	{
		List<TrumpfHolderProfile> holderProfiles = new Arv(filePath).GetHolderProfiles();
		ImportUpperHolders(holderProfiles);
	}

	public void ImportLowerAdapters(string filePath)
	{
		List<TrumpfAdapterProfile> adapterProfiles = new Arv(filePath).GetAdapterProfiles();
		ImportLowerAdapters(adapterProfiles);
	}

	public void ImportUpperAdapters(string filePath)
	{
		List<TrumpfAdapterProfile> adapterProfiles = new Arv(filePath).GetAdapterProfiles();
		ImportUpperAdapters(adapterProfiles);
	}
}
