namespace WiCAM.Pn4000.Contracts.Tools;

public interface IAcad2PNWpf
{
	object GetDrawer(IAcad2PNLauncher acad);

	void ConfigDialog();
}
