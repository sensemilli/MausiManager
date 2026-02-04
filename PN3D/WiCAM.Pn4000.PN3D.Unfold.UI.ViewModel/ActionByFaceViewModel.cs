using System;
using System.Collections.Generic;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class ActionByFaceViewModel : SubViewModelBase
{
	protected readonly IScreen3D Screen;

	private readonly Func<ITriangleEventArgs, bool> _checkPossibleFaceFunc;

	private readonly Action<ITriangleEventArgs> _validFaceSelectedAction;

	private readonly Color _highlightColor;

	private readonly string _descTranslateKey;

	private readonly ITranslator _translator;

	private readonly bool _highlightEdges;

	private bool _isActive;

	private Triangle? _selectedTriangle;

	private global::WiCAM.Pn4000.BendModel.Model? _selectedModel;

	public ActionByFaceViewModel(ITranslator translator, IScreen3D screen, Color highlightColor, bool highlightEdges, string descTranslateKey, Func<ITriangleEventArgs, bool> checkPossibleFaceFunc, Action<ITriangleEventArgs> validFaceSelectedAction)
	{
		this._translator = translator;
		this._highlightEdges = highlightEdges;
		this._highlightColor = highlightColor;
		this.Screen = screen;
		this._checkPossibleFaceFunc = checkPossibleFaceFunc;
		this._validFaceSelectedAction = validFaceSelectedAction;
		this._descTranslateKey = descTranslateKey;
		this.SetActive(active: true);
	}

	public override bool Close()
	{
		if (this._selectedTriangle != null)
		{
			this._selectedTriangle = null;
			this._selectedModel = null;
			base.RaiseRequestRepaint();
		}
		if (this._isActive)
		{
			this.Screen.MouseEnterTriangle -= MouseEnterTriangle;
			this.Screen.MouseLeaveTriangle -= MouseLeaveTriangle;
			this.Screen.HideScreenInfoText();
		}
		return base.Close();
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		if (active != this._isActive)
		{
			this._isActive = active;
			if (this._isActive)
			{
				this.Screen.MouseEnterTriangle += MouseEnterTriangle;
				this.Screen.MouseLeaveTriangle += MouseLeaveTriangle;
				List<string> screenInfoText = new List<string>
				{
					this._translator.Translate(this._descTranslateKey),
					this._translator.Translate("UnfoldView.SelectFaceAbort")
				};
				this.Screen.SetScreenInfoText(screenInfoText);
			}
			else
			{
				this.Screen.MouseEnterTriangle -= MouseEnterTriangle;
				this.Screen.MouseLeaveTriangle -= MouseLeaveTriangle;
				this.Screen.HideScreenInfoText();
			}
		}
	}

	private void MouseEnterTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		if (this._checkPossibleFaceFunc(e) && (this._selectedTriangle != e.Tri || this._selectedModel != e.Model))
		{
			this._selectedTriangle = e.Tri;
			this._selectedModel = e.Model;
			base.RaiseRequestRepaint();
		}
	}

	private void MouseLeaveTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		if (e.Tri != null && e.Tri == this._selectedTriangle && e.Model == this._selectedModel)
		{
			this._selectedTriangle = null;
			this._selectedModel = null;
			base.RaiseRequestRepaint();
		}
	}

	public override void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		base.MouseSelectTriangle(sender, e);
		if (e.MouseEventArgs.ChangedButton == MouseButton.Left)
		{
			if (this._checkPossibleFaceFunc(e))
			{
				this._validFaceSelectedAction?.Invoke(e);
				this.Close();
			}
		}
		else
		{
			this.Close();
		}
		e.MouseEventArgs.Handled = true;
	}

	public override void ColorModelParts(IPaintTool paintTool)
	{
		base.ColorModelParts(paintTool);
		if (this._selectedTriangle == null)
		{
			return;
		}
		if (this._highlightEdges)
		{
			if (this._selectedTriangle?.Face == null)
			{
				return;
			}
			{
				foreach (FaceHalfEdge allEdge in this._selectedTriangle.Face.GetAllEdges())
				{
					paintTool.SetEdgeColor(allEdge, this._selectedModel, this._highlightColor, 5f);
				}
				return;
			}
		}
		paintTool.SetFaceColor(this._selectedTriangle?.Face, this._selectedModel, this._highlightColor);
	}
}
