namespace WiCAM.Pn4000.GuiWpf.TabBend.Developer;

internal class EditToolsMainViewModel : IEditToolsMainViewModel
{
	public OverviewViewModel OverviewVm { get; }

	public EditToolsMainViewModel(OverviewViewModel overviewVm)
	{
		OverviewVm = overviewVm;
	}
}
