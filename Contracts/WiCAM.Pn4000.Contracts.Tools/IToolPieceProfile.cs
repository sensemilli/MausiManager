using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolPieceProfile
{
	IReadOnlyCollection<IAliasPieceProfile> Aliases { get; }

	double Length { get; set; }

	int ToolId { get; }

	IMultiToolProfile MultiToolProfile { get; set; }

	[Obsolete("use MultiToolProfile")]
	IToolProfile Profile => this.MultiToolProfile.ToolProfiles.First();

	string GeometryFile { get; set; }

	string GeometryFileFull { get; set; }

	bool IsSpecialTool { get; }

	SpecialToolTypes SpecialToolType { get; }

	Model Model { get; }

	List<HeightInterval> HeightIntervals { get; }

	double MaxToolLoad { get; }

	double? MaxToolLoadOfPiece { get; set; }

	void AddAliasToolPieceProfile(IAliasPieceProfile profile);

	void RemoveAliasToolPieceProfile(IAliasPieceProfile profile);
}
