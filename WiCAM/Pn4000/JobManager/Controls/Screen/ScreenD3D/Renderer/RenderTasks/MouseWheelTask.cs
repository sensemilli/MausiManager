// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.MouseWheelTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class MouseWheelTask : RenderTaskBase
  {
    public float X { get; }

    public float Y { get; }

    public int Delta { get; }

    public Vector3d ZoomPoint { get; }

    public MouseWheelTask(
      float x,
      float y,
      Vector3d zoomPoint,
      int delta,
      Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.X = x;
      this.Y = y;
      this.Delta = delta;
      this.ZoomPoint = zoomPoint;
    }

    private Matrix4d GetRootTransformAsWicamMatrix(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer) => new Matrix4d()
    {
      M00 = (double) renderer.Root.Transform.Value.M11,
      M01 = (double) renderer.Root.Transform.Value.M12,
      M02 = (double) renderer.Root.Transform.Value.M13,
      M03 = (double) renderer.Root.Transform.Value.M14,
      M10 = (double) renderer.Root.Transform.Value.M21,
      M11 = (double) renderer.Root.Transform.Value.M22,
      M12 = (double) renderer.Root.Transform.Value.M23,
      M13 = (double) renderer.Root.Transform.Value.M24,
      M20 = (double) renderer.Root.Transform.Value.M31,
      M21 = (double) renderer.Root.Transform.Value.M32,
      M22 = (double) renderer.Root.Transform.Value.M33,
      M23 = (double) renderer.Root.Transform.Value.M34,
      M30 = (double) renderer.Root.Transform.Value.M41,
      M31 = (double) renderer.Root.Transform.Value.M42,
      M32 = (double) renderer.Root.Transform.Value.M43,
      M33 = (double) renderer.Root.Transform.Value.M44
    };

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      lock (renderer)
      {
        lock (renderer.Root)
        {
          float num = (float) this.Delta / 1000f;
          Vector3d zoomPoint = this.ZoomPoint;
          this.GetRootTransformAsWicamMatrix(renderer).TransformInPlace(ref zoomPoint);
          double y = zoomPoint.Y;
          renderer.Zoom -= renderer.Zoom * num;
          renderer.Zoom = Math.Min(1E+09f, Math.Max(renderer.Zoom, 0.1f));
          Vector3 vector3 = new Vector3() - new Vector3((float) zoomPoint.X, (float) zoomPoint.Y, (float) zoomPoint.Z) * num;
          Matrix result1;
          Matrix.Translation(ref vector3, out result1);
          Node root1 = renderer.Root;
          Matrix? transform1 = root1.Transform;
          Matrix matrix1 = result1;
          root1.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix1) : new Matrix?();
          if (renderer.ProjectionType == ProjectionType.Perspective)
          {
            renderer.Zoom += (float) (y * (double) num - y);
            vector3 = new Vector3(0.0f, (float) (zoomPoint.Y * (double) num - y), 0.0f);
            Matrix result2;
            Matrix.Translation(ref vector3, out result2);
            Node root2 = renderer.Root;
            Matrix? transform2 = root2.Transform;
            Matrix matrix2 = result2;
            root2.Transform = transform2.HasValue ? new Matrix?(transform2.GetValueOrDefault() * matrix2) : new Matrix?();
          }
          Action<RenderTaskResult> callback = this._callback;
          if (callback == null)
            return;
          callback(new RenderTaskResult((RenderTaskBase) this, true));
        }
      }
    }
  }
}
