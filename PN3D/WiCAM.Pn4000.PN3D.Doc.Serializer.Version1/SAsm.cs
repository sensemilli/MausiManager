using System.Collections.Generic;
using WiCAM.Pn4000.PartsReader.DataClasses;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SAsm : SAsmBase
{
	public string RootPartName { get; set; }

	public string FilenameImport { get; set; }

	public int? LastOpenedPartId { get; set; }

	public List<DisassemblyPart> DisassemblyParts { get; set; } = new List<DisassemblyPart>();

	public SAsmDisassemblyPartNode RootNode { get; set; }

	public int ProcessCode { get; set; }
}
