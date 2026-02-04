using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.GuiWpf;

internal class Screen3DMain : IScreen3DMain
{
	public ScreenD3D11 ScreenD3D => Screen3D.ScreenD3D;

	public Screen3D Screen3D { get; private set; }

	public void Init(Screen3D screen)
	{
		Screen3D = screen;
	}
}
