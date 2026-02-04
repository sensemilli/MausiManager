using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.Telerik;

public interface IRadGlyphConverter
{
	void CopyToBuffer(string glyph, Color color, int size, CreateBuffer createBuffer);

	(int, int) MeasureGlyph(string glyph, int size);
}
