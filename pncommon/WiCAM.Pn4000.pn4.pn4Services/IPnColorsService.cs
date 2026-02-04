using System;
using System.Drawing;
using System.Windows.Media;

namespace WiCAM.Pn4000.pn4.pn4Services;

public interface IPnColorsService : IDisposable
{
	void Init();

	global::System.Drawing.Pen GetPen(int colorIdx);

	global::System.Windows.Media.Brush GetWpfBrush(int colorIdx);

	global::System.Windows.Media.Color GetWpfColor(int colorIdx);

	global::System.Drawing.Color GetDrawingColor(int colorIdx);

	new void Dispose();
}
