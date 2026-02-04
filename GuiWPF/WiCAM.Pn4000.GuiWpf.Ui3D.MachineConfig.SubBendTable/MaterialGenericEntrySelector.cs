using System.Windows.Media;
using System.Windows.Media.Media3D;
using Telerik.Charting;
using Telerik.Windows.Controls.ChartView;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;

public class MaterialGenericEntrySelector : MaterialSelector
{
	private Material _specific = new DiffuseMaterial(Brushes.Aqua);

	private Material _generic = new DiffuseMaterial(Brushes.Orange);

	private Material _error = new DiffuseMaterial(Brushes.Red);

	public override Material SelectMaterial(object context)
	{
		if (context is XyzDataPoint3D { DataItem: BendTableEntry dataItem })
		{
			if (dataItem.IsSpecific)
			{
				return _specific;
			}
			return _generic;
		}
		return _error;
	}
}
