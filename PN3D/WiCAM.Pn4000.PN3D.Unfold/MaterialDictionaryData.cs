using System;
using System.IO;
using System.Security.Permissions;
using System.Xml.Serialization;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnPathServices;

namespace WiCAM.Pn4000.PN3D.Unfold;

[Serializable]
public class MaterialDictionaryData
{
	private static string filename = "MaterialDictionary.xml";

	public SerializableDictionary<string, int> Data { get; set; }

	public MaterialDictionaryData()
	{
		this.Data = new SerializableDictionary<string, int>();
	}

	public static MaterialDictionaryData Load(IPnPathService pathService)
	{
		try
		{
			MaterialDictionaryData materialDictionaryData = null;
			string currentFileName = MaterialDictionaryData.GetCurrentFileName(pathService);
			if (File.Exists(currentFileName))
			{
				using FileStream stream = new FileStream(currentFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				materialDictionaryData = (MaterialDictionaryData)new XmlSerializer(typeof(MaterialDictionaryData)).Deserialize(stream);
			}
			if (materialDictionaryData == null)
			{
				return new MaterialDictionaryData();
			}
			return materialDictionaryData;
		}
		catch
		{
			return new MaterialDictionaryData();
		}
	}

	private static string GetCurrentFileName(IPnPathService pnPathService)
	{
		if (File.Exists(MaterialDictionaryData.filename))
		{
			return MaterialDictionaryData.filename;
		}
		return pnPathService.GetFileInGFiles(MaterialDictionaryData.filename);
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public void Save(IPnPathService pnPathService, ILogCenterService logCenterService)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(MaterialDictionaryData));
		try
		{
			using TextWriter textWriter = new StreamWriter(MaterialDictionaryData.GetCurrentFileName(pnPathService));
			xmlSerializer.Serialize(textWriter, this);
		}
		catch (Exception e)
		{
			logCenterService.CatchRaport(e);
		}
	}

	internal void Clear()
	{
		this.Data = new SerializableDictionary<string, int>();
	}
}
