using System.Drawing;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Services.ConfigProviders.Contracts.DataType;

namespace WiCAM.Pn4000.PN3D.Extensions;

public static class CfgColorExtension
{
	public static global::WiCAM.Pn4000.BendModel.Base.Color ToBendColor(this CfgColor color)
	{
		return new global::WiCAM.Pn4000.BendModel.Base.Color(color.R, color.G, color.B, color.A);
	}

	public static global::System.Windows.Media.Color ToWpfColor(this CfgColor color)
	{
		return global::System.Windows.Media.Color.FromScRgb(color.A, color.R, color.G, color.B);
	}

	public static global::System.Drawing.Color ToFormsColor(this CfgColor color)
	{
		return global::System.Drawing.Color.FromArgb((int)(color.A * 255f), (int)(color.R * 255f), (int)(color.G * 255f), (int)(color.B * 255f));
	}
}
