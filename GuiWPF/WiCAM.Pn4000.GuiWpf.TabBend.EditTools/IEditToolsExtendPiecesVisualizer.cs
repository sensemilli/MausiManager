using System.Collections.Generic;
using System.Windows.Input;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public interface IEditToolsExtendPiecesVisualizer
{
	bool IsActive { get; }

	bool IsExtendingLeft { get; }

	void MouseMove(object sender, MouseEventArgs e);

	void ColorModelParts(IPaintTool painter);

	void Start(bool extendLeft);

	IDictionary<IToolSection, double> Stop();
}
