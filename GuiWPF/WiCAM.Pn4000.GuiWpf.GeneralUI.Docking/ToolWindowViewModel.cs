using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WiCAM.Pn4000.GuiContracts.Implementations;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;

public class ToolWindowViewModel : INotifyPropertyChanged
{
	private const string GlyphPin = "\ue704";

	private const string GlyphUnpin = "\ue705";

	private const string GlyphPaint = "\ue50f";

	private const string GlyphResetColor = "\ue52a";

	public string PinGlyph
	{
		get
		{
			if (!IsPinned)
			{
				return "\ue705";
			}
			return "\ue704";
		}
	}

	public string TransparencyGlyph
	{
		get
		{
			if (!IsTransparencyEnabled)
			{
				return "\ue52a";
			}
			return "\ue50f";
		}
	}

	public bool IsTransparencyEnabled { get; set; }

	public bool IsPinned { get; set; }

	public ICommand ToggleTransparencyCommand { get; }

	public ICommand TogglePinCommand { get; }

	public string Title { get; set; }

	public HashSet<object> Panes { get; } = new HashSet<object>();

	public event PropertyChangedEventHandler? PropertyChanged;

	public ToolWindowViewModel()
	{
		ToggleTransparencyCommand = new RelayCommand(ToggleTransparency);
		TogglePinCommand = new RelayCommand(TogglePin);
	}

	public void ToggleTransparency(object parameter)
	{
		IsTransparencyEnabled = !IsTransparencyEnabled;
		NotifyPropertyChanged("TransparencyGlyph");
	}

	public void TogglePin(object parameter)
	{
		IsPinned = !IsPinned;
		NotifyPropertyChanged("PinGlyph");
	}

	protected virtual void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		NotifyPropertyChanged(propertyName);
		return true;
	}
}
