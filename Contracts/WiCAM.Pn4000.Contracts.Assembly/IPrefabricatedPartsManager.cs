using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Assembly;

public interface IPrefabricatedPartsManager
{
	bool IgnoreNonHorizontalPlaneConnectedPp { get; }

	bool IsDetectionPrefabricatedPartsByNameActive { get; }

	bool DetectEverythingAsPpInAssembly { get; }

	double MaxDistOfPurchasePartsToSheetMetal { get; }

	void AddPart(IPrefabricatedPart pp);

	void RemovePart(string partName);

	IPrefabricatedPart? FindPart(string name, bool checkDetectionEnabled);

	IEnumerable<IPrefabricatedPart> AllParts();

	IEnumerable<(int typeId, string typeDesc)> GetPartTypesOrdered();

	void SetConfig(bool ignoreNonHorizontalPlaneConnectedPp, bool isDetectionPrefabricatedPartsByNameActive, bool detectEverythingAsPpInAssembly, double MaxDistOfPurchasePartsToSheetMetal);

	void SetData(IEnumerable<(int typeId, string typeDesc)> types, IEnumerable<IPrefabricatedPart> parts);

	void ImportXml(string filename);

	string TypeToStr(int type);
}
