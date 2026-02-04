using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.CalculationInfo;

internal class Status3dCalculationInfoViewModel : ViewModelBase
{
	private readonly ITranslator _translator;

	private readonly IMainWindowTaskbarItemInfo _mainWindowTaskbar;

	private readonly IPnPathService _pathService;

	private ICurrentCalculation? _currentCalculation;

	private ICalculationArg? _currentCalcArg;

	private ThumbButtonInfo _taskbarButtonCancel = new ThumbButtonInfo();

	private bool _showCancelButtonInTaskbar;

	public string CancelDesc { get; set; } = "";

	public Visibility CancelVisibility { get; set; } = Visibility.Collapsed;

	public Visibility CalculatingPrefixVisibility { get; set; } = Visibility.Collapsed;

	public string Status { get; set; } = "";

	public double Progress { get; set; }

	public Visibility ProgressVisibility { get; set; } = Visibility.Collapsed;

	public bool ProgressIndeterminate { get; set; }

	public ICommand CmdCancel { get; }

	public Status3dCalculationInfoViewModel(ICurrentDocProvider currentDocProvider, ITranslator translator, IMainWindowTaskbarItemInfo mainWindowTaskbar, IPnPathService pathService)
	{
		_translator = translator;
		_mainWindowTaskbar = mainWindowTaskbar;
		_pathService = pathService;
		currentDocProvider.CurrentDocChanged += CurrentDocProvider_CurrentDocChanged;
		CurrentDocProvider_CurrentDocChanged(null, currentDocProvider.CurrentDoc);
		CmdCancel = new RelayCommand(CancelCalculation);
		string uriString = _pathService.PnMasterOrDrive + "\\u\\pn\\pixmap\\32\\PNENDE.png";
		_taskbarButtonCancel.Description = _translator.Translate("PnInfoFields3D.Calculation.Cancel");
		_taskbarButtonCancel.Command = CmdCancel;
		_taskbarButtonCancel.ImageSource = new BitmapImage(new Uri(uriString));
	}

	private void CurrentDocProvider_CurrentDocChanged(IDoc3d oldDoc, IDoc3d newDoc)
	{
		ICurrentCalculation currentCalculation = newDoc?.Factorio.Resolve<ICurrentCalculation>();
		if (_currentCalculation != null)
		{
			_currentCalculation.CurrentCalculationChanged -= CurrentCalculationCurrentCalculationChanged;
		}
		_currentCalculation = currentCalculation;
		if (_currentCalculation != null)
		{
			_currentCalculation.CurrentCalculationChanged += CurrentCalculationCurrentCalculationChanged;
		}
		CurrentCalculationCurrentCalculationChanged(_currentCalculation?.CurrentCalculationOption);
	}

	private void CurrentCalculationCurrentCalculationChanged(ICalculationArg? arg)
	{
		if (_currentCalcArg != null)
		{
			_currentCalcArg.StatusChanged -= StatusChanged;
		}
		_currentCalcArg = arg;
		if (_currentCalcArg != null)
		{
			_currentCalcArg.StatusChanged += StatusChanged;
		}
		StatusChanged(_currentCalcArg);
	}

	private void StatusChanged(ICalculationArg? arg)
	{
		Status = arg?.Status ?? string.Empty;
		Progress = (arg?.Progress).GetValueOrDefault();
		ProgressVisibility = ((arg == null) ? Visibility.Collapsed : Visibility.Visible);
		ProgressIndeterminate = !(arg?.Progress).HasValue;
		bool? flag = arg?.CancellationToken.CanBeCanceled;
		if (flag.HasValue)
		{
			if (flag == true)
			{
				CancelDesc = (arg.CancellationToken.IsCancellationRequested ? _translator.Translate("PnInfoFields3D.Calculation.Cancelling") : _translator.Translate("PnInfoFields3D.Calculation.Cancel"));
				CancelVisibility = Visibility.Visible;
				CalculatingPrefixVisibility = Visibility.Collapsed;
			}
			else
			{
				CancelVisibility = Visibility.Collapsed;
				CalculatingPrefixVisibility = Visibility.Visible;
			}
		}
		else
		{
			CancelVisibility = Visibility.Collapsed;
			CalculatingPrefixVisibility = Visibility.Collapsed;
		}
		Application.Current.Dispatcher.BeginInvoke(new Action(NotifyAllProperties));
	}

	private void NotifyAllProperties()
	{
		NotifyPropertyChanged("CancelDesc");
		NotifyPropertyChanged("CancelVisibility");
		NotifyPropertyChanged("CalculatingPrefixVisibility");
		NotifyPropertyChanged("Status");
		NotifyPropertyChanged("Progress");
		NotifyPropertyChanged("ProgressVisibility");
		NotifyPropertyChanged("ProgressIndeterminate");
		UpdateMainWindowTaskbar();
	}

	private void UpdateMainWindowTaskbar()
	{
		ICalculationArg currentCalcArg = _currentCalcArg;
		if (currentCalcArg != null)
		{
			double? progress = currentCalcArg.Progress;
			if (progress.HasValue)
			{
				_mainWindowTaskbar.TaskbarItemInfo.ProgressValue = progress.Value;
				_mainWindowTaskbar.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
			}
			else
			{
				_mainWindowTaskbar.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
			}
			if (currentCalcArg.CancellationToken.IsCancellationRequested)
			{
				_mainWindowTaskbar.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;
				_taskbarButtonCancel.IsEnabled = false;
			}
			else
			{
				_taskbarButtonCancel.IsEnabled = true;
			}
			_mainWindowTaskbar.TaskbarItemInfo.Description = currentCalcArg.Status;
		}
		else
		{
			_mainWindowTaskbar.TaskbarItemInfo.Description = null;
			_mainWindowTaskbar.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
		}
		bool flag = _currentCalcArg?.CancellationToken.CanBeCanceled ?? false;
		if (_showCancelButtonInTaskbar != flag)
		{
			if (flag)
			{
				_mainWindowTaskbar.TaskbarItemInfo.ThumbButtonInfos.Add(_taskbarButtonCancel);
			}
			else
			{
				_mainWindowTaskbar.TaskbarItemInfo.ThumbButtonInfos.Remove(_taskbarButtonCancel);
			}
			_showCancelButtonInTaskbar = flag;
		}
	}

	private void CancelCalculation()
	{
		_currentCalcArg?.TryCancelCalculation();
	}
}
