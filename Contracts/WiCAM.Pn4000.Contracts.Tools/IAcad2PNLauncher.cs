namespace WiCAM.Pn4000.Contracts.Tools;

public interface IAcad2PNLauncher
{
	void CommandLineStart(string path);

	bool ImportAutoCadFile(string filename, bool force_text2geometry = false, bool force_keeporgin = false, int time_limit = -1, bool force_explodepolylines = false, int force_layoutID = -1, string layout_name = "?");

	void Save(string filename_CADGEO, string filename_CADTXT = null, string filename_LAY = null, string filename_SIDE = null);

	string GetCadGeo();

	object GetPnDb();
}
