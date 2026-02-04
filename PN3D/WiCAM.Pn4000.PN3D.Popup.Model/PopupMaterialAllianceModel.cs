using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Unfold;

namespace WiCAM.Pn4000.PN3D.Popup.Model;

public class PopupMaterialAllianceModel
{
	public bool isCancel { get; set; }

	public IMaterialManager materials { get; set; }

	public IImportMaterialMapper ImportMaterialMapper { get; set; }

	public PopupMaterialAllianceModel(IMaterialManager materials, IImportMaterialMapper importMaterialMapper)
	{
		this.materials = materials;
		this.ImportMaterialMapper = importMaterialMapper;
	}
}
