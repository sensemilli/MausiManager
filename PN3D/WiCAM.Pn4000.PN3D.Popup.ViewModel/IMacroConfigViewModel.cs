using WiCAM.Pn4000.PN3D.Popup.Model;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public interface IMacroConfigViewModel
{
	void Save(PopupUnfoldSettingModel popupUnfoldSettingModel);

	void Load(PopupUnfoldSettingViewModel popupUnfoldSettingViewModel);
}
