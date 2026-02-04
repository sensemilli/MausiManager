using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.DocManagement;

public interface IDocWrapper
{
	IDoc3d Document { get; }

	string Header { get; }

	string Tooltip { get; }

	bool IsSelected { get; set; }
}
