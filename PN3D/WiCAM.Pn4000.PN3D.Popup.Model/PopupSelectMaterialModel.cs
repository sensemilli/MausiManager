using System.Collections.ObjectModel;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Popup.Model;

public class PopupSelectMaterialModel
{
	private IDoc3d _doc;

	public ObservableCollection<IMaterialArt> Materials { get; set; }

	public IMaterialManager GlobalMaterials { get; set; }

	public double Thickness { get; set; }

	public int? ActualMaterialID { get; set; }

	public IMaterialArt SelectedMaterial { get; set; }

	public PopupSelectMaterialModel(IMaterialManager materials)
	{
		this.Materials = materials.MaterialList.ToObservableCollection();
		this.GlobalMaterials = materials;
	}

	public void Init(IDoc3d doc)
	{
		this._doc = doc;
		this.Thickness = doc?.Thickness ?? 0.0;
		this.ActualMaterialID = doc.Material?.Number;
		this.SelectedMaterial = doc?.Material;
	}

	public void AcceptMaterialAsUser()
	{
		this._doc.PnMaterialByUser = true;
	}
}
