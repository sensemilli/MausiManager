using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class PopupPnColor : INotifyPropertyChanged
{
	public int PnNumber { get; }

	public Brush Brush { get; }

	public global::System.Windows.Media.Color Color { get; }

	public event PropertyChangedEventHandler PropertyChanged;

	public PopupPnColor(int pnNumber, global::WiCAM.Pn4000.BendModel.Base.Color color)
	{
		this.PnNumber = pnNumber;
		this.Color = global::System.Windows.Media.Color.FromRgb((byte)(color.R * 255f), (byte)(color.G * 255f), (byte)(color.B * 255f));
		this.Brush = new SolidColorBrush(this.Color);
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
