using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IAliasPieceProfile
{
	int Id { get; set; }

	IAliasMultiToolProfile AliasMultiTool { get; set; }

	IReadOnlyCollection<IToolPieceProfile> Pieces { get; }

	void AddToolPieceProfile(IToolPieceProfile profile);

	void RemoveToolPieceProfile(IToolPieceProfile profile);
}
