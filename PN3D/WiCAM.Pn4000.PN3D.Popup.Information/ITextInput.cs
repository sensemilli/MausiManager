using System.Windows;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public interface ITextInput
{
	string Result { get; set; }


	bool IsOkExit { get; }

	ITextInput Init(string str1, string str2);
    Window Owner();
    void Owner(Window value);
    void Show();

	void StartLikeModalMode();
}
