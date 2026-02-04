namespace WiCAM.Pn4000.PN3D.Doc;

public class PnBndFileHeader
{
	private string _modelName;

	public string FileVersion { get; set; }

	public string? PkernelVersion { get; set; }

	public string Author { get; set; }

	public string PnUserPath { get; set; }

	public string ModelSourceFileName { get; set; }

	public string SaveDate { get; set; }

	public string CreateDate { get; set; }

	public bool IsFromDisassembly { get; set; }

	public string ImportedFilename { get; set; }

	public string ModelName
	{
		get
		{
			if (this._modelName.EndsWith(".prt"))
			{
				return this._modelName.Remove(this._modelName.Length - 4);
			}
			return this._modelName;
		}
		set
		{
			this._modelName = value;
		}
	}
}
