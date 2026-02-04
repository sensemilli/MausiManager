using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

public class MappingViewModel : ViewModelBase
{
	public class ItemVm
	{
		public int Id { get; init; }

		public string? Desc { get; init; }
	}

	public class MappingVm : INotifyPropertyChanged
	{
		private ItemVm _profile;

		public ItemVm Profile
		{
			get
			{
				return _profile;
			}
			set
			{
				if (!object.Equals(value, _profile))
				{
					_profile = value;
					OnPropertyChanged("Profile");
				}
			}
		}

		public string? PpId { get; set; }

		public string? PpName { get; set; }

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	private readonly IMessageLogGlobal _messageLogGlobal;

	private MachineToolsViewModel _toolConfig;

	private MaterialsViewModel _materialConfig;

	public Action<ChangedConfigType> DataChanged;

	public RadObservableCollection<MappingVm> MultiToolMappings => _toolConfig.MultiToolMappings;

	public RadObservableCollection<MappingVm> ToolMappings => _toolConfig.ToolMappings;

	public RadObservableCollection<MappingVm> MountIdMappings => _toolConfig.MountIdMappings;

	public RadObservableCollection<MappingVm> MaterialMappings => _materialConfig.MaterialMappings;

	public List<ItemVm> AllMultiTools => _toolConfig.MultiToolItemVms;

	public List<ItemVm> AllTools => _toolConfig.ToolItemVms;

	public List<ItemVm> AllMountIds => _toolConfig.MountIdItemVms;

	public IEnumerable<ItemVm> FilteredMultiTools => AllMultiTools.Except(MultiToolMappings.Select((MappingVm x) => x.Profile));

	public IEnumerable<ItemVm> FilteredTools => AllTools.Except(ToolMappings.Select((MappingVm x) => x.Profile));

	public IEnumerable<ItemVm> FilteredMountIds => AllMountIds.Except(MountIdMappings.Select((MappingVm x) => x.Profile));

	public IBendMachine BendMachine { get; set; }

	public MappingViewModel(IMessageLogGlobal messageLogGlobal)
	{
		_messageLogGlobal = messageLogGlobal;
	}

	public void SetMultiToolById(MappingVm mapping, int id)
	{
		ItemVm itemVm = AllMultiTools.FirstOrDefault((ItemVm x) => x.Id == id);
		if (itemVm != null)
		{
			mapping.Profile = itemVm;
		}
	}

	public void SetToolById(MappingVm mapping, int id)
	{
		ItemVm itemVm = AllTools.FirstOrDefault((ItemVm x) => x.Id == id);
		if (itemVm != null)
		{
			mapping.Profile = itemVm;
		}
	}

	public void SetMountById(MappingVm mapping, int mountId)
	{
		ItemVm itemVm = AllMountIds.Find((ItemVm x) => x.Id == mountId);
		if (itemVm != null)
		{
			mapping.Profile = itemVm;
		}
	}

	public void SetMultiToolByName(MappingVm mapping, string name)
	{
		ItemVm itemVm = AllMultiTools.FirstOrDefault((ItemVm x) => x.Desc == name);
		if (itemVm != null)
		{
			mapping.Profile = itemVm;
		}
	}

	public void SetToolByName(MappingVm mapping, string name)
	{
		ItemVm itemVm = AllTools.FirstOrDefault((ItemVm x) => x.Desc == name);
		if (itemVm != null)
		{
			mapping.Profile = itemVm;
		}
	}

	public void Init(IBendMachine bendMachine, MachineToolsViewModel toolModel, MaterialsViewModel materialViewModel)
	{
		_toolConfig = toolModel;
		_materialConfig = materialViewModel;
	}

	public void Dispose()
	{
	}

	public bool CanSave()
	{
		HashSet<ItemVm> hashSet = new HashSet<ItemVm>();
		List<ItemVm> list = new List<ItemVm>();
		foreach (MappingVm toolMapping in ToolMappings)
		{
			if (toolMapping.Profile == null)
			{
				_messageLogGlobal.ShowTranslatedErrorMessage("l_popup.MappingView.SaveErrorToolMappings", toolMapping.PpName);
				return false;
			}
			if (!hashSet.Add(toolMapping.Profile))
			{
				list.Add(toolMapping.Profile);
			}
		}
		if (list.Any())
		{
			_messageLogGlobal.ShowTranslatedErrorMessage("l_popup.MappingView.SaveErrorToolMappingsDuplicateProfiles", list.First().Desc);
			return false;
		}
		List<ItemVm> list2 = new List<ItemVm>();
		foreach (MappingVm multiToolMapping in MultiToolMappings)
		{
			if (multiToolMapping.Profile == null)
			{
				_messageLogGlobal.ShowTranslatedErrorMessage("l_popup.MappingView.SaveErrorToolMappings", multiToolMapping.PpName);
				return false;
			}
			if (!hashSet.Add(multiToolMapping.Profile))
			{
				list2.Add(multiToolMapping.Profile);
			}
		}
		if (list2.Any())
		{
			_messageLogGlobal.ShowTranslatedErrorMessage("l_popup.MappingView.SaveErrorToolMappingsDuplicateProfiles", list2.First().Desc);
			return false;
		}
		List<ItemVm> list3 = new List<ItemVm>();
		foreach (MappingVm mountIdMapping in MountIdMappings)
		{
			if (mountIdMapping.Profile == null)
			{
				_messageLogGlobal.ShowTranslatedErrorMessage("l_popup.MappingView.SaveErrorMountIdMappings", mountIdMapping.PpName);
				return false;
			}
			if (!hashSet.Add(mountIdMapping.Profile))
			{
				list3.Add(mountIdMapping.Profile);
			}
		}
		if (list3.Any())
		{
			_messageLogGlobal.ShowTranslatedErrorMessage("l_popup.MappingView.SaveErrorToolMappings", "Duplicate Mount-IDs!");
			return false;
		}
		return true;
	}
}
