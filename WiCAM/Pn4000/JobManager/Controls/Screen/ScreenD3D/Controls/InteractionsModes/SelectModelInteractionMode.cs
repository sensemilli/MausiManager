// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes.SelectModelInteractionMode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryTools;

namespace WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes
{
  public class SelectModelInteractionMode : IInteractionsMode
  {
    private Model _latestModel;
    private Action<Model> _returnDataAction;
    private Action<bool> _finishAction;
    private readonly Color _highlightColor = new Color(1f, 0.0f, 0.0f, 1f);
    private IInteractionsMode _lastInterActionMode;
    private Screen3D _screen;

    public SelectModelInteractionMode(
      Screen3D screen,
      Action<Model> returnDataAction,
      Action<bool> finishAction)
    {
      this._screen = screen;
      this._returnDataAction = returnDataAction;
      this._finishAction = finishAction;
      this._lastInterActionMode = this._screen.InteractionMode;
    }

    public void Activate()
    {
      this._latestModel = (Model) null;
      this._screen.ImageHost.MouseMove += new MouseEventHandler(this.ImageHostMouseMoveSelectModel);
      this._screen.ImageHost.MouseUp += new MouseButtonEventHandler(this.ImageHostMouseUpSelectModel);
      this._screen.ImageHost.MouseDown += new MouseButtonEventHandler(this.ImageHostMouseDownSelectModel);
      this._screen.ExternalKeyDown += new KeyEventHandler(this.KeyboardClickSelectModel);
    }

    public void Deactivate()
    {
      this._screen.ImageHost.MouseMove -= new MouseEventHandler(this.ImageHostMouseMoveSelectModel);
      this._screen.ImageHost.MouseUp -= new MouseButtonEventHandler(this.ImageHostMouseUpSelectModel);
      this._screen.ImageHost.MouseDown -= new MouseButtonEventHandler(this.ImageHostMouseDownSelectModel);
      this._screen.ExternalKeyDown -= new KeyEventHandler(this.KeyboardClickSelectModel);
    }

    public void IgnoreMouseMove(bool value)
    {
    }

    public void ImageHostMouseMove(object sender, MouseEventArgs e) => throw new NotImplementedException();

    public void ImageHostMouseUp(object sender, MouseButtonEventArgs e) => throw new NotImplementedException();

    public void ImageHostMouseDown(object sender, MouseButtonEventArgs e) => throw new NotImplementedException();

    private void KeyboardClickSelectModel(object sender, KeyEventArgs e)
    {
      if (!Keyboard.IsKeyDown(Key.Escape))
        return;
      this._screen.InteractionMode = this._lastInterActionMode;
      this._finishAction(false);
    }

    private void ImageHostMouseMoveSelectModel(object sender, MouseEventArgs e)
    {
      if (this._screen.ClickSphere.Enabled)
      {
        this._screen.ClickSphere.Enabled = false;
        this._screen.ScreenD3D.Render(true);
      }
      Model model = (Model) null;
      float X;
      float Y;
      this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) sender), out X, out Y);
      Triangle triangle = this._screen.ScreenD3D.PickTriangle(X, Y);
      if (triangle != null)
        model = triangle.Face.Shell.Model;
      if (model == this._latestModel)
        return;
      if (this._latestModel != null)
      {
        this._latestModel.UnHighLightModel();
        this._screen.ScreenD3D.UpdateModelAppearance(this._latestModel);
      }
      if (model != null)
      {
        model.HighLightModel(this._highlightColor);
        this._screen.ScreenD3D.UpdateModelAppearance(model);
      }
      Action<Model> returnDataAction = this._returnDataAction;
      if (returnDataAction != null)
        returnDataAction(model);
      this._latestModel = model;
      this._screen.ScreenD3D.Render(true);
    }

    private void ImageHostMouseDownSelectModel(object sender, MouseButtonEventArgs e)
    {
    }

    private void ImageHostMouseUpSelectModel(object sender, MouseButtonEventArgs e)
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
  }
}
