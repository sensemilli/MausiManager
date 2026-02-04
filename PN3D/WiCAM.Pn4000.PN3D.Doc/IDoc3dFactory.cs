namespace WiCAM.Pn4000.PN3D.Doc;

public interface IDoc3dFactory
{
	IDoc3d EmptyDoc { get; }

	IDoc3d CreateDoc(string filename, bool isAssemblyLoading = false, string? assemblyGuid = null, bool isDevImport = false);

	IDoc3d CreateDoc();

	IDoc3d CreateEmptyDoc(string filename);
}
