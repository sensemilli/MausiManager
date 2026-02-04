using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.GuiContracts.Ribbon;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Ribbon.NewRibbonEntitiesMV;

public class NewRibbonBaseEntity : INotifyPropertyChanged, INewRibbonBaseEntity
{
	public INewRibbonBaseEntity Parent;

	private string _label = string.Empty;

	public ObservableCollection<INewRibbonBaseEntity> Children { get; set; }

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

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
