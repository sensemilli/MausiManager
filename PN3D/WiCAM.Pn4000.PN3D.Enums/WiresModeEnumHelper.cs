using System;
using System.Linq;

namespace WiCAM.Pn4000.PN3D.Enums;

public class WiresModeEnumHelper
{
	public static WiresMode GetNextEnumValueOf(WiresMode value)
	{
		return (from WiresMode val in global::System.Enum.GetValues(typeof(WiresMode))
			where val > value
			orderby val
			select val).DefaultIfEmpty().First();
	}
}
