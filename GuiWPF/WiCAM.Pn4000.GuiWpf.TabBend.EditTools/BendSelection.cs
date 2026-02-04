using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendSimulation;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class BendSelection : IBendSelection
{
	private readonly IBendSimulationFactory _bendSimulationFactory;

	private readonly IEditToolsSelection _editToolsSelection;

	private readonly IToolsToMachineModel _toolsToMachine;

	private readonly IScreen3DMain _screenMain;

	private IBendPositioning? _currentBendHovering;

	private IBendPositioning? _currentBend;

	private HashSet<IBendPositioning> _bends = new HashSet<IBendPositioning>();

	public IBendPositioning? CurrentBendHovering
	{
		get
		{
			return _currentBendHovering;
		}
		set
		{
			if (_currentBendHovering != value)
			{
				_currentBendHovering = value;
				this.CurrentBendHoveringChanged?.Invoke(_currentBendHovering);
			}
		}
	}

	public IBendPositioning? CurrentBend
	{
		get
		{
			return _currentBend;
		}
		set
		{
			if (_currentBend != value)
			{
				_currentBend = value;
				RefreshCurrentBend();
			}
		}
	}

	public IEnumerable<IBendPositioning> SelectedBends => _bends;

	public IPnBndDoc CurrentDoc { get; private set; }

	public IToolsAndBends ToolsAndBends { get; private set; }

	public event Action<IBendPositioning?>? CurrentBendChanged;

	public event Action<IBendPositioning?>? CurrentBendHoveringChanged;

	public event Action? SelectionChanged;

	public event Action? DataChanged;

	public BendSelection(IToolsToMachineModel toolsToMachine, IScreen3DMain screenMain, IBendSimulationFactory bendSimulationFactory, IPnBndDoc doc, IEditToolsSelection editToolsSelection)
	{
		CurrentDoc = doc;
		_toolsToMachine = toolsToMachine;
		_screenMain = screenMain;
		_bendSimulationFactory = bendSimulationFactory;
		_editToolsSelection = editToolsSelection;
		SelectionChanged += BendSelection_SelectionChanged;
		CurrentDoc.ToolsAndBendsChanged += CurrentDoc_ToolsAndBendsChangedBackground;
		CurrentDoc.RefreshSimulation += CurrentDoc_RefreshSimulation;
		CurrentBendChanged += BendSelection_CurrentBendChanged;
		CurrentDoc_ToolsAndBendsChangedUi();
	}

	public ICombinedBendDescriptorInternal GetCbd(IBendPositioning bend)
	{
		return CurrentDoc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == bend.Order);
	}

	public IBendPositioning? GetBendPositioning(ICombinedBendDescriptor cbd)
	{
		if (cbd == null)
		{
			return null;
		}
		return ToolsAndBends.BendPositions.FirstOrDefault((IBendPositioning x) => x.Bend.CombinedBendDescriptor == cbd);
	}

	private void BendSelection_CurrentBendChanged(IBendPositioning? obj)
	{
		IToolSetups toolSetups = _currentBend?.Anchor?.Root;
		if (toolSetups != null)
		{
			_editToolsSelection.SetDataToolSetups(toolSetups);
		}
	}

	private void CurrentDoc_ToolsAndBendsChangedBackground(IToolsAndBends tabOld, IToolsAndBends tabNew)
	{
		Application.Current.Dispatcher.Invoke(CurrentDoc_ToolsAndBendsChangedUi);
	}

	private void CurrentDoc_ToolsAndBendsChangedUi()
	{
		IToolsAndBends toolsAndBends = CurrentDoc.ToolsAndBends;
		_bends.Clear();
		ToolsAndBends = toolsAndBends;
		_currentBend = toolsAndBends?.BendPositions.FirstOrDefault();
		this.DataChanged?.Invoke();
		this.SelectionChanged?.Invoke();
		RefreshCurrentBend();
	}

	private void CurrentDoc_RefreshSimulation(IPnBndDoc obj)
	{
		RefreshSimulation();
	}

	private void BendSelection_SelectionChanged()
	{
		_ = _bends.Count;
		_ = 1;
	}

	public void SetData(IPnBndDoc doc, IToolsAndBends toolsAndBends)
	{
		_bends.Clear();
		CurrentDoc = doc;
		ToolsAndBends = toolsAndBends;
		this.DataChanged?.Invoke();
		this.SelectionChanged?.Invoke();
	}

	public void SetSelection(IBendPositioning bend, bool isSelected)
	{
		if (isSelected)
		{
			_bends.Add(bend);
		}
		else
		{
			_bends.Remove(bend);
		}
		this.SelectionChanged?.Invoke();
	}

	public void ToggleSelection(IBendPositioning bend)
	{
		if (!_bends.Add(bend))
		{
			_bends.Remove(bend);
		}
		this.SelectionChanged?.Invoke();
	}

	public void UnselectAll()
	{
		_bends.Clear();
		this.SelectionChanged?.Invoke();
	}

	public bool IsSelected(IBendPositioning bend)
	{
		return _bends.Contains(bend);
	}

	public void SetCurrentBendBySimulation(int? newOrder)
	{
		IBendPositioning bendPositioning = ToolsAndBends?.BendPositions.FirstOrDefault((IBendPositioning x) => x.Order == newOrder);
		if (bendPositioning != _currentBend)
		{
			_currentBend = bendPositioning;
			this.CurrentBendChanged?.Invoke(_currentBend);
		}
	}

	public void SetCurrentBendHoveredByOrder(int? newOrder)
	{
		IBendPositioning bendPositioning = ToolsAndBends?.BendPositions.FirstOrDefault((IBendPositioning x) => x.Order == newOrder);
		if (bendPositioning != _currentBendHovering)
		{
			_currentBendHovering = bendPositioning;
			this.CurrentBendHoveringChanged?.Invoke(_currentBendHovering);
		}
	}

	public void RefreshCurrentBend()
	{
		if (_currentBend != null)
		{
			IBendPositioning currentBend = _currentBend;
			ISimulationThread simulationThread = CreateNewSim();
			simulationThread?.GoToBend(currentBend.Order, visible: false);
			CurrentDoc.BendSimulation = simulationThread;
		}
		this.CurrentBendChanged?.Invoke(_currentBend);
	}

	public void RefreshSimulation()
	{
		IPnBndDoc currentDoc = CurrentDoc;
		bool flag = currentDoc != null && currentDoc.BendSimulation?.IsRunning == true;
		if (_currentBend != null)
		{
			double currentStep = CurrentDoc.BendSimulation.CurrentStep;
			ISimulationThread simulationThread = CreateNewSim();
			simulationThread?.GotoStep(currentStep, visible: false);
			CurrentDoc.BendSimulation = simulationThread;
			if (flag)
			{
				simulationThread?.Play();
			}
		}
	}

	private ISimulationThread CreateNewSim()
	{
		Model bendModel3D = CurrentDoc.BendModel3D;
		IBendMachine bendMachine = CurrentDoc.BendMachine;
		double machineCenter = (bendMachine?.ToolConfig.LowerBeamXStart + bendMachine?.ToolConfig.LowerBeamXEnd).GetValueOrDefault() * 0.5;
		SimParams creationParameters = new SimParams
		{
			BendMachine = bendMachine,
			Bends = CurrentDoc.CombinedBendDescriptors.Select((ICombinedBendDescriptorInternal cbd) => new SimBendInfo(cbd, ToolsAndBends.BendPositions.FirstOrDefault((IBendPositioning x) => x.Order == cbd.Order))
			{
				MachineCenter = machineCenter
			}).Cast<ISimulationBendInfo>().ToList(),
			Material = CurrentDoc.Material,
			PartModel = bendModel3D,
			SimulationStepFilter = new SimStepFilterAll(),
			IgnoreCollisionsCompletly = false,
			Thickness = CurrentDoc.Thickness,
			ToolSetups = ToolsAndBends.ToolSetups
		};
		return _bendSimulationFactory.CreateSimulation(creationParameters);
	}
}
