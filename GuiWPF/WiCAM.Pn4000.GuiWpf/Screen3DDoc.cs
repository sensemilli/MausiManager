using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.GuiWpf;

internal class Screen3DDoc : IScreen3DDoc
{
	private readonly IScreen3DMain _screen3DMain;

	private readonly ICurrentDocProvider _currentDocProvider;

	private readonly IDoc3d _doc;

	public IScreen3DMain? Screen
	{
		get
		{
			if (_currentDocProvider.CurrentDoc != _doc)
			{
				return null;
			}
			return _screen3DMain;
		}
	}

	public Screen3DDoc(IScreen3DMain screen3DMain, ICurrentDocProvider currentDocProvider, IDoc3d doc)
	{
		_screen3DMain = screen3DMain;
		_currentDocProvider = currentDocProvider;
		_doc = doc;
	}
}
