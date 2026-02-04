using System;
using System.IO;

namespace WiCAM.Pn4000.PN3D.Doc;

public class PnBndFile
{
	public PnBndFileHeader Header { get; set; }

	public PnBndFile(string fileName = null)
	{
		this.Header = new PnBndFileHeader
		{
			ModelSourceFileName = fileName,
			ModelName = Path.GetFileNameWithoutExtension((!string.IsNullOrEmpty(fileName)) ? fileName.Trim() : string.Empty)
		};
	}

	public void SetCreateData(string ModelSourceFileName)
	{
		this.Header.CreateDate = DateTime.Now.ToString();
		this.Header.ModelSourceFileName = ModelSourceFileName;
	}

	public void SetBeforeSaveData(string? pkernelVersion)
	{
		this.Header.FileVersion = "1.0.0.5";
		this.Header.PkernelVersion = pkernelVersion;
		this.Header.Author = Environment.UserName;
		this.Header.PnUserPath = Directory.GetCurrentDirectory();
		this.Header.SaveDate = DateTime.Now.ToString();
	}
}
