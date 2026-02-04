namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;

public interface IAdapterImporter : IToolImporter
{
	void ImportLowerAdapters(string filePath);

	void ImportUpperAdapters(string filePath);
}
