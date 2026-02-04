using System.Collections;

namespace WiCAM.Pn4000.GuiWpf.TabBend.Developer;

internal class EnumerableNester
{
	public IEnumerable Enumerable { get; set; }

	public EnumerableNester(IEnumerable enumerable)
	{
		Enumerable = enumerable;
	}
}
