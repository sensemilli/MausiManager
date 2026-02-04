using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel.Config;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Services.ConfigProviders.Contracts;
using WiCAM.Services.ConfigProviders.Contracts.DataType;

namespace WiCAM.Pn4000.GuiWpf.GeneralSubWindow.Settings;

internal class MacroConfigViewModel : ViewModelBase, IMacroConfigViewModel
{
	internal class DeepeningLevel : ViewModelBase
	{
		private int _deepeningPosColorPn;

		private int _deepeningNegColorPn;

		private double _deepeningDepth;

		public int Level { get; set; }

		public PopupUnfoldSettingViewModel.ColorSetting Color3D { get; set; } = new PopupUnfoldSettingViewModel.ColorSetting();

		public int DeepeningPosColorPn
		{
			get
			{
				return _deepeningPosColorPn;
			}
			set
			{
				if (_deepeningPosColorPn != value)
				{
					_deepeningPosColorPn = value;
					NotifyPropertyChanged("DeepeningPosColorPn");
				}
			}
		}

		public int DeepeningNegColorPn
		{
			get
			{
				return _deepeningNegColorPn;
			}
			set
			{
				if (_deepeningNegColorPn != value)
				{
					_deepeningNegColorPn = value;
					NotifyPropertyChanged("DeepeningNegColorPn");
				}
			}
		}

		public double DeepeningDepth
		{
			get
			{
				return _deepeningDepth;
			}
			set
			{
				if (_deepeningDepth != value)
				{
					_deepeningDepth = value;
					NotifyPropertyChanged("DeepeningDepthConverted");
				}
			}
		}

		public double DeepeningDepthConverted
		{
			get
			{
				return _unitConverter.Length.ToUi(DeepeningDepth, 3);
			}
			set
			{
				DeepeningDepth = _unitConverter.Length.FromUi(value, 3);
			}
		}

		public ICommand DeepeningLevelPosColorPnCommand { get; set; }

		public ICommand DeepeningLevelNegColorPnCommand { get; set; }

		public ICommand DeepeningLevelDeleteRowCommand { get; set; }

		public DeepeningLevel(CfgColor color3D)
		{
			Color3D.Color = color3D;
			if (Color3D.Color != null)
			{
				Color3D.ColorChangeCommand = new RelayCommand((Action<object>)delegate
				{
					CfgColor color = Color3D.Color.Clone();
					if (_popupUnfoldSettingViewModel.subPopup.SelectCfgColor(color))
					{
						Color3D.Color = color;
					}
				});
			}
			else
			{
				Color3D.Color = _popupUnfoldSettingViewModel.modelColors3DConfig.MacroDeepeningFaceColor;
			}
			DeepeningLevelPosColorPnCommand = new RelayCommand<object>(DeepeningLevelPosColorPnClick, CanAlwaysBeClick);
			DeepeningLevelNegColorPnCommand = new RelayCommand<object>(DeepeningLevelNegColorPnClick, CanAlwaysBeClick);
			DeepeningLevelDeleteRowCommand = new RelayCommand<object>(DeepeningLevelDeleteRowPnClick, CanAlwaysBeClick);
		}

		public DeepeningLevel()
		{
			Level = GetNextLevel();
			Color3D.Color = _popupUnfoldSettingViewModel.modelColors3DConfig.MacroDeepeningFaceColor;
			Color3D.ColorChangeCommand = new RelayCommand((Action<object>)delegate
			{
				CfgColor color = Color3D.Color.Clone();
				if (_popupUnfoldSettingViewModel.subPopup.SelectCfgColor(color))
				{
					Color3D.Color = color;
				}
			});
			DeepeningPosColorPn = _popupUnfoldSettingViewModel.DeepeningPosColorPn;
			DeepeningNegColorPn = _popupUnfoldSettingViewModel.DeepeningNegColorPn;
			DeepeningLevelPosColorPnCommand = new RelayCommand<object>(DeepeningLevelPosColorPnClick, CanAlwaysBeClick);
			DeepeningLevelNegColorPnCommand = new RelayCommand<object>(DeepeningLevelNegColorPnClick, CanAlwaysBeClick);
		}

		private void DeepeningLevelPosColorPnClick(object obj)
		{
			DeepeningPosColorPn = _popupUnfoldSettingViewModel.subPopup.SelectColorPnIntEdition(DeepeningPosColorPn);
		}

		private void DeepeningLevelNegColorPnClick(object obj)
		{
			DeepeningNegColorPn = _popupUnfoldSettingViewModel.subPopup.SelectColorPnIntEdition(DeepeningNegColorPn);
		}

		private void DeepeningLevelDeleteRowPnClick(object obj)
		{
			DeepeningLevels.Remove(DeepeningLevels.FirstOrDefault((DeepeningLevel level) => level.Level == Level));
		}

		private bool CanAlwaysBeClick(object obj)
		{
			return true;
		}

		private int GetNextLevel()
		{
			return DeepeningLevels.Select((DeepeningLevel level) => level.Level).DefaultIfEmpty(0).Max() + 1;
		}
	}

	private readonly IConfigProvider _configProvider;

	private static IUnitConverter _unitConverter;

	private readonly IAnalyzeConfigProvider _analyzeConfigProvider;

	private readonly ITranslator _translator;

	private WiCAM.Pn4000.BendModel.Config.AnalyzeConfig _defaultConfig;

	private Macro3DConfig _macro3DConfig;

	private static PopupUnfoldSettingViewModel _popupUnfoldSettingViewModel;

	private bool? _showChamferLine;

	private double? _counterSinkMaxRadius;

	private double? _twoSidedCounterSinkMaxRadius;

	private bool? _exportCounterSinkAngle;

	private bool? _detectLances;

	private double? _blindHoleMaxRadius;

	private double? _borderTolerableOrthogonalOffset;

	private double? _deepeningMinDepth;

	private double? _deepeningMaxDepth;

	private double? _blindHoleMinDepth;

	private bool? _showDeepeningLevels;

	public ICommand CmdResetShowChamferLine { get; }

	public ICommand CmdResetCounterSinkMaxRadius { get; }

	public ICommand CmdResetTwoSidedCounterSinkMaxRadius { get; }

	public ICommand CmdResetExportCounterSinkAngle { get; }

	public ICommand CmdResetDetectLances { get; set; }

	public ICommand CmdResetDeepeningMinDepth { get; }

	public ICommand CmdResetDeepeningMaxDepth { get; }

	public ICommand CmdResetBlindHoleMinDepth { get; }

	public ICommand CmdResetBlindHoleMaxRadius { get; }

	public ICommand CmdResetBorderTolerableOrthogonalOffset { get; }

	public ICommand ChamferPosColorPnCommand { get; set; }

	public ICommand ChamferNegColorPnCommand { get; set; }

	public string Unit => _unitConverter.Length.Unit;

	public string AngleUnit => _unitConverter.Angle.Unit;

	public string MaxDepthHeader => _translator.Translate("l_popup.PopupUnfoldSetting.MaxDepth") + " (" + Unit + ")";

	public int ChamferPosColorPn
	{
		get
		{
			return _macro3DConfig.ChamferPosColorPn;
		}
		set
		{
			_macro3DConfig.ChamferPosColorPn = value;
			NotifyPropertyChanged("ChamferPosColorPn");
		}
	}

	public int ChamferNegColorPn
	{
		get
		{
			return _macro3DConfig.ChamferNegColorPn;
		}
		set
		{
			_macro3DConfig.ChamferNegColorPn = value;
			NotifyPropertyChanged("ChamferNegColorPn");
		}
	}

	public bool? ShowChamferLine
	{
		get
		{
			return _showChamferLine;
		}
		set
		{
			if (_showChamferLine != value)
			{
				_showChamferLine = value;
				NotifyPropertyChanged("ShowChamferLine");
				NotifyPropertyChanged("ShowChamferLineValue");
			}
		}
	}

	public bool ShowChamferLineValue
	{
		get
		{
			return ShowChamferLine ?? _defaultConfig.ShowChamferLine;
		}
		set
		{
			ShowChamferLine = value;
		}
	}

	public double? CounterSinkMaxRadius
	{
		get
		{
			return _counterSinkMaxRadius;
		}
		set
		{
			if (_counterSinkMaxRadius != value)
			{
				_counterSinkMaxRadius = value;
				NotifyPropertyChanged("CounterSinkMaxRadius");
				NotifyPropertyChanged("CounterSinkMaxRadiusConverted");
			}
		}
	}

	public double CounterSinkMaxRadiusConverted
	{
		get
		{
			return _unitConverter.Length.ToUi(CounterSinkMaxRadius ?? _defaultConfig.CounterSinkMaxRadius, 2);
		}
		set
		{
			CounterSinkMaxRadius = _unitConverter.Length.FromUi(value, 2);
		}
	}

	public double? TwoSidedCounterSinkMaxRadius
	{
		get
		{
			return _twoSidedCounterSinkMaxRadius;
		}
		set
		{
			if (_twoSidedCounterSinkMaxRadius != value)
			{
				_twoSidedCounterSinkMaxRadius = value;
				NotifyPropertyChanged("TwoSidedCounterSinkMaxRadius");
				NotifyPropertyChanged("TwoSidedCounterSinkMaxRadiusConverted");
			}
		}
	}

	public double TwoSidedCounterSinkMaxRadiusConverted
	{
		get
		{
			return _unitConverter.Length.ToUi(TwoSidedCounterSinkMaxRadius ?? _defaultConfig.TwoSidedCounterSinkMaxRadius, 2);
		}
		set
		{
			TwoSidedCounterSinkMaxRadius = _unitConverter.Length.FromUi(value, 2);
		}
	}

	public bool? ExportCounterSinkAngle
	{
		get
		{
			return _exportCounterSinkAngle;
		}
		set
		{
			if (_exportCounterSinkAngle != value)
			{
				_exportCounterSinkAngle = value;
				NotifyPropertyChanged("ExportCounterSinkAngle");
				NotifyPropertyChanged("ExportCounterSinkAngleValue");
			}
		}
	}

	public bool ExportCounterSinkAngleValue
	{
		get
		{
			return ExportCounterSinkAngle ?? _defaultConfig.ExportCounterSinkAngle;
		}
		set
		{
			ExportCounterSinkAngle = value;
		}
	}

	public bool? DetectLances
	{
		get
		{
			return _detectLances;
		}
		set
		{
			if (_detectLances != value)
			{
				_detectLances = value;
				NotifyPropertyChanged("DetectLances");
				NotifyPropertyChanged("DetectLancesValue");
			}
		}
	}

	public bool DetectLancesValue
	{
		get
		{
			return DetectLances ?? _defaultConfig.DetectLances;
		}
		set
		{
			DetectLances = value;
		}
	}

	public double? BlindHoleMaxRadius
	{
		get
		{
			return _blindHoleMaxRadius;
		}
		set
		{
			if (_blindHoleMaxRadius != value)
			{
				_blindHoleMaxRadius = value;
				NotifyPropertyChanged("BlindHoleMaxRadius");
				NotifyPropertyChanged("BlindHoleMaxRadiusConverted");
			}
		}
	}

	public double BlindHoleMaxRadiusConverted
	{
		get
		{
			return _unitConverter.Length.ToUi(BlindHoleMaxRadius ?? _defaultConfig.BlindHoleMaxRadius, 2);
		}
		set
		{
			BlindHoleMaxRadius = _unitConverter.Length.FromUi(value, 2);
		}
	}

	public double? BorderTolerableOrthogonalOffset
	{
		get
		{
			return _borderTolerableOrthogonalOffset;
		}
		set
		{
			if (_borderTolerableOrthogonalOffset != value)
			{
				_borderTolerableOrthogonalOffset = value;
				NotifyPropertyChanged("BorderTolerableOrthogonalOffset");
				NotifyPropertyChanged("BorderTolerableOrthogonalOffsetConverted");
			}
		}
	}

	public double BorderTolerableOrthogonalOffsetConverted
	{
		get
		{
			return _unitConverter.Angle.ToUi(BorderTolerableOrthogonalOffset ?? _defaultConfig.BorderTolerableOrthogonalOffset, 4);
		}
		set
		{
			BorderTolerableOrthogonalOffset = _unitConverter.Angle.FromUi(value, 4);
		}
	}

	public double? DeepeningMinDepth
	{
		get
		{
			return _deepeningMinDepth;
		}
		set
		{
			if (_deepeningMinDepth != value)
			{
				_deepeningMinDepth = value;
				NotifyPropertyChanged("DeepeningMinDepth");
				NotifyPropertyChanged("DeepeningMinDepthConverted");
			}
		}
	}

	public double DeepeningMinDepthConverted
	{
		get
		{
			return _unitConverter.Length.ToUi(DeepeningMinDepth ?? _defaultConfig.DeepeningMinDepth, 3) * -1.0;
		}
		set
		{
			DeepeningMinDepth = _unitConverter.Length.FromUi(value, 3) * -1.0;
		}
	}

	public double? DeepeningMaxDepth
	{
		get
		{
			return _deepeningMaxDepth;
		}
		set
		{
			if (_deepeningMaxDepth != value)
			{
				_deepeningMaxDepth = value;
				NotifyPropertyChanged("DeepeningMaxDepth");
				NotifyPropertyChanged("DeepeningMaxDepthConverted");
			}
		}
	}

	public double DeepeningMaxDepthConverted
	{
		get
		{
			return _unitConverter.Length.ToUi(DeepeningMaxDepth ?? _defaultConfig.DeepeningMaxDepth, 3) * -1.0;
		}
		set
		{
			DeepeningMaxDepth = _unitConverter.Length.FromUi(value, 3) * -1.0;
		}
	}

	public double? BlindHoleMinDepth
	{
		get
		{
			return _blindHoleMinDepth;
		}
		set
		{
			if (_blindHoleMinDepth != value)
			{
				_blindHoleMinDepth = value;
				NotifyPropertyChanged("BlindHoleMinDepth");
				NotifyPropertyChanged("BlindHoleMinDepthConverted");
			}
		}
	}

	public double BlindHoleMinDepthConverted
	{
		get
		{
			return _unitConverter.Length.ToUi(BlindHoleMinDepth ?? _defaultConfig.BlindHoleMinDepth, 2);
		}
		set
		{
			BlindHoleMinDepth = _unitConverter.Length.FromUi(value, 2);
		}
	}

	public bool? ShowDeepeningLevels
	{
		get
		{
			return _showDeepeningLevels;
		}
		set
		{
			if (_showDeepeningLevels != value)
			{
				_showDeepeningLevels = value;
				NotifyPropertyChanged("ShowDeepeningLevels");
				NotifyPropertyChanged("ShowDeepeningLevelsValue");
			}
		}
	}

	public bool ShowDeepeningLevelsValue
	{
		get
		{
			return ShowDeepeningLevels ?? _defaultConfig.ShowDeepeningLevels;
		}
		set
		{
			ShowDeepeningLevels = value;
		}
	}

	public static ObservableCollection<DeepeningLevel> DeepeningLevels { get; set; } = new ObservableCollection<DeepeningLevel>();

	public MacroConfigViewModel(IConfigProvider configProvider, IUnitConverter unitConverter, IAnalyzeConfigProvider analyzeConfigProvider, ITranslator translator)
	{
		_configProvider = configProvider;
		_unitConverter = unitConverter;
		_analyzeConfigProvider = analyzeConfigProvider;
		_translator = translator;
		CmdResetShowChamferLine = new RelayCommand(ResetShowChamferLine, CanResetShowChamferLine);
		CmdResetCounterSinkMaxRadius = new RelayCommand(ResetCounterSinkMaxRadius, CanResetCounterSinkMaxRadius);
		CmdResetTwoSidedCounterSinkMaxRadius = new RelayCommand(ResetTwoSidedCounterSinkMaxRadius, CanResetTwoSidedCounterSinkMaxRadius);
		CmdResetExportCounterSinkAngle = new RelayCommand(ResetExportCounterSinkAngle, CanResetExportCounterSinkAngle);
		CmdResetDetectLances = new RelayCommand(ResetDetectLances, CanResetDetectLances);
		CmdResetDeepeningMinDepth = new RelayCommand(ResetDeepeningMinDepth, CanResetDeepeningMinDepth);
		CmdResetDeepeningMaxDepth = new RelayCommand(ResetDeepeningMaxDepth, CanResetDeepeningMaxDepth);
		CmdResetBlindHoleMinDepth = new RelayCommand(ResetBlindHoleMinDepth, CanResetBlindHoleMinDepth);
		CmdResetBlindHoleMaxRadius = new RelayCommand(ResetBlindHoleMaxRadius, CanResetBlindHoleMaxRadius);
		CmdResetBorderTolerableOrthogonalOffset = new RelayCommand(ResetBorderTolerableOrthogonalOffset, CanResetBorderTolerableOrthogonalOffset);
		ChamferPosColorPnCommand = new RelayCommand<object>(ChamferPosColorPnClick, CanAlwaysBeClick);
		ChamferNegColorPnCommand = new RelayCommand<object>(ChamferNegColorPnClick, CanAlwaysBeClick);
	}

	public void Save(PopupUnfoldSettingModel popupUnfoldSettingModel)
	{
		WiCAM.Pn4000.Config.DataStructures.AnalyzeConfig analyzeConfig = popupUnfoldSettingModel.AnalyzeConfig;
		analyzeConfig.ShowChamferLine = ShowChamferLine;
		analyzeConfig.CounterSinkMaxRadius = CounterSinkMaxRadius;
		analyzeConfig.TwoSidedCounterSinkMaxRadius = TwoSidedCounterSinkMaxRadius;
		analyzeConfig.ExportCounterSinkAngle = ExportCounterSinkAngle;
		analyzeConfig.DetectLances = DetectLances;
		analyzeConfig.BorderTolerableOrthogonalOffset = BorderTolerableOrthogonalOffset;
		analyzeConfig.DeepeningMinDepth = DeepeningMinDepth;
		analyzeConfig.DeepeningMaxDepth = DeepeningMaxDepth;
		analyzeConfig.BlindHoleMinDepth = BlindHoleMinDepth;
		analyzeConfig.BlindHoleMaxRadius = BlindHoleMaxRadius;
		analyzeConfig.ShowDeepeningLevels = ShowDeepeningLevels;
		analyzeConfig.DeepeningListColor3D = new List<CfgColor>();
		analyzeConfig.DeepeningListColorsPos = new List<int>();
		analyzeConfig.DeepeningListColorsNeg = new List<int>();
		analyzeConfig.DeepeningListDepth = new List<double>();
		if (DeepeningLevels.Count <= 0)
		{
			return;
		}
		foreach (DeepeningLevel item in DeepeningLevels.OrderBy((DeepeningLevel x) => x.DeepeningDepth))
		{
			analyzeConfig.DeepeningListColor3D.Add(item.Color3D.Color);
			analyzeConfig.DeepeningListColorsPos.Add(item.DeepeningPosColorPn);
			analyzeConfig.DeepeningListColorsNeg.Add(item.DeepeningNegColorPn);
			analyzeConfig.DeepeningListDepth.Add(item.DeepeningDepth);
		}
	}

	public void Load(PopupUnfoldSettingViewModel popupUnfoldSettingViewModel)
	{
		WiCAM.Pn4000.Config.DataStructures.AnalyzeConfig config = _configProvider.InjectOrCreate<WiCAM.Pn4000.Config.DataStructures.AnalyzeConfig>();
		ShowChamferLine = config.ShowChamferLine;
		CounterSinkMaxRadius = config.CounterSinkMaxRadius;
		TwoSidedCounterSinkMaxRadius = config.TwoSidedCounterSinkMaxRadius;
		ExportCounterSinkAngle = config.ExportCounterSinkAngle;
		DetectLances = config.DetectLances;
		BlindHoleMaxRadius = config.BlindHoleMaxRadius;
		BorderTolerableOrthogonalOffset = config.BorderTolerableOrthogonalOffset;
		DeepeningMinDepth = config.DeepeningMinDepth;
		DeepeningMaxDepth = config.DeepeningMaxDepth;
		BlindHoleMinDepth = config.BlindHoleMinDepth;
		ShowDeepeningLevels = config.ShowDeepeningLevels;
		_defaultConfig = _analyzeConfigProvider.ConvertAnalyzeConfig(new WiCAM.Pn4000.Config.DataStructures.AnalyzeConfig());
		_macro3DConfig = popupUnfoldSettingViewModel.macro3DConfig;
		_popupUnfoldSettingViewModel = popupUnfoldSettingViewModel;
		int defaultPosColor = _popupUnfoldSettingViewModel.DeepeningPosColorPn;
		int defaultNegColor = _popupUnfoldSettingViewModel.DeepeningNegColorPn;
		double defaultDepth = 0.0;
		List<DeepeningLevel> list = (from dD in config.DeepeningListColor3D?.Select((CfgColor color3D, int i) => new DeepeningLevel(color3D)
			{
				DeepeningPosColorPn = ((i < config.DeepeningListColorsPos?.Count) ? config.DeepeningListColorsPos[i] : defaultPosColor),
				DeepeningNegColorPn = ((i < config.DeepeningListColorsNeg?.Count) ? config.DeepeningListColorsNeg[i] : defaultNegColor),
				DeepeningDepth = ((i < config.DeepeningListDepth?.Count) ? config.DeepeningListDepth[i] : defaultDepth)
			})
			orderby dD.DeepeningDepth
			select dD).ToList() ?? new List<DeepeningLevel>();
		for (int j = 0; j < list.Count; j++)
		{
			list[j].Level = j;
		}
		DeepeningLevels = new ObservableCollection<DeepeningLevel>(list);
	}

	private void ResetShowChamferLine()
	{
		ShowChamferLine = null;
	}

	private bool CanResetShowChamferLine()
	{
		return ShowChamferLine.HasValue;
	}

	private void ResetCounterSinkMaxRadius()
	{
		CounterSinkMaxRadius = null;
	}

	private bool CanResetCounterSinkMaxRadius()
	{
		return CounterSinkMaxRadius.HasValue;
	}

	private void ResetTwoSidedCounterSinkMaxRadius()
	{
		TwoSidedCounterSinkMaxRadius = null;
	}

	private bool CanResetTwoSidedCounterSinkMaxRadius()
	{
		return TwoSidedCounterSinkMaxRadius.HasValue;
	}

	private void ResetExportCounterSinkAngle()
	{
		ExportCounterSinkAngle = null;
	}

	private bool CanResetExportCounterSinkAngle()
	{
		return ExportCounterSinkAngle.HasValue;
	}

	private void ResetDetectLances()
	{
		DetectLances = null;
	}

	private bool CanResetDetectLances()
	{
		return DetectLances.HasValue;
	}

	private void ResetDeepeningMinDepth()
	{
		DeepeningMinDepth = null;
	}

	private bool CanResetDeepeningMinDepth()
	{
		return DeepeningMinDepth.HasValue;
	}

	private void ResetDeepeningMaxDepth()
	{
		DeepeningMaxDepth = null;
	}

	private bool CanResetDeepeningMaxDepth()
	{
		return DeepeningMaxDepth.HasValue;
	}

	private void ResetBlindHoleMinDepth()
	{
		BlindHoleMinDepth = null;
	}

	private bool CanResetBlindHoleMinDepth()
	{
		return BlindHoleMinDepth.HasValue;
	}

	private void ResetBlindHoleMaxRadius()
	{
		BlindHoleMaxRadius = null;
	}

	private bool CanResetBlindHoleMaxRadius()
	{
		return BlindHoleMaxRadius.HasValue;
	}

	private void ResetBorderTolerableOrthogonalOffset()
	{
		BorderTolerableOrthogonalOffset = null;
	}

	private bool CanResetBorderTolerableOrthogonalOffset()
	{
		return BorderTolerableOrthogonalOffset.HasValue;
	}

	private void ChamferPosColorPnClick(object obj)
	{
		ChamferPosColorPn = _popupUnfoldSettingViewModel.subPopup.SelectColorPnIntEdition(ChamferPosColorPn);
	}

	private void ChamferNegColorPnClick(object obj)
	{
		ChamferNegColorPn = _popupUnfoldSettingViewModel.subPopup.SelectColorPnIntEdition(ChamferNegColorPn);
	}

	private bool CanAlwaysBeClick(object obj)
	{
		return true;
	}
}
