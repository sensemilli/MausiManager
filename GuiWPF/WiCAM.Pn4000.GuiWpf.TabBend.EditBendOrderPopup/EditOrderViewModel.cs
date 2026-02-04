using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BendDataSourceModel.DeepCopy;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Services.PaintTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.MachineAndTools.Implementations;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards.Contents;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.WpfControls.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;
using WiCAM.Services.ConfigProviders.Contracts.DataType;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditBendOrderPopup;

internal class EditOrderViewModel : PopupViewModelBase, IEditOrderViewModel, INotifyPropertyChanged
{
	public enum BendViewOptions
	{
		Flat,
		Bent,
		Progress
	}

	private readonly IUnitConverter _unitConverter;

	private readonly ITranslator _translator;

	private readonly IStyleProvider _styleProvider;

	private readonly IBendSequenceStrategyFactory _bendSequenceStrategyFactory;

	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly IMainWindowDataProvider _mainWindowDataProvider;

	private readonly IConfigProvider _configProvider;

	private readonly IPnBndDoc _originalDoc;

	private readonly Dictionary<Tuple<int, int, int>, string> _bendDescriptorIdMap = new Dictionary<Tuple<int, int, int>, string>();

	private readonly Dictionary<IBendDescriptor, IBillboard> _billboards = new Dictionary<IBendDescriptor, IBillboard>();

	private readonly Dictionary<IBendDescriptor, string> _billboardConstantLabels = new Dictionary<IBendDescriptor, string>();

	private readonly ReplayReorderBuffer _replayReorderBuffer = new ReplayReorderBuffer();

	private Action? _refresh;

	private ModelColors3DConfig _modelColors3D;

	private BendViewOptions _selectedOption = BendViewOptions.Progress;

	private int _currentBentProgress;

	private IPnBndDoc _doc;

	private IBendSequenceOrder? _bendOrderStrategy;

	private bool _isManualMode;

	private Screen3D? _screen;

	private bool _groupParallesInAutoSort;

	public ICommand CmdAutoSortDo { get; set; }

	public ICommand CmdAutoSortAdd { get; set; }

	public ICommand CmdAutoSortAddGroup { get; set; }

	public ICommand CmdDeleteItem { get; set; }

	public ICommand CmdSequenceSave { get; set; }

	public ICommand CmdUndo { get; set; }

	public ICommand CmdSequenceSaveTruncated { get; set; }

	public ICommand CmdSequenceCancel { get; set; }

	public ICommand CmdStartNewBendSequence { get; set; }

	public BendViewOptions SelectedOption
	{
		get
		{
			return _selectedOption;
		}
		set
		{
			if (_selectedOption != value)
			{
				_selectedOption = value;
				SyncWithModel();
			}
		}
	}

	public int CurrentBentProgress
	{
		get
		{
			return _currentBentProgress;
		}
		set
		{
			_currentBentProgress = value;
			SyncWithModel();
		}
	}

	public IPnBndDoc Doc => _doc;

	public IBendSequenceOrder BendOrderStrategy
	{
		get
		{
			return _bendOrderStrategy;
		}
		set
		{
			if (value == BendOrderStrategies[0])
			{
				_bendOrderStrategy = value;
				OnPropertyChanged("BendOrderStrategy");
			}
			else
			{
				if (_bendOrderStrategy == value)
				{
					return;
				}
				_bendOrderStrategy = value;
				AutoSortList.CollectionChanged -= StrategyEditActivate;
				AutoSortListGroup.CollectionChanged -= StrategyEditActivate;
				AutoSortList.Clear();
				AutoSortListGroup.Clear();
				foreach (BendSequenceSorts sequence in _bendOrderStrategy.Sequences)
				{
					SortPrioVm sortPrioVm = new SortPrioVm(sequence, _translator);
					sortPrioVm.PropertyChanged += StrategyEditActivate;
					AutoSortList.Add(sortPrioVm);
				}
				_groupParallesInAutoSort = _bendOrderStrategy.Groupings.Any();
				if (_bendOrderStrategy.Groupings.Count == 1)
				{
					foreach (BendSequenceSorts innerSortSequence in _bendOrderStrategy.Groupings.Single().InnerSortSequences)
					{
						SortPrioVm sortPrioVm2 = new SortPrioVm(innerSortSequence, _translator);
						sortPrioVm2.PropertyChanged += StrategyEditActivate;
						AutoSortListGroup.Add(sortPrioVm2);
					}
				}
				OnPropertyChanged("AutoSortList");
				OnPropertyChanged("AutoSortListGroup");
				OnPropertyChanged("GroupParallesInAutoSort");
				OnPropertyChanged("BendOrderStrategy");
				AutoSortList.CollectionChanged += StrategyEditActivate;
				AutoSortListGroup.CollectionChanged += StrategyEditActivate;
			}
		}
	}

	public bool IsManualMode
	{
		get
		{
			return _isManualMode;
		}
		set
		{
			if (_isManualMode != value)
			{
				_isManualMode = value;
				OnPropertyChanged("IsManualMode");
				SyncWithModel();
			}
		}
	}

	public Screen3D? Screen
	{
		get
		{
			return _screen;
		}
		set
		{
			if (value != Screen)
			{
				_screen = value;
				OnPropertyChanged("Screen");
			}
		}
	}

	public bool GroupParallesInAutoSort
	{
		get
		{
			return _groupParallesInAutoSort;
		}
		set
		{
			if (value != _groupParallesInAutoSort)
			{
				_groupParallesInAutoSort = value;
				StrategyEditActivate(null, null);
				OnPropertyChanged("GroupParallesInAutoSort");
			}
		}
	}

	public ObservableCollection<KeyValuePair<BendViewOptions, string>> Options { get; set; } = new ObservableCollection<KeyValuePair<BendViewOptions, string>>();

	public ObservableCollection<ISortPrioVm> AutoSortList { get; set; } = new ObservableCollection<ISortPrioVm>();

	public ObservableCollection<ISortPrioVm> AutoSortListGroup { get; set; } = new ObservableCollection<ISortPrioVm>();

	public List<ISortPrioVm> AutoSortListTranslation { get; set; } = new List<ISortPrioVm>();

	public ISortPrioVm? SelectedNewTranslation { get; set; }

	public ISortPrioVm? SelectedNewGroupTranslation { get; set; }

	public ObservableCollection<IBendSequenceOrder> BendOrderStrategies { get; } = new ObservableCollection<IBendSequenceOrder>();

	public ObservableCollection<EditOrderListItemViewModel> Items { get; } = new ObservableCollection<EditOrderListItemViewModel>();

	public bool HoveredModel
	{
		get
		{
			return Items.All((EditOrderListItemViewModel x) => x.HoveredModel);
		}
		set
		{
			foreach (EditOrderListItemViewModel item in Items)
			{
				item.HoveredModel = value;
			}
		}
	}

	public bool HoveredBillboards
	{
		get
		{
			return Items.All((EditOrderListItemViewModel x) => x.HoveredBillboards);
		}
		set
		{
			foreach (EditOrderListItemViewModel item in Items)
			{
				item.HoveredBillboards = value;
			}
		}
	}

	private ScreenD3D11? Screen3d => _screen.ScreenD3D;

	public new event PropertyChangedEventHandler? PropertyChanged;

	private void StrategyEditActivate(object? sender, object e)
	{
		BendOrderStrategy = BendOrderStrategies[0];
	}

	public EditOrderViewModel(IPnBndDoc doc, IUnitConverter unitConverter, ITranslator translator, IStyleProvider styleProvider, IBendSequenceStrategyFactory bendSequenceStrategyFactory, IMainWindowBlock mainWindowBlock, IMainWindowDataProvider mainWindowDataProvider, IConfigProvider configProvider)
	{
		_originalDoc = doc;
		_unitConverter = unitConverter;
		_translator = translator;
		_styleProvider = styleProvider;
		_bendSequenceStrategyFactory = bendSequenceStrategyFactory;
		_mainWindowBlock = mainWindowBlock;
		_mainWindowDataProvider = mainWindowDataProvider;
		_configProvider = configProvider;
		CmdStartNewBendSequence = new RelayCommand(StartNewBendSequence);
		CmdAutoSortDo = new RelayCommand(AutoSortDo);
		CmdAutoSortAdd = new RelayCommand(AutoSortAdd);
		CmdAutoSortAddGroup = new RelayCommand(AutoSortAddGroup);
		CmdDeleteItem = new RelayCommand(DeleteItem);
		CmdUndo = new RelayCommand(Undo);
		CmdSequenceSave = new RelayCommand(SequenceSave);
		CmdSequenceSaveTruncated = new RelayCommand(SequenceSaveTruncated);
		CmdSequenceCancel = new RelayCommand(SequenceCancel);
	}

	public void Init(Action refresh)
	{
		if (!_mainWindowBlock.BlockUI_IsBlock(_originalDoc))
		{
			_mainWindowBlock.BlockUI_Block();
			_refresh = refresh;
			Screen = new Screen3D();
			if (_screen != null)
			{
				_screen.Loaded += OnScreenOnLoaded;
			}
		}
	}

	private void OnScreenOnLoaded(object sender, RoutedEventArgs e)
	{
		if (_screen != null)
		{
			_screen.Loaded -= OnScreenOnLoaded;
		}
		_modelColors3D = _configProvider.InjectOrCreate<ModelColors3DConfig>();
		InitAutoSort();
		InitDocCopy();
		foreach (BendViewOptions item in System.Enum.GetValues(typeof(BendViewOptions)).Cast<BendViewOptions>())
		{
			string msgKey = "l_popup.BendView.EditOrder." + item;
			Options.Add(new KeyValuePair<BendViewOptions, string>(item, _translator.Translate(msgKey)));
		}
	}

	private void InitAutoSort()
	{
		List<ISortPrioVm> list = new List<ISortPrioVm>();
		foreach (object value2 in System.Enum.GetValues(typeof(BendSequenceSorts)))
		{
			if (value2 is BendSequenceSorts value)
			{
				list.Add(new SortPrioVm(value, _translator));
			}
		}
		AutoSortListTranslation.AddRange(list);
		SelectedNewTranslation = AutoSortListTranslation.FirstOrDefault();
		SelectedNewGroupTranslation = AutoSortListTranslation.FirstOrDefault();
		AutoSortList.Clear();
		AutoSortListGroup.Clear();
		OnPropertyChanged("AutoSortListTranslation");
		OnPropertyChanged("SelectedNewTranslation");
		OnPropertyChanged("SelectedNewGroupTranslation");
	}

	private void InitDocCopy()
	{
		_doc = _originalDoc.Copy(ModelCopyMode.Reference, ModelCopyMode.Copy, ModelCopyMode.Copy, ModelCopyMode.Reference);
		_replayReorderBuffer.Clear();
		BendOrderStrategies.Clear();
		BendOrderStrategies.Add(new BendSequenceOrder("...", enabled: false, Guid.NewGuid()));
		ObservableCollection<IBendSequenceOrder> bendOrderStrategies = BendOrderStrategies;
		object obj = _originalDoc.BendMachine?.ToolCalculationSettings.BendOrderStrategies;
		if (obj == null)
		{
			int num = 1;
			obj = new List<IBendSequenceOrder>(num);
			CollectionsMarshal.SetCount((List<IBendSequenceOrder>)obj, num);
			Span<IBendSequenceOrder> span = CollectionsMarshal.AsSpan((List<IBendSequenceOrder>?)obj);
			int index = 0;
			ref IBendSequenceOrder reference = ref span[index];
			BendSequenceOrder bendSequenceOrder = new BendSequenceOrder(_translator.Translate("l_popup.BendView.EditOrder.Default"), enabled: false, Guid.NewGuid());
			int num2 = 3;
			List<BendSequenceSorts> list = new List<BendSequenceSorts>(num2);
			CollectionsMarshal.SetCount(list, num2);
			Span<BendSequenceSorts> span2 = CollectionsMarshal.AsSpan(list);
			int num3 = 0;
			span2[num3] = BendSequenceSorts.CommonBendsFirst;
			num3++;
			span2[num3] = BendSequenceSorts.InToOutMainFace;
			num3++;
			span2[num3] = BendSequenceSorts.LongToShortWithoutGaps;
			bendSequenceOrder.Sequences = list;
			reference = bendSequenceOrder;
		}
		bendOrderStrategies.AddRange((IEnumerable<IBendSequenceOrder>)obj);
		BendOrderStrategy = BendOrderStrategies[1];
		_screen?.SetConfigProviderAndApplySettings(_configProvider);
		_screen?.ShowNavigation(show: true);
		Screen3d?.RemoveModel(null);
		Screen3d?.RemoveBillboard(null);
		Matrix4d viewDirection = Matrix4d.RotationX(1.5707963705062866);
		Screen3d?.AddModel(Doc.BendModel3D, render: false);
		Screen3d?.SetViewDirectionByMatrix4d(viewDirection);
		Screen3d?.ZoomExtend();
		if (_screen != null)
		{
			_screen.TriangleSelected += Screen3D_TriangleSelected;
			_screen.MouseEnterTriangle += Screen3D_MouseEnterTriangle;
		}
		TextStyle textStyle = _styleProvider.BillboardTextStyle.Copy();
		_bendDescriptorIdMap.Clear();
		foreach (var item5 in (from x in Doc.CombinedBendDescriptors.SelectMany((ICombinedBendDescriptorInternal x) => x.Enumerable, (ICombinedBendDescriptorInternal a, IBendDescriptor b) => (a: a, b: b))
			group x by x.b into x
			orderby x.Key.BendParams.BendFaceGroup.BendEntryId, x.Key.BendParams.BendFaceGroup.ID
			select x).Select((IGrouping<IBendDescriptor, (ICombinedBendDescriptorInternal a, IBendDescriptor b)> y, int i) => (y: y, i: i)))
		{
			IGrouping<IBendDescriptor, (ICombinedBendDescriptorInternal a, IBendDescriptor b)> bendDescriptorGroup = item5.y;
			int item = item5.i;
			List<string> list2 = new List<string>();
			ButtonBillboard buttonBillboard = new ButtonBillboard(delegate(IButtonBillboard x)
			{
				Screen3d.UpdateBillboardAppearance(x);
			});
			foreach (var item6 in bendDescriptorGroup.Select(((ICombinedBendDescriptorInternal a, IBendDescriptor b) x, int i) => (x: x, i: i)))
			{
				(ICombinedBendDescriptorInternal a, IBendDescriptor b) item2 = item6.x;
				Tuple<int, int, int> tuple = AtomicBendDescriptor.DeriveKey(combinedBendDescriptor: item2.a, bd: item2.b, cbds: _doc.CombinedBendDescriptors);
				string value = item.NumberToAlphabet() + ((bendDescriptorGroup.Count() == 1) ? "" : ((object)(tuple.Item3 + 1)));
				list2.Add(item.NumberToAlphabet());
				_bendDescriptorIdMap.Add(tuple, value);
			}
			string text = list2.First();
			Dictionary<IBendDescriptor, string> billboardConstantLabels = _billboardConstantLabels;
			IBendDescriptor key = bendDescriptorGroup.Key;
			string reference2 = text;
			billboardConstantLabels.Add(key, string.Join(",", new ReadOnlySpan<string>(in reference2)));
			buttonBillboard.Center = bendDescriptorGroup.Key.BendParams.BendFaceGroup.GetCenterPointInModelSpace();
			buttonBillboard.Content = new TextContent
			{
				PlainText = text,
				Background = _styleProvider.BillboardBackgroundStyle,
				TextStyle = textStyle
			};
			buttonBillboard.OnClick += delegate(IPnInputEventArgs e, IBillboard b)
			{
				BillboardOnOnClick(e, b, bendDescriptorGroup.Key.BendParams.BendFaceGroupModel);
			};
			buttonBillboard.OnMouseEnter += delegate(IPnInputEventArgs e, IBillboard b)
			{
				BillboardOnEnter(e, b, bendDescriptorGroup.Key.BendParams.BendFaceGroupModel);
			};
			buttonBillboard.OnMouseLeave += delegate(IPnInputEventArgs e, IBillboard b)
			{
				BillboardOnLeave(e, b, bendDescriptorGroup.Key.BendParams.BendFaceGroupModel);
			};
			_billboards[bendDescriptorGroup.Key] = buttonBillboard;
			Screen3d.AddBillboard(buttonBillboard, bendDescriptorGroup.Key.BendParams.BendFaceGroupModel);
		}
		Screen3d?.Render(skipQueuedFrames: true);
		SyncWithModel();
	}

	public bool Close()
	{
		Screen3d?.Dispose();
		CloseView();
		_mainWindowDataProvider.SetViewForConfig(null);
		_mainWindowBlock.BlockUI_Unblock();
		return true;
	}

	private void StartNewBendSequence()
	{
		CurrentBentProgress = 0;
		_replayReorderBuffer.Clear();
	}

	private void SequenceCancel()
	{
		Close();
	}

	private void SequenceSaveTruncated()
	{
		_originalDoc.ApplyBendSequence(_doc);
		foreach (ICombinedBendDescriptorInternal item in _originalDoc.CombinedBendDescriptors.Skip(CurrentBentProgress))
		{
			item.IsIncluded = false;
		}
		_originalDoc.FreezeCombinedBendDescriptors = true;
		_refresh?.Invoke();
		Close();
	}

	private void Undo()
	{
		Undo(sync: true);
	}

	public void Undo(bool sync)
	{
		int currentBentProgress;
		bool flag = _replayReorderBuffer.Undo(_doc, out currentBentProgress);
		if (sync && flag)
		{
			_currentBentProgress = currentBentProgress;
			SyncWithModel();
		}
	}

	private void SequenceSave()
	{
		_originalDoc.ApplyBendSequence(_doc);
		_originalDoc.FreezeCombinedBendDescriptors = true;
		_refresh?.Invoke();
		Close();
	}

	private void DeleteItem(object obj)
	{
		if (obj is SortPrioVm item)
		{
			AutoSortList.Remove(item);
			AutoSortListGroup.Remove(item);
		}
	}

	private void AutoSortAdd()
	{
		ISortPrioVm? selectedNewTranslation = SelectedNewTranslation;
		if (selectedNewTranslation != null && selectedNewTranslation.SortType.HasValue)
		{
			AutoSortList.Add(new SortPrioVm(SelectedNewTranslation.SortType.Value, _translator));
		}
	}

	private void AutoSortAddGroup()
	{
		ISortPrioVm? selectedNewGroupTranslation = SelectedNewGroupTranslation;
		if (selectedNewGroupTranslation != null && selectedNewGroupTranslation.SortType.HasValue)
		{
			AutoSortListGroup.Add(new SortPrioVm(SelectedNewGroupTranslation.SortType.Value, _translator));
		}
	}

	private void AutoSortDo()
	{
		List<BendSequenceSorts> sortList = (from x in AutoSortList
			where x?.SortType.HasValue ?? false
			select x.SortType.Value).ToList();
		List<BendSequenceSorts> sortListGroup = (from x in AutoSortListGroup
			where x?.SortType.HasValue ?? false
			select x.SortType.Value).ToList();
		_replayReorderBuffer.AddAndDoLast(new ReplayReorderBuffer.StrategyOperation(sortList, GroupParallesInAutoSort, sortListGroup, _bendSequenceStrategyFactory), _doc, CurrentBentProgress);
		_replayReorderBuffer.Clear();
		CurrentBentProgress = 0;
		SyncWithModel();
	}

	private void SyncWithModel()
	{
		FaceGroupModelMapping faceGroupModelMapping = new FaceGroupModelMapping(Doc.BendModel3D);
		Items.Clear();
		Dictionary<IBendDescriptor, List<int>> dictionary = new Dictionary<IBendDescriptor, List<int>>();
		foreach (IBendDescriptor bd in Doc.BendDescriptors)
		{
			dictionary[bd] = new List<int>();
			bd.BendParams.BendFaceGroup.Unfold(Doc.Thickness, faceGroupModelMapping, (int _) => bd.BendParams.KFactor);
		}
		foreach (var item3 in Doc.CombinedBendDescriptors.Select((ICombinedBendDescriptorInternal x, int i) => (x: x, i: i)))
		{
			ICombinedBendDescriptorInternal item = item3.x;
			int item2 = item3.i;
			string orderUi = ((item2 < CurrentBentProgress) ? (item2 + 1).ToString() : "-");
			EditOrderListItemViewModel editOrderListItemViewModel = new EditOrderListItemViewModel(this, item2, orderUi, _modelColors3D);
			IBendDescriptor? bendDescriptor = item.Enumerable.FirstOrDefault();
			editOrderListItemViewModel.AngleNegative = bendDescriptor != null && bendDescriptor.BendParams.AngleSign < 0;
			string text = (editOrderListItemViewModel.AngleNegative ? "-" : "+");
			if (Math.Abs(item.BendAngleAbsStart) < 1E-06)
			{
				editOrderListItemViewModel.AngleChange = text + _unitConverter.Angle.ToUiUnit(item.BendAngleAbsStop, 3);
			}
			else
			{
				editOrderListItemViewModel.AngleChange = $"{text}{_unitConverter.Angle.ToUiUnit(item.BendAngleAbsStart, 3)} -> {text}{_unitConverter.Angle.ToUiUnit(item.BendAngleAbsStop, 3)}";
			}
			editOrderListItemViewModel.LengthTotalWithoutGaps = $"{_unitConverter.Length.ToUi(item.TotalLengthWithoutGaps, 1)}";
			editOrderListItemViewModel.LengthTotalWithGaps = $"{_unitConverter.Length.ToUi(item.TotalLength, 1)}";
			editOrderListItemViewModel.BendZones = new ObservableCollection<EditOrderInnerListItemViewModel>();
			editOrderListItemViewModel.CanMerge = IsManualMode && Doc.CanMergeWithNext(item);
			foreach (IBendDescriptor bd2 in item.Enumerable)
			{
				dictionary[bd2].Add((item2 < CurrentBentProgress) ? item2 : (-1));
				Model model = faceGroupModelMapping.GetModel(bd2.BendParams.BendFaceGroup);
				EditOrderInnerListItemViewModel editOrderInnerListItemViewModel = new EditOrderInnerListItemViewModel(_modelColors3D);
				Tuple<int, int, int> key = AtomicBendDescriptor.DeriveKey(_doc.CombinedBendDescriptors, item, bd2);
				editOrderInnerListItemViewModel.Id = _bendDescriptorIdMap[key];
				editOrderInnerListItemViewModel.Model = bd2.BendParams.BendFaceGroupModel;
				editOrderInnerListItemViewModel.ViewModel = this;
				editOrderInnerListItemViewModel.OuterViewModel = editOrderListItemViewModel;
				editOrderListItemViewModel.BendZones.Add(editOrderInnerListItemViewModel);
				switch (SelectedOption)
				{
				case BendViewOptions.Flat:
					editOrderInnerListItemViewModel.Bent = false;
					break;
				case BendViewOptions.Bent:
					editOrderInnerListItemViewModel.Bent = true;
					break;
				case BendViewOptions.Progress:
					editOrderInnerListItemViewModel.Bent = item2 < CurrentBentProgress;
					break;
				}
				double progressStop = item.ProgressStop;
				if (editOrderInnerListItemViewModel.Bent)
				{
					bd2.BendParams.BendFaceGroup.Unfold(Doc.Thickness, faceGroupModelMapping, (int _) => bd2.BendParams.KFactor, progressStop);
				}
				Screen3d?.UpdateModelGeometry(model);
			}
			editOrderListItemViewModel.CurrentProgressColor = ((item2 == CurrentBentProgress - 1) ? new SolidColorBrush(Colors.Orange) : null);
			Items.Add(editOrderListItemViewModel);
		}
		foreach (IBendDescriptor bendDescriptor2 in Doc.BendDescriptors)
		{
			IEnumerable<string> values = dictionary[bendDescriptor2].Select((int x) => (x < 0) ? "-" : (x + 1).ToString());
			string text2 = string.Join(',', values);
			IBillboard billboard = _billboards[bendDescriptor2];
			TextContent obj = (TextContent)billboard.Content;
			string plainText = text2 + " " + _billboardConstantLabels[bendDescriptor2];
			obj.PlainText = plainText;
			Screen3d?.UpdateBillboardAppearance(billboard, render: false);
		}
		foreach (Model item4 in Doc.BendModel3D.GetAllSubModelsWithSelf())
		{
			Screen3d.UpdateModelTransform(item4);
		}
		ClearChangeStatus();
		ColorModelParts();
		Screen3d?.Render(skipQueuedFrames: true);
	}

	private void Screen3D_TriangleSelected(IScreen3D sender, ITriangleEventArgs e)
	{
		if (e != null && e.MouseEventArgs?.ChangedButton == MouseButton.Right)
		{
			CurrentBentProgress = Math.Max(CurrentBentProgress - 1, 0);
			e.Args.Handle();
		}
		else if (e?.Model != null)
		{
			Model model = e.Model;
			if (!HandleBendReorderInput(model))
			{
				e.Args.Handle();
			}
		}
	}

	private void BillboardOnOnClick(IPnInputEventArgs e, IBillboard billboard, Model model)
	{
		if (e != null && e.MouseButtonEventArgs?.ChangedButton == MouseButton.Right)
		{
			CurrentBentProgress = Math.Max(CurrentBentProgress - 1, 0);
			e.Handle();
		}
		else if (!HandleBendReorderInput(model))
		{
			e.Handle();
		}
	}

	private bool HandleBendReorderInput(Model model)
	{
		bool result = true;
		InteractWithBend(model, delegate(List<ICombinedBendDescriptorInternal> cbds)
		{
			if (cbds.Any())
			{
				IReadOnlyList<ICombinedBendDescriptorInternal> combinedBendDescriptors = Doc.CombinedBendDescriptors;
				cbds = cbds.OrderBy((ICombinedBendDescriptorInternal x) => _doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(x)).ToList();
				ICombinedBendDescriptorInternal item = cbds.First();
				List<Tuple<int, int, int>> oldKeys = item.Enumerable.Select((IBendDescriptor x) => AtomicBendDescriptor.DeriveKey(_doc.CombinedBendDescriptors, item, x)).ToList();
				List<int> list = new List<int>();
				for (int i = 0; i < CurrentBentProgress; i++)
				{
					list.Add(i);
				}
				for (int j = 0; j < cbds.Count; j++)
				{
					list.Add(combinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(cbds[j]));
				}
				for (int k = CurrentBentProgress; k < combinedBendDescriptors.Count; k++)
				{
					if (!cbds.Contains(combinedBendDescriptors[k]))
					{
						list.Add(k);
					}
				}
				TryPermuteBends(list, IsManualMode);
				ICombinedBendDescriptorInternal elementToFind = Doc.CombinedBendDescriptors.First((ICombinedBendDescriptorInternal x) => x.Enumerable.Any((IBendDescriptor y) => oldKeys.Contains(AtomicBendDescriptor.DeriveKey(_doc.CombinedBendDescriptors, x, y))));
				int num = Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(elementToFind);
				CurrentBentProgress = num + 1;
				SyncWithModel();
				result = false;
			}
		});
		return result;
	}

	private void Screen3D_MouseEnterTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		if (Items.SelectMany((EditOrderListItemViewModel x) => x.BendZones).Any((EditOrderInnerListItemViewModel x) => x.HoveredBillboard))
		{
			return;
		}
		HoveredModel = false;
		ClearChangeStatus();
		InteractWithBend(e.Model, delegate(List<ICombinedBendDescriptorInternal> cbds)
		{
			if (cbds.Any())
			{
				foreach (ICombinedBendDescriptorInternal cbd in cbds)
				{
					Items[Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(cbd)].HoveredModel = true;
				}
				SimulateReorderOnHover(e.Model, cbds, rollback: true);
				e.Args.Handle();
			}
		});
	}

	private void BillboardOnEnter(IPnInputEventArgs e, IBillboard billboard, Model model)
	{
		HoveredBillboards = false;
		ClearChangeStatus();
		InteractWithBend(model, delegate(List<ICombinedBendDescriptorInternal> cbds)
		{
			if (cbds.Any())
			{
				foreach (ICombinedBendDescriptorInternal cbd in cbds)
				{
					Items[Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(cbd)].HoveredBillboards = true;
				}
				SimulateReorderOnHover(model, cbds, rollback: true);
				e.Handle();
			}
		});
	}

	private void SimulateReorderOnHover(Model model, List<ICombinedBendDescriptorInternal> cbds, bool rollback)
	{
		IReadOnlyList<ICombinedBendDescriptorInternal> combinedBendDescriptors = Doc.CombinedBendDescriptors;
		cbds = cbds.OrderBy((ICombinedBendDescriptorInternal x) => _doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(x)).ToList();
		ICombinedBendDescriptorInternal item = cbds.First();
		List<Tuple<int, int, int>> oldKeys = item.Enumerable.Select((IBendDescriptor x) => AtomicBendDescriptor.DeriveKey(_doc.CombinedBendDescriptors, item, x)).ToList();
		List<int> list = new List<int>();
		for (int i = 0; i < CurrentBentProgress; i++)
		{
			list.Add(i);
		}
		for (int j = 0; j < cbds.Count; j++)
		{
			list.Add(combinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(cbds[j]));
		}
		for (int k = CurrentBentProgress; k < combinedBendDescriptors.Count; k++)
		{
			if (!cbds.Contains(combinedBendDescriptors[k]))
			{
				list.Add(k);
			}
		}
		int currentBentProgress = CurrentBentProgress;
		if (!TryPermuteBends(list, IsManualMode))
		{
			return;
		}
		ICombinedBendDescriptorInternal elementToFind = Doc.CombinedBendDescriptors.First((ICombinedBendDescriptorInternal x) => x.Enumerable.Any((IBendDescriptor y) => oldKeys.Contains(AtomicBendDescriptor.DeriveKey(_doc.CombinedBendDescriptors, x, y))));
		int num = Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(elementToFind);
		_currentBentProgress = num + 1;
		if (!rollback)
		{
			SyncWithModel();
			return;
		}
		List<List<Tuple<int, int, int>>> newKeys = AtomicBendDescriptor.DeriveAllKeys(_doc.CombinedBendDescriptors).ToList();
		Undo(sync: false);
		SetCurrentBentProgress(currentBentProgress);
		ShowChangingCombinedBendDescriptors(newKeys);
	}

	private void InteractWithBend(Model model, Action<List<ICombinedBendDescriptorInternal>> action)
	{
		if (model == null)
		{
			action?.Invoke(new List<ICombinedBendDescriptorInternal>());
			ColorModelParts();
			return;
		}
		if (model.Shell?.RoundFaceGroups.FirstOrDefault()?.IsBendingZone != true)
		{
			action?.Invoke(new List<ICombinedBendDescriptorInternal>());
			ColorModelParts();
			return;
		}
		List<List<ICombinedBendDescriptorInternal>> source = Doc.GroupCompatibleBends(_currentBentProgress);
		List<ICombinedBendDescriptorInternal> list = (from x in Doc.CombinedBendDescriptors.Skip(CurrentBentProgress)
			where x.Enumerable.Any((IBendDescriptor y) => y.BendParams.BendFaceGroupModel == model)
			select x).ToList();
		if (list.Count == 0)
		{
			ColorModelParts();
			return;
		}
		ICombinedBendDescriptorInternal cb = list.First();
		list = (IsManualMode ? list.Take(1).ToList() : source.Single((List<ICombinedBendDescriptorInternal> x) => x.Contains(cb)));
		if (list.Count == 0)
		{
			ColorModelParts();
			return;
		}
		action?.Invoke(list);
		ColorModelParts();
	}

	private void BillboardOnLeave(IPnInputEventArgs e, IBillboard billboard, Model model)
	{
		e.Handle();
		HoveredBillboards = false;
		ColorModelParts();
	}

	public bool MoveBend(int oldIndex, int newIndex, bool sync = true)
	{
		bool result = TryMoveBends(oldIndex, newIndex, IsManualMode);
		if (sync)
		{
			SyncWithModel();
		}
		return result;
	}

	private bool TryMoveBends(int oldIndex, int newIndex, bool isManualMode)
	{
		ICombinedBendDescriptorInternal item = Doc.CombinedBendDescriptors[oldIndex];
		List<Tuple<int, int, int>> oldKeys = item.Enumerable.Select((IBendDescriptor x) => AtomicBendDescriptor.DeriveKey(_doc.CombinedBendDescriptors, item, x)).ToList();
		List<int> list = Enumerable.Range(0, Doc.CombinedBendDescriptors.Count).ToList();
		list.RemoveAt(oldIndex);
		list.Insert(newIndex, oldIndex);
		if (!TryPermuteBends(list, isManualMode))
		{
			return false;
		}
		ICombinedBendDescriptorInternal elementToFind = Doc.CombinedBendDescriptors.First((ICombinedBendDescriptorInternal x) => x.Enumerable.Any((IBendDescriptor y) => oldKeys.Contains(AtomicBendDescriptor.DeriveKey(_doc.CombinedBendDescriptors, x, y))));
		int num = Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(elementToFind);
		_currentBentProgress = num + 1;
		return true;
	}

	private bool TryPermuteBends(List<int> permutation, bool isManualMode)
	{
		if (!_replayReorderBuffer.AddAndDoLast(new ReplayReorderBuffer.PermutationOperation(permutation, !isManualMode), Doc, CurrentBentProgress))
		{
			return false;
		}
		return true;
	}

	private void ColorModelParts()
	{
		PaintTool paintTool = new PaintTool();
		paintTool.FrameStart();
		foreach (Model model in Doc.BendModel3D.GetAllSubModelsWithSelf())
		{
			if (Items.SelectMany((EditOrderListItemViewModel x) => x.BendZones).All((EditOrderInnerListItemViewModel x) => x.Model != model))
			{
				continue;
			}
			bool flag = Items.SelectMany((EditOrderListItemViewModel x) => x.BendZones).Any((EditOrderInnerListItemViewModel x) => x.Highlight && x.Model == model);
			bool flag2 = Items.Skip(CurrentBentProgress).SelectMany((EditOrderListItemViewModel x) => x.BendZones).Any((EditOrderInnerListItemViewModel x) => x.Model == model);
			bool flag3 = Items.Where((EditOrderListItemViewModel x) => x.AngleNegative).SelectMany((EditOrderListItemViewModel x) => x.BendZones).Any((EditOrderInnerListItemViewModel x) => x.Model == model);
			if (!flag && !flag2)
			{
				paintTool.SetModelColors(model, _modelColors3D.EditOrderFullyBentColor.ToBendColor(), null, null);
			}
			else if (!flag && flag2)
			{
				if (flag3)
				{
					paintTool.SetModelColors(model, _modelColors3D.EditOrderReverseColor.ToBendColor(), null, null);
				}
				else
				{
					paintTool.SetModelColors(model, _modelColors3D.EditOrderDefaultColor.ToBendColor(), null, null);
				}
			}
			else if (flag && !flag2)
			{
				paintTool.SetModelColors(model, _modelColors3D.EditOrderHoverAndFullyBentColor.ToBendColor(), null, null);
			}
			else
			{
				paintTool.SetModelColors(model, _modelColors3D.EditOrderHoverColor.ToBendColor(), null, null);
			}
		}
		paintTool.FrameApply(out HashSet<Model> modifiedModels, out HashSet<Shell> modifiedShells);
		Screen3d.UpdateModelAppearance(ref modifiedModels, ref modifiedShells);
	}

	public void HighlightChanged()
	{
		ColorModelParts();
	}

	public void Split(EditOrderInnerListItemViewModel item)
	{
		if (!IsManualMode)
		{
			return;
		}
		int num = Items.IndexOf(item.OuterViewModel);
		ICombinedBendDescriptorInternal combinedBendDescriptorInternal = Doc.CombinedBendDescriptors[num];
		int num2 = item.OuterViewModel.BendZones.IndexOf(item);
		AtomicBendDescriptor.DeriveKey(_doc.CombinedBendDescriptors, combinedBendDescriptorInternal, combinedBendDescriptorInternal.Enumerable.ElementAt(num2));
		if (combinedBendDescriptorInternal.Enumerable.Count() < 2)
		{
			TryMoveBends(0, 1, IsManualMode);
			TryMoveBends(1, 0, IsManualMode);
			SyncWithModel();
			return;
		}
		List<int> list = new List<int>();
		int currentBentProgress;
		if (num2 == 0)
		{
			list.Add(1);
			currentBentProgress = num + 1;
		}
		else if (num2 == combinedBendDescriptorInternal.Enumerable.Count() - 1)
		{
			list.Add(num2);
			currentBentProgress = num + 2;
		}
		else
		{
			list.Add(num2);
			list.Add(num2 + 1);
			currentBentProgress = num + 1;
		}
		_replayReorderBuffer.AddAndDoLast(new ReplayReorderBuffer.SplitOperation(num, list), Doc, CurrentBentProgress);
		if (list.Count() == 2)
		{
			TryMoveBends(num, num + 1, IsManualMode);
			Merge(num + 1);
		}
		CurrentBentProgress = currentBentProgress;
		SyncWithModel();
	}

	public bool Merge(int order)
	{
		bool num = _replayReorderBuffer.AddAndDoLast(new ReplayReorderBuffer.MergeOperation(order), Doc, CurrentBentProgress);
		if (num)
		{
			SyncWithModel();
		}
		return num;
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
	}

	protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	public void SetCurrentBentProgress(int currentBentProgress)
	{
		_currentBentProgress = currentBentProgress;
	}

	public void ShowChangingCombinedBendDescriptors(List<List<Tuple<int, int, int>>> newKeys)
	{
		List<List<Tuple<int, int, int>>> source = AtomicBendDescriptor.DeriveAllKeys(Doc.CombinedBendDescriptors).ToList();
		for (int i = 0; i < Doc.CombinedBendDescriptors.Count; i++)
		{
			List<Tuple<int, int, int>> queryBend = source.ElementAt(i);
			IEnumerable<List<Tuple<int, int, int>>> source2 = newKeys.Where((List<Tuple<int, int, int>> x) => AtomicBendDescriptor.CompareKeys(x, queryBend) != AtomicBendDescriptor.SetRelation.Disjoint);
			source2.Select((List<Tuple<int, int, int>> x) => AtomicBendDescriptor.CompareKeys(x, queryBend)).First();
			if (source2.All((List<Tuple<int, int, int>> x) => AtomicBendDescriptor.KeysEqual(queryBend, x)))
			{
				Items[i].ChangeStatus = EditOrderListItemViewModel.Status.Unchanged;
			}
			else if (source2.All((List<Tuple<int, int, int>> x) => AtomicBendDescriptor.CompareKeys(queryBend, x) == AtomicBendDescriptor.SetRelation.Subset))
			{
				Items[i].ChangeStatus = EditOrderListItemViewModel.Status.Merged;
			}
			else if (source2.All((List<Tuple<int, int, int>> x) => AtomicBendDescriptor.CompareKeys(queryBend, x) == AtomicBendDescriptor.SetRelation.Superset))
			{
				Items[i].ChangeStatus = EditOrderListItemViewModel.Status.Split;
			}
			else
			{
				Items[i].ChangeStatus = EditOrderListItemViewModel.Status.Neither;
			}
		}
		PaintBillboards();
		ColorModelParts();
	}

	public void ClearChangeStatus()
	{
		foreach (EditOrderListItemViewModel item in Items)
		{
			item.ChangeStatus = EditOrderListItemViewModel.Status.Unchanged;
		}
		PaintBillboards();
		ColorModelParts();
	}

	private void PaintBillboards()
	{
		foreach (IBendDescriptor bd in Doc.BendDescriptors)
		{
			EditOrderListItemViewModel.Status status = EditOrderListItemViewModel.Status.Unchanged;
			if (Doc.CombinedBendDescriptors.Where((ICombinedBendDescriptorInternal x) => x.Enumerable.Contains(bd)).Any((ICombinedBendDescriptorInternal x) => Items[Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(x)].ChangeStatus == EditOrderListItemViewModel.Status.Neither))
			{
				status = EditOrderListItemViewModel.Status.Neither;
			}
			else if (Doc.CombinedBendDescriptors.Where((ICombinedBendDescriptorInternal x) => x.Enumerable.Contains(bd)).Any((ICombinedBendDescriptorInternal x) => Items[Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(x)].ChangeStatus == EditOrderListItemViewModel.Status.Split))
			{
				status = EditOrderListItemViewModel.Status.Split;
			}
			else if (Doc.CombinedBendDescriptors.Where((ICombinedBendDescriptorInternal x) => x.Enumerable.Contains(bd)).Any((ICombinedBendDescriptorInternal x) => Items[Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(x)].ChangeStatus == EditOrderListItemViewModel.Status.Merged))
			{
				status = EditOrderListItemViewModel.Status.Merged;
			}
			CfgColor cfgColor = null;
			switch (status)
			{
			case EditOrderListItemViewModel.Status.Merged:
				cfgColor = _modelColors3D.EditOrderMergeColor;
				break;
			case EditOrderListItemViewModel.Status.Split:
				cfgColor = _modelColors3D.EditOrderSplitColor;
				break;
			case EditOrderListItemViewModel.Status.Neither:
				cfgColor = _modelColors3D.EditOrderAmbiguousColor;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case EditOrderListItemViewModel.Status.Unchanged:
				break;
			}
			IBillboard billboard = _billboards[bd];
			TextContent textContent = (TextContent)billboard.Content;
			billboard.Content = new TextContent
			{
				PlainText = textContent.PlainText,
				Background = new BackgroundStyle
				{
					Color = textContent.Background.Color,
					BorderColor = (cfgColor?.ToBendColor() ?? WiCAM.Pn4000.BendModel.Base.Color.Black),
					BorderThickness = textContent.Background.BorderThickness,
					BorderRadius = textContent.Background.BorderRadius,
					IsCircular = textContent.Background.IsCircular,
					Padding = textContent.Background.Padding,
					MinWidth = textContent.Background.MinWidth,
					MinHeight = textContent.Background.MinHeight
				},
				TextStyle = textContent.TextStyle
			};
			Screen3d?.UpdateBillboardAppearance(billboard, render: false);
		}
	}
}
