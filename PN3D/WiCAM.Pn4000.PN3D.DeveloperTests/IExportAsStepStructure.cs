using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

public interface IExportAsStepStructure
{
	void Export(string dir, IDoc3d doc);
}
