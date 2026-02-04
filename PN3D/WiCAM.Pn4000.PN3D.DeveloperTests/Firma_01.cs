using WiCAM.Pn4000.ModelConverter;
using WiCAM.Pn4000.ModelConverter.SAT.Enums;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

internal class Firma_01
{
	public void Execute(IDoc3d CurrentDoc)
	{
		ModelToSatConverter modelToSatConverter = new ModelToSatConverter();
		modelToSatConverter.ConvertModelToSat(CurrentDoc.EntryModel3D, "d:\\v7.sat");
		modelToSatConverter.ConvertModelToSat(CurrentDoc.EntryModel3D, "d:\\v5.sat", SatVersion.V5);
	}
}
