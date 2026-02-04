using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Serialization;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Doc.Serializer;
using WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

namespace WiCAM.Pn4000.BendDoc;

public class DocSerializer : IDocSerializer
{
	private readonly IGlobals _globals;

	private readonly IFactorio _factorio;

	public DocSerializer(IGlobals globals, IFactorio factorio)
	{
		this._globals = globals;
		this._factorio = factorio;
	}

	public void SerializeAndCompress(string targetPath, IDoc3d doc)
	{
		SPnBndDoc sPnBndDoc = this._factorio.Resolve<IDocConverter>().ConvertDoc(doc, skipBendModel: false);
		using FileStream stream = new FileStream(targetPath, FileMode.Create);
		using GZipStream stream2 = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
		using StreamWriter streamWriter = new StreamWriter(stream2);
		using JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter);
		JsonSerializer obj = new JsonSerializer
		{
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Auto
		};
		streamWriter.WriteLine("version: " + sPnBndDoc.Version);
		obj.Serialize(jsonWriter, sPnBndDoc);
	}

	public IDoc3d DecompressAndDeserialize(string sourcePath)
	{
		return (Doc3d)this.DecompressAndDeserialize(sourcePath, onlyGeometry: false);
	}

	public Model DeserializeGeometry(string sourcePath)
	{
		return (Model)this.DecompressAndDeserialize(sourcePath, onlyGeometry: true);
	}

	private object DecompressAndDeserialize(string sourcePath, bool onlyGeometry)
	{
		if (!File.Exists(sourcePath))
		{
			return null;
		}
		int num = -1;
		while (true)
		{
			using FileStream stream = new FileStream(sourcePath, FileMode.Open);
			using GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
			using StreamReader streamReader = new StreamReader(stream2);
			StreamReader reader = streamReader;
			if (num >= 0)
			{
				goto IL_0064;
			}
			string text = streamReader.ReadLine().ToLowerInvariant().Trim();
			if (text.StartsWith("version:"))
			{
				num = Convert.ToInt32(text.Substring(8).Trim());
				goto IL_0064;
			}
			num = 0;
			goto end_IL_0024;
			IL_0064:
			if (num == 0)
			{
				Dictionary<string, string> obj = new Dictionary<string, string> { { "\"$type\": \"WiCAM.Pn4000.PN3D.Doc.Serializer.SerializationStructure.", "\"$type\": \"WiCAM.Pn4000.PN3D.Doc.Serializer.Version1." } };
				string text2 = streamReader.ReadToEnd();
				foreach (KeyValuePair<string, string> item in obj)
				{
					text2 = text2.Replace(item.Key, item.Value);
				}
				MemoryStream memoryStream = new MemoryStream();
				StreamWriter streamWriter = new StreamWriter(memoryStream);
				streamWriter.Write(text2);
				streamWriter.Flush();
				memoryStream.Seek(0L, SeekOrigin.Begin);
				reader = new StreamReader(memoryStream);
			}
			using (JsonTextReader jsonTextReader = new JsonTextReader(reader))
			{
				JsonSerializer jsonSerializer = new JsonSerializer
				{
					Formatting = Formatting.None,
					NullValueHandling = NullValueHandling.Ignore,
					TypeNameHandling = TypeNameHandling.Auto
				};
				if (onlyGeometry)
				{
					GPnBndDoc gPnBndDoc = jsonSerializer.Deserialize<GPnBndDoc>(jsonTextReader);
					return new ModelConverter().Convert(gPnBndDoc.EntryModel ?? gPnBndDoc.InputModel);
				}
				SPnBndDoc sDoc = SDocBase.ConvertBack(jsonSerializer, jsonTextReader, num);
				return this._factorio.Resolve<IDocConverter>().ConvertDoc(sDoc, sourcePath);
			}
			end_IL_0024:;
		}
	}
}
