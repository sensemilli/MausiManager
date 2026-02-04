using System.Collections.Generic;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class UserProperty
{
	public string Name { get; set; }

	public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}
