using System;

namespace WiCAM.Pn4000.PN3D.CommonBend;

internal class ToolModelLoadError : Exception
{
	public string ToolFilename { get; }

	public override string Message => "Tool model import failed: " + this.ToolFilename;

	public ToolModelLoadError(string toolFilename)
	{
		this.ToolFilename = toolFilename;
	}
}
