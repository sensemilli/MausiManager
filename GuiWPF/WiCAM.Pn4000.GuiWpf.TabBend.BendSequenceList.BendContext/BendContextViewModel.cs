using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.ToolCalculation.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabBend.BendSequenceList.BendContext;

internal class BendContextViewModel : SubViewModelBase
{
	public class AggregateProperty<T> : ViewModelBase where T : struct
	{
		private readonly BendContextViewModel _vm;

		private readonly Func<BendSequenceListViewModel.BendViewModel, T?> _getValue;

		private readonly Action<BendSequenceListViewModel.BendViewModel, T?> _setValue;

		private readonly Func<BendSequenceListViewModel.BendViewModel, T> _getValue2;

		private readonly Action<BendSequenceListViewModel.BendViewModel, T> _setValue2;

		private readonly T? _mixedValue;

		public Action<T?>? CommitAction;

		private T? _value;

		public Visibility Visibility { get; set; }

		public T? Value
		{
			get
			{
				return _value;
			}
			set
			{
				ref T? value2 = ref _value;
				if (!value2.HasValue || !value2.GetValueOrDefault().Equals(value))
				{
					_value = value;
					NotifyPropertyChanged("Value");
				}
			}
		}

		public AggregateProperty(BendContextViewModel vm, Func<BendSequenceListViewModel.BendViewModel, T?> getValue, Action<BendSequenceListViewModel.BendViewModel, T?> setValue)
		{
			_vm = vm;
			_getValue = getValue;
			_setValue = setValue;
			_mixedValue = null;
		}

		public AggregateProperty(BendContextViewModel vm, Func<BendSequenceListViewModel.BendViewModel, T> getValue, Action<BendSequenceListViewModel.BendViewModel, T> setValue)
		{
			_vm = vm;
			_getValue2 = getValue;
			_setValue2 = setValue;
			_getValue = GetValueConvert;
			_setValue = SetValueConvert;
		}

		public AggregateProperty(BendContextViewModel vm, Func<BendSequenceListViewModel.BendViewModel, T> getValue, Action<BendSequenceListViewModel.BendViewModel, T> setValue, T mixedValue)
			: this(vm, getValue, setValue)
		{
			_mixedValue = mixedValue;
		}

		private void SetValueConvert(BendSequenceListViewModel.BendViewModel bend, T? val)
		{
			if (val.HasValue)
			{
				_setValue2(bend, val.Value);
			}
		}

		private T? GetValueConvert(BendSequenceListViewModel.BendViewModel bend)
		{
			return _getValue2(bend);
		}

		public void UpdateUi()
		{
			Value = AggregateProp();
		}

		public void Commit()
		{
			if (CommitAction != null)
			{
				CommitAction(Value);
				return;
			}
			T? value = Value;
			foreach (BendSequenceListViewModel.BendViewModel selectedBend in _vm.SelectedBends)
			{
				_setValue(selectedBend, value);
			}
		}

		private T? AggregateProp()
		{
			if (!_vm.SelectedBends.Any())
			{
				return _mixedValue;
			}
			T? result = _getValue(_vm.SelectedBends.First());
			foreach (BendSequenceListViewModel.BendViewModel item in _vm.SelectedBends.Skip(1))
			{
				if (!result.HasValue || !result.GetValueOrDefault().Equals(_getValue(item)))
				{
					return _mixedValue;
				}
			}
			return result;
		}
	}

	public class AggregateProperty2<T> : ViewModelBase where T : class
	{
		private readonly BendContextViewModel _vm;

		private readonly Func<BendSequenceListViewModel.BendViewModel, T?> _getValue;

		private readonly Action<BendSequenceListViewModel.BendViewModel, T?> _setValue;

		private T? _value;

		public T? Value
		{
			get
			{
				return _value;
			}
			set
			{
				T? value2 = _value;
				if (value2 == null || !value2.Equals(value))
				{
					_value = value;
					NotifyPropertyChanged("Value");
				}
			}
		}

		public AggregateProperty2(BendContextViewModel vm, Func<BendSequenceListViewModel.BendViewModel, T?> getValue, Action<BendSequenceListViewModel.BendViewModel, T?> setValue)
		{
			_vm = vm;
			_getValue = getValue;
			_setValue = setValue;
		}

		public void UpdateUi()
		{
			Value = AggregateProp();
		}

		public void Commit()
		{
			T value = Value;
			foreach (BendSequenceListViewModel.BendViewModel selectedBend in _vm.SelectedBends)
			{
				_setValue(selectedBend, value);
			}
		}

		private T? AggregateProp()
		{
			if (!_vm.SelectedBends.Any())
			{
				return null;
			}
			T val = _getValue(_vm.SelectedBends.First());
			foreach (BendSequenceListViewModel.BendViewModel item in _vm.SelectedBends.Skip(1))
			{
				if (val == null || !val.Equals(_getValue(item)))
				{
					return null;
				}
			}
			return val;
		}
	}

	public class ToolProfileVm
	{
		private readonly string _desc;

		public IToolProfile Profile { get; }

		public string Desc
		{
			get
			{
				if (Profile != null)
				{
					return $"{Profile.ID} {Profile.Name}" + (IsPreferred ? " *" : "");
				}
				return _desc;
			}
		}

		public bool IsPreferred { get; set; }

		public ToolProfileVm(IToolProfile profile)
		{
			Profile = profile;
		}

		public ToolProfileVm(string desc)
		{
			_desc = desc;
		}
	}

	[CompilerGenerated]
	private sealed class _003CCalculateSelectedBendNumbers_003Ed__121 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public BendContextViewModel _003C_003E4__this;

		private IEnumerator<int> _003C_003E7__wrap1;

		private int _003Corder_003E5__3;

		string IEnumerator<string>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCalculateSelectedBendNumbers_003Ed__121(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || (uint)(num - 1) <= 1u)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				BendContextViewModel bendContextViewModel = _003C_003E4__this;
				int num3;
				int num2;
				switch (num)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					List<int> list = (from x in bendContextViewModel.SelectedBends
						select x.OrderUi into x
						orderby x
						select x).ToList();
					if (list.Count > 0)
					{
						num3 = list.First();
						num2 = num3;
						list.Add(list.Last() + 10);
						_003C_003E7__wrap1 = list.Skip(1).GetEnumerator();
						_003C_003E1__state = -3;
						goto IL_016b;
					}
					_003C_003E2__current = "-";
					_003C_003E1__state = 3;
					return true;
				}
				case 1:
					_003C_003E1__state = -3;
					goto IL_015b;
				case 2:
					_003C_003E1__state = -3;
					goto IL_015b;
				case 3:
					{
						_003C_003E1__state = -1;
						break;
					}
					IL_015b:
					num2 = _003Corder_003E5__3;
					goto IL_0163;
					IL_016b:
					if (_003C_003E7__wrap1.MoveNext())
					{
						_003Corder_003E5__3 = _003C_003E7__wrap1.Current;
						if (_003Corder_003E5__3 > num3 + 1)
						{
							if (num3 == num2)
							{
								_003C_003E2__current = num3.ToString();
								_003C_003E1__state = 1;
								return true;
							}
							_003C_003E2__current = $"{num2}-{num3}";
							_003C_003E1__state = 2;
							return true;
						}
						goto IL_0163;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					break;
					IL_0163:
					num3 = _003Corder_003E5__3;
					goto IL_016b;
				}
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CCalculateSelectedBendNumbers_003Ed__121 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CCalculateSelectedBendNumbers_003Ed__121(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	private readonly IUnitConverter _unitConverter;

	private readonly IBendSequenceListViewModel _bendSequenceListViewModel;

	private readonly IBendSelection _bendSelection;

	private readonly IDoc3d _doc;

	private readonly ITranslator _translator;

	private readonly IToolOperator _toolOperator;

	private readonly IDockingService _dockingService;

	private ToolProfileVm _unassignToolProfile;

	private ToolProfileVm _mixedToolProfile;

	private List<BendSequenceListViewModel.BendViewModel> SelectedBends { get; set; }

	public string SelectedBendNumbers { get; private set; }

	public AggregateProperty<int> ToolSetupNo { get; set; }

	public AggregateProperty2<string> Comment { get; set; }

	public AggregateProperty<int> UserStepChangeMode { get; set; }

	public AggregateProperty<bool> FlipGeometry { get; set; }

	public AggregateProperty<bool> AcbLaser { get; set; }

	public AggregateProperty<LiftingAidEnum> LiftingAidLeftFront { get; set; }

	public AggregateProperty<LiftingAidEnum> LiftingAidLeftBack { get; set; }

	public AggregateProperty<LiftingAidEnum> LiftingAidRightFront { get; set; }

	public AggregateProperty<LiftingAidEnum> LiftingAidRightBack { get; set; }

	public AggregateProperty<double> Radius { get; set; }

	public AggregateProperty<double> BendDeduction { get; set; }

	public AggregateProperty2<string> UpperToolProfile { get; set; }

	public AggregateProperty2<string> LowerToolProfile { get; set; }

	public ICommand CmdCommit { get; }

	public ICommand CmdCancel { get; }

	public ICommand CmdSplitAngles { get; }

	public ICommand CmdStepBend { get; }

	public ICommand CmdSplitCombined { get; }

	public string LengthUnit => _unitConverter.Length.Unit;

	public List<ToolProfileVm> AllUpperTools { get; set; }

	public List<ToolProfileVm> AllLowerTools { get; set; }

	public ToolProfileVm SelectedUpperToolGoal { get; set; }

	public ToolProfileVm SelectedLowerToolGoal { get; set; }

	public List<ComboboxEntry<LiftingAidEnum>> AllLiftingAidValues { get; }

	public RadObservableCollection<ComboboxEntry<int>> ToolSetups { get; } = new RadObservableCollection<ComboboxEntry<int>>();

	public BendContextViewModel(IUnitConverter unitConverter, IBendSequenceListViewModel bendSequenceListViewModel, IBendSelection bendSelection, IDoc3d doc, ITranslator translator, IToolOperator toolOperator, IDockingService dockingService)
	{
		_unitConverter = unitConverter;
		_bendSequenceListViewModel = bendSequenceListViewModel;
		_bendSelection = bendSelection;
		_doc = doc;
		_translator = translator;
		_toolOperator = toolOperator;
		_dockingService = dockingService;
		_bendSequenceListViewModel.Bends.CollectionChanged += RefreshBendList;
		_bendSelection.SelectionChanged += RefreshBendList;
		IBendMachineGeometry bendMachineGeometry = _doc.BendMachine?.Geometry;
		ToolSetupNo = new AggregateProperty<int>(this, (BendSequenceListViewModel.BendViewModel x) => x.ToolSetupNo, delegate(BendSequenceListViewModel.BendViewModel model, int val)
		{
			if (val > -2)
			{
				model.ToolSetupNo = val;
			}
		}, -2);
		Comment = new AggregateProperty2<string>(this, (BendSequenceListViewModel.BendViewModel x) => x.Comment, delegate(BendSequenceListViewModel.BendViewModel model, string? val)
		{
			model.Comment = val;
		});
		UserStepChangeMode = new AggregateProperty<int>(this, (BendSequenceListViewModel.BendViewModel x) => x.UserStepChangeMode, delegate(BendSequenceListViewModel.BendViewModel model, int? val)
		{
			model.UserStepChangeMode = val;
		});
		FlipGeometry = new AggregateProperty<bool>(this, (BendSequenceListViewModel.BendViewModel x) => x.FlipGeometry, delegate(BendSequenceListViewModel.BendViewModel model, bool val)
		{
			model.FlipGeometry = val;
		});
		AcbLaser = new AggregateProperty<bool>(this, (BendSequenceListViewModel.BendViewModel x) => x.Lcb, delegate(BendSequenceListViewModel.BendViewModel model, bool? val)
		{
			model.Lcb = val;
		})
		{
			Visibility = ((!_doc.MachineFullyLoaded || _doc.BendMachine.PressBrakeData.MeasurementAngle != 1) ? Visibility.Collapsed : Visibility.Visible)
		};
		LiftingAidLeftFront = new AggregateProperty<LiftingAidEnum>(this, (BendSequenceListViewModel.BendViewModel x) => x.LiftingAidLeftFront, delegate(BendSequenceListViewModel.BendViewModel model, LiftingAidEnum val)
		{
			model.LiftingAidLeftFront = val;
		})
		{
			Visibility = ((bendMachineGeometry?.LeftFrontLiftingAid == null) ? Visibility.Collapsed : Visibility.Visible)
		};
		LiftingAidLeftBack = new AggregateProperty<LiftingAidEnum>(this, (BendSequenceListViewModel.BendViewModel x) => x.LiftingAidLeftBack, delegate(BendSequenceListViewModel.BendViewModel model, LiftingAidEnum val)
		{
			model.LiftingAidLeftBack = val;
		})
		{
			Visibility = ((bendMachineGeometry?.LeftBackLiftingAid == null) ? Visibility.Collapsed : Visibility.Visible)
		};
		LiftingAidRightFront = new AggregateProperty<LiftingAidEnum>(this, (BendSequenceListViewModel.BendViewModel x) => x.LiftingAidRightFront, delegate(BendSequenceListViewModel.BendViewModel model, LiftingAidEnum val)
		{
			model.LiftingAidRightFront = val;
		})
		{
			Visibility = ((bendMachineGeometry?.RightFrontLiftingAid == null) ? Visibility.Collapsed : Visibility.Visible)
		};
		LiftingAidRightBack = new AggregateProperty<LiftingAidEnum>(this, (BendSequenceListViewModel.BendViewModel x) => x.LiftingAidRightBack, delegate(BendSequenceListViewModel.BendViewModel model, LiftingAidEnum val)
		{
			model.LiftingAidRightBack = val;
		})
		{
			Visibility = ((bendMachineGeometry?.RightBackLiftingAid == null) ? Visibility.Collapsed : Visibility.Visible)
		};
		Radius = new AggregateProperty<double>(this, (BendSequenceListViewModel.BendViewModel x) => x.RadiusUiUnits, delegate(BendSequenceListViewModel.BendViewModel model, double val)
		{
			model.RadiusUiUnits = val;
		});
		BendDeduction = new AggregateProperty<double>(this, (BendSequenceListViewModel.BendViewModel x) => x.BendDeductionUiUnits, delegate(BendSequenceListViewModel.BendViewModel model, double val)
		{
			model.BendDeductionUiUnits = val;
		});
		UpperToolProfile = new AggregateProperty2<string>(this, (BendSequenceListViewModel.BendViewModel x) => x.UpperToolProfile, delegate
		{
		});
		LowerToolProfile = new AggregateProperty2<string>(this, (BendSequenceListViewModel.BendViewModel x) => x.LowerToolProfile, delegate
		{
		});
		Radius.CommitAction = CommitRadius;
		BendDeduction.CommitAction = CommitBendDeduction;
		CmdCommit = new RelayCommand(CommitChanges);
		CmdCancel = new RelayCommand(CloseCancel);
		CmdSplitAngles = new RelayCommand(SplitAngles);
		CmdStepBend = new RelayCommand(StepBendSubMenu);
		CmdSplitCombined = new RelayCommand(SplitCombined);
		_unassignToolProfile = new ToolProfileVm("-");
		_mixedToolProfile = new ToolProfileVm(_translator.Translate("l_popup.EditBendContextMenu.MixedState"));
		IBendMachineTools bendMachineTools = doc.BendMachine?.ToolConfig;
		AllUpperTools = bendMachineTools?.UpperTools.Select((IPunchProfile x) => new ToolProfileVm(x)).ToList() ?? new List<ToolProfileVm>();
		AllLowerTools = bendMachineTools?.LowerTools.Select((IDieProfile x) => new ToolProfileVm(x)).ToList() ?? new List<ToolProfileVm>();
		if (bendMachineTools != null)
		{
			bendMachineTools.ProfileSelector.GetPreferedGroupsBasic(bendMachineTools, doc.MaterialNumber, doc.Thickness, out HashSet<int> punchGroupIds, out HashSet<int> dieGroupIds);
			foreach (ToolProfileVm allUpperTool in AllUpperTools)
			{
				_ = allUpperTool.Profile?.GroupID;
				allUpperTool.IsPreferred = punchGroupIds.Contains(allUpperTool.Profile.GroupID);
			}
			foreach (ToolProfileVm allLowerTool in AllLowerTools)
			{
				_ = allLowerTool.Profile?.GroupID;
				allLowerTool.IsPreferred = dieGroupIds.Contains(allLowerTool.Profile.GroupID);
			}
		}
		AllLiftingAidValues = new List<ComboboxEntry<LiftingAidEnum>>(_translator.GetTranslatedComboboxEntries<LiftingAidEnum>());
		AllUpperTools = (from x in AllUpperTools
			orderby x.IsPreferred descending, x.Desc
			select x).ToList();
		AllLowerTools = (from x in AllLowerTools
			orderby x.IsPreferred descending, x.Desc
			select x).ToList();
		AllUpperTools.Insert(0, _unassignToolProfile);
		AllUpperTools.Insert(0, _mixedToolProfile);
		AllLowerTools.Insert(0, _unassignToolProfile);
		AllLowerTools.Insert(0, _mixedToolProfile);
		RefreshToolSetupsList();
	}

	private void CommitRadius(double? radiusUi)
	{
		if (radiusUi.HasValue)
		{
			double newRadius = _unitConverter.Length.FromUi(radiusUi.Value);
			_bendSequenceListViewModel.ChangeRadius(SelectedBends, newRadius);
		}
	}

	private void CommitBendDeduction(double? bdUi)
	{
		if (bdUi.HasValue)
		{
			double newDeduction = _unitConverter.Length.FromUi(bdUi.Value);
			_bendSequenceListViewModel.ChangeDeduction(SelectedBends, newDeduction);
		}
	}

	private void RefreshToolSetupsList()
	{
		ToolSetups.SuspendNotifications();
		ToolSetups.Clear();
		ToolSetups.Add(new ComboboxEntry<int>("-", -1));
		ToolSetups.Add(new ComboboxEntry<int>(_translator.Translate("l_popup.EditBendContextMenu.MixedState"), -2));
		foreach (IToolSetups item in _bendSelection?.ToolsAndBends?.ToolSetups ?? new List<IToolSetups>())
		{
			ToolSetups.Add(new ComboboxEntry<int>($"{item.Number} {item.Desc}", item.Number));
		}
		ToolSetups.ResumeNotifications();
	}

	private void RefreshBendList(object? sender, NotifyCollectionChangedEventArgs e)
	{
		RefreshBendList();
	}

	public void RefreshBendList()
	{
		SetBends(_bendSequenceListViewModel.Bends.Where((BendSequenceListViewModel.BendViewModel x) => x.IsSelectedByBinding));
	}

	public void SetBends(IEnumerable<BendSequenceListViewModel.BendViewModel> bends)
	{
		SelectedBends = bends.ToList();
		UpdateUi();
	}

	public void CommitChanges()
	{
		ToolSetupNo.Commit();
		Comment.Commit();
		UserStepChangeMode.Commit();
		FlipGeometry.Commit();
		AcbLaser.Commit();
		LiftingAidLeftFront.Commit();
		LiftingAidLeftBack.Commit();
		LiftingAidRightFront.Commit();
		LiftingAidRightBack.Commit();
		Radius.Commit();
		BendDeduction.Commit();
		UpperToolProfile.Commit();
		LowerToolProfile.Commit();
		CommitToolProfiles();
		Close();
	}

	private void UpdateUi()
	{
		SelectedBendNumbers = string.Join(", ", CalculateSelectedBendNumbers());
		ToolSetupNo.UpdateUi();
		Comment.UpdateUi();
		UserStepChangeMode.UpdateUi();
		FlipGeometry.UpdateUi();
		AcbLaser.UpdateUi();
		LiftingAidLeftFront.UpdateUi();
		LiftingAidLeftBack.UpdateUi();
		LiftingAidRightFront.UpdateUi();
		LiftingAidRightBack.UpdateUi();
		Radius.UpdateUi();
		BendDeduction.UpdateUi();
		UpperToolProfile.UpdateUi();
		LowerToolProfile.UpdateUi();
		UpdateToolProfiles();
		UpdateSplitAngle();
		NotifyAll();
	}

	private void NotifyAll()
	{
		NotifyPropertyChanged("SelectedBendNumbers");
	}

	private void CommitToolProfiles()
	{
		if (SelectedUpperToolGoal != _mixedToolProfile)
		{
			IPunchProfile punchProfile = SelectedUpperToolGoal?.Profile as IPunchProfile;
			foreach (BendSequenceListViewModel.BendViewModel selectedBend in SelectedBends)
			{
				IBendPositioning bend = selectedBend.Bend;
				ICombinedBendDescriptorInternal cbd = selectedBend.Cbd;
				bend.PunchProfileByUser = punchProfile;
				int? userForcedUpperToolProfile = punchProfile?.ID;
				cbd.UserForcedUpperToolProfile = userForcedUpperToolProfile;
			}
		}
		if (SelectedLowerToolGoal != _mixedToolProfile)
		{
			IDieProfile dieProfile = SelectedLowerToolGoal?.Profile as IDieProfile;
			foreach (BendSequenceListViewModel.BendViewModel selectedBend2 in SelectedBends)
			{
				IBendPositioning bend2 = selectedBend2.Bend;
				ICombinedBendDescriptorInternal cbd2 = selectedBend2.Cbd;
				bend2.DieProfileByUser = dieProfile;
				int? userForcedLowerToolProfile = dieProfile?.ID;
				cbd2.UserForcedLowerToolProfile = userForcedLowerToolProfile;
			}
		}
		foreach (BendSequenceListViewModel.BendViewModel selectedBend3 in SelectedBends)
		{
			IBendPositioning bend3 = selectedBend3.Bend;
			_toolOperator.AssignBendToLocalSections(bend3, _doc.BendMachine.ToolConfig.ProfileSelector);
		}
	}

	private void UpdateToolProfiles()
	{
		List<IGrouping<IPunchProfile, BendSequenceListViewModel.BendViewModel>> list = (from x in SelectedBends
			group x by x.Bend.PunchProfileByUser).ToList();
		if (list.Count > 1)
		{
			SelectedUpperToolGoal = _mixedToolProfile;
		}
		else
		{
			IPunchProfile profile = list.FirstOrDefault()?.Key;
			if (profile == null)
			{
				SelectedUpperToolGoal = _unassignToolProfile;
			}
			else
			{
				SelectedUpperToolGoal = AllUpperTools.FirstOrDefault((ToolProfileVm x) => x.Profile == profile) ?? _mixedToolProfile;
			}
		}
		List<IGrouping<IDieProfile, BendSequenceListViewModel.BendViewModel>> list2 = (from x in SelectedBends
			group x by x.Bend.DieProfileByUser).ToList();
		if (list2.Count > 1)
		{
			SelectedLowerToolGoal = _mixedToolProfile;
		}
		else
		{
			IDieProfile profile2 = list2.FirstOrDefault()?.Key;
			if (profile2 == null)
			{
				SelectedLowerToolGoal = _unassignToolProfile;
			}
			else
			{
				SelectedLowerToolGoal = AllLowerTools.FirstOrDefault((ToolProfileVm x) => x.Profile == profile2) ?? _mixedToolProfile;
			}
		}
		NotifyPropertyChanged("SelectedUpperToolGoal");
		NotifyPropertyChanged("SelectedLowerToolGoal");
	}

	private void UpdateSplitAngle()
	{
		List<IGrouping<ICombinedBendDescriptor, ICombinedBendDescriptorInternal>> source = (from x in SelectedBends
			select x.Cbd into x
			group x by x.SplitPredecessors?.FirstOrDefault() ?? x).ToList();
		if (source.Count() == 1)
		{
			ICombinedBendDescriptor key = source.First().Key;
			key.ToIEnumerable().Concat(key?.SplitSuccessors ?? Array.Empty<ICombinedBendDescriptor>()).ToList();
		}
	}

	[IteratorStateMachine(typeof(_003CCalculateSelectedBendNumbers_003Ed__121))]
	private IEnumerable<string> CalculateSelectedBendNumbers()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCalculateSelectedBendNumbers_003Ed__121(-2)
		{
			_003C_003E4__this = this
		};
	}

	private void CloseCancel()
	{
		Close();
	}

	public override bool Close()
	{
		_bendSequenceListViewModel.Bends.CollectionChanged -= RefreshBendList;
		_bendSelection.SelectionChanged -= RefreshBendList;
		return base.Close();
	}

	private void SplitAngles()
	{
		foreach (int item in (from x in SelectedBends
			select x.Cbd.Order into x
			orderby x descending
			select x).ToList())
		{
			_doc.SplitBend(item, 0.5);
		}
	}

	private void SplitCombined()
	{
		foreach (BendSequenceListViewModel.BendViewModel selectedBend in SelectedBends)
		{
			_ = selectedBend;
		}
	}

	private void StepBendSubMenu()
	{
		_dockingService.Show<BendContextStepBendViewModel>();
	}
}
