using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer;

public class AsmSerializer
{
	public static void SerializeAndCompress(string targetPath, global::WiCAM.Pn4000.PN3D.Assembly.Assembly assembly)
	{
		SAsmBase sAsmBase = AsmSerializer.ConvertAssembly(assembly);
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
		streamWriter.WriteLine("version: " + sAsmBase.MajorVersion + "." + sAsmBase.MinorVersion);
		obj.Serialize(jsonWriter, sAsmBase);
	}

	public static global::WiCAM.Pn4000.PN3D.Assembly.Assembly DecompressAndDeserialize(string sourcePath)
	{
		if (!File.Exists(sourcePath))
		{
			return null;
		}
		double num = -1.0;
		using FileStream stream = new FileStream(sourcePath, FileMode.Open);
		using GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
		using StreamReader streamReader = new StreamReader(stream2);
		if (num < 0.0)
		{
			string text = streamReader.ReadLine().ToLowerInvariant().Trim();
			if (text.StartsWith("version:"))
			{
				num = Convert.ToDouble(text.Substring(8).Trim());
			}
		}
		using JsonTextReader reader = new JsonTextReader(streamReader);
		JsonSerializer jsonSerializer = new JsonSerializer
		{
			Formatting = Formatting.None,
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Auto
		};
		if (num == 0.0)
		{
			return AsmSerializer.ConvertBackAssembly(jsonSerializer.Deserialize<SAsm>(reader));
		}
		throw new Exception($"Assembly Version {num} undefined for deserialization");
	}

	private static SAsmBase ConvertAssembly(global::WiCAM.Pn4000.PN3D.Assembly.Assembly assembly)
	{
		int num = 0;
		Dictionary<global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart, int> dictPartId = new Dictionary<global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart, int>();
		foreach (global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart disassemblyPart in assembly.DisassemblyParts)
		{
			dictPartId.Add(disassemblyPart, num++);
		}
		return new SAsm
		{
			MajorVersion = assembly.MajorVersion,
			MinorVersion = assembly.MinorVersion,
			FilenameImport = assembly.FilenameImport,
			RootPartName = assembly.RootPartName,
			LastOpenedPartId = assembly.LastOpenedPartId,
			ProcessCode = Convert.ToInt32(assembly.ProcessCode),
			DisassemblyParts = assembly.DisassemblyParts.Select((global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart x) => x.GetConvertedPart()).ToList(),
			RootNode = Conv(assembly.RootNode)
		};
		SAsmDisassemblyPartNode Conv(DisassemblyPartNode node)
		{
			return new SAsmDisassemblyPartNode
			{
				Children = node.Children.Select(Conv).ToList(),
				Transform = node.Transform.GetComponents().ToArray(),
				HiddenInAssembly = node.HiddenInAssembly,
				SuppressedInAssembly = node.SuppressedInAssembly,
				DisassemblyPartId = ((node.Part == null) ? (-1) : dictPartId[node.Part])
			};
		}
	}

	private static global::WiCAM.Pn4000.PN3D.Assembly.Assembly ConvertBackAssembly(SAsm assembly)
	{
		List<global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart> list = assembly.DisassemblyParts.Select((global::WiCAM.Pn4000.PartsReader.DataClasses.DisassemblyPart x) => new global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart(x)).ToList();
		int num = 0;
		Dictionary<int, global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart> dictPartId = new Dictionary<int, global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart>();
		foreach (global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart item in list)
		{
			dictPartId.Add(num++, item);
		}
		return new global::WiCAM.Pn4000.PN3D.Assembly.Assembly
		{
			MajorVersion = assembly.MajorVersion,
			MinorVersion = assembly.MinorVersion,
			FilenameImport = assembly.FilenameImport,
			RootPartName = assembly.RootPartName,
			LastOpenedPartId = assembly.LastOpenedPartId,
			ProcessCode = (F2exeReturnCode)assembly.ProcessCode,
			DisassemblyParts = list,
			RootNode = Conv(assembly.RootNode)
		};
		DisassemblyPartNode Conv(SAsmDisassemblyPartNode node)
		{
			DisassemblyPartNode disassemblyPartNode = new DisassemblyPartNode
			{
				Children = node.Children.Select(Conv).ToList(),
				HiddenInAssembly = node.HiddenInAssembly,
				SuppressedInAssembly = node.SuppressedInAssembly,
				Transform = global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Deserialize(node.Transform),
				Part = ((node.DisassemblyPartId < 0) ? null : dictPartId[node.DisassemblyPartId])
			};
			foreach (DisassemblyPartNode child in disassemblyPartNode.Children)
			{
				child.Parent = disassemblyPartNode;
			}
			return disassemblyPartNode;
		}
	}
}
