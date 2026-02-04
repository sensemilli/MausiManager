using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.PN3D.Doc;

public interface IDocSerializer
{
	void SerializeAndCompress(string targetPath, IDoc3d doc);

	Model DeserializeGeometry(string sourcePath);

	IDoc3d DecompressAndDeserialize(string sourcePath);
}
