using System;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.GuiContracts.Billboards;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditLiftingAids;

public interface IBillboardFactory
{
	IButtonBillboard CreateButtonGlyph(string glyph, Action<IButtonBillboard> updateScreen);

	IButtonBillboard CreateButtonGlyph(Vector2d offset, string glyph, GlyphStyle glyphStyle, Action<IButtonBillboard> updateScreen);
}
