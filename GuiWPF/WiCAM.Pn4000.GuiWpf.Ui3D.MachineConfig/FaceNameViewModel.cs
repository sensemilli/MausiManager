using WiCAM.Pn4000.ToolCalculationGuiWpf.EditTools.SubViews;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class FaceNameViewModel : ICustomAutoCompleteBoxViewModel.IItem
{
	public string Name { get; }

	public string CustomAutoCompleteBoxItemDisplayName { get; }

	public FaceNameViewModel(string name, string? displayName = null)
	{
		Name = name;
		CustomAutoCompleteBoxItemDisplayName = displayName ?? name;
		//base._002Ector();
	}
}
