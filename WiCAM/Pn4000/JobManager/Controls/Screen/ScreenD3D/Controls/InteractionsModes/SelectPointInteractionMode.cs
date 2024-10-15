// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes.SelectPointInteractionMode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes
{
  public class SelectPointInteractionMode : NavigateInteractionMode
  {
    private IInteractionsMode _lastInterActionMode;
    private Vector3d _latestCursorPoint;
    private Action<Vector3d> _returnDataAction;
    private Action<bool> _finishAction;

    public SelectPointInteractionMode(
      Screen3D screen,
      Action<Vector3d> returnDataAction,
      Action<bool> finishAction)
      : base(screen)
    {
      this._lastInterActionMode = screen.InteractionMode;
      this._returnDataAction = returnDataAction;
      this._finishAction = finishAction;
    }

    public override void Activate()
    {
      this._screen.ExternalKeyDown += new KeyEventHandler(this.KeyboardClickSelectPoint);
      this._screen.TriangleSelected += new Action<NavigateInteractionMode, Triangle, MouseButtonEventArgs, Vector3d>(this._screen_TriangleSelected);
      this._screen.ScreenD3D.ResetSpecialPoints();
      base.Activate();
    }

    public override void Deactivate()
    {
      this._screen.ExternalKeyDown -= new KeyEventHandler(this.KeyboardClickSelectPoint);
      this._screen.TriangleSelected -= new Action<NavigateInteractionMode, Triangle, MouseButtonEventArgs, Vector3d>(this._screen_TriangleSelected);
      base.Deactivate();
    }

    public override void ImageHostMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
        base.ImageHostMouseMove(sender, e);
      else
        this.ImageHostMouseMoveInteractionPoint(sender, e);
    }

    private void KeyboardClickSelectPoint(object sender, KeyEventArgs e)
    {
      if (!Keyboard.IsKeyDown(Key.Escape))
        return;
      this._screen.InteractionMode = this._lastInterActionMode;
      this._finishAction(false);
    }

    private void _screen_TriangleSelected(
      NavigateInteractionMode arg1,
      Triangle arg2,
      MouseButtonEventArgs arg3,
      Vector3d arg4)
    {
      this.ImageHostMouseUpInteractionPoint((object) null, arg3);
    }

    private void ImageHostMouseUpInteractionPoint(object sender, MouseButtonEventArgs e)
    {
      switch (e.ChangedButton)
      {
        case MouseButton.Left:
          this._screen.InteractionMode = this._lastInterActionMode;
          this._finishAction(true);
          break;
        case MouseButton.Right:
          this._screen.InteractionMode = this._lastInterActionMode;
          this._finishAction(false);
          break;
      }
    }

    private void ImageHostMouseMoveInteractionPoint(object sender, MouseEventArgs e)
    {
      float X;
      float Y;
      this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) sender), out X, out Y);
      Vertex pointOnModel;
      bool isSpecialPoint;
      Vector3d point = this._screen.ScreenD3D.ClosestPointOnModel(X, Y, out pointOnModel, out isSpecialPoint);
      if (point == this._latestCursorPoint)
        return;
      if (!isSpecialPoint && pointOnModel != null)
      {
        if (pointOnModel.InboundFaceEdges.Count > 0)
          isSpecialPoint = pointOnModel.InboundFaceEdges[0].StartVertex == pointOnModel || pointOnModel.InboundFaceEdges[0].EndVertex == pointOnModel;
        else if (pointOnModel.OutboundFaceEdges.Count > 0)
          isSpecialPoint = pointOnModel.OutboundFaceEdges[0].StartVertex == pointOnModel || pointOnModel.OutboundFaceEdges[0].EndVertex == pointOnModel;
      }
      int specialColor = 0;
      if (isSpecialPoint)
        specialColor = 1;
      Action<Vector3d> returnDataAction = this._returnDataAction;
      if (returnDataAction != null)
        returnDataAction(point);
      this._latestCursorPoint = point;
      this._screen.ShowClickSphere(point, specialColor);
    }
  }
}
