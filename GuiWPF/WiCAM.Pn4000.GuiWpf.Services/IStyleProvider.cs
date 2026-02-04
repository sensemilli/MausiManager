using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.GuiContracts.Billboards;

namespace WiCAM.Pn4000.GuiWpf.Services;

public interface IStyleProvider
{
	Color AccentBackgroundColor { get; }

	Color AccentBorderColor { get; }

	Color AccentForegroundColor { get; }

	Color AccentMouseOverBackgroundColor { get; }

	Color AccentMouseOverBorderColor { get; }

	Color AccentPressedBackgroundColor { get; }

	Color BaseBackgroundColor { get; }

	Color ButtonBackgroundColor { get; }

	Color DisabledBackgroundColor { get; }

	Color DisabledBorderColor { get; }

	Color DisabledForegroundColor { get; }

	Color DisabledIconColor { get; }

	Color IconColor { get; }

	Color MainBackgroundColor { get; }

	Color MainBorderColor { get; }

	Color MainForegroundColor { get; }

	Color MouseOverBackgroundColor { get; }

	Color MouseOverBorderColor { get; }

	Color PressedBackgroundColor { get; }

	Color ReadOnlyBackgroundColor { get; }

	Color ReadOnlyBorderColor { get; }

	Color SecondaryBackgroundColor { get; }

	Color SecondaryForegroundColor { get; }

	Color ValidationColor { get; }

	double BillboardSizeMultiplier { get; set; }

	TextStyle BillboardTextStyle { get; }

	GlyphStyle BillboardGlyphStyle { get; }

	BackgroundStyle BillboardBackgroundStyle { get; }

	void RegisterBillboardStilesChanged(IStyleReceiver styleReceiver);
}
