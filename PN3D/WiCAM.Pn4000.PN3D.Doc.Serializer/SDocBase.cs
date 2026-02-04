using System;
using Newtonsoft.Json;
using WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer;

public class SDocBase
{
	public int Version { get; set; }

	public static SDocBase Convert(IDoc3d doc)
	{
		return SPnBndDoc.Convert(doc);
	}

	public static SPnBndDoc ConvertBack(JsonSerializer jsonSerializer, JsonTextReader jsonReader, int version)
	{
		return (SPnBndDoc)((SDocBase)jsonSerializer.Deserialize(jsonReader, typeof(SPnBndDoc))).GetUpdatedDoc();
	}

	public virtual SDocBase GetUpdatedDoc()
	{
		throw new NotImplementedException();
	}
}
