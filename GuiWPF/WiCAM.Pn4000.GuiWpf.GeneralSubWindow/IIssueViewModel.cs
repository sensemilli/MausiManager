using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.GeneralSubWindow;

public interface IIssueViewModel
{
	void Init(IDoc3d doc);

	void RefreshVisibilities();

	void Close();
}
