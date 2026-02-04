using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

namespace WiCAM.Pn4000.GuiWpf.TabBend.BendSequenceList.BendContext;

internal class BendContextStepBendViewModel : SubViewModelBase
{
	private readonly IUnitConverter _unitConverter;

	private double _radius;

	private bool _useUserBendDeduction;

	public int Amount { get; set; } = 42;

	public double RadiusUi
	{
		get
		{
			return _unitConverter.Length.ToUi(Radius, 2);
		}
		set
		{
			Radius = _unitConverter.Length.FromUi(value);
		}
	}

	public double Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
			NotifyPropertyChanged("RadiusUi");
		}
	}

	public bool UseUserBendDeduction
	{
		get
		{
			return _useUserBendDeduction;
		}
		set
		{
			if (_useUserBendDeduction != value)
			{
				_useUserBendDeduction = value;
				NotifyPropertyChanged("UseUserBendDeduction");
			}
		}
	}

	public double BendDeductionMiddle { get; set; }

	public double BendDeductionStartEnd { get; set; }

	public ICommand CmdCommit { get; }

	public ICommand CmdTurnOffStepBend { get; }

	public BendContextStepBendViewModel(IUnitConverter unitConverter)
	{
		_unitConverter = unitConverter;
		CmdCommit = new RelayCommand(Commit);
		CmdTurnOffStepBend = new RelayCommand(TurnOffStepBend);
	}

	private void Commit()
	{
	}

	private void TurnOffStepBend()
	{
	}
}
