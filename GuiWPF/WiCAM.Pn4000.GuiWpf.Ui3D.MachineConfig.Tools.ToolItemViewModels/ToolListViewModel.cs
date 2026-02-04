using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class ToolListViewModel : ToolItemViewModel
{
	private readonly IGlobalToolStorage _toolStorage;

	private bool _usedByMachine;

	private string _description;

	private bool _isChanged;

	private Dictionary<ToolPieceViewModel, int> PiecesAvailable { get; init; }

	public bool IsChanged => _isChanged;

	public int? ID { get; private set; }

	public bool UsedByMachine
	{
		get
		{
			return _usedByMachine;
		}
		set
		{
			if (_usedByMachine == value)
			{
				return;
			}
			_usedByMachine = value;
			foreach (var (toolPieceViewModel2, num2) in PiecesAvailable)
			{
				if (_usedByMachine)
				{
					toolPieceViewModel2.TotalAmount += num2;
				}
				else
				{
					toolPieceViewModel2.TotalAmount -= num2;
				}
			}
			NotifyPropertyChanged("UsedByMachine");
		}
	}

	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (_description != value)
			{
				_description = value;
				_isChanged = true;
				NotifyPropertyChanged("Description");
				NotifyPropertyChanged("DisplayText");
			}
		}
	}

	public string DisplayText => $"{ID} - {Description}";

	public int GetAmount(ToolPieceViewModel piece)
	{
		return PiecesAvailable.GetValueOrDefault(piece);
	}

	public void SetAmount(ToolPieceViewModel piece, int newAmount)
	{
		int valueOrDefault = PiecesAvailable.GetValueOrDefault(piece);
		if (newAmount != valueOrDefault)
		{
			if (newAmount == 0)
			{
				PiecesAvailable.Remove(piece);
			}
			else
			{
				PiecesAvailable[piece] = newAmount;
			}
			if (UsedByMachine)
			{
				piece.TotalAmount += newAmount - valueOrDefault;
			}
			_isChanged = true;
		}
	}

	public void ReplacePiece(ToolPieceViewModel oldPiece, ToolPieceViewModel newPiece)
	{
		if (PiecesAvailable.TryGetValue(oldPiece, out var value))
		{
			PiecesAvailable.Remove(oldPiece);
			PiecesAvailable.TryAdd(newPiece, value);
		}
		_isChanged = true;
	}

	public ToolListViewModel(IGlobalToolStorage toolStorage)
	{
		_toolStorage = toolStorage;
		PiecesAvailable = new Dictionary<ToolPieceViewModel, int>();
	}

	public ToolListViewModel(IGlobalToolStorage toolStorage, ToolListViewModel copy)
	{
		_toolStorage = toolStorage;
		PiecesAvailable = copy.PiecesAvailable.ToDictionary<KeyValuePair<ToolPieceViewModel, int>, ToolPieceViewModel, int>((KeyValuePair<ToolPieceViewModel, int> x) => x.Key, (KeyValuePair<ToolPieceViewModel, int> x) => x.Value);
		Description = copy.Description + " Copy";
	}

	public ToolListViewModel(IToolListAvailable toolList, IGlobalToolStorage toolStorage, Dictionary<int, List<ToolPieceViewModel>> piecesToVm)
	{
		_toolStorage = toolStorage;
		ID = toolList.Id;
		Description = toolList.Description;
		Dictionary<ToolPieceViewModel, int> dictionary = new Dictionary<ToolPieceViewModel, int>();
		foreach (var (aliasPieceProfile2, value) in toolList.PiecesAvailable)
		{
			if (piecesToVm.TryGetValue(aliasPieceProfile2.Id, out List<ToolPieceViewModel> value2))
			{
				dictionary.Add(value2.First(), value);
			}
		}
		PiecesAvailable = dictionary;
	}

	public void Save(IToolListAvailable toolList)
	{
		if (toolList == null)
		{
			toolList = _toolStorage.CreateToolList();
		}
		ID = toolList.Id;
		toolList.Description = Description;
		toolList.PiecesAvailable.Clear();
		foreach (var (toolPieceViewModel2, num2) in PiecesAvailable.ToList())
		{
			if (num2 == 0 || toolPieceViewModel2.IsDeleted)
			{
				continue;
			}
			IToolPieceProfile toolPieceProfile = _toolStorage.TryGetPieceProfile(toolPieceViewModel2.ID.Value);
			if (toolPieceProfile == null)
			{
				continue;
			}
			foreach (IAliasPieceProfile alias in toolPieceProfile.Aliases)
			{
				toolList.PiecesAvailable[alias] = toolList.PiecesAvailable.GetValueOrDefault(alias, 0) + num2;
			}
		}
	}
}
