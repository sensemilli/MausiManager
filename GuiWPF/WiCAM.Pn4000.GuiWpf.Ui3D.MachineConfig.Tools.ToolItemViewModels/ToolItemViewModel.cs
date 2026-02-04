using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public abstract class ToolItemViewModel : ViewModelBase
{
	public ToolItemViewModel Copy()
	{
		return MemberwiseClone() as ToolItemViewModel;
	}

	public virtual string GetDescriptionTranslation(string name)
	{
		return "";
	}

	public virtual string GetNameTranslation(string name)
	{
		return "";
	}

	public virtual void ValidateGeometry()
	{
	}
}
