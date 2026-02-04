using System.Windows.Controls;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.Tools;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class DrawToolProfile : IDrawToolProfile
{
	private readonly IDrawToolProfiles _drawToolProfiles;

	public void Draw(Canvas canvas, IPunchProfile? punchProfile, IBendMachineTools bendMachine)
	{
		_drawToolProfiles.LoadPreview2D(punchProfile?.MultiToolProfile.GeometryFileFull, 0.0, punchProfile?.WorkingHeight ?? 0.0, 0.0, canvas);
	}

	public void Draw(Canvas canvas, IDieProfile? dieProfile, IBendMachineTools bendMachine)
	{
		_drawToolProfiles.LoadPreview2D(dieProfile?.MultiToolProfile.GeometryFileFull, (dieProfile != null) ? (0.0 - dieProfile.WorkingHeight) : 0.0, 0.0, dieProfile?.OffsetInX ?? 0.0, canvas);
	}

	public void DrawAdapter(Canvas canvas, IToolProfile? toolProfile, IBendMachineTools bendMachine)
	{
		if (toolProfile != null)
		{
			if (toolProfile.ProfileType.HasFlag(ToolProfileTypes.Upper))
			{
				_drawToolProfiles.LoadPreview2D(toolProfile?.MultiToolProfile.GeometryFileFull, 0.0, toolProfile?.WorkingHeight ?? 0.0, 0.0, canvas);
			}
			else
			{
				_drawToolProfiles.LoadPreview2D(toolProfile?.MultiToolProfile.GeometryFileFull, (toolProfile != null) ? (0.0 - toolProfile.WorkingHeight) : 0.0, 0.0, 0.0, canvas);
			}
		}
	}

	public DrawToolProfile(IDrawToolProfiles drawToolProfiles)
	{
		_drawToolProfiles = drawToolProfiles;
	}
}
