using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup.Information;

public class QuickTableRecord
{
	public ObservableCollection<object> Data { get; set; } = new ObservableCollection<object>();

	public ToolTip RecordToolTip { get; set; }

	public int Inte1 { get; set; }

	public int Id1 { get; set; }

	public bool IsDataRecord { get; set; } = true;

	public bool IsSelectable { get; set; } = true;

	public bool IsColored { get; set; }

	public bool IsForEnabled
	{
		get
		{
			if (this.IsDataRecord)
			{
				return this.IsSelectable;
			}
			return false;
		}
	}

	public Brush ConstBrush { get; set; } = new SolidColorBrush(Colors.Transparent);

	public Brush FontBrush { get; set; } = new SolidColorBrush(Colors.Black);
}
