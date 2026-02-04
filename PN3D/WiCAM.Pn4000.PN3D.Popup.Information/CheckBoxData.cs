using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class CheckBoxData<T> : INotifyPropertyChanged
{
	private bool _isSelected;

	public Action NotifyFilterChange;

	public string Name { get; set; }

	public int Index { get; set; }

	public T Value { get; set; }

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
			this.NotifyFilterChange?.Invoke();
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
