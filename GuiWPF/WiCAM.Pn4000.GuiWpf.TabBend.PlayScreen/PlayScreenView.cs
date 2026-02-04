using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WiCAM.Pn4000.GuiWpf.TabBend.PlayScreen;

public partial class PlayScreenView : UserControl, IComponentConnector
{
	private PlayScreenViewModel currentVm;

	public PlayScreenView()
	{
		InitializeComponent();
		base.DataContextChanged += PlayScreenView_DataContextChanged;
		OnDataContextChanged();
	}

	private void PlayScreenView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		OnDataContextChanged();
	}

	private void OnDataContextChanged()
	{
		if (currentVm != null)
		{
			PlayScreenViewModel playScreenViewModel = currentVm;
			playScreenViewModel.SetTimelineIntervals = (Action<double, IEnumerable<(double, double, bool)>>)Delegate.Remove(playScreenViewModel.SetTimelineIntervals, new Action<double, IEnumerable<(double, double, bool)>>(SetTimelineIntervals));
		}
		if (base.DataContext is PlayScreenViewModel playScreenViewModel2)
		{
			playScreenViewModel2.SetTimelineIntervals = (Action<double, IEnumerable<(double, double, bool)>>)Delegate.Combine(playScreenViewModel2.SetTimelineIntervals, new Action<double, IEnumerable<(double, double, bool)>>(SetTimelineIntervals));
			currentVm = playScreenViewModel2;
		}
		else
		{
			currentVm = null;
		}
	}

	private void SetTimelineIntervals(double maxStep, IEnumerable<(double start, double end, bool userAccepted)> listCollisions)
	{
		double num = 0.01 * maxStep;
		CanvasTimelineBackground.Width = maxStep;
		CanvasTimelineBackground.Children.Clear();
		foreach (var listCollision in listCollisions)
		{
			double num2 = listCollision.start;
			double num3 = listCollision.end - num2;
			if (num3 < num)
			{
				num2 -= (num - num3) / 2.0;
				num3 = num;
			}
			num2 = Math.Min(Math.Max(0.0, num2), maxStep - num3);
			Rectangle element = new Rectangle
			{
				Fill = (listCollision.userAccepted ? Brushes.Orange : Brushes.Red),
				Width = listCollision.end - num2,
				Height = 1.0,
				StrokeThickness = 0.0
			};
			Canvas.SetLeft(element, num2);
			Canvas.SetTop(element, 0.0);
			CanvasTimelineBackground.Children.Add(element);
		}
	}

	private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
	{
	}

	private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
	{
	}
}
