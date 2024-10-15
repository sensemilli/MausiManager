// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.ZoomExtendTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class ZoomExtendTask : RenderTaskBase
  {
    private bool _fitFront;
    private bool _predefinedBB;
    private Vector3d _bbMin;
    private Vector3d _bbMax;
    private double _extraSpaceFactor = 1.0;

    public ZoomExtendTask(Action<RenderTaskResult> callback, bool fitFront = false)
      : base(callback)
    {
      this._fitFront = fitFront;
      this._predefinedBB = false;
    }

    public ZoomExtendTask(
      Action<RenderTaskResult> callback,
      Vector3d bbMin,
      Vector3d bbMax,
      double extraSpaceFactor = 1.0,
      bool fitFront = false)
      : base(callback)
    {
      this._fitFront = fitFront;
      this._predefinedBB = true;
      this._bbMin = bbMin;
      this._bbMax = bbMax;
      this._extraSpaceFactor = extraSpaceFactor;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      try
      {
        this.ZoomExtend(renderer);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        Console.WriteLine((object) renderer.RenderData.Device11.DeviceRemovedReason);
      }
      Action<RenderTaskResult> callback = this._callback;
      if (callback == null)
        return;
      callback(new RenderTaskResult((RenderTaskBase) this, true));
    }

    private void ZoomExtend(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      lock (renderer.Root)
      {
        WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node root = renderer.Root;
        Matrix? transform = renderer.Root.Transform;
        Matrix matrix1 = transform.Value;
        Vector3 vector3 = new Vector3();
        transform = renderer.Root.Transform;
        Vector3 translationVector = transform.Value.TranslationVector;
        Matrix matrix2 = Matrix.Translation(vector3 - translationVector);
        Matrix? nullable = new Matrix?(matrix1 * matrix2);
        root.Transform = nullable;
      }
      Vector3d vector3d1 = this._bbMin;
      Vector3d vector3d2 = this._bbMax;
      if (!this._predefinedBB)
      {
        Vector3d min;
        Vector3d max;
        if (ZoomExtendTask.DetermineBoundingBox(renderer, out min, out max))
        {
          vector3d1 = min;
          vector3d2 = max;
        }
        else
        {
          vector3d1 = Vector3d.Zero;
          vector3d2 = Vector3d.Zero;
        }
      }
      Vector3 vector3_1 = new Vector3((float) ((vector3d1.X + vector3d2.X) / 2.0), (float) ((vector3d1.Y + vector3d2.Y) / 2.0), (float) ((vector3d1.Z + vector3d2.Z) / 2.0));
      lock (renderer.Root)
        renderer.Root.Transform = new Matrix?(Matrix.Translation(new Vector3(-vector3_1.X, -vector3_1.Y, -vector3_1.Z)) * renderer.Root.Transform.Value);
      if (this._fitFront)
      {
        vector3d1.Y = 0.0;
        vector3d2.Y = 0.0;
        vector3d1.Z = 0.0;
        vector3d2.Z = 0.0;
      }
      double val1;
      if (renderer.ProjectionType == ProjectionType.Perspective)
      {
        double num1 = (vector3d2 - vector3d1).Length / 2.0;
        double d = Math.PI / 4.0;
        double num2 = 1.0 + this._extraSpaceFactor;
        val1 = num1 * num2 / Math.Atan(d);
      }
      else
      {
        Matrix4d matrix4d = new Matrix4d()
        {
          M00 = (double) renderer.View.M11,
          M01 = (double) renderer.View.M12,
          M02 = (double) renderer.View.M13,
          M03 = (double) renderer.View.M14,
          M10 = (double) renderer.View.M21,
          M11 = (double) renderer.View.M22,
          M12 = (double) renderer.View.M23,
          M13 = (double) renderer.View.M24,
          M20 = (double) renderer.View.M31,
          M21 = (double) renderer.View.M32,
          M22 = (double) renderer.View.M33,
          M23 = (double) renderer.View.M34,
          M30 = (double) renderer.View.M41,
          M31 = (double) renderer.View.M42,
          M32 = (double) renderer.View.M43,
          M33 = (double) renderer.View.M44
        };
        Vector3d v1 = new Vector3d(vector3d1.X, vector3d1.Y, vector3d1.Z);
        Vector3d v2 = new Vector3d(vector3d2.X, vector3d1.Y, vector3d1.Z);
        Vector3d v3 = new Vector3d(vector3d2.X, vector3d2.Y, vector3d1.Z);
        Vector3d v4 = new Vector3d(vector3d1.X, vector3d2.Y, vector3d1.Z);
        Vector3d v5 = new Vector3d(vector3d1.X, vector3d1.Y, vector3d2.Z);
        Vector3d v6 = new Vector3d(vector3d2.X, vector3d1.Y, vector3d2.Z);
        Vector3d v7 = new Vector3d(vector3d2.X, vector3d2.Y, vector3d2.Z);
        Vector3d v8 = new Vector3d(vector3d1.X, vector3d2.Y, vector3d2.Z);
        HashSet<Vector3d> source = new HashSet<Vector3d>()
        {
          matrix4d.Transform(ref v1),
          matrix4d.Transform(ref v2),
          matrix4d.Transform(ref v3),
          matrix4d.Transform(ref v4),
          matrix4d.Transform(ref v5),
          matrix4d.Transform(ref v6),
          matrix4d.Transform(ref v7),
          matrix4d.Transform(ref v8)
        };
        Vector3d vector3d3 = new Vector3d(source.Min<Vector3d>((Func<Vector3d, double>) (x => x.X)), source.Min<Vector3d>((Func<Vector3d, double>) (x => x.Y)), source.Min<Vector3d>((Func<Vector3d, double>) (x => x.Z)));
        val1 = (new Vector3d(source.Max<Vector3d>((Func<Vector3d, double>) (x => x.X)), source.Max<Vector3d>((Func<Vector3d, double>) (x => x.Y)), source.Max<Vector3d>((Func<Vector3d, double>) (x => x.Z))) - vector3d3).Length * (1.0 + this._extraSpaceFactor);
      }
      renderer.Zoom = (float) Math.Min(1000000000.0, Math.Max(val1, 0.10000000149011612));
    }

    public static bool DetermineBoundingBox(
      WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer,
      out Vector3d min,
      out Vector3d max,
      bool fallout = true)
    {
      List<Model> list = renderer.Root.Children.Where<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>((Func<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node, bool>) (x => x is ModelNode modelNode1 && modelNode1.Model.ModelType != ModelType.System && modelNode1.Model.ModelType != ModelType.Static)).Select<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node, Model>((Func<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node, Model>) (x => !(x is ModelNode modelNode2) ? (Model) null : modelNode2.Model)).ToList<Model>();
      if (list.Count == 0 & fallout)
        list = renderer.Root.Children.Where<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>((Func<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node, bool>) (x => x is ModelNode)).Select<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node, Model>((Func<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node, Model>) (x => !(x is ModelNode modelNode3) ? (Model) null : modelNode3.Model)).ToList<Model>();
      return ZoomExtendTask.DetermineBoundingBox(list, out min, out max);
    }

    public static bool DetermineBoundingBox(List<Model> models, out Vector3d min, out Vector3d max)
    {
      min = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
      max = new Vector3d(double.MinValue, double.MinValue, double.MinValue);
      if (models.Count > 0)
      {
        foreach (Model model in models.Where<Model>((Func<Model, bool>) (x => x != null)))
        {
          Matrix4d worldMatrix = Matrix4d.Identity;
          if (model.Parent != null)
            worldMatrix = model.Parent.WorldMatrix;
          Pair<Vector3d, Vector3d> boundary = model.GetBoundary(worldMatrix, true);
          ref Vector3d local1 = ref min;
          Vector3d vector3d1 = boundary.Item1;
          double x1 = Math.Min(vector3d1.X, min.X);
          vector3d1 = boundary.Item1;
          double y1 = Math.Min(vector3d1.Y, min.Y);
          vector3d1 = boundary.Item1;
          double z1 = Math.Min(vector3d1.Z, min.Z);
          Vector3d vector3d2 = new Vector3d(x1, y1, z1);
          local1 = vector3d2;
          ref Vector3d local2 = ref max;
          vector3d1 = boundary.Item2;
          double x2 = Math.Max(vector3d1.X, max.X);
          vector3d1 = boundary.Item2;
          double y2 = Math.Max(vector3d1.Y, max.Y);
          vector3d1 = boundary.Item2;
          double z2 = Math.Max(vector3d1.Z, max.Z);
          Vector3d vector3d3 = new Vector3d(x2, y2, z2);
          local2 = vector3d3;
        }
      }
      bool boundingBox = true;
      if (min.X > max.X)
      {
        boundingBox = false;
        min.X = 0.0;
        max.X = 0.0;
      }
      if (min.Y > max.Y)
      {
        boundingBox = false;
        min.Y = 0.0;
        max.Y = 0.0;
      }
      if (min.Z > max.Z)
      {
        boundingBox = false;
        min.Z = 0.0;
        max.Z = 0.0;
      }
      return boundingBox;
    }
  }
}
