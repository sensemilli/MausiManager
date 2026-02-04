using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WiCAM.Pn4000.PN3D.BendSimulation.Configuration.UI;

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
		this._adornerLayer = adornerLayer;
		this._contentPresenter = new ContentPresenter
		{
			Content = dragDropData,
			ContentTemplate = dragDropTemplate,
			Opacity = 0.7
		};
		this._adornerLayer.Add(this);
	}

	public void SetPosition(double left, double top)
	{
		this._left = left - 1.0;
		this._top = top + 13.0;
		if (this._adornerLayer != null)
		{
			try
			{
				this._adornerLayer.Update(base.AdornedElement);
			}
			catch
			{
			}
		}
	}

	protected override Size MeasureOverride(Size constraint)
	{
		this._contentPresenter.Measure(constraint);
		return this._contentPresenter.DesiredSize;
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		this._contentPresenter.Arrange(new Rect(finalSize));
		return finalSize;
	}

	protected override Visual GetVisualChild(int index)
	{
		return this._contentPresenter;
	}

	public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
	{
		return new GeneralTransformGroup
		{
			Children = 
			{
				base.GetDesiredTransform(transform),
				(GeneralTransform)new TranslateTransform(this._left, this._top)
			}
		};
	}

	public void Detach()
	{
		this._adornerLayer.Remove(this);
	}
}
