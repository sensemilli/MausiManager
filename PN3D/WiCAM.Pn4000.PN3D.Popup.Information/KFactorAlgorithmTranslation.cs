using System.Windows;
using WiCAM.Pn4000.Contracts.BendDataBase;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class KFactorAlgorithmTranslation
{
	public static string GetTranslation(BendTableReturnValues algorithm)
	{
		string resourceKey = string.Format("l_enum.KFactorAlgorithm.{0}", algorithm.ToString().Replace(" ", "").Replace(",", ""));
		string text = Application.Current.TryFindResource(resourceKey) as string;
		if (text == null)
		{
			text = algorithm.ToString();
		}
		return text;
	}

	public static string GetExplanation(BendTableReturnValues algorithm)
	{
		string resourceKey = string.Format("l_enum.KFactorAlgorithm.{0}", algorithm.ToString().Replace(" ", "").Replace(",", "") + "_EXPLANATION");
		string text = Application.Current.TryFindResource(resourceKey) as string;
		if (text == null)
		{
			text = algorithm.ToString();
		}
		return text;
	}
}
