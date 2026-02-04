using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.PKernelFlow.Adapters.Data;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup;

public class FullyDynamicPopupDefinition
{
	public string Name { get; set; }

	public string Label { get; set; }

	public bool ListMode { get; set; }

	public int ListModeType { get; set; }

	public bool OldStyleData { get; set; }

	public int[] Manipulators { get; set; }

	public List<PopupLine> Lines { get; set; }

	public List<string> Tabs { get; set; }

	public FullyDynamicPopupDefinition()
	{
		this.Name = string.Empty;
		this.Label = string.Empty;
		this.ListMode = false;
		this.ListModeType = 0;
		this.OldStyleData = false;
		this.Manipulators = new int[18];
		for (int i = 0; i < this.Manipulators.Count(); i++)
		{
			this.Manipulators[i] = 0;
		}
		this.Lines = new List<PopupLine>();
		this.Tabs = new List<string>();
	}
}
