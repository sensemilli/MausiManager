namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class ComboboxEntry<T>
{
	public string Desc { get; set; }

	public T Value { get; set; }

	public ComboboxEntry(string desc, T value)
	{
		this.Desc = desc;
		this.Value = value;
	}
}
