using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.GuiContracts.Implementations;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.ToolCalculation.Implementations;
using WiCAM.Pn4000.ToolCalculationMediator;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class ToolCalculationOptionViewModel : SubViewModelBase, IToolCalculationOptionViewModel, ISubViewModel, IPopupViewModel
{
	public class ProfileVm : ViewModelBase
	{
		private IToolCalculationOptionOverwrite _data;

		public bool IsNew { get; set; }

		public IToolCalculationOptionOverwrite Source { get; }

		public IToolCalculationOptionOverwrite Data
		{
			get
			{
				return _data;
			}
			set
			{
				if (_data != value)
				{
					_data = value;
					NotifyPropertyChanged("Data");
				}
			}
		}

		public string Desc => Data.Description;

		public ProfileVm(IToolCalculationOptionOverwrite source, bool isNew)
		{
			IsNew = isNew;
			Source = source;
			Data = source.CreateClone();
		}
	}

	private readonly IToolCalculations _toolCalculations;

	private readonly IDoc3d _doc;

	private readonly ITranslator _translator;

	private readonly IMachineBendFactory _machineBendFactory;

	private ObservableCollection<ProfileVm> _profiles;

	private ProfileVm? _selectedProfile;

	public ICommand CmdOk { get; }

	public ICommand CmdSave { get; }

	public ICommand CmdCancel { get; }

	public ICommand CmdDelete { get; }

	public ICommand CmdAdd { get; }

	public ObservableCollection<ProfileVm> Profiles
	{
		get
		{
			return _profiles;
		}
		set
		{
			if (_profiles != value)
			{
				_profiles = value;
				NotifyPropertyChanged("Profiles");
			}
		}
	}

	public ProfileVm? SelectedProfile
	{
		get
		{
			return _selectedProfile;
		}
		set
		{
			if (_selectedProfile != value)
			{
				_selectedProfile = value;
				NotifyPropertyChanged("SelectedProfile");
			}
		}
	}

	public List<ComboboxEntry<ToolPieceSortingStrategies?>> AllToolPieceSortingStrategies { get; }

	public List<ComboboxEntry<IToolCalculationOption.ProfileOrders?>> AllProfileOrders { get; }

	public List<ComboboxEntry<Guid>> AllBendOrderStrategyGuids { get; }

	public Dictionary<string, object> DefaultValues { get; }

	public ToolCalculationOptionViewModel(IToolCalculations toolCalculations, IDoc3d doc, ITranslator translator, IMachineBendFactory machineBendFactory)
	{
		_toolCalculations = toolCalculations;
		_doc = doc;
		_translator = translator;
		_machineBendFactory = machineBendFactory;
		CmdOk = new WiCAM.Pn4000.GuiContracts.Implementations.RelayCommand(Ok);
		CmdSave = new WiCAM.Pn4000.GuiContracts.Implementations.RelayCommand(Save);
		CmdCancel = new WiCAM.Pn4000.GuiContracts.Implementations.RelayCommand(Cancel);
		CmdDelete = new WiCAM.Pn4000.GuiContracts.Implementations.RelayCommand(DeleteProfile);
		CmdAdd = new WiCAM.Pn4000.GuiContracts.Implementations.RelayCommand(AddProfile);
		IToolCalculationOption option = new ToolCalculationOptionOverwrite().FillWithDefaults();
		DefaultValues = option.GetType().GetProperties().ToDictionary((PropertyInfo x) => x.Name, (PropertyInfo x) => x.GetValue(option));
		AllToolPieceSortingStrategies = _translator.GetTranslatedComboboxEntriesNullable<ToolPieceSortingStrategies>().ToList();
		AllProfileOrders = _translator.GetTranslatedComboboxEntriesNullable<IToolCalculationOption.ProfileOrders>().ToList();
		string text = _translator.Translate("ToolCalcSettings.DefaultValue");
		object defaultStrategy = DefaultValues["DefaultSortingStrategy"];
		AllToolPieceSortingStrategies.First((ComboboxEntry<ToolPieceSortingStrategies?> x) => !x.Value.HasValue).Desc = text + " " + AllToolPieceSortingStrategies.First((ComboboxEntry<ToolPieceSortingStrategies?> x) => x.Value.Equals(defaultStrategy)).Desc;
		object defaultOrder = DefaultValues["ProfileOrder"];
		AllProfileOrders.First((ComboboxEntry<IToolCalculationOption.ProfileOrders?> x) => !x.Value.HasValue).Desc = text + " " + AllProfileOrders.First((ComboboxEntry<IToolCalculationOption.ProfileOrders?> x) => x.Value.Equals(defaultOrder)).Desc;
		AllBendOrderStrategyGuids = new List<ComboboxEntry<Guid>>();
	}

	private void Ok()
	{
		CalculateToolsWithSettings();
	}

	private void Save()
	{
		ApplySettings();
	}

	private void Cancel()
	{
		Close();
	}

	private void DeleteProfile()
	{
		if (_profiles.Count > 1 && _selectedProfile != null)
		{
			if (!_selectedProfile.IsNew)
			{
				_doc.BendMachine.ToolCalculationSettings.Options.Remove(_selectedProfile.Source);
			}
			_profiles.Remove(_selectedProfile);
		}
	}

	private void AddProfile()
	{
		if (_selectedProfile != null)
		{
			ProfileVm profileVm = new ProfileVm(_selectedProfile.Data.CreateClone(), isNew: true);
			profileVm.Data.Description = "Profil " + (_profiles.Count + 1);
			_profiles.Add(profileVm);
			SelectedProfile = profileVm;
		}
	}

	private void CalculateToolsWithSettings()
	{
		if (SelectedProfile != null)
		{
			_toolCalculations.SetToolsDefault(_doc, _toolCalculations.CreateToolCalcOption(_doc, SelectedProfile.Source));
		}
	}

	public void ImportOptions()
	{
		AllBendOrderStrategyGuids.Clear();
		if (_doc.BendMachine != null)
		{
			AllBendOrderStrategyGuids.AddRange(_doc.BendMachine.ToolCalculationSettings.BendOrderStrategies.Select((IBendSequenceOrder x) => new ComboboxEntry<Guid>(x.Description, x.Id)));
		}
		NotifyPropertyChanged("AllBendOrderStrategyGuids");
		Profiles = (_doc.BendMachine?.ToolCalculationSettings.Options.Select((IToolCalculationOptionOverwrite x) => new ProfileVm(x, isNew: false))).EmptyIfNull().ToObservableCollection();
		SelectedProfile = Profiles.FirstOrDefault();
	}

	public void ApplySettings()
	{
		ProfileVm selectedProfile = SelectedProfile;
		if (selectedProfile != null)
		{
			selectedProfile.Source.ApplySettingsFrom(selectedProfile.Data);
			if (selectedProfile.IsNew)
			{
				_doc.BendMachine.ToolCalculationSettings.Options.Add(selectedProfile.Source);
				selectedProfile.IsNew = false;
			}
			_machineBendFactory.SaveMachine(_doc.BendMachine);
		}
	}
}
