using System;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.TabBend.SnapPointSettings;

internal class SnapPointViewModel : SubViewModelBase, ISnapPointViewModel, ISubViewModel, IPopupViewModel
{
	private readonly IConfigProvider _configProvider;

	private SnapOptions _snapOptions = new SnapOptions();

	private bool _raiseChanged = true;

	public bool SnapLeft
	{
		get
		{
			return _snapOptions.SnapLeft;
		}
		set
		{
			if (SnapLeft != value)
			{
				_snapOptions.SnapLeft = value;
				NotifyPropertyChanged2("SnapLeft");
			}
		}
	}

	public bool SnapRight
	{
		get
		{
			return _snapOptions.SnapRight;
		}
		set
		{
			if (SnapRight != value)
			{
				_snapOptions.SnapRight = value;
				NotifyPropertyChanged2("SnapRight");
			}
		}
	}

	public bool SnapCenter
	{
		get
		{
			return _snapOptions.SnapCenter;
		}
		set
		{
			if (SnapCenter != value)
			{
				_snapOptions.SnapCenter = value;
				NotifyPropertyChanged2("SnapCenter");
			}
		}
	}

	public bool SnapToAdapters
	{
		get
		{
			return _snapOptions.SnapToAdapters;
		}
		set
		{
			if (SnapToAdapters != value)
			{
				_snapOptions.SnapToAdapters = value;
				NotifyPropertyChanged2("SnapToAdapters");
			}
		}
	}

	public bool SnapToDies
	{
		get
		{
			return _snapOptions.SnapToDies;
		}
		set
		{
			if (SnapToDies != value)
			{
				_snapOptions.SnapToDies = value;
				NotifyPropertyChanged2("SnapToDies");
			}
		}
	}

	public bool SnapToPunches
	{
		get
		{
			return _snapOptions.SnapToPunches;
		}
		set
		{
			if (SnapToPunches != value)
			{
				_snapOptions.SnapToPunches = value;
				NotifyPropertyChanged2("SnapToPunches");
			}
		}
	}

	public bool SnapToMachine
	{
		get
		{
			return _snapOptions.SnapToMachine;
		}
		set
		{
			if (SnapToMachine != value)
			{
				_snapOptions.SnapToMachine = value;
				NotifyPropertyChanged2("SnapToMachine");
			}
		}
	}

	public bool SnapToBend
	{
		get
		{
			return _snapOptions.SnapToBend;
		}
		set
		{
			if (SnapToBend != value)
			{
				_snapOptions.SnapToBend = value;
				NotifyPropertyChanged2("SnapToBend");
			}
		}
	}

	public bool? SnapToTools
	{
		get
		{
			if (SnapToAdapters != SnapToPunches || SnapToAdapters != SnapToDies)
			{
				return null;
			}
			return SnapToAdapters;
		}
		set
		{
			if (value.HasValue)
			{
				_raiseChanged = false;
				SnapToAdapters = value.Value;
				SnapToPunches = value.Value;
				SnapToDies = value.Value;
				_raiseChanged = true;
				NotifyPropertyChanged2("SnapToTools");
			}
		}
	}

	public bool? SnapToAll
	{
		get
		{
			bool[] array = new bool[8] { SnapLeft, SnapRight, SnapCenter, SnapToAdapters, SnapToDies, SnapToPunches, SnapToMachine, SnapToBend };
			for (int i = 1; i < array.Length; i++)
			{
				if (array[0] != array[i])
				{
					return null;
				}
			}
			return array[0];
		}
		set
		{
			if (value.HasValue)
			{
				_raiseChanged = false;
				SnapLeft = value.Value;
				SnapRight = value.Value;
				SnapCenter = value.Value;
				SnapToAdapters = value.Value;
				SnapToPunches = value.Value;
				SnapToDies = value.Value;
				SnapToMachine = value.Value;
				SnapToBend = value.Value;
				_raiseChanged = true;
				NotifyPropertyChanged2("SnapToAll");
			}
		}
	}

	public bool BlockTools
	{
		get
		{
			return _snapOptions.BlockTools;
		}
		set
		{
			if (BlockTools != value)
			{
				_snapOptions.BlockTools = value;
				NotifyPropertyChanged2("BlockTools");
			}
		}
	}

	public bool BlockCollisions
	{
		get
		{
			return _snapOptions.BlockCollisions;
		}
		set
		{
			if (BlockCollisions != value)
			{
				_snapOptions.BlockCollisions = value;
				NotifyPropertyChanged2("BlockCollisions");
			}
		}
	}

	public bool MovePieceInSection
	{
		get
		{
			return _snapOptions.MovePieceInSection;
		}
		set
		{
			if (MovePieceInSection != value)
			{
				_snapOptions.MovePieceInSection = value;
				NotifyPropertyChanged2("MovePieceInSection");
			}
		}
	}

	public Action OnChanged { get; set; }

	public SnapPointViewModel(IConfigProvider configProvider)
	{
		_configProvider = configProvider;
	}

	public void Init(SnapOptions options)
	{
		_snapOptions = options;
		SnapPointConfig snapPointConfig = _configProvider.InjectOrCreate<SnapPointConfig>();
		_snapOptions.SnapLeft = snapPointConfig.SnapLeft;
		_snapOptions.SnapRight = snapPointConfig.SnapRight;
		_snapOptions.SnapCenter = snapPointConfig.SnapCenter;
		_snapOptions.SnapToAdapters = snapPointConfig.SnapToAdapters;
		_snapOptions.SnapToDies = snapPointConfig.SnapToDies;
		_snapOptions.SnapToPunches = snapPointConfig.SnapToPunches;
		_snapOptions.SnapToMachine = snapPointConfig.SnapToMachine;
		_snapOptions.SnapToBend = snapPointConfig.SnapToBend;
		_snapOptions.BlockTools = snapPointConfig.BlockTools;
		_snapOptions.BlockCollisions = snapPointConfig.BlockCollisions;
		_snapOptions.MovePieceInSection = snapPointConfig.MovePieceInSection;
		NotifyPropertyChanged("SnapLeft");
		NotifyPropertyChanged("SnapRight");
		NotifyPropertyChanged("SnapCenter");
		NotifyPropertyChanged("SnapToAdapters");
		NotifyPropertyChanged("SnapToDies");
		NotifyPropertyChanged("SnapToPunches");
		NotifyPropertyChanged("SnapToMachine");
		NotifyPropertyChanged("SnapToBend");
		NotifyPropertyChanged("SnapToTools");
		NotifyPropertyChanged("SnapToAll");
		NotifyPropertyChanged("BlockTools");
		NotifyPropertyChanged("BlockCollisions");
		NotifyPropertyChanged("MovePieceInSection");
	}

	private void SaveConfig()
	{
		SnapPointConfig snapPointConfig = _configProvider.InjectOrCreate<SnapPointConfig>();
		snapPointConfig.SnapLeft = _snapOptions.SnapLeft;
		snapPointConfig.SnapRight = _snapOptions.SnapRight;
		snapPointConfig.SnapCenter = _snapOptions.SnapCenter;
		snapPointConfig.SnapToAdapters = _snapOptions.SnapToAdapters;
		snapPointConfig.SnapToDies = _snapOptions.SnapToDies;
		snapPointConfig.SnapToPunches = _snapOptions.SnapToPunches;
		snapPointConfig.SnapToMachine = _snapOptions.SnapToMachine;
		snapPointConfig.SnapToBend = _snapOptions.SnapToBend;
		snapPointConfig.BlockTools = _snapOptions.BlockTools;
		snapPointConfig.BlockCollisions = _snapOptions.BlockCollisions;
		snapPointConfig.MovePieceInSection = _snapOptions.MovePieceInSection;
		_configProvider.Push(snapPointConfig);
		_configProvider.Save<SnapPointConfig>();
	}

	private void NotifyPropertyChanged2([CallerMemberName] string propertyName = "")
	{
		NotifyPropertyChanged(propertyName);
		if (_raiseChanged)
		{
			OnChanged?.Invoke();
			NotifyPropertyChanged("SnapToTools");
			NotifyPropertyChanged("SnapToAll");
			SaveConfig();
		}
	}
}
