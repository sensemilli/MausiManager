using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PKernelStatusBarSubVisualData : INotifyPropertyChanged, IPKernelStatusBarSubVisualData
{
	private string _label = string.Empty;

	private string _info = string.Empty;

	private Visibility _visibility = Visibility.Collapsed;

	public string Label
	{
		get
		{
			return _label;
		}
		set
		{
			_label = value;
			OnPropertyChanged("Label");
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

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
