using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WiCAM.Pn4000.PN3D.Popup.UI;

public class GridViewSort
{
	public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(GridViewSort), new UIPropertyMetadata(null, delegate(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		if (o is ItemsControl itemsControl && !GridViewSort.GetAutoSort(itemsControl))
		{
			if (e.OldValue != null && e.NewValue == null)
			{
				itemsControl.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
			}
			if (e.OldValue == null && e.NewValue != null)
			{
				itemsControl.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
			}
		}
	}));

	public static readonly DependencyProperty AutoSortProperty = DependencyProperty.RegisterAttached("AutoSort", typeof(bool), typeof(GridViewSort), new UIPropertyMetadata(false, delegate(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		if (o is ListView listView && GridViewSort.GetCommand(listView) == null)
		{
			bool num = (bool)e.OldValue;
			bool flag = (bool)e.NewValue;
			if (num && !flag)
			{
				listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
			}
			if (!num && flag)
			{
				listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
			}
		}
	}));

	public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.RegisterAttached("PropertyName", typeof(string), typeof(GridViewSort), new UIPropertyMetadata(null));

	public static ICommand GetCommand(DependencyObject obj)
	{
		return (ICommand)(obj?.GetValue(GridViewSort.CommandProperty));
	}

	public static void SetCommand(DependencyObject obj, ICommand value)
	{
		obj?.SetValue(GridViewSort.CommandProperty, value);
	}

	public static bool GetAutoSort(DependencyObject obj)
	{
		return (bool)obj?.GetValue(GridViewSort.AutoSortProperty);
	}

	public static void SetAutoSort(DependencyObject obj, bool value)
	{
		obj?.SetValue(GridViewSort.AutoSortProperty, value);
	}

	public static string GetPropertyName(DependencyObject obj)
	{
		return (string)obj?.GetValue(GridViewSort.PropertyNameProperty);
	}

	public static void SetPropertyName(DependencyObject obj, string value)
	{
		obj?.SetValue(GridViewSort.PropertyNameProperty, value);
	}

	private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
	{
		if (!(e.OriginalSource is GridViewColumnHeader gridViewColumnHeader))
		{
			return;
		}
		string text = GridViewSort.GetPropertyName(gridViewColumnHeader.Column);
		if (string.IsNullOrEmpty(text))
		{
			if (gridViewColumnHeader?.Column?.DisplayMemberBinding is Binding binding)
			{
				text = binding.Path.Path;
			}
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
		}
		ListView ancestor = GridViewSort.GetAncestor<ListView>(gridViewColumnHeader);
		if (ancestor == null)
		{
			return;
		}
		ICommand command = GridViewSort.GetCommand(ancestor);
		if (command != null)
		{
			if (command.CanExecute(text))
			{
				command.Execute(text);
			}
		}
		else if (GridViewSort.GetAutoSort(ancestor))
		{
			GridViewSort.ApplySort(ancestor.Items, text);
		}
		if (ancestor.Items.Count > 0)
		{
			ancestor.SelectedItem = ancestor.Items[0];
		}
		else
		{
			ancestor.SelectedItem = null;
		}
	}

	public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
	{
		DependencyObject parent = VisualTreeHelper.GetParent(reference);
		while (!(parent is T))
		{
			parent = VisualTreeHelper.GetParent(parent);
		}
		return (T)parent;
	}

	public static void ApplySort(ICollectionView view, string propertyName)
	{
		ListSortDirection direction = ListSortDirection.Ascending;
		if (view.SortDescriptions.Count > 0)
		{
			SortDescription sortDescription = view.SortDescriptions[0];
			if (sortDescription.PropertyName == propertyName)
			{
				direction = ((sortDescription.Direction == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending);
			}
			view.SortDescriptions.Clear();
		}
		if (!string.IsNullOrEmpty(propertyName))
		{
			view.SortDescriptions.Add(new SortDescription(propertyName, direction));
		}
	}
}
