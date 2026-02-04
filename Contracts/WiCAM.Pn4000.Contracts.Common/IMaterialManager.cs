using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Common;

public interface IMaterialManager
{
	string MaterialErrorString { get; }

	string Mat3DGroupFile { get; }

	List<IMaterialArt> MaterialList { get; }

	Dictionary<int, string> MaterialGroup { get; }

	List<IMaterialUnf> Material3DGroups { get; }

	Dictionary<int, string> Material3DGroup { get; }

	string? GetMaterial3DGroupName(int id);

	string GetMaterialGroupName(int id);

	string GetMaterialName(int id);

	IMaterialArt GetMaterialById(int id);

	int GetMaterialIdByName(string v);

	int GetMaterialGroupIdByName(string name);

	int GetMaterial3DGroupNameByMaterialName(string name);

	int GetMaterial3DGroupIdByName(string name);

	string GetClosestMaterial3DGroupName(string initial);

	string GetClosestMaterialGroupName(string initial);

	string GetClosestMaterialName(string initial);
}
