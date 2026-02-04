using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IAliasMultiToolProfile
{
	int ID { get; set; }

	IReadOnlyCollection<IMultiToolProfile> MultiTools { get; }

	IReadOnlyCollection<IAliasPieceProfile> Pieces { get; }

	string GeometryFile { get; set; }

	string GeometryFileFull { get; set; }

	double OffsetFront { get; set; }

	double OffsetBack { get; set; }

	double OffsetTop { get; }

	double OffsetBottom { get; }

	void AddAliasMultiToolProfile(IMultiToolProfile profile);

	void AddAliasToolPieceProfile(IAliasPieceProfile profile);

	void RemoveAliasMultiToolProfile(IMultiToolProfile profile);

	void RemoveAliasToolPieceProfile(IAliasPieceProfile profile);
}
