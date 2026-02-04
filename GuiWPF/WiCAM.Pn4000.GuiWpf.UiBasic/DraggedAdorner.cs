using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WiCAM.Pn4000.GuiWpf.UiBasic;

public class DraggedAdorner : Adorner
{
	private readonly ContentPresenter _contentPresenter;

	private double _left;

	private double _top;

	private readonly AdornerLayer _adornerLayer;

	protected override int VisualChildrenCount => 1;

	public DraggedAdorner(object dragDropData, DataTemplate dragDropTemplate, UIElement adornedElement, AdornerLayer adornerLayer)
		: base(adornedElement)
	{
		_adornerLayer = adornerLayer;
		_contentPresenter = new ContentPresenter
		{
			Content = dragDropData,
			ContentTemplate = dragDropTemplate,
			Opacity = 0.7
		};
		_adornerLayer.Add(this);
	}

	public void SetPosition(double left, double top)
	{
		_left = left - 1.0;
		_top = top + 13.0;
		if (_adornerLayer != null)
		{
			try
			{
				_adornerLayer.Update(base.AdornedElement);
			}
			catch
			{
			}
		}
	}

	protected override Size MeasureOverride(Size constraint)
	{
		_contentPresenter.Measure(constraint);
		return _contentPresenter.DesiredSize;
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		_contentPresenter.Arrange(new Rect(finalSize));
		return finalSize;
	}

	protected override Visual GetVisualChild(int index)
	{
		return _contentPresenter;
	}

	public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
	{
		return new GeneralTransformGroup
		{
			Children = 
			{
				base.GetDesiredTransform(transform),
				(GeneralTransform)new TranslateTransform(_left, _top)
			}
		};
	}

	public void Detach()
	{
		_adornerLayer.Remove(this);
	}
}
