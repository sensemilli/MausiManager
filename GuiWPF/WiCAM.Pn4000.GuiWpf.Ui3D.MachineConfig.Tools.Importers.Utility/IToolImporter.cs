namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;

public interface IToolImporter
{
	void Init(MachineToolsViewModel machineTools);

	void ImportPunches(string filePath);

	void ImportDies(string filePath);

	string GetFilterString();
}
