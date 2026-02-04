using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Modifiers;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

public class BPX
{
	private bool Serialize(string path)
	{
		return Xml.SerializeToXml(this, path);
	}

	private BBX Deserialize(string path)
	{
		return Xml.DeserializeFromXml<BBX>(path);
	}
}
