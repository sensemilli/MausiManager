namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SPnBndFileHeader
{
	public string FileVersion { get; set; }

	public string PkernelVersion { get; set; }

	public string Author { get; set; }

	public string PnUserPath { get; set; }

	public string ModelSourceFileName { get; set; }

	public string SaveDate { get; set; }

	public string CreateDate { get; set; }

	public bool IsFromDisassembly { get; set; }

	public string ModelName { get; set; }

	public string ImportedFilename { get; set; }
}
