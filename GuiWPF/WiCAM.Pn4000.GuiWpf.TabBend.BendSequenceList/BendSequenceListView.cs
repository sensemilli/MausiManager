using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.DragDrop;
using WiCAM.Pn4000.WpfControls.DragDrop;
using DragEventArgs = Telerik.Windows.DragDrop.DragEventArgs;

namespace WiCAM.Pn4000.GuiWpf.TabBend.BendSequenceList;

public partial class BendSequenceListView : UserControl, IComponentConnector, IStyleConnector
{
	public static readonly DependencyProperty MyStorageIdProperty = DependencyProperty.Register("StorageId", typeof(string), typeof(BendSequenceListView), new PropertyMetadata(null, StorageIdChanged));

	public string StorageId
	{
		get
		{
			return (string)GetValue(MyStorageIdProperty);
		}
		set
		{
			SetValue(MyStorageIdProperty, value);
		}
	}

	private static void StorageIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is BendSequenceListView)
		{
			_ = e.NewValue;
			_ = e.OldValue;
		}
	}

	public BendSequenceListView()
	{
		InitializeComponent();
		RowReorderBehavior.InitializeAdvanced(GridViewSequence, RowDragOverEvent, DropEvent);
	}

	private new void DropEvent(DropIndicationDetails details, DragEventArgs e)
	{
		if (base.DataContext is BendSequenceListViewModel bendSequenceListViewModel)
		{
			bendSequenceListViewModel.MoveBend(details, e);
			((RoutedEventArgs)(object)e).Handled = true;
		}
	}

	private void RowDragOverEvent(DropIndicationDetails details, DragEventArgs e)
	{
		if (base.DataContext is BendSequenceListViewModel)
		{
			_ = details.CurrentDraggedOverItem is BendSequenceListViewModel.BendViewModel;
		}
	}

	private void EventSetter_MouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: BendSequenceListViewModel.BendViewModel dataContext })
		{
			dataContext.IsHovering = true;
		}
	}

	private void EventSetter_MouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: BendSequenceListViewModel.BendViewModel dataContext })
		{
			dataContext.IsHovering = false;
		}
	}

	private void EventSetter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: BendSequenceListViewModel.BendViewModel dataContext } && base.DataContext is BendSequenceListViewModel)
		{
			dataContext.SetCurrentBend();
		}
	}

	private void GridViewSequence_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
	{
		if (e.OriginalSource is FrameworkElement { DataContext: BendSequenceListViewModel.BendViewModel dataContext } && base.DataContext is BendSequenceListViewModel bendSequenceListViewModel)
		{
			bendSequenceListViewModel.OpenContextMenu(dataContext);
			e.Handled = true;
		}
	}

	private void GridViewSequence_OnPreparedCellForEdit(object? sender, GridViewPreparingCellForEditEventArgs e)
	{
		if (!(e.EditingElement is RadComboBox radComboBox))
		{
			return;
		}
		radComboBox.SelectionChanged += delegate(object s, SelectionChangedEventArgs a)
		{
			if (a.AddedItems.Count > 0)
			{
				ParentOfTypeExtensions.ParentOfType<GridViewCell>((DependencyObject)(a.OriginalSource as RadComboBox))?.CommitEdit();
			}
		};
	}
}
