using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class AliasPieceViewModel
{
	private readonly MachineToolsViewModel _machineVm;

	public IAliasPieceProfile? Alias { get; private set; }

	public int Id { get; private set; }

	public bool Deleted { get; set; }

	public List<ToolPieceViewModel> Pieces { get; } = new List<ToolPieceViewModel>();

	public AliasPieceViewModel(IAliasPieceProfile alias, MachineToolsViewModel machineVm)
	{
		_machineVm = machineVm;
		Alias = alias;
		Id = alias.Id;
	}

	public AliasPieceViewModel(MachineToolsViewModel machineVm)
	{
		_machineVm = machineVm;
		Id = machineVm.GenerateFakeId();
	}

	public bool CanSave()
	{
		if (!Deleted)
		{
			List<IAliasMultiToolProfile> list = Pieces.Select((ToolPieceViewModel x) => x.MultiTool.AliasMultiToolViewModel.Profile).Distinct().ToList();
			if (list.Count > 1)
			{
				_machineVm.MessageLogGlobal.ShowTranslatedErrorMessage("l_popup.PopupMachineConfig.SaveErrorPieceAlias", Alias.Id, list.Count);
				return false;
			}
		}
		return true;
	}

	public void Save(IGlobalToolStorage storage)
	{
		if (Pieces.Count == 0)
		{
			Deleted = true;
		}
		if (Deleted)
		{
			if (Alias != null)
			{
				storage.DeleteAliasPiece(Alias.Id);
			}
			return;
		}
		List<IAliasMultiToolProfile> list = Pieces.Select((ToolPieceViewModel x) => x.MultiTool.AliasMultiToolViewModel.Profile).Distinct().ToList();
		if (list.Count != 1)
		{
			throw new Exception($"each alias tool piece must have exactly 1 multiTool alias. This one id {Alias.Id} has {list.Count}");
		}
		IAliasMultiToolProfile aliasMultiToolProfile = list.First();
		if (Alias == null)
		{
			IAliasPieceProfile aliasPieceProfile2 = (Alias = storage.CreateAliasPieceProfile(aliasMultiToolProfile));
		}
		Id = Alias.Id;
		if (Alias.AliasMultiTool != aliasMultiToolProfile)
		{
			Alias.AliasMultiTool?.RemoveAliasToolPieceProfile(Alias);
			Alias.AliasMultiTool = aliasMultiToolProfile;
			Alias.AliasMultiTool.AddAliasToolPieceProfile(Alias);
		}
	}
}
