using System;
using System.ComponentModel;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

public abstract class SubViewModelBase : ViewModelBase, ISubViewModel, IPopupViewModel
{
	public event Action CloseView;

	public event Action<ISubViewModel, Triangle, Model, double, double, Vector3d, MouseButtonEventArgs>? Closed;

	public event Action? RequestRepaint;

	protected void RaiseRequestRepaint()
	{
		this.RequestRepaint?.Invoke();
	}

	public virtual void SetActive(bool active)
	{
	}

	public virtual bool Close()
	{
		this.RequestRepaint = null;
		this.Closed?.Invoke(this, null, null, 0.0, 0.0, Vector3d.Zero, null);
		this.CloseView?.Invoke();
		return true;
	}

	public virtual void KeyUp(object sender, IPnInputEventArgs e)
	{
	}

	public virtual void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
	}

	public virtual void MouseMove(object sender, MouseEventArgs e)
	{
	}

	public virtual void ColorModelParts(IPaintTool paintTool)
	{
	}

	public void ViewClosing(object? sender, CancelEventArgs e)
	{
		this.Close();
	}
}
