using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WiCAM.Pn4000.Screen;
using WiCAM.Pn4000.Screen.SystemLibs;

namespace WiCAM.Pn4000.PN3D.Draw._2D;

public class ScreenPolygon : IScreenGeometry
{
	private IScreen2D iScreen2D;

	public Color Color { get; set; }

	public List<Point> Points { get; set; }

	public double Width { get; set; }

	public ScreenPolygon()
		: this(new List<Point>(), 1.0, Colors.Black)
	{
	}

	public ScreenPolygon(List<Point> points)
		: this(points, 1.0, Colors.Black)
	{
	}

	public ScreenPolygon(List<Point> points, double width)
		: this(points, width, Colors.Black)
	{
	}

	public ScreenPolygon(List<Point> points, double width, Color c)
	{
		this.Points = points;
		this.Width = width;
		this.Color = c;
	}

	public void ToScreen()
	{
		GL.glPolygonMode(1032u, 6913u);
		GL.glColor3f((int)this.Color.R, (int)this.Color.G, (int)this.Color.B);
		GL.glLineWidth((float)this.Width);
		GL.glBegin(9u);
		foreach (Point point in this.Points)
		{
			GL.glVertex2f((float)point.X, (float)((double)this.iScreen2D.Height() - point.Y));
		}
		GL.glEnd();
		GL.glPolygonMode(1032u, 6914u);
	}

	internal void SetServiceProvider(IScreen2D screen2D)
	{
		this.iScreen2D = screen2D;
	}
}
