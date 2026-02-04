namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface IL26MaterialMapper
{
	string GetTrumpfMaterial(int pnMaterialNumber, double thickness);

	string GetWicamMaterial(string trumpfMaterial);
}
