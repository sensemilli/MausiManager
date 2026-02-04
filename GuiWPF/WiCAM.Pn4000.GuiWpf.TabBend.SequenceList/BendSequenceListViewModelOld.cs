using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration.UI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.GuiWpf.TabBend.SequenceList;

public class BendSequenceListViewModelOld : ViewModelBase
{
	private string ID = Guid.NewGuid().ToString();

	private BendSequenceListItemViewModel _hoveredCommonBendFace;

	private BendSequenceListItemViewModel _selectedCommonBendFace;

	private ObservableCollection<BendSequenceListItemViewModel> _commonBendFaces;

	private ICommand _editBendSequence;

	private ICommand _splitBendSequence;

	private ICommand _excludeBendCommand;

	private ICommand _orderChangedCommand;

	private bool _expanded;

	private bool _enabled;

	private readonly IGlobals _globals;

	private readonly IDoc3d _doc;

	private readonly IDoc3d _docOriginal;

	private readonly IBendSequenceSortViewModel _sortVm;

	private readonly IMainWindowBlock _mainWindowBlock;

	private double _dataViewHight = 200.0;

	private Action<ICombinedBendDescriptorInternal> _selectedCommonBendFaceChanged;

	private Action<ICombinedBendDescriptorInternal> _hoveredCommonBendFaceChanged;

	private Action _repaintBendModel;

	private readonly ITranslator _translator;

	private static WiCAM.Pn4000.BendModel.Base.Color colSel = new WiCAM.Pn4000.BendModel.Base.Color(0f, 1f, 0f, 1f);

	private static WiCAM.Pn4000.BendModel.Base.Color colSel2 = new WiCAM.Pn4000.BendModel.Base.Color(0f, 0.5f, 0f, 1f);

	public ICommand OrderChangedCommand => _orderChangedCommand ?? (_orderChangedCommand = new RelayCommand(ReOrderBends));

	public ICommand ExcludeBendCommand => _excludeBendCommand ?? (_excludeBendCommand = new RelayCommand(ExcludeBend));

	public ICommand EditBendSequenceClick => _editBendSequence ?? (_editBendSequence = new RelayCommand(EditBendSequence));

	public bool Expanded
	{
		get
		{
			return _expanded;
		}
		set
		{
			_expanded = value;
			NotifyPropertyChanged("Expanded");
		}
	}

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
			NotifyPropertyChanged("Enabled");
		}
	}

	public BendSequenceListItemViewModel SelectedCommonBendFace
	{
		get
		{
			return _selectedCommonBendFace;
		}
		set
		{
			if (_selectedCommonBendFace == value)
			{
				return;
			}
			_selectedCommonBendFace = value;
			NotifyPropertyChanged("SelectedCommonBendFace");
			_selectedCommonBendFaceChanged?.Invoke(_selectedCommonBendFace?.CommonBendFace);
			if (SortVm == null)
			{
				return;
			}
			int? num = CommonBendFaces.IndexOf(SelectedCommonBendFace);
			if (num == -1)
			{
				num = null;
			}
			foreach (BendSequenceListItemViewModel commonBendFace in CommonBendFaces)
			{
				commonBendFace.SelectedBendNumber = num;
			}
		}
	}

	public BendSequenceListItemViewModel HoveredCommonBendFace
	{
		get
		{
			return _hoveredCommonBendFace;
		}
		set
		{
			if (_hoveredCommonBendFace != value)
			{
				if (_hoveredCommonBendFace != null)
				{
					_hoveredCommonBendFace.IsHovered = false;
				}
				_hoveredCommonBendFace = value;
				NotifyPropertyChanged("HoveredCommonBendFace");
				if (_hoveredCommonBendFace != null)
				{
					_hoveredCommonBendFace.IsHovered = true;
				}
				_hoveredCommonBendFaceChanged?.Invoke(_hoveredCommonBendFace?.CommonBendFace);
			}
		}
	}

	public double DataViewHeight
	{
		get
		{
			return _dataViewHight;
		}
		set
		{
			_dataViewHight = value;
			NotifyPropertyChanged("DataViewHeight");
		}
	}

	public int SelectedIndex { get; set; }

	public int BendSequenceListLastIndex { get; private set; } = -1;

	public ObservableCollection<BendSequenceListItemViewModel> CommonBendFaces
	{
		get
		{
			return _commonBendFaces;
		}
		set
		{
			_commonBendFaces = value;
			NotifyPropertyChanged("CommonBendFaces");
		}
	}

	public bool Render { get; set; } = true;

	public IBendSequenceSortViewModel SortVm => _sortVm;

	public Visibility AutoSortVisibility
	{
		get
		{
			if (SortVm != null)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public Visibility StartChangeSequenceVisibility
	{
		get
		{
			if (SortVm == null)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public Action<ICombinedBendDescriptorInternal> OpenContextMenu { get; private set; }

	private bool IsChangingSequence => SortVm != null;

	public void SetHoveredBendFaceGroup(Triangle? triangle, IBillboard? billboard, Func<IBillboard, IBendDescriptor?> getBendDescriptor)
	{
		int? id = null;
		if (billboard != null)
		{
			id = getBendDescriptor(billboard)?.BendParams.UnfoldFaceGroup.ID;
		}
		if (triangle != null && !id.HasValue)
		{
			FaceGroup fg = triangle?.Face?.FaceGroup;
			id = fg.GetParentRoot()?.ID;
		}
		if (!id.HasValue)
		{
			HoveredCommonBendFace = null;
			return;
		}
		BendSequenceListItemViewModel hoveredCommonBendFace = CommonBendFaces.FirstOrDefault((BendSequenceListItemViewModel x) => x.CommonBendFace.Enumerable.Any((IBendDescriptor y) => y.BendParams.BendFaceGroup.ID == id));
		HoveredCommonBendFace = hoveredCommonBendFace;
	}

	public BendSequenceListViewModelOld(IGlobals globals, IDoc3d docOriginal, IBendSequenceSortViewModel sortVm, IMainWindowBlock windowBlock, Action<ICombinedBendDescriptorInternal> selectedCommonBendFaceChanged, Action<ICombinedBendDescriptorInternal> hoveredCommonBendFaceChanged, Action repaintBendModel, Action<ICombinedBendDescriptorInternal> openContextMenu, ITranslator translator)
	{
		_selectedCommonBendFaceChanged = selectedCommonBendFaceChanged;
		_repaintBendModel = repaintBendModel;
		_translator = translator;
		OpenContextMenu = openContextMenu;
		_hoveredCommonBendFaceChanged = hoveredCommonBendFaceChanged;
		_globals = globals;
		_doc = sortVm?.DocTemp ?? docOriginal;
		_docOriginal = docOriginal;
		_sortVm = sortVm;
		_mainWindowBlock = windowBlock;
		if (SortVm != null)
		{
			_selectedCommonBendFaceChanged = delegate
			{
				if (_selectedCommonBendFace?.CommonBendFace != null)
				{
					SortVm.SetSelectedBend(_selectedCommonBendFace.CommonBendFace);
				}
			};
			SortVm.OnSelectedBendChanged += SortVmSelectedBendChanged;
		}
		CommonBendFaces = new ObservableCollection<BendSequenceListItemViewModel>();
		_doc.CombinedBendDescriptorsChanged += BendsChanged;
		_doc.UpdateBendDataInfoEvent += BendDataChanged;
		BendsChanged();
	}

	private void SortVmSelectedBendChanged(BendSequenceItem x)
	{
		SelectedCommonBendFace = CommonBendFaces.FirstOrDefault((BendSequenceListItemViewModel cbf) => cbf.CommonBendFace == x?.CombinedBendDescriptor);
		_repaintBendModel?.Invoke();
	}

	public void HoverBend(BendSequenceListItemViewModel cbf)
	{
		if (cbf != HoveredCommonBendFace)
		{
			HoveredCommonBendFace = cbf;
		}
	}

	public void DeHoverBend(BendSequenceListItemViewModel cbf)
	{
		if (cbf == HoveredCommonBendFace)
		{
			HoveredCommonBendFace = null;
		}
	}

	public void ColorModelParts(IPaintTool paintTool)
	{
		if (HoveredCommonBendFace != null)
		{
			foreach (FaceHalfEdge item in HoveredCommonBendFace.CommonBendFace.Enumerable.SelectMany((IBendDescriptor bz) => bz.BendParams.BendFaceGroup.GetAllFaces()).SelectMany((Face f) => f.GetAllEdges()))
			{
				paintTool.SetEdgeColorInShell(item, colSel, 5f);
			}
		}
		if (SelectedCommonBendFace == null)
		{
			return;
		}
		if (!IsChangingSequence)
		{
			IDoc3d doc = _doc;
			if (doc == null || doc.HasFingers)
			{
				return;
			}
		}
		foreach (FaceHalfEdge item2 in SelectedCommonBendFace.CommonBendFace.Enumerable.SelectMany((IBendDescriptor bz) => bz.BendParams.BendFaceGroup.GetAllFaces()).SelectMany((Face f) => f.GetAllEdges()))
		{
			paintTool.SetEdgeColorInShell(item2, colSel2, 5f);
		}
	}

	public void ColorBorders()
	{
		foreach (BendSequenceListItemViewModel entry in CommonBendFaces)
		{
			List<WiCAM.Pn4000.BendModel.Base.Color?> borderEdgesColors = entry.CommonBendFace.Enumerable.Select((IBendDescriptor x) => x.BendParams.BendFaceGroupModel.EdgeColor).ToList();
			WiCAM.Pn4000.BendModel.Base.Color? color = borderEdgesColors.FirstOrDefault((WiCAM.Pn4000.BendModel.Base.Color? x) => x.HasValue);
			if (color.HasValue)
			{
				Application.Current.Dispatcher.Invoke(delegate
				{
					if (borderEdgesColors.All(delegate(WiCAM.Pn4000.BendModel.Base.Color? c)
					{
						WiCAM.Pn4000.BendModel.Base.Color? color2 = borderEdgesColors.FirstOrDefault();
						WiCAM.Pn4000.BendModel.Base.Color? color3 = c;
						if (color2.HasValue != color3.HasValue)
						{
							return false;
						}
						return !color2.HasValue || color2.GetValueOrDefault() == color3.GetValueOrDefault();
					}))
					{
						entry.Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(Convert.ToByte(color.Value.A * 255f), Convert.ToByte(color.Value.R * 255f), Convert.ToByte(color.Value.G * 255f), Convert.ToByte(color.Value.B * 255f)));
					}
					else
					{
						entry.Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(Convert.ToByte(color.Value.A * 255f), Convert.ToByte(color.Value.R * 255f), Convert.ToByte(color.Value.G * 255f), Convert.ToByte(color.Value.B * 255f)) + Colors.LightGray);
					}
				});
			}
			else
			{
				entry.Color = Brushes.DarkGray;
			}
		}
	}

	public void ToolsChanged()
	{
		foreach (BendSequenceListItemViewModel commonBendFace in CommonBendFaces)
		{
			commonBendFace.GenerateLabelInformation();
			commonBendFace.UpdateFingerStatus();
		}
	}

	public void SimulationChanged()
	{
		foreach (BendSequenceListItemViewModel commonBendFace in CommonBendFaces)
		{
			commonBendFace.UpdateFingerStatus();
		}
	}

	private void BendDataChanged(IDoc3d doc)
	{
		ToolsChanged();
	}

	private void BendsChanged()
	{
		if (_doc.CombinedBendDescriptors.Count > 0)
		{
			Enabled = true;
			Expanded = true;
			SetCommonBendFaces(_doc.CombinedBendDescriptors);
		}
		else
		{
			CommonBendFaces.Clear();
			Enabled = false;
			Expanded = false;
		}
	}

	private void EditBendSequence()
	{
	}

	private void ReOrderBends(object param)
	{
		if (IsChangingSequence)
		{
			SortVm.DocTemp.ApplyBendOrder(CommonBendFaces.Select((BendSequenceListItemViewModel x) => x.CommonBendFace).ToList());
			if (param is DragDropEventArgs { MovedItem: BendSequenceListItemViewModel movedItem })
			{
				SortVm.SetSelectedBend(movedItem?.CommonBendFace);
			}
		}
		else if (!_doc.ApplyBendOrder(CommonBendFaces.Select((BendSequenceListItemViewModel x) => x.CommonBendFace).ToList()))
		{
			CommonBendFaces = new ObservableCollection<BendSequenceListItemViewModel>(CommonBendFaces.OrderBy((BendSequenceListItemViewModel x) => x.CommonBendFace.Order));
			return;
		}
		BendsChanged();
	}

	private void SetCommonBendFaces(IEnumerable<ICombinedBendDescriptorInternal> cbfs)
	{
		int? selectedBendNumber = null;
		if (SortVm != null)
		{
			selectedBendNumber = CommonBendFaces.IndexOf(SelectedCommonBendFace);
			if (selectedBendNumber == -1)
			{
				selectedBendNumber = null;
			}
		}
		ICombinedBendDescriptorInternal oldSelectedItem = SortVm?.SelectedBend?.CombinedBendDescriptor;
		CommonBendFaces = new ObservableCollection<BendSequenceListItemViewModel>(cbfs.Select((ICombinedBendDescriptorInternal cbf) => new BendSequenceListItemViewModel(cbf, null, _globals.LanguageDictionary, GetOldOrder(cbf), _translator, selectedBendNumber)));
		if (oldSelectedItem != null)
		{
			SelectedCommonBendFace = CommonBendFaces.FirstOrDefault((BendSequenceListItemViewModel x) => x.CommonBendFace == oldSelectedItem);
		}
		int? GetOldOrder(ICombinedBendDescriptorInternal newBend)
		{
			if (!IsChangingSequence)
			{
				return null;
			}
			return (from cbdOld in _docOriginal.CombinedBendDescriptors
				where cbdOld.Enumerable.Any((IBendDescriptor x) => newBend.Enumerable.Any((IBendDescriptor y) => x == y))
				orderby Math.Abs(cbdOld.StopProductAngleSigned - newBend.StopProductAngleSigned)
				select cbdOld).FirstOrDefault()?.Order;
		}
	}

	private void ExcludeBend()
	{
		if (!IsChangingSequence && _doc.HasToolSetups)
		{
			_mainWindowBlock.InitWait(_doc);
			SetCommonBendFaces(_doc.CombinedBendDescriptors);
			_doc.BendSimulation?.GotoStep(0.0);
			_doc.DisableSimulationCalculated();
			_mainWindowBlock.CloseWait(_doc);
		}
		else
		{
			_doc.BendSimulation = null;
		}
	}

	public void Dispose()
	{
		_selectedCommonBendFaceChanged = null;
		OpenContextMenu = null;
		_hoveredCommonBendFaceChanged = null;
		CommonBendFaces = null;
		_doc.CombinedBendDescriptorsChanged -= BendsChanged;
		_doc.UpdateBendDataInfoEvent -= BendDataChanged;
		if (SortVm != null)
		{
			SortVm.OnSelectedBendChanged -= SortVmSelectedBendChanged;
		}
	}

	public void SelectCommonBendFace(ICombinedBendDescriptorInternal newCbf)
	{
		if (SelectedCommonBendFace?.CommonBendFace == newCbf)
		{
			return;
		}
		if (newCbf == null)
		{
			SelectedCommonBendFace = null;
			return;
		}
		SelectedCommonBendFace = CommonBendFaces.FirstOrDefault((BendSequenceListItemViewModel x) => x.CommonBendFace == newCbf);
	}

	public void HoverCommonBendFace(ICombinedBendDescriptorInternal newCbf)
	{
		if (HoveredCommonBendFace?.CommonBendFace == newCbf)
		{
			return;
		}
		if (newCbf == null)
		{
			HoveredCommonBendFace = null;
			return;
		}
		HoveredCommonBendFace = CommonBendFaces.FirstOrDefault((BendSequenceListItemViewModel x) => x.CommonBendFace == newCbf);
	}
}
