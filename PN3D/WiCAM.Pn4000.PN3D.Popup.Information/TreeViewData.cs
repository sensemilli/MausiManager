using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class TreeViewData : INotifyPropertyChanged
{
	private string _text;

	private int _iD;

	private ObservableCollection<TreeViewData> _subData;

	private bool _isExpanded;

	private bool _isSelected;

	public string Text
	{
		get
		{
			return this._text;
		}
		set
		{
			this._text = value;
			this.OnPropertyChanged("Text");
		}
	}

	public int ID
	{
		get
		{
			return this._iD;
		}
		set
		{
			this._iD = value;
			this.OnPropertyChanged("ID");
		}
	}

	public ObservableCollection<TreeViewData> SubData
	{
		get
		{
			return this._subData;
		}
		set
		{
			this._subData = value;
			this.OnPropertyChanged("SubData");
		}
	}

	public bool IsExpanded
	{
		get
		{
			return this._isExpanded;
		}
		set
		{
			this._isExpanded = value;
			this.OnPropertyChanged("IsExpanded");
		}
	}

	public bool IsSelected
	{
		get
		{
			return this._isSelected;
		}
		set
		{
			this._isSelected = value;
			this.OnPropertyChanged("IsSelected");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public TreeViewData()
	{
		this.SubData = new ObservableCollection<TreeViewData>();
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
