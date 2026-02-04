using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class ContextMenuItemBase : ViewModelBase
{
	private ImageSource _image;

	public ImageSource Image
	{
		get
		{
			return this._image;
		}
		set
		{
			this._image = value;
			base.NotifyPropertyChanged("Image");
		}
	}

	public ContextMenuItemBase(string imagePath)
	{
		this.Image = (File.Exists(imagePath) ? new BitmapImage(new Uri(imagePath)) : this.CreateImage());
	}

	public BitmapSource CreateImage()
	{
		int num = 32;
		int num2 = 32 / 8;
		return BitmapSource.Create(pixels: new byte[num * num2], palette: new BitmapPalette(new List<Color> { Colors.Transparent }), pixelWidth: 32, pixelHeight: num, dpiX: 96.0, dpiY: 96.0, pixelFormat: PixelFormats.Indexed1, stride: num2);
	}
}
