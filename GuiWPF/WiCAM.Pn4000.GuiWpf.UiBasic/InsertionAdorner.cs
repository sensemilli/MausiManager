using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace WiCAM.Pn4000.GuiWpf.UiBasic;

public class InsertionAdorner : Adorner
{
	private readonly bool _isSeparatorHorizontal;

	private readonly AdornerLayer _adornerLayer;

	private static readonly Pen Pen;

	private static readonly PathGeometry Triangle;

	public bool IsInFirstHalf { get; set; }

	static InsertionAdorner()
	{
		Pen = new Pen
		{
			Brush = Brushes.Gray,
			Thickness = 2.0
		};
		Pen.Freeze();
		LineSegment lineSegment = new LineSegment(new Point(0.0, -5.0), isStroked: false);
		lineSegment.Freeze();
		LineSegment lineSegment2 = new LineSegment(new Point(0.0, 5.0), isStroked: false);
		lineSegment2.Freeze();
		PathFigure pathFigure = new PathFigure
		{
			StartPoint = new Point(5.0, 0.0)
		};
		pathFigure.Segments.Add(lineSegment);
		pathFigure.Segments.Add(lineSegment2);
		pathFigure.Freeze();
		Triangle = new PathGeometry();
		Triangle.Figures.Add(pathFigure);
		Triangle.Freeze();
	}

	public InsertionAdorner(bool isSeparatorHorizontal, bool isInFirstHalf, UIElement adornedElement, AdornerLayer adornerLayer)
		: base(adornedElement)
	{
		_isSeparatorHorizontal = isSeparatorHorizontal;
		IsInFirstHalf = isInFirstHalf;
		_adornerLayer = adornerLayer;
		base.IsHitTestVisible = false;
		_adornerLayer.Add(this);
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		CalculateStartAndEndPoint(out var startPoint, out var endPoint);
		drawingContext.DrawLine(Pen, startPoint, endPoint);
		if (_isSeparatorHorizontal)
		{
			DrawTriangle(drawingContext, startPoint, 0.0);
			DrawTriangle(drawingContext, endPoint, 180.0);
		}
		else
		{
			DrawTriangle(drawingContext, startPoint, 90.0);
			DrawTriangle(drawingContext, endPoint, -90.0);
		}
	}

	private void DrawTriangle(DrawingContext drawingContext, Point origin, double angle)
	{
		drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
		drawingContext.PushTransform(new RotateTransform(angle));
		drawingContext.DrawGeometry(Pen.Brush, null, Triangle);
		drawingContext.Pop();
		drawingContext.Pop();
	}

	private void CalculateStartAndEndPoint(out Point startPoint, out Point endPoint)
	{
		startPoint = default(Point);
		endPoint = default(Point);
		double width = base.AdornedElement.RenderSize.Width;
		double height = base.AdornedElement.RenderSize.Height;
		if (_isSeparatorHorizontal)
		{
			endPoint.X = width;
			if (!IsInFirstHalf)
			{
				startPoint.Y = height;
				endPoint.Y = height;
			}
		}
		else
		{
			endPoint.Y = height;
			if (!IsInFirstHalf)
			{
				startPoint.X = width;
				endPoint.X = width;
			}
		}
	}

	public void Detach()
	{
		_adornerLayer.Remove(this);
	}
}
