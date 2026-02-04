using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiWpf.TabBend.OrderBillboards;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.TabBend.SequenceList;

public class BendSequenceSortViewModel : PopupViewModelBase, IBendSequenceSortViewModel, ISubViewModel
{
	public class SortPrioVm : ViewModelBase, IBendSequenceSortViewModel.ISortPrioVm
	{
		private readonly ITranslator _translator;

		private BendSequenceSorts? _sortType;

		public BendSequenceSorts? SortType
		{
			get
			{
				return _sortType;
			}
			set
			{
				if (_sortType != value)
				{
					_sortType = value;
					string text = "l_enum.BendSequence." + _sortType.ToString();
					Description = _translator.Translate(text);
					DescriptionLong = _translator.Translate(text + "_EXPLANATION");
					NotifyPropertyChanged("SortType");
					NotifyPropertyChanged("Description");
					NotifyPropertyChanged("DescriptionLong");
				}
			}
		}

		public string Description { get; private set; }

		public string DescriptionLong { get; private set; }

		public SortPrioVm(BendSequenceSorts? type = null, ITranslator translator = null)
		{
			_translator = translator;
			SortType = type;
		}
	}

	private IDoc3d _docOriginal;

	private BendSequenceItem? _selectedBend;

	private readonly IScreen3DMain _screen3D;

	private readonly ITranslator _translator;

	private readonly IOrderBillboardsViewModel _orderBillboardsViewModel;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly IBendSequenceStrategyFactory _bendSequenceStrategyFactory;

	private readonly IUndo3dService _undo3dService;

	private IBendSequenceSortViewModel.ISortPrioVm _selectedNewTranslation;

	private bool _groupParallesInAutoSort;

	public IDoc3d DocTemp { get; private set; }

	public ObservableCollection<BendSequenceItem> BendSequence { get; private set; }

	public UiModelType CurrentModel { get; private set; }

	public BendSequenceItem? SelectedBend
	{
		get
		{
			return _selectedBend;
		}
		set
		{
			if (_selectedBend != value)
			{
				_selectedBend = value;
				_orderBillboardsViewModel.SetMaxCombinedBend(_selectedBend?.CombinedBendDescriptor);
				this.OnSelectedBendChanged?.Invoke(_selectedBend);
			}
		}
	}

	public RelayCommand CmdSequenceSelectItem { get; }

	public RelayCommand CmdSequenceSave { get; }

	public RelayCommand CmdSequenceCancel { get; }

	public IBendSequenceSortViewModel.ISortPrioVm SelectedNewTranslation
	{
		get
		{
			return _selectedNewTranslation;
		}
		set
		{
			_selectedNewTranslation = value;
			OnPropertyChanged("SelectedNewTranslation");
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
			_groupParallesInAutoSort = value;
			OnPropertyChanged("GroupParallesInAutoSort");
		}
	}

	public List<IBendSequenceSortViewModel.ISortPrioVm> AutoSortListTranslation { get; set; }

	public ObservableCollection<IBendSequenceSortViewModel.ISortPrioVm> AutoSortList { get; set; }

	public RelayCommand CmdAutoSortAdd { get; set; }

	public RelayCommand CmdAutoSortDo { get; set; }

	public RelayCommand CmdAutoSortClear { get; set; }

	public RelayCommand CmdAutoSortDoDefault { get; set; }

	public RelayCommand CmdDeleteItem { get; set; }

	public RelayCommand CmdStartNewBendSequence { get; set; }

	public event Action<BendSequenceItem?> OnSelectedBendChanged;

	public event Action<ISubViewModel, Triangle, Model, double, double, Vector3d, MouseButtonEventArgs> Closed;

	public event Action? RequestRepaint;

	public BendSequenceSortViewModel(IScreen3DMain screen3D, ITranslator translator, IOrderBillboardsViewModel orderBillboardsViewModel, IShortcutSettingsCommon shortcutSettingsCommon, IBendSequenceStrategyFactory bendSequenceStrategyFactory, IUndo3dService undo3dService)
	{
		CmdSequenceSelectItem = new RelayCommand(SequenceSelectItem);
		CmdSequenceSave = new RelayCommand(SaveAndExit);
		CmdSequenceCancel = new RelayCommand(CancelAndExit);
		_screen3D = screen3D;
		_translator = translator;
		_orderBillboardsViewModel = orderBillboardsViewModel;
		_shortcutSettingsCommon = shortcutSettingsCommon;
		_bendSequenceStrategyFactory = bendSequenceStrategyFactory;
		_undo3dService = undo3dService;
	}

	public void Init(IDoc3d docTemp, IDoc3d docOriginal, UiModelType model)
	{
		CurrentModel = model;
		DocTemp = docTemp;
		_docOriginal = docOriginal;
		DocTemp.CombinedBendDescriptorsChanged += CombinedBendDescriptorsChanged;
		CombinedBendDescriptorsChanged();
		InitAutoSort();
		OnSelectedBendChanged += delegate
		{
			UpdateModelUnfoldStatus();
		};
	}

	public void SetSelectedBend(ICombinedBendDescriptorInternal bend)
	{
		SelectedBend = BendSequence.FirstOrDefault((BendSequenceItem x) => x.CombinedBendDescriptor == bend);
	}

	private void SaveAndExit()
	{
		SelectedBend = null;
		_docOriginal.ApplyBendSequence(DocTemp);
		_docOriginal.FreezeCombinedBendDescriptors = true;
		this.Closed?.Invoke(this, null, null, 0.0, 0.0, Vector3d.Zero, null);
		_undo3dService.Save(_docOriginal, _translator.Translate("Undo3d.ApplyBendOrder"));
	}

	private void CancelAndExit()
	{
		SelectedBend = null;
		this.Closed?.Invoke(this, null, null, 0.0, 0.0, Vector3d.Zero, null);
	}

	private void SequenceSelectItem(object obj)
	{
		if (obj is BendSequenceItem selectedBend)
		{
			SelectedBend = selectedBend;
		}
		else
		{
			SelectedBend = null;
		}
	}

	private void CombinedBendDescriptorsChanged()
	{
		BendSequence = new ObservableCollection<BendSequenceItem>(DocTemp.CombinedBendDescriptors.Select((ICombinedBendDescriptorInternal bd) => new BendSequenceItem(bd)));
	}

	private bool SelectNextBend(FaceGroup? fg, IBillboard? billboard)
	{
		int? id = null;
		if (billboard != null)
		{
			id = _orderBillboardsViewModel.GetBend(billboard)?.BendParams.UnfoldFaceGroup.ID;
		}
		if (fg != null && !id.HasValue)
		{
			id = fg.GetParentRoot()?.ID;
		}
		if (!id.HasValue)
		{
			return false;
		}
		IOrderedEnumerable<BendSequenceItem> source = from x in BendSequence
			where x.CombinedBendDescriptor.Enumerable.Any((IBendDescriptor y) => y.BendParams.UnfoldFaceGroup.ID == id)
			orderby x.NewIndex
			select x;
		int currentPos = SelectedBend?.NewIndex ?? (-1);
		BendSequenceItem bendSequenceItem = source.FirstOrDefault((BendSequenceItem x) => x.NewIndex > currentPos);
		if (bendSequenceItem == null)
		{
			return false;
		}
		if (DocTemp.MoveBend(bendSequenceItem.NewIndex, currentPos + 1))
		{
			int splitBendOrder = bendSequenceItem.CombinedBendDescriptor.SplitBendOrder;
			BendSequenceItem selectedBend = BendSequence.FirstOrDefault((BendSequenceItem x) => x.CombinedBendDescriptor.SplitBendOrder == splitBendOrder && x.CombinedBendDescriptor.Enumerable.Any((IBendDescriptor y) => y.BendParams.UnfoldFaceGroup.ID == id));
			SelectedBend = selectedBend;
			return true;
		}
		return false;
	}

	private void UpdateModelUnfoldStatus()
	{
		int num = SelectedBend?.NewIndex ?? (-1);
		HashSet<Model> hashSet = new HashSet<Model>();
		foreach (BendSequenceItem item in BendSequence)
		{
			bool flag = false;
			foreach (Model item2 in item.CombinedBendDescriptor.Enumerable.Select((IBendDescriptor x) => x.BendParams.ModelFaceGroup(CurrentModel).model))
			{
				flag |= hashSet.Add(item2);
			}
			if (flag)
			{
				item.CombinedBendDescriptor.UnfoldBendInModel(1.0, relative: false, CurrentModel);
			}
		}
		foreach (BendSequenceItem item3 in BendSequence)
		{
			if (item3.NewIndex <= num)
			{
				item3.CombinedBendDescriptor.UnfoldBendInModel(0.0, relative: true, CurrentModel);
			}
		}
		foreach (Model item4 in hashSet)
		{
			_screen3D.ScreenD3D.UpdateModelGeometry(item4, render: false);
		}
		_screen3D.ScreenD3D.UpdateAllModelTransform();
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
	}

	public void Dispose()
	{
		DocTemp.CombinedBendDescriptorsChanged -= CombinedBendDescriptorsChanged;
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
	}

	private void InitAutoSort()
	{
		CmdAutoSortDo = new RelayCommand(AutoSortDo);
		CmdAutoSortAdd = new RelayCommand(AutoSortAdd);
		CmdAutoSortClear = new RelayCommand(AutoSortClear);
		CmdAutoSortDoDefault = new RelayCommand(AutoSortDoDefault);
		CmdDeleteItem = new RelayCommand(DeleteItem);
		CmdStartNewBendSequence = new RelayCommand(StartNewBendSequence, CanStartNewBendSequence);
		Array values = System.Enum.GetValues(typeof(BendSequenceSorts));
		List<IBendSequenceSortViewModel.ISortPrioVm> list = new List<IBendSequenceSortViewModel.ISortPrioVm>();
		foreach (object item in values)
		{
			if (item is BendSequenceSorts value)
			{
				list.Add(new SortPrioVm(value, _translator));
			}
		}
		AutoSortListTranslation = list;
		SelectedNewTranslation = AutoSortListTranslation.FirstOrDefault();
		AutoSortList = new ObservableCollection<IBendSequenceSortViewModel.ISortPrioVm>();
		OnPropertyChanged("AutoSortList");
	}

	private void DeleteItem(object obj)
	{
		if (obj is SortPrioVm item)
		{
			AutoSortList.Remove(item);
		}
	}

	private void AutoSortAdd()
	{
		if (SelectedNewTranslation.SortType.HasValue)
		{
			AutoSortList.Add(new SortPrioVm(SelectedNewTranslation.SortType.Value, _translator));
		}
	}

	private void AutoSortClear()
	{
		AutoSortList.Clear();
	}

	private void AutoSortDo()
	{
		List<BendSequenceSorts> list = (from x in AutoSortList
			where x?.SortType.HasValue ?? false
			select x.SortType.Value).ToList();
		if (list.Count == 0)
		{
			list = null;
		}
		IBendSequenceOrder bendSequenceOrder = _bendSequenceStrategyFactory.CreateNewSequenceOrder("", enabled: true, Guid.NewGuid());
		bendSequenceOrder.Sequences.AddRange((list ?? new List<BendSequenceSorts>()).Select((BendSequenceSorts x) => x));
		if (GroupParallesInAutoSort)
		{
			bendSequenceOrder.Groupings.Add(_bendSequenceStrategyFactory.CreateGrouping("", 1));
		}
		DocTemp.CombinedBendsAutoSort(bendSequenceOrder);
		this.RequestRepaint?.Invoke();
	}

	private void AutoSortDoDefault()
	{
		DocTemp.CombinedBendsAutoSort(null);
		this.RequestRepaint?.Invoke();
	}

	private void StartNewBendSequence()
	{
		SelectedBend = null;
	}

	private bool CanStartNewBendSequence()
	{
		return SelectedBend != null;
	}

	public void SetActive(bool active)
	{
	}

	public bool Close()
	{
		CancelAndExit();
		return true;
	}

	public void KeyUp(object sender, IPnInputEventArgs e)
	{
		if (!e.Handled)
		{
			if (_shortcutSettingsCommon.Cancel.IsShortcut(e))
			{
				CancelAndExit();
			}
			else if (_shortcutSettingsCommon.Commit.IsShortcut(e))
			{
				SaveAndExit();
			}
		}
		e.Handle();
	}

	public void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		switch (e.MouseEventArgs?.ChangedButton)
		{
		case MouseButton.Left:
		case MouseButton.XButton1:
			SelectNextBend(e.Tri?.Face?.FaceGroup, e.Billboard);
			break;
		case MouseButton.Right:
		case MouseButton.XButton2:
			SelectedBend = null;
			break;
		}
		e.Args?.Handle();
	}

	public void ColorModelParts(IPaintTool paintTool)
	{
	}

	ICommand CloseCommand()
	{
		return base.CloseCommand;
	}

	void CloseCommand(ICommand value)
	{
		base.CloseCommand = value;
	}

	void IBendSequenceSortViewModel.CloseView()
	{
		CloseView();
	}

	void IBendSequenceSortViewModel.SetListViewColumnHeader(int id, string text)
	{
		SetListViewColumnHeader(id, text);
	}

	void IBendSequenceSortViewModel.TrySetupLearnButton(PopupModelBase popupModelBase)
	{
		TrySetupLearnButton(popupModelBase);
	}
}
