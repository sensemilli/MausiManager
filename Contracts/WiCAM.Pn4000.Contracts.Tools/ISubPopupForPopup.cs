namespace WiCAM.Pn4000.Contracts.Tools;

public interface ISubPopupForPopup
{
	int SelectColorPnIntEdition(int pnColorId);

	ISubPopupForPopup Init(object parent);
}
