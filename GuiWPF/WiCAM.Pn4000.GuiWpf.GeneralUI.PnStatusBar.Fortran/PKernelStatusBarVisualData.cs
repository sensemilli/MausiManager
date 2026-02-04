using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PKernelStatusBarVisualData : INotifyPropertyChanged, IPKernelStatusBarVisualData
{
	private readonly IPKernelStatusBarData _data;

	private Visibility _visibility = Visibility.Hidden;

	private string _info = string.Empty;

	private Brush _brush;

	private Brush _textBrush;

	private bool _subItemsVisibility;

	public bool SubItemsVisibility
	{
		get
		{
			return _subItemsVisibility;
		}
		set
		{
			_subItemsVisibility = value;
			OnPropertyChanged("SubItemsVisibility");
		}
	}

	public string Info
	{
		get
		{
			return _info;
		}
		set
		{
			_info = value;
			OnPropertyChanged("Info");
		}
	}

	public Visibility Visibility
	{
		get
		{
			return _visibility;
		}
		set
		{
			_visibility = value;
			OnPropertyChanged("Visibility");
		}
	}

	public Brush Brush
	{
		get
		{
			return _brush;
		}
		set
		{
			_brush = value;
			OnPropertyChanged("Brush");
		}
	}

	public Brush TextBrush
	{
		get
		{
			return _textBrush;
		}
		set
		{
			_textBrush = value;
			OnPropertyChanged("TextBrush");
		}
	}

	public ObservableCollection<IPKernelStatusBarSubVisualData> SubItems { get; set; } = new ObservableCollection<IPKernelStatusBarSubVisualData>();

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public PKernelStatusBarVisualData(IPKernelStatusBarData data)
	{
		_data = data;
	}

	public void UpdateData()
	{
		if (_data.isVisible)
		{
			Visibility = Visibility.Visible;
		}
		else
		{
			Visibility = Visibility.Hidden;
		}
		SplitValueTwoParts(_data.MainStatus, out string value, out string value2);
		PKernelStatusBarStringInterpreter pKernelStatusBarStringInterpreter = new PKernelStatusBarStringInterpreter(value2.Trim());
		Info = pKernelStatusBarStringInterpreter.Text;
		Brush = pKernelStatusBarStringInterpreter.Brush;
		TextBrush = pKernelStatusBarStringInterpreter.TextBrush;
		for (int i = 0; i < _data.SubStatusList.Count; i++)
		{
			if (i >= SubItems.Count)
			{
				SubItems.Add(new PKernelStatusBarSubVisualData());
			}
			string value3 = _data.SubStatusList[i];
			if (string.IsNullOrEmpty(value3))
			{
				SubItems[i].Visibility = Visibility.Collapsed;
				continue;
			}
			SplitValueTwoParts(value3, out value, out value2);
			SubItems[i].Visibility = Visibility.Visible;
			SubItems[i].Label = value.Trim();
			SubItems[i].Info = value2.Trim();
		}
		for (int j = _data.SubStatusList.Count; j < SubItems.Count; j++)
		{
			SubItems[j].Visibility = Visibility.Collapsed;
		}
		SubItemsVisibility = GetSubItemsPanelVisibility(SubItems);
	}

	private bool GetSubItemsPanelVisibility(ObservableCollection<IPKernelStatusBarSubVisualData> subItems)
	{
		return subItems.Any((IPKernelStatusBarSubVisualData x) => x.Visibility == Visibility.Visible);
	}

	private void SplitValueTwoParts(string value, out string value1, out string value2)
	{
		if (!value.Contains('#'))
		{
			value1 = string.Empty;
			value2 = value;
			return;
		}
		int num = value.IndexOf('#');
		if (value.Length > num + 1)
		{
			value2 = value.Substring(num + 1);
		}
		else
		{
			value2 = string.Empty;
		}
		if (num > 0)
		{
			value1 = value.Substring(0, num);
		}
		else
		{
			value1 = string.Empty;
		}
	}
}
