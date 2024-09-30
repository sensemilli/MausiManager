using System.Diagnostics;
using System.Reflection;

namespace WiCAM.Pn4000.JobManager;

[DebuggerDisplay("{DatKey}   {Property}")]
public class ReadPropertyInfo
{
	public string DatKey { get; set; }

	public PropertyInfo Property { get; set; }

	public ReadPropertyInfo()
	{
	}

	public ReadPropertyInfo(string key, PropertyInfo property)
	{
		DatKey = key;
		Property = property;
	}
}
