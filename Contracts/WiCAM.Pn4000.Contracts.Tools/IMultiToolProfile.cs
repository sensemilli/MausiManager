using System.Collections.Generic;
using System.Linq;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IMultiToolProfile
{
	int ID { get; set; }

	string? CustomMultiToolName { get; set; }

	string Name => this.CustomMultiToolName ?? string.Join(" + ", this.ToolProfiles.Select((IToolProfile x) => x.Name));

	bool IsUpper { get; }

	IReadOnlyCollection<IToolProfile> ToolProfiles { get; }

	IReadOnlyCollection<IToolPieceProfile> PieceProfiles { get; }

	IAliasMultiToolProfile Aliases { get; set; }

	double AliasRotation { get; set; }

	string GeometryFile { get; set; }

	string GeometryFileFull { get; set; }

	double OffsetFront { get; set; }

	double OffsetBack { get; set; }

	double OffsetTop { get; }

	double OffsetBottom { get; }

	double AnchorPoint { get; }

	double AnchorPointAbs { get; }

	int MountTypeId
	{
		get
		{
			return this.Plug.Id.Value;
		}
		set
		{
			this.Plug.Id = value;
		}
	}

	InstallationDirection PlugInstallationDirection { get; set; }

	IPlug Plug { get; set; }

	IReadOnlyList<double> Heights { get; }

	void AddToolProfile(IToolProfile profile);

	void RemoveToolProfile(IToolProfile profile);

	void SetToolProfile(List<IToolProfile> profile);

	void AddToolPieceProfile(IEnumerable<IToolPieceProfile> profiles);

	void AddToolPieceProfile(IToolPieceProfile profile);

	void CalculateHeights();
}
