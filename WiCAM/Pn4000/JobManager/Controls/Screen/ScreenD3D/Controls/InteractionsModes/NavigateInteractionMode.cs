// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes.NavigateInteractionMode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;
using WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks;

namespace WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes
{
  public class NavigateInteractionMode : IInteractionsMode
  {
    protected Screen3D _screen;
    protected bool _wasIgnoredMouseMove;
    protected float _lastMouseDownX;
    protected float _lastMouseDownY;
    protected MouseRotationMode _rotationMode;
    private Triangle _lastTriangle;
    private bool _mouseMoveRegistered;

    protected Vector3d RotationPoint { get; set; }

    public event Action ViewChanged;

    public NavigateInteractionMode(Screen3D screen) => this._screen = screen;

    public virtual void Activate()
    {
      this._screen.ImageHost.MouseDown += new MouseButtonEventHandler(this.ImageHostMouseDown);
      this._screen.ImageHost.MouseUp += new MouseButtonEventHandler(this.ImageHostMouseUp);
      this._screen.ImageHost.MouseMove += new MouseEventHandler(this.ImageHostMouseMove);
      this._screen.ImageHost.MouseWheel += new MouseWheelEventHandler(this.ImageHostMouseWheel);
      this._mouseMoveRegistered = true;
    }

    public virtual void Deactivate()
    {
      this._screen.ImageHost.MouseDown -= new MouseButtonEventHandler(this.ImageHostMouseDown);
      this._screen.ImageHost.MouseUp -= new MouseButtonEventHandler(this.ImageHostMouseUp);
      this._screen.ImageHost.MouseMove -= new MouseEventHandler(this.ImageHostMouseMove);
      this._screen.ImageHost.MouseWheel -= new MouseWheelEventHandler(this.ImageHostMouseWheel);
      this._mouseMoveRegistered = false;
    }

    public void IgnoreMouseMove(bool ignore)
    {
      if (ignore)
      {
        this._wasIgnoredMouseMove = ignore;
        this._screen.ImageHost.MouseMove -= new MouseEventHandler(this.ImageHostMouseMove);
        this._mouseMoveRegistered = false;
      }
      else
      {
        if (this._mouseMoveRegistered)
          return;
        this._screen.ImageHost.MouseMove += new MouseEventHandler(this.ImageHostMouseMove);
        this._mouseMoveRegistered = true;
      }
    }

    public virtual void ImageHostMouseMove(object sender, MouseEventArgs e)
    {
      float X;
      float Y;
      this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) this._screen), out X, out Y);
      if (this._wasIgnoredMouseMove)
      {
        this._wasIgnoredMouseMove = false;
        this._screen.ScreenD3D.Renderer.LastMouseX = X;
        this._screen.ScreenD3D.Renderer.LastMouseY = Y;
      }
      else
      {
        Matrix transformation = this._screen.ScreenD3D.Renderer.Root.Transform.Value;
        double velocity = (this.RotationPoint - this._screen.GetCamWorldPos(X, Y, ref transformation)).Length * 0.001;
        EventWaitHandle waitHande = new EventWaitHandle(false, EventResetMode.ManualReset);
        Action<RenderTaskResult> action = (Action<RenderTaskResult>) (result => waitHande.Set());
        Matrix? transform1 = this._screen.ScreenD3D.Renderer.Root.Transform;
        this._screen.ScreenD3D.MouseMove(X, Y, (float) velocity, this.RotationPoint, this._rotationMode, e.LeftButton == MouseButtonState.Pressed, e.RightButton == MouseButtonState.Pressed, e.MiddleButton == MouseButtonState.Pressed, action);
        waitHande.WaitOne();
        Matrix? transform2 = this._screen.ScreenD3D.Renderer.Root.Transform;
        Matrix? nullable = transform1;
        if ((transform2.HasValue == nullable.HasValue ? (transform2.HasValue ? (transform2.GetValueOrDefault() != nullable.GetValueOrDefault() ? 1 : 0) : 0) : 1) != 0)
        {
          Action viewChanged = this.ViewChanged;
          if (viewChanged != null)
            viewChanged();
        }
        if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed)
        {
          this._screen.ScreenD3D.Render(true);
        }
        else
        {
          if (this._screen.MouseLeaveTriangle != null || this._screen.MouseEnterTriangle != null)
          {
            Triangle e1 = this._screen.ScreenD3D.PickTriangle(X, Y);
            int? id1 = e1?.ID;
            int? id2 = this._lastTriangle?.ID;
            if (!(id1.GetValueOrDefault() == id2.GetValueOrDefault() & id1.HasValue == id2.HasValue))
            {
              EventHandler<Triangle> mouseLeaveTriangle = this._screen.MouseLeaveTriangle;
              if (mouseLeaveTriangle != null)
                mouseLeaveTriangle((object) this, this._lastTriangle);
              EventHandler<Triangle> mouseEnterTriangle = this._screen.MouseEnterTriangle;
              if (mouseEnterTriangle != null)
                mouseEnterTriangle((object) this, e1);
              this._lastTriangle = e1;
            }
          }
          if (!this._screen.ClickSphere.Enabled)
            return;
          this._screen.ClickSphere.Enabled = false;
          this._screen.ScreenD3D.Render(true);
        }
      }
    }

    public virtual void ImageHostMouseWheel(object sender, MouseWheelEventArgs e)
    {
      float X;
      float Y;
      this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) this._screen), out X, out Y);
      this.RotationPoint = this._screen.ScreenD3D.ClosestPoint(X, Y);
      Action<RenderTaskResult> callback = new Action<RenderTaskResult>(this.ImageHostMouseWheelDone);
      EventWaitHandle waitHandleMouse = new EventWaitHandle(false, EventResetMode.ManualReset);
      EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
      Action<RenderTaskResult> action1 = (Action<RenderTaskResult>) (result => waitHandleMouse.Set());
      Action<RenderTaskResult> action2 = (Action<RenderTaskResult>) (result =>
      {
        waitHandle.Set();
        callback(result);
      });
      this._screen.ScreenD3D.MouseWheel(X, Y, this.RotationPoint, this._screen.MouseWheelInverted ? e.Delta : -e.Delta, action1);
      waitHandleMouse.WaitOne();
      this._screen.ScreenD3D.Render(true, action2);
      waitHandle.WaitOne();
      Action viewChanged = this.ViewChanged;
      if (viewChanged == null)
        return;
      viewChanged();
    }

    public virtual void ImageHostMouseWheelDone(RenderTaskResult renderTaskResult) => this._screen.ShowClickSphere(this.RotationPoint);

    public virtual void ImageHostMouseUp(object sender, MouseButtonEventArgs e)
    {
      float X;
      float Y;
      this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) this._screen), out X, out Y);
      if ((double) X == (double) this._lastMouseDownX && (double) Y == (double) this._lastMouseDownY)
      {
        Vector3d hitPoint;
        Triangle tri = this._screen.ScreenD3D.PickTriangle(X, Y, (HashSet<Model>) null, out hitPoint);
        this._screen.ExecuteTriangleSelected(this, tri, e, hitPoint);
        this._screen.DebugInfo.SelectTriangle(tri);
      }
      if (!this._screen.ClickSphere.Enabled)
        return;
      this._screen.ClickSphere.Enabled = false;
      this._screen.ScreenD3D.Render(true);
    }

    public virtual void ImageHostMouseDown(object sender, MouseButtonEventArgs e)
    {
      float X;
      float Y;
      this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) this._screen), out X, out Y);
      if (e.ChangedButton != MouseButton.Left && e.ChangedButton != MouseButton.Right)
        return;
      this.RotationPoint = this._screen.ScreenD3D.ClosestPoint(X, Y);
      Matrix transformation = this._screen.ScreenD3D.Renderer.Root.Transform.Value;
      double length = (this.RotationPoint - this._screen.GetCamWorldPos(X, Y, ref transformation)).Length;
      this._screen.ShowClickSphere(this.RotationPoint);
      this._lastMouseDownX = X;
      this._lastMouseDownY = Y;
      this._screen.ScreenD3D.Renderer.LastMouseX = X;
      this._screen.ScreenD3D.Renderer.LastMouseY = Y;
    }
  }
}
