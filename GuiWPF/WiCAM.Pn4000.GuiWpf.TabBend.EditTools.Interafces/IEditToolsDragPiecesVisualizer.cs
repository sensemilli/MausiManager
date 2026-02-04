using System.Windows.Input;
using WiCAM.Pn4000.Contracts.PaintTools;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools.Interafces;

internal interface IEditToolsDragPiecesVisualizer
{
	bool IsDragging { get; }

	void ColorModelParts(IPaintTool painter);

	void MouseMove(object sender, MouseEventArgs e);

	void Start();

	double Stop();

	int? GetIndexForDistance(double distance);
}
