using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.PN3D;

namespace WiCAM.Pn4000.GuiWpf.Ui3D;

public partial class InfoPopupWithCancel : Window, IWaitCancel, INotifyPropertyChanged, IComponentConnector
{
	private readonly IMainWindowTaskbarItemInfo _mainWindowTaskbar;

	private bool _isCancel;

	private string _message;

	private double? _progress;

	private ThumbButtonInfo _taskbarButtonCancel = new ThumbButtonInfo();

	public bool IsCancel
	{
		get
		{
			return _isCancel;
		}
		set
		{
			if (_isCancel != value)
			{
				_isCancel = value;
				_taskbarButtonCancel.IsEnabled = !_isCancel;
				_mainWindowTaskbar.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;
			}
		}
	}

	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			if (_message != value)
			{
				_message = value;
				Application.Current.Dispatcher.BeginInvoke(new Action(MessageNotify));
			}
		}
	}

	public double ProgressUi => Progress.GetValueOrDefault();

	public double? Progress
	{
		get
		{
			return _progress;
		}
		set
		{
			if (_progress != value)
			{
				_progress = value;
				Application.Current.Dispatcher.BeginInvoke(new Action(ProgressChanged));
			}
		}
	}

	public ICommand CmdCancel { get; }

	public Visibility ProgressVisibility { get; set; } = Visibility.Collapsed;

	public event PropertyChangedEventHandler? PropertyChanged;

	private void MessageNotify()
	{
		OnPropertyChanged("Message");
	}

	private void ProgressChanged()
	{
		double? progress = _progress;
		OnPropertyChanged("Progress");
		OnPropertyChanged("ProgressUi");
		ProgressVisibility = ((!progress.HasValue) ? Visibility.Collapsed : Visibility.Visible);
		OnPropertyChanged("ProgressVisibility");
		if (progress.HasValue)
		{
			_mainWindowTaskbar.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
			_mainWindowTaskbar.TaskbarItemInfo.ProgressValue = progress.Value;
		}
		else
		{
			_mainWindowTaskbar.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
		}
	}

	public InfoPopupWithCancel(IPnPathService pathService, IMainWindowTaskbarItemInfo mainWindowTaskbar, ITranslator translator)
	{
		_mainWindowTaskbar = mainWindowTaskbar;
		InitializeComponent();
		base.DataContext = this;
		CmdCancel = new RelayCommand(OnCancel);
		string uriString = pathService.PnMasterOrDrive + "\\u\\pn\\pixmap\\32\\PNENDE.png";
		_taskbarButtonCancel.Description = translator.Translate("l_popup.Button5_Cancel");
		_taskbarButtonCancel.Command = CmdCancel;
		_taskbarButtonCancel.ImageSource = new BitmapImage(new Uri(uriString));
		_mainWindowTaskbar.TaskbarItemInfo.ThumbButtonInfos.Add(_taskbarButtonCancel);
		_mainWindowTaskbar.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		((Storyboard)TryFindResource("TimeAnimation")).Begin();
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		if (sizeInfo.HeightChanged)
		{
			base.Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2.0;
		}
		if (sizeInfo.WidthChanged)
		{
			base.Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2.0;
		}
	}

	private void OnCancel()
	{
		IsCancel = true;
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void InfoPopupWithCancel_OnClosed(object? sender, EventArgs e)
	{
		_mainWindowTaskbar.TaskbarItemInfo.ThumbButtonInfos.Remove(_taskbarButtonCancel);
		_mainWindowTaskbar.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
	}
}
