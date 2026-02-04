using System;
using System.Windows;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.Implementations;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;



namespace WiCAM.Pn4000.GuiWpf.TabBend.Developer;

internal class OverviewViewModel : ViewModelBase
{
    private readonly ICurrentCalculation _currentToolCalculation;

    private readonly IScreen3DMain _screen;

    private readonly IFactorio _factorio;

    private bool _pauseDuringCommands;

    private bool _logActive;

    private object _selectedItem;

    public RelayCommand CmdRefresh { get; }

    public RelayCommand CmdCurrentCalculationCancel { get; }

    public RelayCommand CmdCurrentCalculationContinue { get; }

    public bool PauseDuringCommands
    {
        get
        {
            return _currentToolCalculation.CurrentCalculationOption?.DebugInfo?.PauseEnabled ?? _pauseDuringCommands;
        }
        set
        {
            ICalculationDebugArg calculationDebugArg = _currentToolCalculation.CurrentCalculationOption?.DebugInfo;
            if (calculationDebugArg != null)
            {
                calculationDebugArg.PauseEnabled = value;
            }
            _pauseDuringCommands = value;
            RaisePropertyChanged("PauseDuringCommands");
        }
    }

    public bool LogActive
    {
        get
        {
            return _logActive;
        }
        set
        {
            _logActive = value;
            RaisePropertyChanged("LogActive");
        }
    }

    public string CurrentCalculationInfo { get; }

    public object SelectedItem
    {
        get
        {
            return _selectedItem;
        }
        set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }
    }

    public OverviewViewModel(ICurrentCalculation currentToolCalculation, IScreen3DMain screen, IFactorio factorio)
    {
        _currentToolCalculation = currentToolCalculation;
        _screen = screen;
        _factorio = factorio;
        CmdRefresh = new RelayCommand(Refresh);
        CmdCurrentCalculationCancel = new RelayCommand(CurrentCalculationCancel, CanCurrentCalculationCancel);
        CmdCurrentCalculationContinue = new RelayCommand(CurrentCalculationContinue, CanCurrentCalculationContinue);
        _currentToolCalculation.CalculationWaiting += CrrentToolCalculation_CalculationWaiting;
        _currentToolCalculation.CurrentCalculationChanged += CurrentCalculationCurrentCalculationChanged;
    }

    private void CurrentCalculationCurrentCalculationChanged(ICalculationArg? option)
    {
        if (option != null && _logActive)
        {
            ICalculationDebugArg calculationDebugArg = _factorio.Resolve<ICalculationDebugArg>();
            calculationDebugArg.PauseEnabled = _pauseDuringCommands;
            option.DebugInfo = calculationDebugArg;
        }
    }

    public void AddSolution(IToolsAndBends? toolsAndBends)
    {
        if (toolsAndBends != null)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
            });
        }
    }

    public void Refresh()
    {
    }

    private bool CanCurrentCalculationCancel()
    {
        return _currentToolCalculation.CurrentCalculationOption?.CancellationToken.CanBeCanceled ?? false;
    }

    private void CurrentCalculationCancel()
    {
        _currentToolCalculation.CurrentCalculationOption?.TryCancelCalculation();
    }

    private bool CanCurrentCalculationContinue()
    {
        return true;
    }

    private void CurrentCalculationContinue()
    {
        (_currentToolCalculation.CurrentCalculationOption?.DebugInfo)?.ContinueCalculation();
    }

    private void CrrentToolCalculation_CalculationWaiting()
    {
        Application.Current.Dispatcher.BeginInvoke((Action)delegate
        {
            _screen.ScreenD3D.UpdateAllModelAppearance(render: false);
            _screen.ScreenD3D.UpdateAllModelTransform();
            _screen.ScreenD3D.UpdateAllModelGeometry();
        });
    }
}