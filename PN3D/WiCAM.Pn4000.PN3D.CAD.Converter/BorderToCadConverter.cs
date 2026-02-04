using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class BorderToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 25;

	public static bool AddCadGeoElements(Border border, Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
	{
		return true;
	}
}
