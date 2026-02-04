using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.BendPP;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class PressDataViewModel : ViewModelBase
{
	private readonly ITranslator _translator;

	private ChangedConfigType _changed;

	private string _name;

	private string _manufacturer;

	private string _type;

	private double _pressForceMin;

	private double _pressForceMax;

	private bool _limitPressForceToMaxPressForceOfTools;

	private int _totalHeight;

	private int _totalLength;

	private int _length;

	private int _footToggleCount;

	private int _footToggleDefault;

	private int _stepChangeDefault;

	private int _minBeamOpeningHeight;

	private int _safetyOffsetBeamOpeningHeight;

	private double _defaultMachineSpeedChangePosition;

	private ClampPositionOptions _clampPositionOption;

	private double _defaultClampingPositionFactor;

	private double _absolutClampPosition;

	private bool _useAngleMeasurement;

	private bool _frontLiftingAid;

	private bool _backLiftingAid;

	private bool _antiDeflectionSystem;

	private BendTableOptions _useGlobalBendTable;

	private BendTableOptions _useGlobalBendSequenceList;

	private string _version;

	private ControlerType _controlerType;

	private VWidthTypes _defualtVWidthType;

	public Action<ChangedConfigType> DataChanged;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("Name");
		}
	}

	public string Manufacturer
	{
		get
		{
			return _manufacturer;
		}
		set
		{
			_manufacturer = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("Manufacturer");
		}
	}

	public string Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("Type");
		}
	}

	public double PressForceMin
	{
		get
		{
			return _pressForceMin;
		}
		set
		{
			_pressForceMin = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("PressForceMin");
		}
	}

	public double PressForceMax
	{
		get
		{
			return _pressForceMax;
		}
		set
		{
			_pressForceMax = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("PressForceMax");
		}
	}

	public bool LimitPressForceToMaxPressForceOfTools
	{
		get
		{
			return _limitPressForceToMaxPressForceOfTools;
		}
		set
		{
			_limitPressForceToMaxPressForceOfTools = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("LimitPressForceToMaxPressForceOfTools");
		}
	}

	public int TotalHeight
	{
		get
		{
			return _totalHeight;
		}
		set
		{
			_totalHeight = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("TotalHeight");
		}
	}

	public int TotalLength
	{
		get
		{
			return _totalLength;
		}
		set
		{
			_totalLength = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("TotalLength");
		}
	}

	public int Length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("Length");
		}
	}

	public int FootToggleCount
	{
		get
		{
			return _footToggleCount;
		}
		set
		{
			_footToggleCount = value;
			_changed = ChangedConfigType.MachineConfig;
			FootToggleDescriptions = new ObservableCollection<ComboboxEntry<int>>();
			for (int i = 0; i < value; i++)
			{
				FootToggleDescriptions.Add(new ComboboxEntry<int>(_translator.Translate("l_popup.MachineConfigView.FootToggleItem") + " " + (i + 1), i + 1));
			}
			if (_footToggleDefault > _footToggleCount)
			{
				_footToggleDefault = _footToggleCount;
			}
			NotifyPropertyChanged("FootToggleCount");
			NotifyPropertyChanged("FootToggleDescriptions");
			NotifyPropertyChanged("FootToggleDefault");
		}
	}

	public int FootToggleDefault
	{
		get
		{
			return _footToggleDefault;
		}
		set
		{
			_footToggleDefault = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("FootToggleDefault");
		}
	}

	public int StepChangeDefault
	{
		get
		{
			return _stepChangeDefault;
		}
		set
		{
			_stepChangeDefault = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("StepChangeDefault");
		}
	}

	public int MinBeamOpeningHeight
	{
		get
		{
			return _minBeamOpeningHeight;
		}
		set
		{
			_minBeamOpeningHeight = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("MinBeamOpeningHeight");
		}
	}

	public int SafetyOffsetBeamOpeningHeight
	{
		get
		{
			return _safetyOffsetBeamOpeningHeight;
		}
		set
		{
			_safetyOffsetBeamOpeningHeight = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("SafetyOffsetBeamOpeningHeight");
		}
	}

	public double DefaultMachineSpeedChangePosition
	{
		get
		{
			return _defaultMachineSpeedChangePosition;
		}
		set
		{
			_defaultMachineSpeedChangePosition = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("DefaultMachineSpeedChangePosition");
		}
	}

	public ClampPositionOptions ClampPositionOption
	{
		get
		{
			return _clampPositionOption;
		}
		set
		{
			_clampPositionOption = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("ClampPositionOption");
		}
	}

	public double DefaultClampingPositionFactor
	{
		get
		{
			return _defaultClampingPositionFactor;
		}
		set
		{
			_defaultClampingPositionFactor = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("DefaultClampingPositionFactor");
		}
	}

	public double AbsolutClampPosition
	{
		get
		{
			return _absolutClampPosition;
		}
		set
		{
			_absolutClampPosition = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("AbsolutClampPosition");
		}
	}

	public bool UseAngleMeasurement
	{
		get
		{
			return _useAngleMeasurement;
		}
		set
		{
			_useAngleMeasurement = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("UseAngleMeasurement");
		}
	}

	public bool FrontLiftingAid
	{
		get
		{
			return _frontLiftingAid;
		}
		set
		{
			_frontLiftingAid = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("FrontLiftingAid");
		}
	}

	public bool BackLiftingAid
	{
		get
		{
			return _backLiftingAid;
		}
		set
		{
			_backLiftingAid = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("BackLiftingAid");
		}
	}

	public bool AntiDeflectionSystem
	{
		get
		{
			return _antiDeflectionSystem;
		}
		set
		{
			_antiDeflectionSystem = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("AntiDeflectionSystem");
		}
	}

	public BendTableOptions UseGlobalBendTable
	{
		get
		{
			return _useGlobalBendTable;
		}
		set
		{
			_useGlobalBendTable = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("UseGlobalBendTable");
		}
	}

	public BendTableOptions UseGlobalBendSequenceList
	{
		get
		{
			return _useGlobalBendSequenceList;
		}
		set
		{
			_useGlobalBendSequenceList = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("UseGlobalBendSequenceList");
		}
	}

	public string Version
	{
		get
		{
			return _version;
		}
		set
		{
			_version = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("Version");
		}
	}

	public ControlerType ControlerType
	{
		get
		{
			return _controlerType;
		}
		set
		{
			_controlerType = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("ControlerType");
		}
	}

	public VWidthTypes DefaultVWidthType
	{
		get
		{
			return _defualtVWidthType;
		}
		set
		{
			_defualtVWidthType = value;
			_changed = ChangedConfigType.MachineConfig;
			NotifyPropertyChanged("DefaultVWidthType");
		}
	}

	public RadObservableCollection<ComboboxEntry<int>> StepChangeDescriptions { get; set; } = new RadObservableCollection<ComboboxEntry<int>>();

	public ObservableCollection<ComboboxEntry<int>> FootToggleDescriptions { get; set; }

	public ObservableCollection<ComboboxEntry<ControlerType>> ControlerTypesColl { get; set; }

	public ObservableCollection<ComboboxEntry<VWidthTypes>> VWidthTypesColl { get; set; }

	public PressDataViewModel(ITranslator translator)
	{
		_translator = translator;
		InitControlerTypes();
		InitVWidthTypes();
	}

	public void Init(IPressBrakeData data, IPressBrakeInfo info, IPostProcessor? bendMachinePostProcessor)
	{
		InitStepChangeModes(bendMachinePostProcessor);
		Name = data.Name;
		Manufacturer = data.Manufacturer;
		Type = data.Type;
		PressForceMin = data.PressForceMin;
		PressForceMax = data.PressForceMax;
		LimitPressForceToMaxPressForceOfTools = data.LimitPressForceToMaxPressForceOfTools;
		TotalHeight = data.TotalHeight;
		TotalLength = data.TotalLength;
		Length = data.Length;
		FootToggleCount = data.FootToggleCount;
		FootToggleDefault = data.FootToggleDefault;
		StepChangeDefault = data.StepChangeDefault;
		MinBeamOpeningHeight = data.MinBeamOpeningHeight;
		SafetyOffsetBeamOpeningHeight = data.SafetyOffsetBeamOpeningHeight;
		DefaultMachineSpeedChangePosition = data.DefaultMachineSpeedChangePosition;
		ClampPositionOption = info.ClampPositionOption;
		DefaultClampingPositionFactor = data.DefaultClampingPositionFactor;
		AbsolutClampPosition = data.AbsolutClampPosition;
		UseAngleMeasurement = Convert.ToBoolean(data.MeasurementAngle);
		BackLiftingAid = Convert.ToBoolean(data.BackLiftingAid);
		FrontLiftingAid = Convert.ToBoolean(data.FrontLiftingAid);
		AntiDeflectionSystem = Convert.ToBoolean(data.AntiDeflectionsystem);
		UseGlobalBendTable = info.UseGlobalBendTable;
		UseGlobalBendSequenceList = info.UseGlobalBendSequenceList;
		ControlerType = info.Type;
		Version = info.Version;
		DefaultVWidthType = info.DefaultVWidthType;
		_changed = ChangedConfigType.NoChanges;
	}

	public void Save(IPressBrakeData saveTargetData, IPressBrakeInfo saveTargetInfo)
	{
		saveTargetData.Name = _name;
		saveTargetData.Manufacturer = _manufacturer;
		saveTargetData.Type = _type;
		saveTargetData.PressForceMin = _pressForceMin;
		saveTargetData.PressForceMax = _pressForceMax;
		saveTargetData.LimitPressForceToMaxPressForceOfTools = _limitPressForceToMaxPressForceOfTools;
		saveTargetData.TotalHeight = _totalHeight;
		saveTargetData.TotalLength = _totalLength;
		saveTargetData.Length = _length;
		saveTargetData.FootToggleCount = _footToggleCount;
		saveTargetData.FootToggleDefault = _footToggleDefault;
		saveTargetData.StepChangeDefault = _stepChangeDefault;
		saveTargetData.MinBeamOpeningHeight = _minBeamOpeningHeight;
		saveTargetData.SafetyOffsetBeamOpeningHeight = _safetyOffsetBeamOpeningHeight;
		saveTargetData.DefaultMachineSpeedChangePosition = _defaultMachineSpeedChangePosition;
		saveTargetInfo.ClampPositionOption = _clampPositionOption;
		saveTargetData.DefaultClampingPositionFactor = _defaultClampingPositionFactor;
		saveTargetData.AbsolutClampPosition = _absolutClampPosition;
		saveTargetData.MeasurementAngle = Convert.ToInt32(_useAngleMeasurement);
		saveTargetData.BackLiftingAid = Convert.ToInt32(_backLiftingAid);
		saveTargetData.FrontLiftingAid = Convert.ToInt32(_frontLiftingAid);
		saveTargetData.AntiDeflectionsystem = Convert.ToInt32(_antiDeflectionSystem);
		saveTargetInfo.UseGlobalBendTable = _useGlobalBendTable;
		saveTargetInfo.UseGlobalBendSequenceList = _useGlobalBendSequenceList;
		saveTargetInfo.Type = _controlerType;
		saveTargetInfo.Version = _version;
		saveTargetInfo.DefaultVWidthType = _defualtVWidthType;
		DataChanged?.Invoke(_changed);
	}

	private void InitControlerTypes()
	{
		ControlerTypesColl = new ObservableCollection<ComboboxEntry<ControlerType>>();
		foreach (int value in System.Enum.GetValues(typeof(ControlerType)))
		{
			ObservableCollection<ComboboxEntry<ControlerType>> controlerTypesColl = ControlerTypesColl;
			ControlerType controlerType = (ControlerType)value;
			controlerTypesColl.Add(new ComboboxEntry<ControlerType>(controlerType.ToString(), (ControlerType)value));
		}
	}

	private void InitVWidthTypes()
	{
		VWidthTypesColl = new ObservableCollection<ComboboxEntry<VWidthTypes>>();
		foreach (int value in System.Enum.GetValues(typeof(VWidthTypes)))
		{
			ObservableCollection<ComboboxEntry<VWidthTypes>> vWidthTypesColl = VWidthTypesColl;
			VWidthTypes vWidthTypes = (VWidthTypes)value;
			vWidthTypesColl.Add(new ComboboxEntry<VWidthTypes>(vWidthTypes.ToString(), (VWidthTypes)value));
		}
	}

	private void InitStepChangeModes(IPostProcessor? pp)
	{
		StepChangeDescriptions.SuspendNotifications();
		StepChangeDescriptions.Clear();
		List<int> list = (pp?.GetAllStepChangeModes()).EmptyIfNull().ToList();
		StepChangeDescriptions.Add(new ComboboxEntry<int>("-", int.MinValue));
		if (list.Count > 0)
		{
			foreach (int item in list)
			{
				string desc = _translator.Translate($"l_enum.PpSpecific.{pp.TranslationBaseKey}.StepChange.{item}");
				StepChangeDescriptions.Add(new ComboboxEntry<int>(desc, item));
			}
		}
		StepChangeDescriptions.ResumeNotifications();
	}

	public void Dispose()
	{
	}
}
