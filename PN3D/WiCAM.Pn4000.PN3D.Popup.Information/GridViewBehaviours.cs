using System.Windows;
using System.Windows.Controls;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class GridViewBehaviours
{
	public static readonly DependencyProperty CollapsableColumnProperty = DependencyProperty.RegisterAttached("CollapseableColumn", typeof(bool), typeof(GridViewBehaviours), new UIPropertyMetadata(false, CollapsableColumnChanged));

	public static bool GetCollapsableColumn(DependencyObject d)
	{
		return (bool)d.GetValue(GridViewBehaviours.CollapsableColumnProperty);
	}

	public static void SetCollapsableColumn(DependencyObject d, bool value)
	{
		d.SetValue(GridViewBehaviours.CollapsableColumnProperty, value);
	}

	private static void CollapsableColumnChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
	{
		if (sender is GridViewColumnHeader gridViewColumnHeader)
		{
			gridViewColumnHeader.IsVisibleChanged += AdjustWidth;
		}
	}

	private static void AdjustWidth(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is GridViewColumnHeader gridViewColumnHeader)
		{
			gridViewColumnHeader.Column.Width = ((gridViewColumnHeader.Visibility == Visibility.Collapsed) ? 0.0 : double.NaN);
		}
	}
}
