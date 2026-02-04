using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration.ViewModels;

namespace WiCAM.Pn4000.PN3D.BendSimulation.Configuration.Views;

public class ScreenControlBaseView : UserControl
{
	public ScreenControlBaseView()
	{
		base.MouseEnter += OnMouseEnter;
		base.MouseLeave += OnMouseLeave;
		base.MouseMove += OnMouseMove;
		base.MouseDown += OnMouseDown;
		base.MouseUp += OnMouseUp;
		base.IsVisibleChanged += OnIsVisibleChanged;
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		(base.DataContext as ScreenControlBaseViewModel)?.OnVisibilityChanged((bool)e.NewValue);
	}

	private void WindowOnKeyDown(object sender, KeyEventArgs e)
	{
		(base.DataContext as ScreenControlBaseViewModel)?.OnKeyDown(sender, e);
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		(base.DataContext as ScreenControlBaseViewModel)?.OnMouseMove(sender, e);
	}

	private void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		(base.DataContext as ScreenControlBaseViewModel)?.OnMouseDown(sender, e);
	}

	private void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		(base.DataContext as ScreenControlBaseViewModel)?.OnMouseUp(sender, e);
	}

	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
		(base.DataContext as ScreenControlBaseViewModel)?.OnMouseEnter(sender, e);
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		(base.DataContext as ScreenControlBaseViewModel)?.OnMouseLeave(sender, e);
	}
}
