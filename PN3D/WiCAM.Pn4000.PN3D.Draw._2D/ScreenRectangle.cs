using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WiCAM.Pn4000.PN3D.Draw._2D;

public class ScreenRectangle : ScreenPolygon
{
	public ScreenRectangle(Point lowerLeft, Point upperRight)
		: this(lowerLeft, new Point(upperRight.X, lowerLeft.Y), upperRight, new Point(lowerLeft.X, upperRight.Y), 1.0, Colors.Black)
	{
	}

	public ScreenRectangle(Point lowerLeft, Point upperRight, double width)
		: this(lowerLeft, new Point(upperRight.X, lowerLeft.Y), upperRight, new Point(lowerLeft.X, upperRight.Y), width, Colors.Black)
	{
	}

	public ScreenRectangle(Point lowerLeft, Point upperRight, Color c)
		: this(lowerLeft, new Point(upperRight.X, lowerLeft.Y), upperRight, new Point(lowerLeft.X, upperRight.Y), 1.0, c)
	{
	}

	public ScreenRectangle(Point lowerLeft, Point upperRight, double width, Color c)
		: this(lowerLeft, new Point(upperRight.X, lowerLeft.Y), upperRight, new Point(lowerLeft.X, upperRight.Y), width, c)
	{
	}

	public ScreenRectangle(Point lowerLeft, Point lowerRight, Point upperRight, Point upperLeft)
		: this(lowerLeft, lowerRight, upperRight, upperLeft, 1.0, Colors.Black)
	{
	}

	public ScreenRectangle(Point lowerLeft, Point lowerRight, Point upperRight, Point upperLeft, double width)
		: this(lowerLeft, lowerRight, upperRight, upperLeft, width, Colors.Black)
	{
	}

	public ScreenRectangle(Point lowerLeft, Point lowerRight, Point upperRight, Point upperLeft, double width, Color c)
	{
		base.Points = new List<Point>();
		base.Width = width;
		base.Color = c;
		base.Points.Add(lowerLeft);
		base.Points.Add(lowerRight);
		base.Points.Add(upperRight);
		base.Points.Add(upperLeft);
	}

	public void ReCalculate(Point lowerLeft, Point upperRight)
	{
		Point item = lowerLeft;
		Point item2 = new Point(upperRight.X, lowerLeft.Y);
		Point item3 = upperRight;
		Point item4 = new Point(lowerLeft.X, upperRight.Y);
		base.Points = new List<Point> { item, item2, item3, item4 };
	}
}
