using Telerik.Windows.Data;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.Legend;

public interface ILegendViewModel
{
	public interface ILegendEntry
	{
		int No { get; }

		string Desc { get; }

		string Value { get; set; }

		string Type { get; }
	}

	RadObservableCollection<ILegendEntry> Entries { get; }

	void Init(IPnBndDoc doc);

	void Save();
}
