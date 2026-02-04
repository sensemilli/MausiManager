using System;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class SelectSpecialVisibleFaceViewModel : ActionByFaceViewModel
{
	private readonly bool _restoreAnalyzeColors;

	public SelectSpecialVisibleFaceViewModel(ITranslator translator, Screen3D screen, Color highlightColor, bool highlightEdges, string descTranslateKey, Func<ITriangleEventArgs, bool> checkPossibleFaceFunc, Action<ITriangleEventArgs> validFaceSelectedAction, bool restoreAnalyzeColors)
		: base(translator, screen, highlightColor, highlightEdges, descTranslateKey, checkPossibleFaceFunc, validFaceSelectedAction)
	{
		this._restoreAnalyzeColors = restoreAnalyzeColors;
		base.Closed += isClosing;
	}

	private void isClosing(ISubViewModel arg1, Triangle arg2, global::WiCAM.Pn4000.BendModel.Model model, double arg3, double arg4, Vector3d arg5, MouseButtonEventArgs arg6)
	{
		if (this._restoreAnalyzeColors)
		{
			base.Screen.SetColorsToOriginal(value: false);
		}
		base.Closed -= isClosing;
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		if (this._restoreAnalyzeColors)
		{
			if (active)
			{
				base.Screen.SetColorsToOriginal(value: true);
			}
			else
			{
				base.Screen.SetColorsToOriginal(value: false);
			}
		}
	}
}
