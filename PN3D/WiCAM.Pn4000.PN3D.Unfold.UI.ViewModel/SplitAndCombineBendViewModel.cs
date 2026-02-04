using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration.ViewModels;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Unfold.UI.Model;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class SplitAndCombineBendViewModel : ScreenControlBaseViewModel, ISubViewModel
{
	private ICombinedBendDescriptorInternal _selectedCommonBend;

	private SplitAndCombineBendModel _model;

	private ObservableCollection<BendFaceViewModel> _items;

	private ICommand _splitOrCombineCommand;

	private IDoc3d _doc;

	private BendFaceViewModel _cbfHovered;

	private readonly global::WiCAM.Pn4000.BendModel.Model _currentRenderModel;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	public ICombinedBendDescriptorInternal SelectedCommonBend
	{
		get
		{
			return this._selectedCommonBend;
		}
		set
		{
			this._selectedCommonBend = value;
			base.NotifyPropertyChanged("SelectedCommonBend");
		}
	}

	public ObservableCollection<BendFaceViewModel> Items
	{
		get
		{
			return this._items;
		}
		set
		{
			this._items = value;
			base.NotifyPropertyChanged("Items");
		}
	}

	public ICommand SplitOrCombineCommand => this._splitOrCombineCommand ?? (this._splitOrCombineCommand = new RelayCommand(SplitOrCombine));

	public event Action<ISubViewModel, Triangle, global::WiCAM.Pn4000.BendModel.Model, double, double, Vector3d, MouseButtonEventArgs> Closed;

	public event Action RequestRepaint;

	public SplitAndCombineBendViewModel(SplitAndCombineBendModel model, Screen3D screen3d, IDoc3d doc, global::WiCAM.Pn4000.BendModel.Model bendModel, Vector3d anchorPoint, global::WiCAM.Pn4000.BendModel.Model currentRenderModel, IShortcutSettingsCommon shortcutSettingsCommon)
		: base(screen3d, bendModel, anchorPoint)
	{
		this._currentRenderModel = currentRenderModel;
		this._shortcutSettingsCommon = shortcutSettingsCommon;
		this._model = model;
		this._doc = doc;
		this.SelectedCommonBend = model.SelectedCommonBend;
		this.GetItems();
		this.OnViewChanged();
	}

	private void GetItems()
	{
		this.Items = new ObservableCollection<BendFaceViewModel>();
		if (this.SelectedCommonBend == null)
		{
			return;
		}
		int count = this.SelectedCommonBend.Count;
		for (int i = 0; i < count; i++)
		{
			this.Items.Add(new BendFaceViewModel(this.SelectedCommonBend[i], this.SelectedCommonBend, i != 0, i > 0));
		}
		foreach (ICombinedBendDescriptorInternal item in this._doc.CombinedBendDescriptors.Where((ICombinedBendDescriptorInternal b) => b != this.SelectedCommonBend && ((base.Model == this._doc.UnfoldModel3D && b.IsCompatibleBendUnfoldModel(this.SelectedCommonBend)) || (base.Model == this._doc.BendModel3D && b.IsCompatibleBendBendModel(this.SelectedCommonBend)))))
		{
			count = item.Count;
			for (int j = 0; j < count; j++)
			{
				this.Items.Add(new BendFaceViewModel(item[j], item, isNotFirstItem: true, j > 0));
			}
		}
	}

	private void SplitOrCombine(object obj)
	{
		BendFaceViewModel bendFaceViewModel = obj as BendFaceViewModel;
		BendFaceViewModel previousBend = this.Items[this.Items.IndexOf(bendFaceViewModel) - 1];
		if (bendFaceViewModel.IsCombined)
		{
			this.SplitBend(bendFaceViewModel, previousBend);
		}
		else
		{
			this.CombineBends(bendFaceViewModel, previousBend);
		}
		this.GetItems();
	}

	private void SplitBend(BendFaceViewModel selectedBend, BendFaceViewModel previousBend)
	{
		List<IBendDescriptor> list = null;
		if (base.Model == this._doc.UnfoldModel3D)
		{
			list = selectedBend.ParentCommonBend.BendOrderUnfoldModel.ToList();
		}
		else if (base.Model == this._doc.BendModel3D)
		{
			list = selectedBend.ParentCommonBend.BendOrderBendModel.ToList();
		}
		int num = list.IndexOf(selectedBend.BendFace);
		int num2 = list.IndexOf(previousBend.BendFace);
		if (num != -1 && num2 != -1 && Math.Abs(num - num2) == 1)
		{
			int num3 = this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(selectedBend.ParentCommonBend);
			this._doc.SplitCombinedBends(num3, Math.Max(num, num2));
			this.SelectedCommonBend = this._doc.CombinedBendDescriptors[num3];
		}
	}

	private void CombineBends(BendFaceViewModel selectedBend, BendFaceViewModel previousBend)
	{
		if (base.Model == this._doc.UnfoldModel3D)
		{
			this._doc.MergeCombinedBendsInUnfoldModel(this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(selectedBend.ParentCommonBend), this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(previousBend.ParentCommonBend));
		}
		else if (base.Model == this._doc.BendModel3D)
		{
			this._doc.MergeCombinedBendsInBendModel(this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(selectedBend.ParentCommonBend), this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(previousBend.ParentCommonBend));
		}
		this.SelectedCommonBend = this._doc.CombinedBendDescriptors[this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(selectedBend.ParentCommonBend)];
	}

	public void SetActive(bool active)
	{
	}

	public void KeyUp(object sender, IPnInputEventArgs e)
	{
		if (!e.Handled && this._shortcutSettingsCommon.Cancel.IsShortcut(e))
		{
			this.Close();
			e.Handle();
		}
	}

	public void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
	}

	public new bool Close()
	{
		this.Closed?.Invoke(this, null, null, 0.0, 0.0, Vector3d.Zero, null);
		base.Close();
		return true;
	}

	public void MouseEnterCommand()
	{
		base.Opacity = base.OpacityMax;
	}

	public void MouseLeaveCommand()
	{
		base.Opacity = base.OpacityMin;
	}

	public void HoverBend(BendFaceViewModel cbf)
	{
		if (cbf != this._cbfHovered)
		{
			this.DeHoverBend(this._cbfHovered);
			this._cbfHovered = cbf;
			if (cbf != null)
			{
				cbf.Color = Brushes.SkyBlue;
				this.RequestRepaint?.Invoke();
			}
		}
	}

	public void DeHoverBend(BendFaceViewModel cbf)
	{
		if (cbf == this._cbfHovered && cbf != null)
		{
			this._cbfHovered.Color = Brushes.Black;
			this._cbfHovered = null;
			this.RequestRepaint?.Invoke();
		}
	}

	public void ColorModelParts(IPaintTool paintTool)
	{
		if (this._cbfHovered == null)
		{
			return;
		}
		FaceGroup g = null;
		if (this._currentRenderModel == this._doc.ReconstructedEntryModel)
		{
			g = this._cbfHovered.BendFace.BendParams.EntryFaceGroup;
		}
		else if (this._currentRenderModel == this._doc.ModifiedEntryModel3D)
		{
			g = this._cbfHovered.BendFace.BendParams.ModifiedEntryFaceGroup;
		}
		else if (this._currentRenderModel == this._doc.UnfoldModel3D)
		{
			g = this._cbfHovered.BendFace.BendParams.UnfoldFaceGroup;
		}
		else if (this._currentRenderModel == this._doc.BendModel3D)
		{
			g = this._cbfHovered.BendFace.BendParams.BendFaceGroup;
		}
		foreach (FaceHalfEdge item in g.GetAllFaces().SelectMany((Face f) => f.GetAllEdges()))
		{
			paintTool.SetEdgeColorInShell(item, new global::WiCAM.Pn4000.BendModel.Base.Color(0.5f, 0.7f, 1f, 1f), 5f);
		}
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
	}
}
