using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WiCAM.Pn4000.PN3D.Draw._2D;

public interface IScreenGeometry
{
	Color Color { get; set; }

	List<Point> Points { get; set; }

	double Width { get; set; }

	void ToScreen();
}
