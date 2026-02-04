using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;
using WiCAM.Pn4000.MachineAndTools.Implementations;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class MaterialsViewModel : ViewModelBase
{
	public Action<ChangedConfigType> DataChanged;

	private readonly IMaterialManager _materialManager;

	public RadObservableCollection<MappingViewModel.MappingVm> MaterialMappings { get; } = new RadObservableCollection<MappingViewModel.MappingVm>();

	public MaterialsViewModel(IMaterialManager materialManager)
	{
		_materialManager = materialManager;
	}

	public void Init(IMaterialMappings materialMappings)
	{
		foreach (IMaterialUnf material in _materialManager.Material3DGroups.OrderBy((IMaterialUnf x) => x.Number))
		{
			IMaterialMapping materialMapping = materialMappings.Mappings.Find((IMaterialMapping x) => x.MaterialId == material.Number);
			MaterialMappings.Add(new MappingViewModel.MappingVm
			{
				Profile = new MappingViewModel.ItemVm
				{
					Desc = material.Name,
					Id = material.Number
				},
				PpId = materialMapping?.PpId,
				PpName = materialMapping?.PpIdentifier
			});
		}
	}

	public void Save(IMaterialMappings materialMappings)
	{
		List<IMaterialMapping> mappings = materialMappings.Mappings;
		mappings.Clear();
		foreach (MappingViewModel.MappingVm materialMapping in MaterialMappings)
		{
			if (!string.IsNullOrEmpty(materialMapping.PpId) || !string.IsNullOrEmpty(materialMapping.PpName))
			{
				mappings.Add(new MaterialMapping
				{
					MaterialId = materialMapping.Profile.Id,
					PpId = materialMapping.PpId,
					PpIdentifier = materialMapping.PpName
				});
			}
		}
	}

	public void Dispose()
	{
	}
}
