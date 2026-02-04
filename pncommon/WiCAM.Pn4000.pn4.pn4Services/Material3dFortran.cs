using System.Linq;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PKernelFlow.Adapters;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class Material3dFortran : IMaterial3dFortran
{
	private readonly IMaterialManager _materialManager;

	public Material3dFortran(IMaterialManager materialManager)
	{
		this._materialManager = materialManager;
	}

	public IMaterialArt GetActiveMaterial(bool isAssembly)
	{
		int num = (int)GeneralSystemComponentsAdapter.Material;
		foreach (IMaterialArt material in this._materialManager.MaterialList)
		{
			if (material.Number == num)
			{
				return material;
			}
		}
		if (!isAssembly)
		{
			return null;
		}
		IMaterialArt materialArt = this._materialManager.MaterialList.MinBy((IMaterialArt x) => x.Number);
		if (materialArt != null)
		{
			this.SetActiveMaterial(materialArt.Number);
		}
		return materialArt;
	}

	public void SetActiveMaterial(int matId)
	{
		GeneralSystemComponentsAdapter.Material = matId;
		GeneralSystemComponentsAdapter.UpdateInfoPaneForMaterialAndThickness();
	}
}
