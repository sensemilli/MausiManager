using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.GuiWpf.UiBendMachine.SelectMachine;

public class VisualPair<T1, T2> : ViewModelBase
{
	private T1 _item1;

	private T2 _item2;

	public T1 Item1
	{
		get
		{
			return _item1;
		}
		set
		{
			_item1 = value;
			NotifyPropertyChanged("Item1");
		}
	}

	public T2 Item2
	{
		get
		{
			return _item2;
		}
		set
		{
			_item2 = value;
			NotifyPropertyChanged("Item2");
		}
	}

	public VisualPair(T1 item1, T2 item2)
	{
		Item1 = item1;
		Item2 = item2;
	}
}
