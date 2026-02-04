namespace WiCAM.Pn4000.Contracts.LogCenterServices;

public interface IPnDebug
{
	bool WasUsing { get; set; }

	bool IsVisible { get; }

	void AddString(string str);

	void Clear();

	void ClearInternal();

	void DebugThat(string str);

	void DebugThatOptionally(int v, string str);

	void Flush();

	void HidePlus();

	void InitializeComponent();

	void SaveConfig();

	void ScrollToEnd();

	void ShowPlus();
}
