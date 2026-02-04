namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public interface ICopyTab
{
	bool IsCopyButtonEnabled { get; }

	void CopyButtonClick();
}
