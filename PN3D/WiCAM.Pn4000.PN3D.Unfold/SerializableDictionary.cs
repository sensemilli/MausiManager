using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.Unfold;

[XmlRoot("dictionary")]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(TKey));
		XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(TValue));
		bool isEmptyElement = reader.IsEmptyElement;
		reader.Read();
		if (!isEmptyElement)
		{
			reader.ReadStartElement("item");
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				reader.ReadStartElement("key");
				TKey key = (TKey)xmlSerializer.Deserialize(reader);
				reader.ReadEndElement();
				reader.ReadStartElement("value");
				TValue value = (TValue)xmlSerializer2.Deserialize(reader);
				reader.ReadEndElement();
				base.Add(key, value);
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}
	}

	public void WriteXml(XmlWriter writer)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(TKey));
		XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(TValue));
		writer.WriteStartElement("item");
		foreach (TKey key in base.Keys)
		{
			writer.WriteStartElement("key");
			xmlSerializer.Serialize(writer, key);
			writer.WriteEndElement();
			writer.WriteStartElement("value");
			xmlSerializer2.Serialize(writer, base[key]);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}
}
