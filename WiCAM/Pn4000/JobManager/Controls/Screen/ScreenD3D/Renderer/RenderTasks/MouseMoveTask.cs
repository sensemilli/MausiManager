// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.MouseMoveTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class MouseMoveTask : RenderTaskBase
  {
    public float X { get; }

    public float Y { get; }

    public bool LeftMouseButtonPressed { get; }

    public bool RightMouseButtonPressed { get; }

    public bool MiddleMouseButtonPressed { get; }

    public Vector3d RotationPoint { get; }

    public MouseRotationMode RotationMode { get; set; }

    public float Velocity { get; set; }

    public MouseMoveTask(
      float x,
      float y,
      float velocity,
      Vector3d rotationPoint,
      MouseRotationMode rotationMode,
      bool leftMouseButtonPressed,
      bool rightMouseButtonPressed,
      bool middleMouseButtonPressed,
      Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.X = x;
      this.Y = y;
      this.Velocity = velocity;
      this.RotationPoint = rotationPoint;
      this.RotationMode = rotationMode;
      this.LeftMouseButtonPressed = leftMouseButtonPressed;
      this.RightMouseButtonPressed = rightMouseButtonPressed;
      this.MiddleMouseButtonPressed = middleMouseButtonPressed;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      float num1 = this.X - renderer.LastMouseX;
      float num2 = this.Y - renderer.LastMouseY;
      Matrix? transform1;
      if (this.LeftMouseButtonPressed)
      {
        Vector3d rotationPoint = this.RotationPoint;
        Matrix4d matrix4d = new Matrix4d();
        matrix4d.M00 = (double) renderer.Root.Transform.Value.M11;
        ref Matrix4d local1 = ref matrix4d;
        Matrix? transform2 = renderer.Root.Transform;
        double m12 = (double) transform2.Value.M12;
        local1.M01 = m12;
        ref Matrix4d local2 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m13 = (double) transform2.Value.M13;
        local2.M02 = m13;
        ref Matrix4d local3 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m14 = (double) transform2.Value.M14;
        local3.M03 = m14;
        ref Matrix4d local4 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m21 = (double) transform2.Value.M21;
        local4.M10 = m21;
        ref Matrix4d local5 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m22 = (double) transform2.Value.M22;
        local5.M11 = m22;
        ref Matrix4d local6 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m23 = (double) transform2.Value.M23;
        local6.M12 = m23;
        ref Matrix4d local7 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m24 = (double) transform2.Value.M24;
        local7.M13 = m24;
        ref Matrix4d local8 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m31 = (double) transform2.Value.M31;
        local8.M20 = m31;
        ref Matrix4d local9 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m32 = (double) transform2.Value.M32;
        local9.M21 = m32;
        ref Matrix4d local10 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m33 = (double) transform2.Value.M33;
        local10.M22 = m33;
        ref Matrix4d local11 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m34 = (double) transform2.Value.M34;
        local11.M23 = m34;
        ref Matrix4d local12 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m41 = (double) transform2.Value.M41;
        local12.M30 = m41;
        ref Matrix4d local13 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m42 = (double) transform2.Value.M42;
        local13.M31 = m42;
        ref Matrix4d local14 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m43 = (double) transform2.Value.M43;
        local14.M32 = m43;
        ref Matrix4d local15 = ref matrix4d;
        transform2 = renderer.Root.Transform;
        double m44 = (double) transform2.Value.M44;
        local15.M33 = m44;
        matrix4d.TransformInPlace(ref rotationPoint);
        Vector3 vector3_1 = new Vector3() - new Vector3((float) rotationPoint.X, (float) rotationPoint.Y, (float) rotationPoint.Z);
        Matrix result1;
        Matrix.Translation(ref vector3_1, out result1);
        Node root1 = renderer.Root;
        transform1 = root1.Transform;
        Matrix matrix1 = result1;
        root1.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix1) : new Matrix?();
        switch (this.RotationMode)
        {
          case MouseRotationMode.Free:
            Node root2 = renderer.Root;
            transform1 = root2.Transform;
            Matrix matrix2 = Matrix.RotationYawPitchRoll(0.0f, (float) (-(double) num2 * 0.0099999997764825821), num1 * 0.01f);
            root2.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix2) : new Matrix?();
            break;
          case MouseRotationMode.X:
            Node root3 = renderer.Root;
            transform1 = root3.Transform;
            Matrix matrix3 = Matrix.RotationYawPitchRoll(0.0f, (float) (-(double) num2 * 0.0099999997764825821), 0.0f);
            root3.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix3) : new Matrix?();
            break;
          case MouseRotationMode.Y:
            Node root4 = renderer.Root;
            transform1 = root4.Transform;
            Matrix matrix4 = Matrix.RotationYawPitchRoll(num1 * 0.01f, 0.0f, 0.0f);
            root4.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix4) : new Matrix?();
            break;
          case MouseRotationMode.Z:
            Node root5 = renderer.Root;
            transform1 = root5.Transform;
            Matrix matrix5 = Matrix.RotationYawPitchRoll(0.0f, 0.0f, num1 * 0.01f);
            root5.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix5) : new Matrix?();
            break;
          case MouseRotationMode.None:
            Node root6 = renderer.Root;
            transform1 = root6.Transform;
            Matrix matrix6 = Matrix.RotationYawPitchRoll(0.0f, (float) (-(double) num2 * 0.0099999997764825821), num1 * 0.01f);
            root6.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix6) : new Matrix?();
            break;
          default:
            Node root7 = renderer.Root;
            transform1 = root7.Transform;
            Matrix matrix7 = Matrix.RotationYawPitchRoll(0.0f, (float) (-(double) num2 * 0.0099999997764825821), num1 * 0.01f);
            root7.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix7) : new Matrix?();
            break;
        }
        Vector3 vector3_2 = new Vector3() - new Vector3(-(float) rotationPoint.X, -(float) rotationPoint.Y, -(float) rotationPoint.Z);
        Matrix result2;
        Matrix.Translation(ref vector3_2, out result2);
        Node root8 = renderer.Root;
        transform1 = root8.Transform;
        Matrix matrix8 = result2;
        root8.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix8) : new Matrix?();
      }
      if (this.MiddleMouseButtonPressed || this.RightMouseButtonPressed)
      {
        if (renderer.ProjectionType == ProjectionType.Perspective)
        {
          Node root = renderer.Root;
          transform1 = root.Transform;
          Matrix matrix = Matrix.Translation(-num1 * this.Velocity, 0.0f, -num2 * this.Velocity);
          root.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix) : new Matrix?();
        }
        else
        {
          Node root = renderer.Root;
          transform1 = root.Transform;
          Matrix matrix = Matrix.Translation(-num1 * renderer.DistPerPix.X, 0.0f, -num2 * renderer.DistPerPix.Y);
          root.Transform = transform1.HasValue ? new Matrix?(transform1.GetValueOrDefault() * matrix) : new Matrix?();
        }
      }
      renderer.LastMouseX = this.X;
      renderer.LastMouseY = this.Y;
      Action<RenderTaskResult> callback = this._callback;
      if (callback == null)
        return;
      callback(new RenderTaskResult((RenderTaskBase) this, true));
    }
  }
}
