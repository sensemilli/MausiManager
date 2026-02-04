using System.Collections.Generic;
using System.IO;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PartsReader;
using WiCAM.Pn4000.PartsReader.Contracts;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Pipes;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

public class ExportAsStepStructure : IExportAsStepStructure
{
	private readonly IPN3DDocPipe _docPipe;

	private readonly IPnPathService _pathService;

	public ExportAsStepStructure(IPN3DDocPipe docPipe, IPnPathService pathService)
	{
		this._docPipe = docPipe;
		this._pathService = pathService;
	}

	public void Export(string dir, IDoc3d doc)
	{
		if (!File.Exists(Path.Combine(this._pathService.FolderCad3d2Pn, "Parts.xml")))
		{
			this._docPipe.AnalyzeDisassemblyData(doc);
		}
		if (!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}
		string path = Path.Combine(this._pathService.PNHOMEPATH, "cad3d2pn");
		List<global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart> list = new List<global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart>();
		foreach (global::WiCAM.Pn4000.PartsReader.DataClasses.DisassemblyPart item in ((IPartsReader)new global::WiCAM.Pn4000.PartsReader.PartsReader()).DeserializeAssembly(Path.Combine(this._pathService.FolderCad3d2Pn, "Parts.xml"))?.DisassemblyParts)
		{
			list.Add(new global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart(item));
		}
		foreach (global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart item2 in list)
		{
			string text = Path.Combine(dir, item2.PartInfo.PartType.ToString());
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (string.IsNullOrEmpty(item2.Name))
			{
				item2.Name = item2.OriginalGeometryName;
			}
			string text2 = Path.Combine(path, $"{item2.ID}.step");
			if (File.Exists(text2))
			{
				File.Copy(text2, Path.Combine(text, item2.Name + ".step"), overwrite: true);
			}
		}
	}
}
