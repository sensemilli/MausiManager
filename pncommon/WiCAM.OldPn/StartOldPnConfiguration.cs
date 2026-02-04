using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Xml.Serialization;

namespace WiCAM.OldPn;

[Serializable]
public class StartOldPnConfiguration
{
	public string PNDRIVE { get; set; }

	public string PNHOMEDRIVE { get; set; }

	public string ARDRIVE { get; set; }

	public string PNHOMEPATH { get; set; }

	public string StartParameters { get; set; }

	public List<string> FromNewHomeToOldFileCopyList { get; set; }

	public List<string> FromOldHomeToNewFileCopyList { get; set; }

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public static StartOldPnConfiguration Load(string filename)
	{
		if (!File.Exists(filename))
		{
			return null;
		}
		StartOldPnConfiguration startOldPnConfiguration = null;
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(StartOldPnConfiguration));
		try
		{
			using FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			return (StartOldPnConfiguration)xmlSerializer.Deserialize(stream);
		}
		catch (Exception)
		{
			return null;
		}
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public void Save(string filename)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(StartOldPnConfiguration));
		try
		{
			using TextWriter textWriter = new StreamWriter(filename);
			xmlSerializer.Serialize(textWriter, this);
		}
		catch (Exception)
		{
		}
	}
}
