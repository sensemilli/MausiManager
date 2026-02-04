using System;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Telerik;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards.Contents;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditLiftingAids;

public class BillboardFactory : IBillboardFactory
{
	private readonly IRadGlyphConverter _glyphConverter;

	private readonly BackgroundStyle _background;

	private readonly BackgroundStyle _backgroundHover;

	private readonly BackgroundStyle _backgroundMouseDown;

	private readonly GlyphStyle _glyphStyleDefault;

	public BillboardFactory(IRadGlyphConverter glyphConverter, IStyleProvider styles)
	{
		_glyphConverter = glyphConverter;
		_background = new BackgroundStyle
		{
			Color = styles.AccentBackgroundColor,
			BorderColor = styles.AccentBorderColor,
			BorderThickness = 5f,
			Padding = 5f,
			MinWidth = 50f,
			MinHeight = 50f
		};
		BackgroundStyle background = _background;
		background.Color = styles.AccentMouseOverBackgroundColor;
		background.BorderColor = styles.AccentMouseOverBorderColor;
		_backgroundHover = background;
		background = _background;
		background.Color = styles.AccentPressedBackgroundColor;
		background.BorderColor = styles.AccentMouseOverBorderColor;
		_backgroundMouseDown = background;
		_glyphStyleDefault = new GlyphStyle
		{
			Size = 30,
			Color = styles.AccentForegroundColor
		};
	}

	public IButtonBillboard CreateButtonGlyph(string glyph, Action<IButtonBillboard> updateScreen)
	{
		return CreateButtonGlyph(Vector2d.Zero, glyph, _glyphStyleDefault, updateScreen);
	}

	public IButtonBillboard CreateButtonGlyph(Vector2d offset, string glyph, GlyphStyle glyphStyle, Action<IButtonBillboard> updateScreen)
	{
		return new ButtonBillboard(updateScreen)
		{
			Content = new GlyphContent(_glyphConverter)
			{
				Glyph = glyph,
				Background = _background,
				GlyphStyle = glyphStyle
			},
			HoverContent = new GlyphContent(_glyphConverter)
			{
				Glyph = glyph,
				Background = _backgroundHover,
				GlyphStyle = glyphStyle
			},
			MouseDownContent = new GlyphContent(_glyphConverter)
			{
				Glyph = glyph,
				Background = _backgroundMouseDown,
				GlyphStyle = glyphStyle
			},
			IsVisible = true,
			IsInteractive = true,
			Offset = offset
		};
	}
}
