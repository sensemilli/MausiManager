using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.DragDrop;
using WiCAM.Pn4000.WpfControls.DragDrop;
using DragEventArgs = Telerik.Windows.DragDrop.DragEventArgs;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditBendOrderPopup;

internal partial class EditOrderView : UserControl, IComponentConnector, IStyleConnector
{
	private IEditOrderViewModel _vm;

	private int? _oldIdx;

	private int? _newIdx;

	private DragDropEffects? _lastEffect;

	public EditOrderView()
	{
		InitializeComponent();
	}

	public EditOrderView(IEditOrderViewModel viewModel)
	{
		_vm = viewModel;
	}

	public void Init(IEditOrderViewModel vm, Action refresh)
	{
		base.DataContext = vm;
		_vm = vm;
		_vm.Init(refresh);
		InitializeComponent();
		RowReorderBehavior.InitializeAdvanced(GridViewSequence, RowDragOverEvent, DropEvent);
	}

	private new void DropEvent(DropIndicationDetails details, DragEventArgs e)
	{
		if (!(base.DataContext is EditOrderViewModel editOrderViewModel))
		{
			return;
		}
		editOrderViewModel.ClearChangeStatus();
		EditOrderListItemViewModel editOrderListItemViewModel = details.CurrentDraggedItem as EditOrderListItemViewModel;
		EditOrderListItemViewModel editOrderListItemViewModel2 = details.CurrentDraggedOverItem as EditOrderListItemViewModel;
		if (editOrderListItemViewModel == null || editOrderListItemViewModel2 == null)
		{
			((RoutedEventArgs)(object)e).Handled = true;
			return;
		}
		int num = editOrderViewModel.Items.IndexOf(editOrderListItemViewModel);
		int dropIndex = details.DropIndex;
		if (num == dropIndex)
		{
			((RoutedEventArgs)(object)e).Handled = true;
			return;
		}
		editOrderViewModel.MoveBend(num, dropIndex);
		((RoutedEventArgs)(object)e).Handled = true;
		_oldIdx = null;
		_newIdx = null;
		_lastEffect = null;
	}

	private void RowDragOverEvent(DropIndicationDetails details, DragEventArgs e)
	{
		if (!(base.DataContext is EditOrderViewModel editOrderViewModel))
		{
			return;
		}
		EditOrderListItemViewModel editOrderListItemViewModel = details.CurrentDraggedItem as EditOrderListItemViewModel;
		EditOrderListItemViewModel editOrderListItemViewModel2 = details.CurrentDraggedOverItem as EditOrderListItemViewModel;
		if (editOrderListItemViewModel == null || editOrderListItemViewModel2 == null)
		{
			editOrderViewModel.ClearChangeStatus();
			e.Effects = DragDropEffects.None;
			_oldIdx = null;
			_newIdx = null;
			_lastEffect = null;
			((RoutedEventArgs)(object)e).Handled = true;
			return;
		}
		int num = editOrderViewModel.Items.IndexOf(editOrderListItemViewModel);
		int dropIndex = details.DropIndex;
		if (num == dropIndex)
		{
			editOrderViewModel.ClearChangeStatus();
			e.Effects = DragDropEffects.None;
			((RoutedEventArgs)(object)e).Handled = true;
			_oldIdx = null;
			_newIdx = null;
			_lastEffect = null;
			return;
		}
		if (num == _oldIdx && dropIndex == _newIdx)
		{
			e.Effects = _lastEffect.Value;
			((RoutedEventArgs)(object)e).Handled = true;
			return;
		}
		_oldIdx = num;
		_newIdx = dropIndex;
		int currentBentProgress = editOrderViewModel.CurrentBentProgress;
		_ = editOrderViewModel.Items.Count;
		if (!editOrderViewModel.MoveBend(num, dropIndex, sync: false))
		{
			_lastEffect = DragDropEffects.None;
			e.Effects = DragDropEffects.None;
			((RoutedEventArgs)(object)e).Handled = true;
			return;
		}
		List<List<Tuple<int, int, int>>> newKeys = AtomicBendDescriptor.DeriveAllKeys(editOrderViewModel.Doc.CombinedBendDescriptors).ToList();
		editOrderViewModel.Undo(sync: false);
		_ = editOrderViewModel.Items.Count;
		editOrderViewModel.SetCurrentBentProgress(currentBentProgress);
		editOrderViewModel.ShowChangingCombinedBendDescriptors(newKeys);
		_lastEffect = DragDropEffects.Move;
		e.Effects = DragDropEffects.Move;
		((RoutedEventArgs)(object)e).Handled = true;
	}

	private void OnMouseEnterInner(object sender, MouseEventArgs e)
	{
		if ((sender as TextBlock).DataContext is EditOrderInnerListItemViewModel editOrderInnerListItemViewModel)
		{
			editOrderInnerListItemViewModel.OuterViewModel.ViewModel.HoveredModel = false;
			editOrderInnerListItemViewModel.OuterViewModel.ViewModel.HoveredBillboards = false;
			editOrderInnerListItemViewModel.HoveredModel = true;
		}
	}

	private void OnMouseLeaveInner(object sender, MouseEventArgs e)
	{
		if ((sender as TextBlock).DataContext is EditOrderInnerListItemViewModel editOrderInnerListItemViewModel)
		{
			editOrderInnerListItemViewModel.OuterViewModel.ViewModel.HoveredModel = false;
			editOrderInnerListItemViewModel.OuterViewModel.ViewModel.HoveredBillboards = false;
		}
	}

	private void OnMouseDoubleClickInner(object sender, MouseButtonEventArgs e)
	{
		TextBlock textBlock = sender as TextBlock;
		if (e.ClickCount == 2 && textBlock.DataContext is EditOrderInnerListItemViewModel editOrderInnerListItemViewModel)
		{
			editOrderInnerListItemViewModel.ViewModel.Split(editOrderInnerListItemViewModel);
			e.Handled = true;
		}
	}

	private void OnMouseEnterOuter(object sender, MouseEventArgs e)
	{
		if (sender is GridViewRow { DataContext: EditOrderListItemViewModel dataContext })
		{
			dataContext.ViewModel.HoveredModel = false;
			dataContext.ViewModel.HoveredBillboards = false;
			dataContext.HoveredModel = true;
		}
	}

	private void OnMouseLeaveOuter(object sender, MouseEventArgs e)
	{
		Mouse.OverrideCursor = null;
		if (sender is GridViewRow { DataContext: EditOrderListItemViewModel dataContext })
		{
			dataContext.ViewModel.HoveredModel = false;
			dataContext.ViewModel.HoveredBillboards = false;
		}
	}

	private static T FindParent<T>(DependencyObject child) where T : DependencyObject
	{
		while (child != null)
		{
			DependencyObject parent = VisualTreeHelper.GetParent(child);
			if (parent is T result)
			{
				return result;
			}
			child = parent;
		}
		return null;
	}
}
