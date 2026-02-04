using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;

namespace WiCAM.Pn4000.PN3D.CAD;

public interface ICad2DDatabase
{
	void AddText(CadTxtText cadTxtText);

	void AddInnerLine(HashSet<CadGeoElement> element);
}
