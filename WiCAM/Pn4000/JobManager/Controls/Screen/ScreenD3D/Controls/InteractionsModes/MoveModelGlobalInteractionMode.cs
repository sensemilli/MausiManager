// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes.MoveModelGlobalInteractionMode
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
using WiCAM.Pn4000.ScreenD3D.Renderer;

namespace WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes
{
  public class MoveModelGlobalInteractionMode : MoveModelInteractionMode
  {
    private double _arrowOffsetX;
    private double _arrowOffsetY;
    private double _arrowOffsetZ;

    public event Action<MoveModelInteractionMode, Model, AlignmentInfo3D> ObjectAligned;

    public event Action<MoveModelInteractionMode> InteractionStopped;

    public event Action<MoveModelInteractionMode, Model> InteractionStarted;

    public Func<Vector3d, AlignmentInfo3D> CalcAlignmentFunc { get; set; }

    public MoveModelGlobalInteractionMode(
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
      : base(screen, selectableModels, xArrow, yArrow, zArrow, minX, maxX, minY, maxY, minZ, maxZ)
    {
    }

    public override void Activate()
    {
      this._selectedModel = (Model) null;
      this._mode = MoveModelInteractionMode.Mode.ObjectSelected;
      base.Activate();
      if (this._mode != MoveModelInteractionMode.Mode.ObjectSelected)
        return;
      this.AddArrowAtClickPoint(this._lastMouseDownX, this._lastMouseDownY);
    }

    private bool AddArrowAtClickPoint(float x, float y)
    {
      HashSet<Model> models2Pick = new HashSet<Model>()
      {
        this._xArrow,
        this._yArrow,
        this._zArrow
      };
      Triangle triangle = this._screen.ScreenD3D.PickTriangle(x, y, models2Pick, out this._moveHitPoint);
      if (triangle == null)
        return false;
      Model model = triangle.Face.Shell.Model;
      if (triangle.Face.Shell.Model == this._xArrow)
        this._mode = MoveModelInteractionMode.Mode.MoveX;
      else if (triangle.Face.Shell.Model == this._yArrow)
        this._mode = MoveModelInteractionMode.Mode.MoveY;
      else if (triangle.Face.Shell.Model == this._zArrow)
        this._mode = MoveModelInteractionMode.Mode.MoveZ;
      lock (model)
        triangle.Face.Shell.WorldMatrix.Inverted.TransformInPlace(ref this._moveHitPoint);
      this.RaiseSelectedEvent((MoveModelInteractionMode) this, this._selectedModel);
      return true;
    }

    public void SetArrowOffsets(double x, double y, double z)
    {
      this._arrowOffsetX = x;
      this._arrowOffsetY = y;
      this._arrowOffsetZ = z;
    }

    public void StopInteraction()
    {
      this.RemoveArrows();
      this._mode = MoveModelInteractionMode.Mode.ObjectSelected;
      this._selectedModel = (Model) null;
      Action<MoveModelInteractionMode> interactionStopped = this.InteractionStopped;
      if (interactionStopped == null)
        return;
      interactionStopped((MoveModelInteractionMode) this);
    }

    protected override void AddArrows(Model parent, Vector3d hitPointWorld)
    {
      Model selectableRootModel = this.FindSelectableRootModel(parent);
      Action<MoveModelInteractionMode, Model> interactionStarted = this.InteractionStarted;
      if (interactionStarted != null)
        interactionStarted((MoveModelInteractionMode) this, selectableRootModel);
      Matrix4d inverted = selectableRootModel.Transform.Inverted;
      hitPointWorld += new Vector3d(this._arrowOffsetX, this._arrowOffsetY, this._arrowOffsetZ);
      Matrix4d matrix4d = Matrix4d.Translation(hitPointWorld) * inverted;
      if (this._xArrow != null)
      {
        this._xArrow.Transform = matrix4d;
        this._screen.ScreenD3D.AddModel(this._xArrow, selectableRootModel, this._yArrow == null && this._zArrow == null);
        this._xArrow.Parent = selectableRootModel;
      }
      if (this._yArrow != null)
      {
        this._yArrow.Transform = matrix4d;
        this._screen.ScreenD3D.AddModel(this._yArrow, selectableRootModel, this._zArrow == null);
        this._yArrow.Parent = selectableRootModel;
      }
      if (this._zArrow == null)
        return;
      this._zArrow.Transform = matrix4d;
      this._screen.ScreenD3D.AddModel(this._zArrow, selectableRootModel, true);
      this._zArrow.Parent = selectableRootModel;
    }

    public override bool ShowArrows(Model model, Vector3d hitPoint)
    {
      bool flag = base.ShowArrows(model, hitPoint);
      Model selectableRootModel = this.FindSelectableRootModel(model);
      this.RemoveArrows();
      if (selectableRootModel == null)
        return true;
      Matrix4d inverted = selectableRootModel.Transform.Inverted;
      hitPoint += new Vector3d(this._arrowOffsetX, this._arrowOffsetY, this._arrowOffsetZ);
      Matrix4d matrix4d = Matrix4d.Translation(hitPoint) * inverted;
      if (this._xArrow != null)
      {
        this._xArrow.Transform = matrix4d;
        this._screen.ScreenD3D.AddModel(this._xArrow, selectableRootModel, this._yArrow == null && this._zArrow == null);
        this._xArrow.Parent = selectableRootModel;
      }
      if (this._yArrow != null)
      {
        this._yArrow.Transform = matrix4d;
        this._screen.ScreenD3D.AddModel(this._yArrow, selectableRootModel, this._zArrow == null);
        this._yArrow.Parent = selectableRootModel;
      }
      if (this._zArrow != null)
      {
        this._zArrow.Transform = matrix4d;
        this._screen.ScreenD3D.AddModel(this._zArrow, selectableRootModel, true);
        this._zArrow.Parent = selectableRootModel;
      }
      return flag;
    }

    public override void ImageHostMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this._mode == MoveModelInteractionMode.Mode.MoveX || this._mode == MoveModelInteractionMode.Mode.MoveY || this._mode == MoveModelInteractionMode.Mode.MoveZ)
      {
        this.RaiseMovedEvent((MoveModelInteractionMode) this, this._selectedModel);
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
        Triangle tri = this._screen.ScreenD3D.PickTriangle(X, Y, (HashSet<Model>) null, out hitPoint);
        if (this.ShowArrows(tri?.Face.Shell.Model, hitPoint))
          this._screen.ExecuteTriangleSelected((NavigateInteractionMode) this, tri, e, hitPoint);
        else
          base.ImageHostMouseUp(sender, e);
      }
    }

    public override void ImageHostMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this._mode == MoveModelInteractionMode.Mode.ObjectSelected)
      {
        float X;
        float Y;
        this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) sender), out X, out Y);
        if (this.AddArrowAtClickPoint(X, Y))
          return;
      }
      base.ImageHostMouseDown(sender, e);
    }

    public override void ImageHostMouseMove(object sender, MouseEventArgs e)
    {
      float X;
      float Y;
      this._screen.RecalculateWpFPointToPixelPoint(e.GetPosition((IInputElement) sender), out X, out Y);
      if (this._screen.ScreenD3D.Renderer.RenderData.RenderMode != RenderData.RndMode.Standard)
        return;
      Matrix transformation = this._screen.ScreenD3D.Renderer.Root.Transform.Value;
      Vector3d eyePos;
      Vector3d eyeDir;
      this._screen.ScreenD3D.CreateRay(X, Y, ref transformation, out eyePos, out eyeDir);
      Vector3d v1 = eyePos;
      Vector3d v2 = eyeDir;
      if (this._selectedModel != null && (this._mode == MoveModelInteractionMode.Mode.MoveX || this._mode == MoveModelInteractionMode.Mode.MoveY || this._mode == MoveModelInteractionMode.Mode.MoveZ))
      {
        Matrix4d inverted;
        lock (this._selectedModel)
        {
          Model model = this._xArrow;
          if (this._mode == MoveModelInteractionMode.Mode.MoveY)
            model = this._yArrow;
          if (this._mode == MoveModelInteractionMode.Mode.MoveZ)
            model = this._zArrow;
          inverted = model.WorldMatrix.Inverted;
        }
        inverted.TransformInPlace(ref v1);
        inverted.TransformNormalInPlace(ref v2);
        Vector3d dir = new Vector3d();
        if (this._mode == MoveModelInteractionMode.Mode.MoveX)
          dir.X = 1.0;
        else if (this._mode == MoveModelInteractionMode.Mode.MoveY)
          dir.Y = 1.0;
        else if (this._mode == MoveModelInteractionMode.Mode.MoveZ)
          dir.Z = 1.0;
        Line line = new Line(this._moveHitPoint, dir);
        Vector3d vector3d = dir.Cross(v2);
        Vector3d normalized = vector3d.Normalized;
        vector3d = dir.Cross(normalized);
        Vector3d? nullable = new WiCAM.Pn4000.BendModel.Base.Plane(this._moveHitPoint, vector3d.Normalized).IntersectRay(v1, v2);
        if (!nullable.HasValue)
          return;
        double num = line.ParameterOfClosestPointOnAxis(nullable.Value);
        lock (this._selectedModel)
        {
          this._selectedModel.Transform *= Matrix4d.Translation(dir * num);
          Matrix4d transform = this._selectedModel.Transform;
          Vector3d translationVector = this._selectedModel.Transform.TranslationVector;
          transform.M30 = this._mode == MoveModelInteractionMode.Mode.MoveX ? MathExt.Clamp(translationVector.X, this.MinX, this.MaxX) : translationVector.X;
          transform.M31 = this._mode == MoveModelInteractionMode.Mode.MoveY ? MathExt.Clamp(translationVector.Y, this.MinY, this.MaxY) : translationVector.Y;
          transform.M32 = this._mode == MoveModelInteractionMode.Mode.MoveZ ? MathExt.Clamp(translationVector.Z, this.MinZ, this.MaxZ) : translationVector.Z;
          if (this.CalcAlignmentFunc != null)
          {
            AlignmentInfo3D alignmentInfo3D = this.CalcAlignmentFunc(transform.TranslationVector);
            Action<MoveModelInteractionMode, Model, AlignmentInfo3D> objectAligned = this.ObjectAligned;
            if (objectAligned != null)
              objectAligned((MoveModelInteractionMode) this, this._selectedModel, alignmentInfo3D);
            if (alignmentInfo3D.AlignmentType != Alignment.None)
            {
              transform.M30 = alignmentInfo3D.AlignedPos.X;
              transform.M31 = alignmentInfo3D.AlignedPos.Y;
              transform.M32 = alignmentInfo3D.AlignedPos.Z;
            }
            this._selectedModel.Transform = transform;
          }
        }
        this._screen.ScreenD3D.UpdateModelTransform(this._selectedModel, true);
        this.RaiseMovingEvent((MoveModelInteractionMode) this, this._selectedModel, dir * num);
      }
      else
      {
        if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed)
          this._mode = MoveModelInteractionMode.Mode.Navigate;
        base.ImageHostMouseMove(sender, e);
      }
    }
  }
}
