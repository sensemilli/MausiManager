using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;

namespace WiCAM.Pn4000.PN3D.CAD;

public class CadTxtText : CadGeoElement
{
	public Vector2d Position { get; set; }

	public double Angle { get; set; }

	public double Height { get; set; }

	public string Text { get; set; }
}
