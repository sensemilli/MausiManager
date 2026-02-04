namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;

internal interface IHolderImporter : IToolImporter
{
	void ImportLowerHolders(string filePath);

	void ImportUpperHolders(string filePath);
}
