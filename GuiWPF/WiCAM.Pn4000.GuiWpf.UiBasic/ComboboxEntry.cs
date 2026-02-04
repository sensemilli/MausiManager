namespace WiCAM.Pn4000.GuiWpf.UiBasic;

public class ComboboxEntry<T>
{
	public string Desc { get; set; }

	public T Value { get; set; }

	public ComboboxEntry(string desc, T value)
	{
		Desc = desc;
		Value = value;
	}
}
