using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.PartsReader.DataClasses;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class SimpleHoleToCadConverter : MacroToCadConverterBase
{
	public static SimpleHoleXml GetXmlElement(SimpleHole simpleHole, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = simpleHole.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		return new SimpleHoleXml
		{
			ID = simpleHole.ID,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v.X,
				Y = v.Y,
				Z = v.Z
			},
			Radius = simpleHole.Radius
		};
	}
}
