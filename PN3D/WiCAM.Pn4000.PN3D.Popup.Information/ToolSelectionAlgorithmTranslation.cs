using System.Windows;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class ToolSelectionAlgorithmTranslation
{
	public static string GetTranslation(ToolSelectionType algorithm)
	{
		string resourceKey = string.Format("l_enum.ToolSelectionAlgorithm.{0}", algorithm.ToString().Replace(" ", "").Replace(",", ""));
		string text = Application.Current.TryFindResource(resourceKey) as string;
		if (text == null)
		{
			text = algorithm.ToString();
		}
		return text;
	}

	public static string GetExplanation(ToolSelectionType algorithm)
	{
		string resourceKey = string.Format("l_enum.ToolSelectionAlgorithm.{0}", algorithm.ToString().Replace(" ", "").Replace(",", "") + "_EXPLANATION");
		string text = Application.Current.TryFindResource(resourceKey) as string;
		if (text == null)
		{
			text = algorithm.ToString();
		}
		return text;
	}
}
