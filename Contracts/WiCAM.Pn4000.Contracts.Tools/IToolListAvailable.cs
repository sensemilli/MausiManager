using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolListAvailable
{
	int Id { get; set; }

	string Description { get; set; }

	Dictionary<IAliasPieceProfile, int> PiecesAvailable { get; set; }
}
