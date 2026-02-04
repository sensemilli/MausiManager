using WiCAM.Pn4000.PN3D.Popup.Model;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public interface IValidationSettingsViewModel
{
	void Save();

	IValidationSettingsViewModel Init(PopupUnfoldSettingModel model, string pndrive);
}
