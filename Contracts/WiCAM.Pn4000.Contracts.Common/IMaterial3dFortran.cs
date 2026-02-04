namespace WiCAM.Pn4000.Contracts.Common;

public interface IMaterial3dFortran
{
	void SetActiveMaterial(int matId);

	IMaterialArt GetActiveMaterial(bool isAssembly);
}
