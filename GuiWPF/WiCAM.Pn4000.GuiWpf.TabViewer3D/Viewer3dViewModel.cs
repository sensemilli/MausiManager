using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.SpatialDataStructures;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ScreenD3D.Renderer;

namespace WiCAM.Pn4000.GuiWpf.TabViewer3D;

public class Viewer3dViewModel : ViewModelBase, IViewer3dViewModel, ITab
{
	[CompilerGenerated]
	private sealed class _003CGetNodeWithChildren_003Ed__42 : IEnumerable<HierarchicNode>, IEnumerable, IEnumerator<HierarchicNode>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private HierarchicNode _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private HierarchicNode node;

		public HierarchicNode _003C_003E3__node;

		public Viewer3dViewModel _003C_003E4__this;

		private List<HierarchicNode>.Enumerator _003C_003E7__wrap1;

		private IEnumerator<HierarchicNode> _003C_003E7__wrap2;

		HierarchicNode IEnumerator<HierarchicNode>.Current
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
		public _003CGetNodeWithChildren_003Ed__42(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 2)
			{
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(List<HierarchicNode>.Enumerator);
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				Viewer3dViewModel viewer3dViewModel = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E2__current = node;
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = node.SubNodes.GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00d9;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00bf;
					}
					IL_00d9:
					if (_003C_003E7__wrap1.MoveNext())
					{
						HierarchicNode current = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap2 = viewer3dViewModel.GetNodeWithChildren(current).GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00bf;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = default(List<HierarchicNode>.Enumerator);
					return false;
					IL_00bf:
					if (_003C_003E7__wrap2.MoveNext())
					{
						HierarchicNode current2 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current2;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_00d9;
				}
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
			((IDisposable)_003C_003E7__wrap1/*cast due to .constrained prefix*/).Dispose();
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<HierarchicNode> IEnumerable<HierarchicNode>.GetEnumerator()
		{
			_003CGetNodeWithChildren_003Ed__42 _003CGetNodeWithChildren_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetNodeWithChildren_003Ed__ = this;
			}
			else
			{
				_003CGetNodeWithChildren_003Ed__ = new _003CGetNodeWithChildren_003Ed__42(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CGetNodeWithChildren_003Ed__.node = _003C_003E3__node;
			return _003CGetNodeWithChildren_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<HierarchicNode>)this).GetEnumerator();
		}
	}

	private readonly Color _selectColor = new Color(1f, 1f, 0f, 1f);

	private readonly Color _highLightColor = new Color(0.5f, 1f, 0.5f, 1f);

	private readonly IScreen3DMain _screen3DMain;

	private readonly IPnStatusBarHelper _statusBarHelper;

	private readonly IDoc3d _currentDoc;

	private CameraState? _lastRenderCameraState;

	private Model _hoverModel;

	private Visibility _visible = Visibility.Collapsed;

	private Dictionary<Model, HashSet<HierarchicNode>> DictNodes = new Dictionary<Model, HashSet<HierarchicNode>>();

	private IScreen3D Screen3D => _screen3DMain.Screen3D;

	public RelayCommand CmdCollapseAll { get; }

	public RelayCommand CmdExpandAll { get; }

	public ISubViewModel ActiveSubViewModel { get; set; }

	public bool IsActive { get; private set; }

	public Visibility Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (_visible != value)
			{
				_visible = value;
				NotifyPropertyChanged("Visible");
			}
		}
	}

	public Visibility VisibleAnyObjects { get; private set; }

	public ObservableCollection<HierarchicNode> HierarchicList { get; set; } = new ObservableCollection<HierarchicNode>();

	public Viewer3dViewModel(IScreen3DMain screen3D, IPnStatusBarHelper statusBarHelper, IDoc3d doc)
	{
		CmdCollapseAll = new RelayCommand(CollapseAll);
		CmdExpandAll = new RelayCommand(ExpandAll);
		_screen3DMain = screen3D;
		_statusBarHelper = statusBarHelper;
		_currentDoc = doc;
		Model view3DModel = _currentDoc.View3DModel;
		if (view3DModel != null)
		{
			HierarchicNode hierarchicNode = CreateTree(view3DModel, null, 0);
			foreach (HierarchicNode nodeWithChild in GetNodeWithChildren(hierarchicNode))
			{
				nodeWithChild.IsExpanded = true;
			}
			HierarchicList.Add(hierarchicNode);
		}
		if (HierarchicList.Count > 0)
		{
			VisibleAnyObjects = Visibility.Visible;
		}
		else
		{
			VisibleAnyObjects = Visibility.Collapsed;
		}
	}

	private void ExpandAll(object obj)
	{
		foreach (HierarchicNode item in HierarchicList.SelectMany(GetNodeWithChildren))
		{
			item.IsExpanded = true;
		}
	}

	private void CollapseAll(object obj)
	{
		foreach (HierarchicNode item in HierarchicList.SelectMany(GetNodeWithChildren))
		{
			item.IsExpanded = false;
		}
	}

	public void Dispose()
	{
		SetActive(show: false);
		DictNodes.Clear();
	}

	public void SetActive(bool show)
	{
		if (IsActive != show)
		{
			IsActive = show;
			if (IsActive)
			{
				Visible = Visibility.Visible;
				Screen3D.TriangleSelected += MouseSelectTriangle;
				Screen3D.MouseEnterTriangle += MouseEnterTriangle;
				Screen3D.MouseLeaveTriangle += MouseLeaveTriangle;
				Screen3D.KeyUp += Pn3DScreenOnKeyUp;
				IScreen3D screen3D = Screen3D;
				screen3D.ExternalKeyDown = (KeyEventHandler)Delegate.Combine(screen3D.ExternalKeyDown, new KeyEventHandler(Pn3DScreenOnKeyUp));
				Screen3D.ScreenD3D.Renderer.RenderData.AutoAdjustFloorToScreenRotation = true;
				RecalculateVisibility();
				RepaintModels();
				_statusBarHelper.RemoveSlotControl(2);
				_statusBarHelper.RemoveSlotControl(3);
				_statusBarHelper.RemoveSlotControl(4);
				_statusBarHelper.RemoveSlotControl(5);
				_statusBarHelper.RemoveSlotControl(6);
				_statusBarHelper.RemoveSlotControl(7);
			}
			else
			{
				_lastRenderCameraState = Screen3D.ScreenD3D.Renderer.ExportCameraState();
				Visible = Visibility.Collapsed;
				Screen3D.TriangleSelected -= MouseSelectTriangle;
				Screen3D.MouseEnterTriangle -= MouseEnterTriangle;
				Screen3D.MouseLeaveTriangle -= MouseLeaveTriangle;
				Screen3D.KeyUp -= Pn3DScreenOnKeyUp;
				IScreen3D screen3D2 = Screen3D;
				screen3D2.ExternalKeyDown = (KeyEventHandler)Delegate.Remove(screen3D2.ExternalKeyDown, new KeyEventHandler(Pn3DScreenOnKeyUp));
			}
			ActiveSubViewModel?.SetActive(IsActive);
		}
	}

	private void Pn3DScreenOnKeyUp(object sender, KeyEventArgs e)
	{
		if (!e.Handled)
		{
			PnInputEventArgs e2 = new PnInputEventArgs(e, keyUp: true);
			Screen3D.NavigationKeyDown(sender, e2);
		}
	}

	[IteratorStateMachine(typeof(_003CGetNodeWithChildren_003Ed__42))]
	private IEnumerable<HierarchicNode> GetNodeWithChildren(HierarchicNode node)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetNodeWithChildren_003Ed__42(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__node = node
		};
	}

	private void RepaintModels()
	{
		_screen3DMain.ScreenD3D.RemoveModel(null, render: false);
		_screen3DMain.ScreenD3D.RemoveBillboard(null, render: false);
		if (_currentDoc.View3DModel != null)
		{
			_screen3DMain.ScreenD3D.AddModel(_currentDoc.View3DModel, render: false);
		}
		if (_lastRenderCameraState.HasValue)
		{
			_screen3DMain.ScreenD3D.Renderer.ImportCameraState(_lastRenderCameraState.Value);
		}
		else
		{
			Matrix4d identity = Matrix4d.Identity;
			identity *= Matrix4d.RotationZ(0.7853981852531433);
			identity *= Matrix4d.RotationX(1.0471975803375244);
			_screen3DMain.ScreenD3D.SetViewDirectionByMatrix4d(identity, render: false);
			_screen3DMain.ScreenD3D.ZoomExtend(render: false);
		}
		_screen3DMain.ScreenD3D.Render(skipQueuedFrames: false);
	}

	public void HoverModel(Model newM)
	{
		foreach (HierarchicNode item in HierarchicList.SelectMany(GetNodeWithChildren))
		{
			item.IsHovered = false;
		}
		if (_hoverModel != newM && newM != null)
		{
			if (DictNodes.TryGetValue(newM, out HashSet<HierarchicNode> value))
			{
				foreach (HierarchicNode item2 in value.SelectMany(GetNodeWithChildren))
				{
					item2.IsHovered = true;
				}
			}
			_hoverModel = newM;
		}
		RecolourModel();
	}

	public void DeHoverModel(Model model)
	{
		if (_hoverModel != null && _hoverModel == model)
		{
			if (DictNodes.TryGetValue(_hoverModel, out HashSet<HierarchicNode> value))
			{
				foreach (HierarchicNode item in value.SelectMany(GetNodeWithChildren))
				{
					item.IsHovered = false;
				}
			}
			_hoverModel = null;
		}
		RecolourModel();
	}

	private void SelectModel(Model model)
	{
		foreach (HierarchicNode item in HierarchicList.SelectMany(GetNodeWithChildren))
		{
			item.IsSelected = false;
		}
		if (model != null && DictNodes.TryGetValue(model, out HashSet<HierarchicNode> value))
		{
			foreach (HierarchicNode item2 in value.SelectMany(GetNodeWithChildren))
			{
				item2.IsSelected = true;
			}
		}
		RecolourModel();
	}

	private void MouseEnterTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		HoverModel(e.Model);
	}

	private void MouseLeaveTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		DeHoverModel(e.Model);
	}

	private void MouseSelectTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		SelectModel(e.Model);
	}

	private void RecolourModel()
	{
		if (_currentDoc?.View3DModel == null)
		{
			return;
		}
		foreach (Model item in _currentDoc.View3DModel.GetAllSubModelsWithSelf())
		{
			item.UnHighLightModel();
		}
		foreach (HierarchicNode item2 in HierarchicList.SelectMany(GetNodeWithChildren))
		{
			if (item2.IsSelected)
			{
				ColorFaces(item2.Model, _selectColor);
			}
			else if (item2.IsHovered)
			{
				ColorFaces(item2.Model, _highLightColor);
			}
		}
		_screen3DMain.ScreenD3D.UpdateAllModelAppearance(render: true);
		static void ColorFaces(Model model, Color highlightColor)
		{
			foreach (Face item3 in model.Shells.SelectMany((Shell s) => s.Faces))
			{
				item3.HighlightColor = highlightColor;
			}
		}
	}

	private HierarchicNode CreateTree(Model model, HierarchicNode parentItem, int treeLevel)
	{
		string descBoundingBox = "";
		if (model.Shells.Count > 0 && model.Shells.First().AABBTree.Root != null)
		{
			AABB<Vector3d> boundingBox = model.Shells.First().AABBTree.Root.BoundingBox;
			double value = boundingBox.Max.X - boundingBox.Min.X;
			double value2 = boundingBox.Max.Y - boundingBox.Min.Y;
			double value3 = boundingBox.Max.Z - boundingBox.Min.Z;
			descBoundingBox = $" [{value:0.00} x {value2:0.00} x {value3:0.00} mm]";
		}
		HierarchicNode hierarchicNode = new HierarchicNode
		{
			Model = model,
			Level = treeLevel,
			DescBoundingBox = descBoundingBox,
			SettingVisibility = HierarchicItemSetVisibility,
			SetSelectedByTreeView = SetSelectedByTreeView
		};
		if (!DictNodes.TryGetValue(model, out HashSet<HierarchicNode> value4))
		{
			value4 = new HashSet<HierarchicNode>();
			DictNodes.Add(model, value4);
		}
		value4.Add(hierarchicNode);
		if (parentItem != null)
		{
			parentItem.SubNodes.Add(hierarchicNode);
			hierarchicNode.ParentNode = parentItem;
		}
		foreach (Model subModel in model.SubModels)
		{
			if (subModel != null)
			{
				CreateTree(subModel, hierarchicNode, treeLevel + 1);
			}
		}
		if (model.ReferenceModel.Count > 0)
		{
			foreach (Model item in model.ReferenceModel.Select((ModelInstance m) => m.Reference))
			{
				if (item != null)
				{
					CreateTree(item, hierarchicNode, treeLevel);
				}
			}
		}
		return hierarchicNode;
	}

	private void SetSelectedByTreeView(HierarchicNode obj)
	{
		SelectModel(obj.Model);
	}

	private void HierarchicItemSetVisibility(HierarchicNode sender, bool newVis)
	{
		SetVis(sender);
		RecalculateVisibility();
		_screen3DMain.ScreenD3D.Render(skipQueuedFrames: false);
		void SetVis(HierarchicNode item)
		{
			if (item.Model != null)
			{
				item.Model.Enabled = newVis;
			}
			if (item.SubNodes.Count > 0)
			{
				foreach (HierarchicNode subNode in item.SubNodes)
				{
					SetVis(subNode);
				}
			}
		}
	}

	private void RecalculateVisibility()
	{
		foreach (HierarchicNode hierarchic in HierarchicList)
		{
			UpdateNodeCheckbox(hierarchic);
		}
		static bool? UpdateNodeCheckbox(HierarchicNode nodeU)
		{
			bool? flag = null;
			if (nodeU.SubNodes.Count > 0)
			{
				flag = nodeU.SubNodes.Select(UpdateNodeCheckbox).Aggregate((bool? x, bool? y) => (x != y) ? null : x);
			}
			if (nodeU.Model.Shells.Count > 0)
			{
				flag = nodeU.Model.Enabled;
			}
			if (flag != false && nodeU.Model != null)
			{
				nodeU.Model.Enabled = true;
			}
			nodeU.SetVisibilityForCb(flag);
			return flag;
		}
	}

	public void RefreshScreen()
	{
	}
}
