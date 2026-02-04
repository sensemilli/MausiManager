using System.Windows;
using System.Windows.Controls;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class GroupToolViewModel : ViewModelBase
{
	private Canvas _imageProfile;

	public Canvas ImageProfile
	{
		get
		{
			return this._imageProfile;
		}
		set
		{
			this._imageProfile = value;
			base.NotifyPropertyChanged("ImageProfile");
		}
	}

	public GroupToolViewModel(double width, double height)
	{
		this.ImageProfile = new Canvas
		{
			Height = height,
			Width = width
		};
		GroupToolViewModel.MeasureAndArrange(this.ImageProfile);
	}

	private static void MeasureAndArrange(UIElement element)
	{
		element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		element.Arrange(new Rect(0.0, 0.0, element.DesiredSize.Width, element.DesiredSize.Height));
	}
}
