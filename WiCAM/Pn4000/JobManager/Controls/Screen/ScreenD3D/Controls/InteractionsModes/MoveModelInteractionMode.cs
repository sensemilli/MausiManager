// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes.MoveModelInteractionMode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes
{
  public class MoveModelInteractionMode : NavigateInteractionMode
  {
    protected MoveModelInteractionMode.Mode _mode;
    protected Model _xArrow;
    protected Model _yArrow;
    protected Model _zArrow;
    protected HashSet<Model> _selectableModels;
    protected Vector3d _moveHitPoint;

    public double MinX { get; set; } = double.MinValue;

    public double MaxX { get; set; } = double.MinValue;

    public double MinY { get; set; } = double.MinValue;

    public double MaxY { get; set; } = double.MinValue;

    public double MinZ { get; set; } = double.MinValue;

    public double MaxZ { get; set; } = double.MinValue;

    public bool UseWorldCoordinates { get; set; }

    public bool RenderMotion { get; set; } = true;

    protected Model _selectedModel { get; set; }

    public event Action<MoveModelInteractionMode, Model> ObjectSelected;

    public event Action<MoveModelInteractionMode, Model, Vector3d> ObjectMoving;

    public event Action<MoveModelInteractionMode, Model> ObjectMoved;

    public event Action<MoveModelInteractionMode, Model, Vector3d> ResetArrows;

    public MoveModelInteractionMode(
      Screen3D screen,
      HashSet<Model> selectableModels,
      Model xArrow,
      Model yArrow,
      Model zArrow,
      double minX = -1.7976931348623157E+308,
      double maxX = -1.7976931348623157E+308,
      double minY = -1.7976931348623157E+308,
      double maxY = -1.7976931348623157E+308,
      double minZ = -1.7976931348623157E+308,
      double maxZ = -1.7976931348623157E+308)
      : base(screen)
    {
      this._selectableModels = selectableModels;
      if (xArrow != null)
      {
        this._xArrow = xArrow;
        this._xArrow.ModelType = ModelType.System;
        this._xArrow.PartRole = PartRole.ArrowX;
      }
      if (yArrow != null)
      {
        this._yArrow = yArrow;
        this._yArrow.ModelType = ModelType.System;
        this._yArrow.PartRole = PartRole.ArrowY;
      }
      if (zArrow != null)
      {
        this._zArrow = zArrow;
        this._zArrow.ModelType = ModelType.System;
        this._zArrow.PartRole = PartRole.ArrowZ;
      }
      this.MinX = minX;
      this.MinY = minY;
      this.MinZ = minZ;
      this.MaxX = maxX;
      this.MaxY = maxY;
      this.MaxZ = maxZ;
    }

    public override void Activate()
    {
      this._selectedModel = (Model) null;
      this._mode = MoveModelInteractionMode.Mode.ObjectSelected;
      base.Activate();
    }

    protected void RaiseMovedEvent(MoveModelInteractionMode mode, Model model)
    {
      Action<MoveModelInteractionMode, Model> objectMoved = this.ObjectMoved;
      if (objectMoved == null)
        return;
      objectMoved(this, this._selectedModel);
    }

    protected void RaiseSelectedEvent(MoveModelInteractionMode mode, Model model)
    {
      Action<MoveModelInteractionMode, Model> objectSelected = this.ObjectSelected;
      if (objectSelected == null)
        return;
      objectSelected(this, this._selectedModel);
    }

    protected void RaiseMovingEvent(MoveModelInteractionMode mode, Model model, Vector3d pos)
    {
      Action<MoveModelInteractionMode, Model, Vector3d> objectMoving = this.ObjectMoving;
      if (objectMoving == null)
        return;
      objectMoving(this, this._selectedModel, pos);
    }

    public override void Deactivate()
    {
      this.RemoveArrows();
      base.Deactivate();
    }

    protected virtual void AddArrows(Model parent, Vector3d translation)
    {
      Matrix4d matrix4d1 = Matrix4d.Translation(translation);
      Matrix4d matrix4d2 = Matrix4d.Scale(4.0, 4.0, 4.0) * matrix4d1;
      if (this._xArrow != null)
      {
        this._xArrow.Transform = matrix4d2;
        this._screen.ScreenD3D.AddModel(this._xArrow, parent, false);
      }
      if (this._yArrow != null)
      {
        this._yArrow.Transform = Matrix4d.RotationZ(Math.PI) * matrix4d2;
        this._screen.ScreenD3D.AddModel(this._yArrow, parent, false);
      }
      if (this._zArrow == null)
        return;
      this._zArrow.Transform = matrix4d2;
      this._screen.ScreenD3D.AddModel(this._zArrow, parent, true);
    }

    protected virtual void RemoveArrows()
    {
      if (this._xArrow != null)
        this._screen.ScreenD3D.RemoveModel(this._xArrow, false);
      if (this._yArrow != null)
        this._screen.ScreenD3D.RemoveModel(this._yArrow, false);
      if (this._zArrow == null)
        return;
      this._screen.ScreenD3D.RemoveModel(this._zArrow, true);
    }

    public override void ImageHostMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this._mode == MoveModelInteractionMode.Mode.MoveX || this._mode == MoveModelInteractionMode.Mode.MoveY || this._mode == MoveModelInteractionMode.Mode.MoveZ)
      {
        Action<MoveModelInteractionMode, Model> objectMoved = this.ObjectMoved;
        if (objectMoved != null)
          objectMoved(this, this._selectedModel);
        this._mode = MoveModelInteractionMode.Mode.ObjectSelected;
      }
      else if (this._mode == MoveModelInteractionMode.Mode.Navigate)
      {
        this._mode = MoveModelInteractionMode.Mode.ObjectSelected;
      }
      else
      {
        float X;
        float Y;
        this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) sender), out X, out Y);
        Vector3d hitPoint;
        if (this.ShowArrows(this._screen.ScreenD3D.PickTriangle(X, Y, (HashSet<Model>) null, out hitPoint)?.Face.Shell.Model, hitPoint))
          return;
        base.ImageHostMouseUp(sender, e);
      }
    }

    public virtual bool ShowArrows(Model model, Vector3d hitPoint)
    {
      this.RemoveArrows();
      if (model != null)
      {
        lock (this._screen.ScreenD3D.Renderer)
        {
          Matrix4d matrix4d = model.WorldMatrix;
          matrix4d = matrix4d.Inverted;
          matrix4d.TransformInPlace(ref hitPoint);
        }
        if (this._selectableModels == null || this._selectableModels.Contains(model))
        {
          this.AddArrows(model, hitPoint);
          this._mode = MoveModelInteractionMode.Mode.ObjectSelected;
          this._selectedModel = this.FindSelectableRootModel(model);
          Action<MoveModelInteractionMode, Model, Vector3d> resetArrows = this.ResetArrows;
          if (resetArrows != null)
            resetArrows(this, model, hitPoint);
          return true;
        }
      }
      else
      {
        this._mode = MoveModelInteractionMode.Mode.ObjectSelected;
        this._selectedModel = (Model) null;
      }
      return false;
    }

    protected Model FindSelectableRootModel(Model model) => model?.Parent == null || !this._selectableModels.Contains(model.Parent) ? model : this.FindSelectableRootModel(model.Parent);

    public override void ImageHostMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this._mode == MoveModelInteractionMode.Mode.ObjectSelected)
      {
        float X;
        float Y;
        this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) sender), out X, out Y);
        Triangle triangle = this._screen.ScreenD3D.PickTriangle(X, Y, new HashSet<Model>()
        {
          this._xArrow,
          this._yArrow,
          this._zArrow
        }, out this._moveHitPoint);
        if (triangle != null)
        {
          lock (this._selectedModel)
          {
            Matrix4d matrix4d = this._selectedModel.WorldMatrix * Matrix4d.Translation((this._xArrow ?? this._yArrow ?? this._zArrow).Transform.TranslationVector);
            matrix4d = matrix4d.Inverted;
            matrix4d.TransformInPlace(ref this._moveHitPoint);
          }
          if (triangle.Face.Shell.Model == this._xArrow)
            this._mode = MoveModelInteractionMode.Mode.MoveX;
          else if (triangle.Face.Shell.Model == this._yArrow)
            this._mode = MoveModelInteractionMode.Mode.MoveY;
          else if (triangle.Face.Shell.Model == this._zArrow)
            this._mode = MoveModelInteractionMode.Mode.MoveZ;
          Action<MoveModelInteractionMode, Model> objectSelected = this.ObjectSelected;
          if (objectSelected == null)
            return;
          objectSelected(this, this._selectedModel);
          return;
        }
      }
      base.ImageHostMouseDown(sender, e);
    }

    public override void ImageHostMouseMove(object sender, MouseEventArgs e)
    {
      float X;
      float Y;
      this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) sender), out X, out Y);
      Matrix transformation = this._screen.ScreenD3D.Renderer.Root.Transform.Value;
      Vector3d eyePos;
      Vector3d eyeDir;
      this._screen.ScreenD3D.CreateRay(X, Y, ref transformation, out eyePos, out eyeDir);
      if (this._selectedModel != null && (this._mode == MoveModelInteractionMode.Mode.MoveX || this._mode == MoveModelInteractionMode.Mode.MoveY || this._mode == MoveModelInteractionMode.Mode.MoveZ))
      {
        Matrix4d inverted;
        lock (this._selectedModel)
          inverted = (this._selectedModel.WorldMatrix * Matrix4d.Translation((this._xArrow ?? this._yArrow ?? this._zArrow).Transform.TranslationVector)).Inverted;
        inverted.TransformInPlace(ref eyePos);
        inverted.TransformNormalInPlace(ref eyeDir);
        Vector3d dir = new Vector3d();
        if (this._mode == MoveModelInteractionMode.Mode.MoveX)
          dir.X = 1.0;
        else if (this._mode == MoveModelInteractionMode.Mode.MoveY)
          dir.Y = 1.0;
        else if (this._mode == MoveModelInteractionMode.Mode.MoveZ)
          dir.Z = 1.0;
        Line line = new Line(this._moveHitPoint, dir);
        Vector3d vector3d = dir.Cross(eyeDir);
        Vector3d normalized = vector3d.Normalized;
        vector3d = dir.Cross(normalized);
        Vector3d? nullable = new WiCAM.Pn4000.BendModel.Base.Plane(this._moveHitPoint, vector3d.Normalized).IntersectRay(eyePos, eyeDir);
        if (!nullable.HasValue)
          return;
        double num1 = line.ParameterOfClosestPointOnAxis(nullable.Value);
        lock (this._selectedModel)
        {
          if (!this.UseWorldCoordinates)
          {
            this._selectedModel.Transform *= Matrix4d.Translation(dir * num1);
            Matrix4d transform = this._selectedModel.Transform;
            Vector3d translationVector = this._selectedModel.Transform.TranslationVector;
            transform.M30 = this._mode == MoveModelInteractionMode.Mode.MoveX ? MathExt.Clamp(translationVector.X, this.MinX, this.MaxX) : translationVector.X;
            transform.M31 = this._mode == MoveModelInteractionMode.Mode.MoveY ? MathExt.Clamp(translationVector.Y, this.MinY, this.MaxY) : translationVector.Y;
            transform.M32 = this._mode == MoveModelInteractionMode.Mode.MoveZ ? MathExt.Clamp(translationVector.Z, this.MinZ, this.MaxZ) : translationVector.Z;
            this._selectedModel.Transform = transform;
          }
          else
          {
            Matrix4d worldMatrix = this._selectedModel.WorldMatrix;
            Vector3d translationVector = worldMatrix.TranslationVector;
            Vector3d dirRay = worldMatrix.TransformNormal(dir * num1);
            double num2 = num1;
            foreach (WiCAM.Pn4000.BendModel.Base.Plane plane in new List<WiCAM.Pn4000.BendModel.Base.Plane>()
            {
              new WiCAM.Pn4000.BendModel.Base.Plane(new Vector3d(this.MinX, 0.0, 0.0), new Vector3d(-1.0, 0.0, 0.0)),
              new WiCAM.Pn4000.BendModel.Base.Plane(new Vector3d(this.MaxX, 0.0, 0.0), new Vector3d(1.0, 0.0, 0.0)),
              new WiCAM.Pn4000.BendModel.Base.Plane(new Vector3d(0.0, this.MinY, 0.0), new Vector3d(0.0, -1.0, 0.0)),
              new WiCAM.Pn4000.BendModel.Base.Plane(new Vector3d(0.0, this.MaxY, 0.0), new Vector3d(0.0, 1.0, 0.0)),
              new WiCAM.Pn4000.BendModel.Base.Plane(new Vector3d(0.0, 0.0, this.MinZ), new Vector3d(0.0, 0.0, -1.0)),
              new WiCAM.Pn4000.BendModel.Base.Plane(new Vector3d(0.0, 0.0, this.MaxZ), new Vector3d(0.0, 0.0, 1.0))
            })
            {
              double t;
              if (plane.IntersectRay(translationVector, dirRay, out t).HasValue && plane.SignedDistanceToPoint(translationVector + dirRay) > 0.0)
                num1 = t * num2;
            }
            this._selectedModel.Transform *= Matrix4d.Translation(dir * num1);
          }
        }
        if (this.RenderMotion)
          this._screen.ScreenD3D.UpdateModelTransform(this._selectedModel, true);
        Action<MoveModelInteractionMode, Model, Vector3d> objectMoving = this.ObjectMoving;
        if (objectMoving == null)
          return;
        objectMoving(this, this._selectedModel, dir * num1);
      }
      else
      {
        if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed)
          this._mode = MoveModelInteractionMode.Mode.Navigate;
        base.ImageHostMouseMove(sender, e);
      }
    }

    protected enum Mode
    {
      Navigate,
      ObjectSelected,
      MoveX,
      MoveY,
      MoveZ,
    }
  }
}
