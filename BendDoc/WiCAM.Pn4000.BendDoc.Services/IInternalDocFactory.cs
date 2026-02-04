using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.BendDoc.Services;

public interface IInternalDocFactory : IDoc3dFactory
{
	Doc3d InternalCreateDoc();

	Doc3d InternalCreateDoc(string filename);
}
