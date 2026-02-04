using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Tool.Interfaces;
using WiCAM.Pn4000.pn4.pn4Services;

namespace WiCAM.Pn4000.GuiWpf.TabBend.SequenceList;

public class BendSequenceListItemViewModel : ViewModelBase
{
	private readonly ITranslator _translator;

	private readonly IToolExpert _toolExpert;

	private readonly ILanguageDictionary _languageDictionary;

	public int? OrderOld;

	private static Brush _brushFingerStable = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, 0, byte.MaxValue, 0));

	private static Brush _brushFingerUnstable = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 0));

	private static Brush _brushFingerSemiStable = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0));

	private static Brush _brushFingerUser = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, 0, 0, byte.MaxValue));

	private static Brush _brushFingerNone = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, 128, 128, 128));

	private Brush _color = Brushes.DarkGray;

	private bool _isHovered;

	private string _extendBendMultiLabel;

	private string _mainBendLabel;

	private Brush _fingerStabilityColor;

	private string _fingerStabilityTooltip;

	private int? _selectedBendNumber;

	public ICombinedBendDescriptorInternal CommonBendFace { get; }

	public Brush Color
	{
		get
		{
			return _color;
		}
		set
		{
			if (_color != value)
			{
				_color = value;
				NotifyPropertyChanged("Color");
			}
		}
	}

	public bool IsHovered
	{
		get
		{
			return _isHovered;
		}
		set
		{
			if (_isHovered != value)
			{
				_isHovered = value;
				NotifyPropertyChanged("IsHovered");
			}
		}
	}

	public string ExtendBendMultiLabel
	{
		get
		{
			return _extendBendMultiLabel;
		}
		set
		{
			if (_extendBendMultiLabel != value)
			{
				_extendBendMultiLabel = value;
				NotifyPropertyChanged("ExtendBendMultiLabel");
			}
		}
	}

	public string MainBendLabel
	{
		get
		{
			return _mainBendLabel;
		}
		set
		{
			if (_mainBendLabel != value)
			{
				_mainBendLabel = value;
				NotifyPropertyChanged("MainBendLabel");
			}
		}
	}

	public string OrderUi { get; set; }

	public Brush FingerStabilityColor
	{
		get
		{
			return _fingerStabilityColor;
		}
		set
		{
			_fingerStabilityColor = value;
			NotifyPropertyChanged("FingerStabilityColor");
		}
	}

	public string FingerStabilityTooltip
	{
		get
		{
			return _fingerStabilityTooltip;
		}
		set
		{
			_fingerStabilityTooltip = value;
			NotifyPropertyChanged("FingerStabilityTooltip");
		}
	}

	public int? SelectedBendNumber
	{
		get
		{
			return _selectedBendNumber;
		}
		set
		{
			if (_selectedBendNumber != value)
			{
				_selectedBendNumber = value;
				GenerateLabelInformation();
			}
		}
	}

	public BendSequenceListItemViewModel(ICombinedBendDescriptorInternal cbf, IToolExpert toolExpert, ILanguageDictionary languageDictionary, int? oldOrder, ITranslator translator, int? selectedBendNumber)
	{
		_selectedBendNumber = selectedBendNumber;
		_translator = translator;
		_toolExpert = toolExpert;
		_languageDictionary = languageDictionary;
		CommonBendFace = cbf;
		OrderOld = oldOrder;
		GenerateLabelInformation();
		UpdateFingerStatus();
	}

	public void GenerateLabelInformation()
	{
		bool flag = _languageDictionary.GetInchMode() == 1;
		ICombinedBendDescriptorInternal commonBendFace = CommonBendFace;
		if (!flag)
		{
			_ = commonBendFace[0].BendParams.FinalRadius;
		}
		else
		{
			WiCAM.Pn4000.PN3D.Converter.Convert.MmToInch(commonBendFace[0].BendParams.FinalRadius);
		}
		if (!flag)
		{
			_ = commonBendFace[0].BendParams.OriginalRadius;
		}
		else
		{
			WiCAM.Pn4000.PN3D.Converter.Convert.MmToInch(commonBendFace[0].BendParams.OriginalRadius);
		}
		double value = (flag ? WiCAM.Pn4000.PN3D.Converter.Convert.MmToInch(commonBendFace.TotalLength) : commonBendFace.TotalLength);
		string text = _translator.Translate("Enums.CombinedBendType." + commonBendFace.BendType);
		_translator.Translate("l_popup.PopupBendSequence.ToolStation");
		string orderUi = ((SelectedBendNumber.HasValue && commonBendFace.Order > SelectedBendNumber.Value) ? "-" : (commonBendFace.Order + 1).ToString());
		string text2 = $"{commonBendFace.StopProductAngleAbs * 180.0 / Math.PI:0.##}° ";
		if (commonBendFace.StartProductAngleAbs > 1E-06)
		{
			text2 = $"{commonBendFace.StartProductAngleAbs * 180.0 / Math.PI:0.##}° -> " + text2;
		}
		MainBendLabel = text + " " + ((commonBendFace.SplitBendCount > 0) ? $"[{commonBendFace.SplitBendOrder + 1}/{commonBendFace.SplitBendCount + 1}] " : "") + text2 + $"L = {value:0.##} {(flag ? "inch" : "mm")}";
		OrderUi = orderUi;
		NotifyPropertyChanged("OrderUi");
		StringBuilder stringBuilder = new StringBuilder();
		switch (CommonBendFace.BendType)
		{
		case CombinedBendType.Bend:
		{
			stringBuilder.AppendLine(flag ? $"BA = {WiCAM.Pn4000.PN3D.Converter.Convert.MmToInch(CommonBendFace[0].BendParams.BendingAllowance):0.####}" : $"BA = {CommonBendFace[0].BendParams.BendingAllowance:0.####}");
			IEnumerator<IBendDescriptor> enumerator = CommonBendFace.BendOrderUnfoldModel.GetEnumerator();
			enumerator.MoveNext();
			IBendDescriptor bendDescriptor = enumerator.Current;
			stringBuilder.AppendLine(FormatBendForCommonList(bendDescriptor, flag));
			while (enumerator.MoveNext())
			{
				IBendDescriptor current = enumerator.Current;
				stringBuilder.AppendLine(FormatGapForCommonList(bendDescriptor, current, flag));
				stringBuilder.AppendLine(FormatBendForCommonList(current, flag));
				bendDescriptor = current;
			}
			ExtendBendMultiLabel = stringBuilder.ToString();
			break;
		}
		case CombinedBendType.HemBend:
			ExtendBendMultiLabel = "warning: not supported!";
			break;
		}
	}

	public void UpdateFingerStatus()
	{
		ICombinedBendDescriptorInternal commonBendFace = CommonBendFace;
		switch (commonBendFace.FingerPositioningMode)
		{
		case FingerPositioningMode.Auto:
			switch (commonBendFace.FingerStability)
			{
			case FingerStability.SemiStable:
				FingerStabilityColor = _brushFingerSemiStable;
				FingerStabilityTooltip = _translator.Translate("l_popup.PopupBendSequence.FingerSemiStableTt");
				break;
			case FingerStability.Stable:
				FingerStabilityColor = _brushFingerStable;
				FingerStabilityTooltip = _translator.Translate("l_popup.PopupBendSequence.FingerStableTt");
				break;
			case FingerStability.Unstable:
				FingerStabilityColor = _brushFingerUnstable;
				FingerStabilityTooltip = _translator.Translate("l_popup.PopupBendSequence.FingerUnStableTt");
				break;
			default:
				throw new NotImplementedException();
			}
			break;
		case FingerPositioningMode.User:
			FingerStabilityColor = _brushFingerUser;
			FingerStabilityTooltip = _translator.Translate("l_popup.PopupBendSequence.FingerUserTt");
			break;
		case FingerPositioningMode.None:
			FingerStabilityColor = _brushFingerNone;
			FingerStabilityTooltip = _translator.Translate("l_popup.PopupBendSequence.FingerNoneTt");
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private string FormatBendForCommonList(IBendDescriptor face, bool isInchMode)
	{
		if (isInchMode)
		{
			return $"{face.Type}: L = {WiCAM.Pn4000.PN3D.Converter.Convert.MmToInch(face.BendParams.Length):0.##}";
		}
		return $"{face.Type}: L = {face.BendParams.Length:0.##}";
	}

	private string FormatGapForCommonList(IBendDescriptor actFace, IBendDescriptor nextFace, bool isInchMode)
	{
		Vector3d origin = actFace.BendParams.BendLineUnfoldModel.Origin;
		Vector3d vector3d = actFace.BendParams.BendLineUnfoldModel.Origin + actFace.BendParams.BendLineUnfoldModel.Direction;
		Vector3d origin2 = nextFace.BendParams.BendLineUnfoldModel.Origin;
		Vector3d vector3d2 = nextFace.BendParams.BendLineUnfoldModel.Origin + nextFace.BendParams.BendLineUnfoldModel.Direction;
		double value = ((IEnumerable<double>)new List<double>
		{
			(origin2 - origin).Length,
			(origin2 - vector3d).Length,
			(vector3d2 - origin).Length,
			(vector3d2 - vector3d).Length
		}).Min<double>();
		if (isInchMode)
		{
			return $"Gap: L = {WiCAM.Pn4000.PN3D.Converter.Convert.MmToInch(value):0.##}";
		}
		return $"Gap: L = {value:0.##}";
	}
}
