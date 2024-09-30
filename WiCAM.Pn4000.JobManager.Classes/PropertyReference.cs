using System;
using System.Reflection;

namespace WiCAM.Pn4000.JobManager.Classes;

public class PropertyReference
{
	public PropertyInfo Property { get; set; }

	public Type ItemType { get; set; }
}
