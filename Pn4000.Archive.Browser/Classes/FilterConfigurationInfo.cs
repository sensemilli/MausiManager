using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.Archive.Browser.Classes
{
	[Serializable]
	public class FilterConfigurationInfo
	{
		[XmlAttribute]
		public int Index
		{
			get;
			set;
		}

		[XmlAttribute]
		public string Key
		{
			get;
			set;
		}

		public FilterConfigurationInfo()
		{
		}
	}
}