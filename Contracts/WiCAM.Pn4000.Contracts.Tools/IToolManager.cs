using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolManager
{
	IToolListManager ToolListManager { get; }

	IEnumerable<IToolPieceProfile> GetToolPieces(int profileId, bool adapter, bool upper);

	IEnumerable<IToolProfile> GetUpperToolProfiles(int groupId);

	IEnumerable<IToolProfile> GetUpperAdapterProfilesByGroup(int groupId);

	IEnumerable<IToolProfile> GetUpperAdapterProfiles();

	IEnumerable<IToolProfile> GetLowerToolProfiles(int groupId);

	IEnumerable<IToolProfile> GetLowerAdapterProfilesByGroup(int groupId);

	IEnumerable<IToolProfile> GetLowerAdapterProfiles();

	IEnumerable<IToolProfile> GetLowerToolExtensionProfiles();

	double LengthModulo();

	double LengthModulo(List<IToolPieceProfile> tools);
}
