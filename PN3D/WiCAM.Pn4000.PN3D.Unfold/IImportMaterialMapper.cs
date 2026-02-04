using System.Collections.Generic;
using System.Collections.ObjectModel;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Popup.Information;

namespace WiCAM.Pn4000.PN3D.Unfold;

public interface IImportMaterialMapper
{
	int GetMaterialId(string key);

	void AddMaterial(string key, int value);

	ObservableCollection<VisualMaterialAllianceItem> GetMvvm(IMaterialManager materials);

	void SetFromMvvm(IEnumerable<VisualMaterialAllianceItem> items);
}
