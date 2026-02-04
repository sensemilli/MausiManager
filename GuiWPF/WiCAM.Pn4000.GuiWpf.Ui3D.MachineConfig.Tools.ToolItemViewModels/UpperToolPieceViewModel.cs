using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class UpperToolPieceViewModel : ToolPieceViewModel
{
	private bool _isHeelLeft;

	private bool _isHeelRight;

	private bool _isSensorTool;

	private double? _lengthPlug;

	public bool HasHeelLeft
	{
		get
		{
			return _isHeelLeft;
		}
		set
		{
			if (_isHeelLeft != value)
			{
				_isHeelLeft = value;
				_isChanged = true;
				NotifyPropertyChanged("HasHeelLeft");
			}
		}
	}

	public bool HasHeelRight
	{
		get
		{
			return _isHeelRight;
		}
		set
		{
			if (_isHeelRight != value)
			{
				_isHeelRight = value;
				_isChanged = true;
				NotifyPropertyChanged("HasHeelRight");
			}
		}
	}

	public bool IsAngleMeasurementTool
	{
		get
		{
			return _isSensorTool;
		}
		set
		{
			if (_isSensorTool != value)
			{
				_isSensorTool = value;
				_isChanged = true;
				NotifyPropertyChanged("IsAngleMeasurementTool");
			}
		}
	}

	public double? LengthPlug
	{
		get
		{
			return _lengthPlug;
		}
		set
		{
			if (_lengthPlug != value)
			{
				_lengthPlug = value;
				_isChanged = true;
				NotifyPropertyChanged("LengthPlug");
			}
		}
	}

	public UpperToolPieceViewModel(MachineToolsViewModel machineToolsViewModel, ToolListViewModel currentToolList, MultiToolViewModel multiTool, string fileName, double length = 0.0, int amount = 0)
		: base(machineToolsViewModel, currentToolList, multiTool, fileName, length, amount)
	{
	}

	public UpperToolPieceViewModel(ToolListViewModel currentToolList, MultiToolViewModel multiTool, IEnumerable<AliasPieceViewModel> aliasPieces, int totalAmount, IUpperToolPieceProfile part, MachineToolsViewModel machineToolsViewModel)
		: base(machineToolsViewModel, currentToolList, multiTool, aliasPieces, totalAmount, part)
	{
		_isHeelLeft = part.IsHeelLeft;
		_isHeelRight = part.IsHeelRight;
		_isSensorTool = part.IsSensorTool;
		_lengthPlug = part.LengthPlug;
	}

	public new IToolPieceProfile Save(IToolPieceProfile toolPieceProfile)
	{
		if (toolPieceProfile == null)
		{
			toolPieceProfile = base._toolStorage.CreatePunchPiece(base.MultiTool.MultiToolProfile);
		}
		base.Save(toolPieceProfile);
		if (toolPieceProfile is IUpperToolPieceProfile upperToolPieceProfile)
		{
			upperToolPieceProfile.IsHeelLeft = _isHeelLeft;
			upperToolPieceProfile.IsHeelRight = _isHeelRight;
			upperToolPieceProfile.IsSensorTool = _isSensorTool;
			upperToolPieceProfile.LengthPlug = _lengthPlug;
		}
		return toolPieceProfile;
	}
}
