using WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

namespace WiCAM.Pn4000.PN3D.Doc;

public interface IDocConverter
{
	SPnBndDoc ConvertDoc(IDoc3d doc, bool skipBendModel);

	IDoc3d ConvertDoc(SPnBndDoc sDoc, string sourcePath);

	void ConvertDoc(SPnBndDoc sDoc, IDoc3d doc3d, string sourcePath);
}
