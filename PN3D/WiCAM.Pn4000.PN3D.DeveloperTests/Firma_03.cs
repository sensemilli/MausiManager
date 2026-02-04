using System;
using System.Collections.Generic;
using System.IO;
using WiCAM.Pn4000.PartsReader;
using WiCAM.Pn4000.PartsReader.Contracts;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PKernelFlow.Adapters.Data;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

internal class Firma_03
{
	public void Execute(IDoc3d CurrentDoc)
	{
		AsmComAdapter.AsmComCount = 10;
		for (int i = 0; i < 10; i++)
		{
			AsmComAdapter.SetElement(i, new AsmComEntity
			{
				PartName = "asname",
				Iasarc = 0,
				LengthX = 0f,
				LengthY = 0f,
				LengthZ = 0f,
				PartId = 0f,
				InstanceNumber = 0f,
				Zasrot = 0f,
				Iasanz = 0,
				PartType = "Name2",
				Thickness = 1f
			});
		}
	}

	public void Execute_Old(IDoc3d CurrentDoc)
	{
		if (!File.Exists("C:\\pnusers\\default\\cad3d2pn\\Parts.xml"))
		{
			return;
		}
		string environmentVariable = Environment.GetEnvironmentVariable("PNHOMEDRIVE");
		string text = Environment.GetEnvironmentVariable("PNHOMEPATH") + "\\cad3d2pn\\";
		string path = environmentVariable + text + "name.txt";
		string text2 = "";
		if (!File.Exists(path))
		{
			return;
		}
		string[] array = File.ReadAllLines(path);
		if (array.Length == 1)
		{
			int num = array[0].LastIndexOf("\\");
			int num2 = array[0].LastIndexOf(".");
			text2 = array[0].Substring(num + 1, num2 - num - 1);
		}
		string text3 = "D:\\test\\" + text2 + "\\";
		if (!Directory.Exists(text3))
		{
			Directory.CreateDirectory(text3);
		}
		List<global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart> list = new List<global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart>();
		foreach (global::WiCAM.Pn4000.PartsReader.DataClasses.DisassemblyPart item in ((IPartsReader)new global::WiCAM.Pn4000.PartsReader.PartsReader()).DeserializeAssembly("C:\\pnusers\\default\\cad3d2pn\\Parts.xml")?.DisassemblyParts)
		{
			list.Add(new global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart(item));
		}
		foreach (global::WiCAM.Pn4000.PN3D.Assembly.DisassemblyPart item2 in list)
		{
			string text4 = text3 + item2.PartInfo.PartType.ToString() + "\\";
			if (!Directory.Exists(text4))
			{
				Directory.CreateDirectory(text4);
			}
			File.Copy(environmentVariable + text + item2.ID + ".step", text4 + item2.Name + ".step", overwrite: true);
		}
	}
}
