using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class ContextMenuItemGoToStepViewModel : ContextMenuItemBase
{
	private ImageSource _imageUpperDpFlat;

	private ImageSource _imageClosed;

	private ImageSource _imageBend;

	private ImageSource _imageUpperDpBend;

	private ICommand _commandUpperDpFlat;

	private ICommand _commandClosed;

	private ICommand _commandBend;

	private ICommand _commandUpperDpBend;

	public ImageSource ImageUpperDpFlat
	{
		get
		{
			return this._imageUpperDpFlat;
		}
		set
		{
			this._imageUpperDpFlat = value;
			base.NotifyPropertyChanged("ImageUpperDpFlat");
		}
	}

	public ImageSource ImageClosed
	{
		get
		{
			return this._imageClosed;
		}
		set
		{
			this._imageClosed = value;
			base.NotifyPropertyChanged("ImageClosed");
		}
	}

	public ImageSource ImageBend
	{
		get
		{
			return this._imageBend;
		}
		set
		{
			this._imageBend = value;
			base.NotifyPropertyChanged("ImageBend");
		}
	}

	public ImageSource ImageUpperDpBend
	{
		get
		{
			return this._imageUpperDpBend;
		}
		set
		{
			this._imageUpperDpBend = value;
			base.NotifyPropertyChanged("ImageUpperDpBend");
		}
	}

	public ICommand CommandUpperDpFlat
	{
		get
		{
			return this._commandUpperDpFlat;
		}
		set
		{
			this._commandUpperDpFlat = value;
			base.NotifyPropertyChanged("CommandUpperDpFlat");
		}
	}

	public ICommand CommandClosed
	{
		get
		{
			return this._commandClosed;
		}
		set
		{
			this._commandClosed = value;
			base.NotifyPropertyChanged("CommandClosed");
		}
	}

	public ICommand CommandBend
	{
		get
		{
			return this._commandBend;
		}
		set
		{
			this._commandBend = value;
			base.NotifyPropertyChanged("CommandBend");
		}
	}

	public ICommand CommandUpperDpBend
	{
		get
		{
			return this._commandUpperDpBend;
		}
		set
		{
			this._commandUpperDpBend = value;
			base.NotifyPropertyChanged("CommandUpperDpBend");
		}
	}

	public ContextMenuItemGoToStepViewModel(Action actionUpperDpFlat, Action actionClosed, Action actionBend, Action actionUpperDpBend, string imagePathUppDpFlat, string imagePathClosed, string imagePathBend, string imagePathUpperDpBend)
		: base("")
	{
		this.CommandUpperDpFlat = new RelayCommand((Action<object>)delegate
		{
			actionUpperDpFlat();
		});
		this.CommandClosed = new RelayCommand((Action<object>)delegate
		{
			actionClosed();
		});
		this.CommandBend = new RelayCommand((Action<object>)delegate
		{
			actionBend();
		});
		this.CommandUpperDpBend = new RelayCommand((Action<object>)delegate
		{
			actionUpperDpBend();
		});
		this.ImageUpperDpFlat = (File.Exists(imagePathUppDpFlat) ? new BitmapImage(new Uri(imagePathUppDpFlat)) : base.CreateImage());
		this.ImageClosed = (File.Exists(imagePathClosed) ? new BitmapImage(new Uri(imagePathClosed)) : base.CreateImage());
		this.ImageBend = (File.Exists(imagePathBend) ? new BitmapImage(new Uri(imagePathBend)) : base.CreateImage());
		this.ImageUpperDpBend = (File.Exists(imagePathUpperDpBend) ? new BitmapImage(new Uri(imagePathUpperDpBend)) : base.CreateImage());
	}
}
