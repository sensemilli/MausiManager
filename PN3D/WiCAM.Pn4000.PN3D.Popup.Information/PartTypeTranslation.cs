using System.Windows;
using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class PartTypeTranslation
{
	public static string Get(PartType type)
	{
		string resourceKey = string.Format("l_enum.DisassemblyPartType.{0}", type.ToString().Replace(" ", "").Replace(",", ""));
		string text = Application.Current.TryFindResource(resourceKey) as string;
		if (text == null)
		{
			text = type.ToString();
		}
		return text;
	}
}
