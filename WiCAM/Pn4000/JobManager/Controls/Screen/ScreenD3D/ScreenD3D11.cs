// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.ScreenD3D11
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using ControlzEx.Standard;
using Microsoft.Office.Interop.Excel;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;
using WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks;
using Line = WiCAM.Pn4000.BendModel.Base.Line;
using Model = WiCAM.Pn4000.BendModel.Model;

namespace WiCAM.Pn4000.ScreenD3D
{
  public class ScreenD3D11 : IDisposable, ISimulationScreen
  {
    private D3DImage _d3d9Image;
    private WiCAM.Pn4000.ScreenD3D.Renderer.Renderer _renderer;
    private RenderThread _renderThread;
    private TripodControl _tripodControl;
       
        public WiCAM.Pn4000.ScreenD3D.Renderer.Renderer Renderer => this._renderer;

    public ProjectionType ProjectionType
    {
      get => this._renderer.ProjectionType;
      set => this._renderer.ProjectionType = value;
    }

    public ScreenD3D11(IntPtr hwnd, D3DImage d3dImage, int width, int height, Screen3D screen)
    {
      this._d3d9Image = d3dImage;
      this._renderer = new WiCAM.Pn4000.ScreenD3D.Renderer.Renderer(hwnd, width, height, screen);
      this._renderThread = new RenderThread(this._renderer);
      this._tripodControl = new TripodControl(0.3f, 200, 200);
      this.AddModel((Model) this._tripodControl);
      this.Render(true);
    }

    public void Dispose()
    {
      this._d3d9Image.Lock();
      this._d3d9Image.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
      this._d3d9Image.Unlock();
      this._renderThread.Dispose();
    }

    public virtual void Render(bool skipQueuedFrames) => this.Render(skipQueuedFrames, (Action<RenderTaskResult>) null);

    public virtual void Render(bool skipQueuedFrames, Action<RenderTaskResult> action) => this._renderThread.Enqueue((RenderTaskBase) new RenderTask(skipQueuedFrames, this._renderThread.RenderData, this._d3d9Image, action));

    public void Resize(int width, int height) => this.Resize(width, height, (Action<RenderTaskResult>) null);

    public void Resize(int width, int height, Action<RenderTaskResult> action)
    {
      this._tripodControl.Resized(width, height);
      this.UpdateModelGeometry((Model) this._tripodControl);
      this._renderThread.Enqueue((RenderTaskBase) new ResizeTask(width, height, action));
      this.Render(true);
    }

    public void AddBillboard(Texture2DBillboard billboard) => this.AddBillboard(billboard, (Model) null, true, (Action<RenderTaskResult>) null);

    public void AddBillboard(Texture2DBillboard billboard, bool render) => this.AddBillboard(billboard, (Model) null, render, (Action<RenderTaskResult>) null);

    public void AddBillboard(Texture2DBillboard billboard, Model parent, bool render) => this.AddBillboard(billboard, parent, render, (Action<RenderTaskResult>) null);

    public void AddBillboard(
      Texture2DBillboard billboard,
      Model parent,
      bool render,
      Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new AddBillboardTask(billboard, parent, render ? action : (Action<RenderTaskResult>) null));
      if (!render)
        return;
      this.Render(true, action);
    }

    public void UpdateModelTransform(Model model) => this.UpdateModelTransform(model, true, (Action<RenderTaskResult>) null);

    public void UpdateModelTransform(Model model, bool render) => this.UpdateModelTransform(model, render, (Action<RenderTaskResult>) null);

    public void UpdateModelTransform(Model model, bool render, Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new UpdateModelTransformTask(model, render ? action : (Action<RenderTaskResult>) null));
      if (!render)
        return;
      this.Render(true, action);
    }

    public void UpdateAllModelTransform(bool render, Action<RenderTaskResult> action = null)
    {
      lock (this._renderer)
      {
        foreach (Model model in this._renderer.ProtoModelMap.Keys.ToList<Model>())
          this.UpdateModelTransform(model, false);
      }
      if (!render)
        return;
      this.Render(true, action);
    }
        internal void AddModel(AnyCAD.Foundation.TopoShape topoShape)
        {
            //   RenderTask renderTask = new RenderTask();
        
           this.AddModel(topoShape, (Model)null, true, (Action<RenderTaskResult>)null);

            this.Render(true);
        }

        private void AddModel(AnyCAD.Foundation.TopoShape topoShape, Model parent, bool render, Action<RenderTaskResult> action)
        {
            this._renderThread.Enqueue((RenderTaskBase)new AddModelTask(topoShape, parent, render ? action : (Action<RenderTaskResult>)null));
            if (!render)
                return;
            this.Render(true, action);
        }

        public void AddModel(Model model) => this.AddModel(model, (Model) null, true, (Action<RenderTaskResult>) null);

    public void AddModel(Model model, bool render) => this.AddModel(model, (Model) null, render, (Action<RenderTaskResult>) null);

    public void AddModel(Model model, bool render, Action<RenderTaskResult> action) => this.AddModel(model, (Model) null, render, action);

    public void AddModel(Model model, Model parent, bool render) => this.AddModel(model, parent, render, (Action<RenderTaskResult>) null);

    public void AddModel(Model model, Model parent, bool render, Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new AddModelTask(model, parent, render ? action : (Action<RenderTaskResult>) null));
      if (!render)
        return;
      this.Render(true, action);
    }

    public void RemoveModel(Model model) => this.RemoveModel(model, true, (Action<RenderTaskResult>) null);

    public void RemoveModel(Model model, bool render) => this.RemoveModel(model, render, (Action<RenderTaskResult>) null);

    public void RemoveModel(Model model, bool render, Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new RemoveModelTask(model, render ? action : (Action<RenderTaskResult>) null));
      if (!render)
        return;
      this.Render(true, action);
    }

    public void ReplaceModel(Model model, Model parent) => this.ReplaceModel(model, parent, true, (Action<RenderTaskResult>) null);

    public void ReplaceModel(Model model, Model parent, bool render) => this.ReplaceModel(model, parent, render, (Action<RenderTaskResult>) null);

    public void ReplaceModel(
      Model model,
      Model parent,
      bool render,
      Action<RenderTaskResult> action)
    {
      this.RemoveModel(model, false, action);
      this.AddModel(model, parent, render, action);
    }

    public void UpdateModelGeometry(Model model) => this.UpdateModelGeometry(model, (Model) null, true, (Action<RenderTaskResult>) null);

    public void UpdateModelGeometry(Model model, bool render) => this.UpdateModelGeometry(model, (Model) null, render, (Action<RenderTaskResult>) null);

    public void UpdateModelGeometry(Model model, bool render, Action<RenderTaskResult> action) => this.UpdateModelGeometry(model, (Model) null, render, action);

    public void UpdateModelGeometry(Model model, Model parent, bool render) => this.UpdateModelGeometry(model, parent, render, (Action<RenderTaskResult>) null);

    public void UpdateModelGeometry(
      Model model,
      Model parent,
      bool render,
      Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new UpdateModelTask(model, parent, render ? action : (Action<RenderTaskResult>) null));
      if (!render)
        return;
      this.Render(true, action);
    }

    public void MoveModel(Model model, ref Vector3 translation) => this.MoveModel(model, ref translation, (Action<RenderTaskResult>) null);

    public void MoveModel(Model model, ref Vector3 translation, Action<RenderTaskResult> action)
    {
    }

    public void RotateModel(Model model, ref Matrix3x3 rotation) => this.RotateModel(model, ref rotation, (Action<RenderTaskResult>) null);

    public void RotateModel(Model model, ref Matrix3x3 rotation, Action<RenderTaskResult> action)
    {
    }

    public void UpdateModelAppearance(Model model) => this.UpdateModelAppearance(model, true, (Action<RenderTaskResult>) null);

    public void UpdateModelAppearance(Model model, bool render) => this.UpdateModelAppearance(model, render, (Action<RenderTaskResult>) null);

    public void UpdateModelAppearance(Model model, bool render, Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new UpdateModelAppearanceTask(model, render ? (Action<RenderTaskResult>) null : action));
      if (!render)
        return;
      this.Render(true, action);
    }

    public void UpdateAllModelAppearance(bool render, Action<RenderTaskResult> action = null)
    {
      lock (this._renderer)
      {
        foreach (Model key in this._renderer.ProtoModelMap.Keys)
          this.UpdateModelAppearance(key, false);
      }
      if (!render)
        return;
      this.Render(true, action);
    }

    public void ZoomExtend() => this.ZoomExtend(true);

    public void ZoomExtend(bool render) => this.ZoomExtend(render, false, (Action<RenderTaskResult>) null);

    public void ZoomExtend(bool render, bool fitFront) => this.ZoomExtend(render, fitFront, (Action<RenderTaskResult>) null);

    public void ZoomExtend(bool render, bool fitFront, Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new ZoomExtendTask(render ? (Action<RenderTaskResult>) null : action, fitFront));
      if (!render)
        return;
      this.Render(true, action);
    }

    public void ZoomExtend(
      bool render,
      Vector3d bbMin,
      Vector3d bbMax,
      double extraSpaceFactor,
      bool fitFront,
      Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new ZoomExtendTask(render ? (Action<RenderTaskResult>) null : action, bbMin, bbMax, extraSpaceFactor, fitFront));
      if (!render)
        return;
      this.Render(true, action);
    }

    public void UpdateModelVisibility(Model model) => this.UpdateModelVisibility(model, true, (Action<RenderTaskResult>) null);

    public void UpdateModelVisibility(Model model, bool render) => this.UpdateModelVisibility(model, render, (Action<RenderTaskResult>) null);

    public void UpdateModelVisibility(Model model, bool render, Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new UpdateModelVisibilityTask(model, render ? (Action<RenderTaskResult>) null : action));
      if (!render)
        return;
      this.Render(true, action);
    }

    public void UpdateAllModelVisibility(bool render, Action<RenderTaskResult> action = null)
    {
      lock (this._renderer)
      {
        foreach (Model key in this._renderer.ProtoModelMap.Keys)
          this.UpdateModelVisibility(key, false);
      }
      if (!render)
        return;
      this.Render(true, action);
    }

    public Triangle PickTriangle(float mx, float my) => this.PickTriangle(mx, my, (HashSet<Model>) null, out Vector3d _);

    public Triangle PickTriangle(
      float mx,
      float my,
      HashSet<Model> models2Pick,
      out Vector3d hitPoint)
    {
      Matrix transformation;
      lock (this.Renderer)
        transformation = this._renderer.Root.Transform.Value;
      Vector3d eyePos;
      Vector3d eyeDir;
      this.CreateRay(mx, my, ref transformation, out eyePos, out eyeDir);
      Triangle triangle1 = (Triangle) null;
      double num = double.MaxValue;
      Stack<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node> nodeStack = new Stack<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>();
      Stack<Matrix> matrixStack = new Stack<Matrix>();
      nodeStack.Push(this._renderer.Root);
      matrixStack.Push(transformation);
      while (nodeStack.Count > 0)
      {
        WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node node1 = nodeStack.Pop();
        Matrix transform = matrixStack.Pop();
        lock (this.Renderer)
        {
          if (node1.Transform.HasValue)
          {
            Matrix matrix = node1.Transform.Value;
            matrix.Invert();
            transform *= matrix;
          }
        }
        Vector3 vector = new Vector3((float) eyePos.X, (float) eyePos.Y, (float) eyePos.Z);
        Vector3 normal = new Vector3((float) eyeDir.X, (float) eyeDir.Y, (float) eyeDir.Z);
        Vector3 result1;
        Vector3.Transform(ref vector, ref transform, out result1);
        Vector3 result2;
        Vector3.TransformNormal(ref normal, ref transform, out result2);
        Vector3d vector3d = new Vector3d((double) result1.X, (double) result1.Y, (double) result1.Z);
        Vector3d dir = new Vector3d((double) result2.X, (double) result2.Y, (double) result2.Z);
        Model model = (Model) null;
        lock (node1)
        {
          if (node1 is ModelNode modelNode)
            model = modelNode.Model;
        }
        if (model != null && model.Enabled && (models2Pick == null || models2Pick.Contains(model)))
        {
          List<Triangle> triangleList = new List<Triangle>();
          foreach (Shell shell in model.Shells)
          {
            if (shell.AABBTree != null)
              triangleList.AddRange((IEnumerable<Triangle>) shell.AABBTree.RayQuery(vector3d, dir));
          }
          foreach (Triangle triangle2 in triangleList)
          {
            double t = 0.0;
            if (TriangleFunctions.RayTriangleIntersect(vector3d, dir, triangle2.V0.Pos, triangle2.V1.Pos, triangle2.V2.Pos, ref t) && t < num)
            {
              triangle1 = triangle2;
              num = t;
            }
          }
        }
        List<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node> list;
        lock (node1)
          list = node1.Children.ToList<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>();
        foreach (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node node2 in list)
        {
          nodeStack.Push(node2);
          matrixStack.Push(transform);
        }
      }
      hitPoint = eyePos + num * eyeDir;
      return triangle1;
    }

    public void ResetSpecialPoints()
    {
      Stack<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node> nodeStack = new Stack<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>();
      nodeStack.Push(this._renderer.Root);
      while (nodeStack.Count > 0)
      {
        if (nodeStack.Pop() is ModelNode modelNode && modelNode.Model.Enabled && modelNode.Model.ModelType != ModelType.System && modelNode.Model.ModelType != ModelType.Static)
          modelNode.Model.ResetSpecialCharacteristicPoints();
      }
    }

    public Vector3d ClosestPointOnModel(
      float mx,
      float my,
      out Vertex pointOnModel,
      out bool isSpecialPoint)
    {
      pointOnModel = (Vertex) null;
      isSpecialPoint = false;
      Matrix transformation = this._renderer.Root.Transform.Value;
      Vector3d eyePos;
      Vector3d eyeDir;
      this.CreateRay(mx, my, ref transformation, out eyePos, out eyeDir);
      Stack<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node> nodeStack = new Stack<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>();
      Stack<Matrix> matrixStack = new Stack<Matrix>();
      nodeStack.Push(this._renderer.Root);
      matrixStack.Push(transformation);
      List<Vector3d> vector3dList = new List<Vector3d>();
      double num = double.MaxValue;
      Vector3d vector3d = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
      while (nodeStack.Count > 0)
      {
        WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node node1 = nodeStack.Pop();
        Matrix transform1 = matrixStack.Pop();
        if (node1.Transform.HasValue)
        {
          Matrix matrix = node1.Transform.Value;
          matrix.Invert();
          transform1 *= matrix;
        }
        Vector3 vector1 = new Vector3((float) eyePos.X, (float) eyePos.Y, (float) eyePos.Z);
        Vector3 normal = new Vector3((float) eyeDir.X, (float) eyeDir.Y, (float) eyeDir.Z);
        Matrix transform2 = transform1;
        transform2.Invert();
        Vector3 result1;
        Vector3.Transform(ref vector1, ref transform1, out result1);
        Vector3 result2;
        Vector3.TransformNormal(ref normal, ref transform1, out result2);
        Vector3d eyePosTransformed = new Vector3d((double) result1.X, (double) result1.Y, (double) result1.Z);
        Vector3d eyeDirTransformed = new Vector3d((double) result2.X, (double) result2.Y, (double) result2.Z);
        Line line = new Line(eyePosTransformed, eyeDirTransformed);
        if (node1 is ModelNode modelNode && modelNode.Model.Enabled && modelNode.Model.ModelType != ModelType.System && modelNode.Model.ModelType != ModelType.Static)
        {
          foreach (Vector3d characteristicPoint in modelNode.Model.GetSpecialCharacteristicPoints())
          {
            double t = line.ParameterOfClosestPointOnAxis(characteristicPoint);
            double length = (line.GetPointOnAxisByParameter(t) - characteristicPoint).Length;
            if (length < num)
            {
              num = length;
              Vector3 vector2 = new Vector3((float) characteristicPoint.X, (float) characteristicPoint.Y, (float) characteristicPoint.Z);
              Vector3 result3;
              Vector3.Transform(ref vector2, ref transform2, out result3);
              vector3d = new Vector3d((double) result3.X, (double) result3.Y, (double) result3.Z);
              isSpecialPoint = true;
            }
          }
          foreach (Shell shell in modelNode.Model.Shells)
          {
            if (shell.AABBTree != null)
            {
              Triangle rayQuery = shell.AABBTree.NearestPointToRayQuery(eyePosTransformed, eyeDirTransformed, (Func<Triangle, Pair<double, double>>) (tri =>
              {
                double maxValue = double.MaxValue;
                double t = -1.0;
                TriangleFunctions.RayTriangleClosestPoint(eyePosTransformed, eyeDirTransformed, tri.V0.Pos, tri.V1.Pos, tri.V2.Pos, ref t, ref maxValue);
                return new Pair<double, double>(maxValue, t);
              }));
              if (rayQuery != null)
              {
                foreach (Vertex vertex in rayQuery.Vertices)
                {
                  double t = line.ParameterOfClosestPointOnAxis(vertex.Pos);
                  double length = (line.GetPointOnAxisByParameter(t) - vertex.Pos).Length;
                  if (length < num)
                  {
                    num = length;
                    Vector3 vector3 = new Vector3((float) vertex.Pos.X, (float) vertex.Pos.Y, (float) vertex.Pos.Z);
                    Vector3 result4;
                    Vector3.Transform(ref vector3, ref transform2, out result4);
                    vector3d = new Vector3d((double) result4.X, (double) result4.Y, (double) result4.Z);
                    isSpecialPoint = false;
                    pointOnModel = vertex;
                  }
                }
              }
            }
          }
        }
        List<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node> list;
        lock (node1)
          list = node1.Children.ToList<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>();
        foreach (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node node2 in list)
        {
          nodeStack.Push(node2);
          matrixStack.Push(transform1);
        }
      }
      return vector3d;
    }

    public Vector3d ClosestPoint(float mx, float my)
    {
      lock (this._renderer)
      {
        Matrix transformation = this._renderer.Root.Transform.Value;
        Vector3d eyePos;
        Vector3d eyeDir;
        this.CreateRay(mx, my, ref transformation, out eyePos, out eyeDir);
        Vector3d vector3d1 = new Vector3d();
        double num1 = double.MaxValue;
        double num2 = double.MaxValue;
        Stack<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node> nodeStack = new Stack<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>();
        Stack<Matrix> matrixStack = new Stack<Matrix>();
        nodeStack.Push(this._renderer.Root);
        matrixStack.Push(transformation);
        while (nodeStack.Count > 0)
        {
          WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node node1 = nodeStack.Pop();
          Matrix transform1 = matrixStack.Pop();
          if (node1.Transform.HasValue)
          {
            Matrix matrix = node1.Transform.Value;
            matrix.Invert();
            transform1 *= matrix;
          }
          Vector3 vector1 = new Vector3((float) eyePos.X, (float) eyePos.Y, (float) eyePos.Z);
          Vector3 normal = new Vector3((float) eyeDir.X, (float) eyeDir.Y, (float) eyeDir.Z);
          Vector3 result1;
          Vector3.Transform(ref vector1, ref transform1, out result1);
          Vector3 result2;
          Vector3.TransformNormal(ref normal, ref transform1, out result2);
          Vector3d eyePosTransformed = new Vector3d((double) result1.X, (double) result1.Y, (double) result1.Z);
          Vector3d eyeDirTransformed = new Vector3d((double) result2.X, (double) result2.Y, (double) result2.Z);
          if (node1 is ModelNode modelNode && modelNode.Model.Enabled && modelNode.Model.ModelType != ModelType.System && modelNode.Model.ModelType != ModelType.Static)
          {
            List<Triangle> triangleList = new List<Triangle>();
            foreach (Shell shell in modelNode.Model.Shells)
            {
              if (shell.AABBTree != null)
              {
                Triangle rayQuery = shell.AABBTree.NearestPointToRayQuery(eyePosTransformed, eyeDirTransformed, (Func<Triangle, Pair<double, double>>) (tri =>
                {
                  double maxValue = double.MaxValue;
                  double t = -1.0;
                  TriangleFunctions.RayTriangleClosestPoint(eyePosTransformed, eyeDirTransformed, tri.V0.Pos, tri.V1.Pos, tri.V2.Pos, ref t, ref maxValue);
                  return new Pair<double, double>(maxValue, t);
                }));
                if (rayQuery != null)
                  triangleList.Add(rayQuery);
              }
            }
            if (triangleList.Count > 0)
            {
              foreach (Triangle triangle in triangleList)
              {
                double t = -1.0;
                double maxValue = double.MaxValue;
                Vector3d vector3d2 = TriangleFunctions.RayTriangleClosestPoint(eyePosTransformed, eyeDirTransformed, triangle.V0.Pos, triangle.V1.Pos, triangle.V2.Pos, ref t, ref maxValue);
                if (t > 0.0 && (maxValue < num2 || maxValue == num2 && t < num1))
                {
                  num2 = maxValue;
                  num1 = t;
                  Vector3 vector2 = new Vector3((float) vector3d2.X, (float) vector3d2.Y, (float) vector3d2.Z);
                  Matrix transform2 = transform1;
                  transform2.Invert();
                  Vector3 result3;
                  Vector3.Transform(ref vector2, ref transform2, out result3);
                  vector3d1 = new Vector3d((double) result3.X, (double) result3.Y, (double) result3.Z);
                }
              }
            }
          }
          List<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node> list;
          lock (node1)
            list = node1.Children.ToList<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>();
          foreach (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node node2 in list)
          {
            nodeStack.Push(node2);
            matrixStack.Push(transform1);
          }
        }
        return vector3d1;
      }
    }

    public void CreateRay(
      float mx,
      float my,
      ref Matrix transformation,
      out Vector3d eyePos,
      out Vector3d eyeDir)
    {
      float x = (float) (2.0 * (double) mx / (double) this._renderer.Width - 1.0);
      float y1 = (float) (1.0 - 2.0 * (double) my / (double) this._renderer.Height);
      if (this.Renderer.ProjectionType == ProjectionType.Isometric)
      {
        Matrix transform = Matrix.Identity;
        lock (this._renderer)
          transform = transformation * this._renderer.View * this._renderer.Projection;
        transform.Invert();
        Vector3 result1 = new Vector3(x, y1, 0.0f);
        Vector3.Transform(ref result1, ref transform, out result1);
        Vector3 result2 = new Vector3(x, y1, 1f);
        Vector3.Transform(ref result2, ref transform, out result2);
        Vector3 vector3 = result2 - result1;
        vector3.Normalize();
        eyePos = new Vector3d((double) result1.X, (double) result1.Y, (double) result1.Z);
        eyeDir = new Vector3d((double) vector3.X, (double) vector3.Y, (double) vector3.Z);
      }
      else
      {
        float y2 = 0.0f;
        Matrix projection;
        Matrix view;
        lock (this._renderer)
        {
          projection = this._renderer.Projection;
          view = this._renderer.View;
          y2 = this._renderer.Zoom;
        }
        projection.Invert();
        view.Invert();
        Matrix transform = transformation;
        transform.Invert();
        Vector4 vector = new Vector4(x, y1, -1f, 1f);
        Vector4 result3;
        Vector4.Transform(ref vector, ref projection, out result3);
        result3 = new Vector4(result3.X, result3.Y, -1f, 0.0f);
        Vector4 result4;
        Vector4.Transform(ref result3, ref view, out result4);
        Vector3 normal = new Vector3(result4.X, result4.Y, result4.Z);
        normal.Normalize();
        Vector3 coordinate = new Vector3(0.0f, y2, 0.0f);
        Vector3 result5;
        Vector3.TransformCoordinate(ref coordinate, ref transform, out result5);
        Vector3 result6;
        Vector3.TransformNormal(ref normal, ref transform, out result6);
        eyePos = new Vector3d((double) result5.X, (double) result5.Y, (double) result5.Z);
        eyeDir = new Vector3d((double) result6.X, (double) result6.Y, (double) result6.Z);
      }
    }

    public void MouseMove(
      float x,
      float y,
      float velocity,
      Vector3d rotationPoint,
      MouseRotationMode rotationMode,
      bool leftMouseButtonPressed,
      bool rightMouseButtonPressed,
      bool middleMouseButtonPressed)
    {
      this.MouseMove(x, y, velocity, rotationPoint, rotationMode, leftMouseButtonPressed, rightMouseButtonPressed, middleMouseButtonPressed, (Action<RenderTaskResult>) null);
    }

    public void MouseMove(
      float x,
      float y,
      float velocity,
      Vector3d rotationPoint,
      MouseRotationMode rotationMode,
      bool leftMouseButtonPressed,
      bool rightMouseButtonPressed,
      bool middleMouseButtonPressed,
      Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new MouseMoveTask(x, y, velocity, rotationPoint, rotationMode, leftMouseButtonPressed, rightMouseButtonPressed, middleMouseButtonPressed, action));
    }

    public void MouseWheel(float x, float y, Vector3d zoomPoint, int delta) => this.MouseWheel(x, y, zoomPoint, delta, (Action<RenderTaskResult>) null);

    public void MouseWheel(
      float x,
      float y,
      Vector3d zoomPoint,
      int delta,
      Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new MouseWheelTask(x, y, zoomPoint, delta, action));
    }

    public void SetViewDirectionByMatrix4d(
      Matrix4d viewDirection,
      bool render = true,
      Action<RenderTaskResult> action = null)
    {
      this.SetViewDirection(new Matrix((float) viewDirection[0, 0], (float) viewDirection[0, 1], (float) viewDirection[0, 2], 0.0f, (float) viewDirection[1, 0], (float) viewDirection[1, 1], (float) viewDirection[1, 2], 0.0f, (float) viewDirection[2, 0], (float) viewDirection[2, 1], (float) viewDirection[2, 2], 0.0f, (float) viewDirection[3, 0], (float) viewDirection[3, 1], (float) viewDirection[3, 2], 1f), render, action);
    }

    public void SetViewDirection(Matrix viewDirection, bool render) => this.SetViewDirection(viewDirection, render, (Action<RenderTaskResult>) null);

    public void SetViewDirection(
      Matrix viewDirection,
      bool render,
      Action<RenderTaskResult> action)
    {
      this._renderThread.Enqueue((RenderTaskBase) new SetViewDirectionTask(viewDirection, action));
      if (!render)
        return;
      this.Render(true);
    }

    public void UpdateProjectionType(ProjectionType projectionType) => this.UpdateProjectionType(projectionType, true);

    public void UpdateProjectionType(ProjectionType projectionType, bool render) => this.UpdateProjectionType(projectionType, render, (Action<RenderTaskResult>) null);

    public void UpdateProjectionType(
      ProjectionType projectionType,
      bool render,
      Action<RenderTaskResult> action)
    {
      this.ProjectionType = projectionType;
      this._renderThread.Enqueue((RenderTaskBase) new UpdateProjectionTask(projectionType, action));
      if (!render)
        return;
      this.Render(true);
    }

    public void PrintScreen() => this._renderThread.Enqueue((RenderTaskBase) new ScreenshotTask(this._renderThread.RenderData, "3Dprint.png", (Action<RenderTaskResult>) null));

    public void PrintScreenDepth() => this._renderThread.Enqueue((RenderTaskBase) new ScreenshotTask(this._renderThread.RenderData, "3Dprint_Depth.png", (Action<RenderTaskResult>) null, renderMode: RenderData.RndMode.Depth));

    public void PrintScreen(
      string targetPath,
      int border = -1,
      int width = -1,
      int height = -1,
      Action<RenderTaskResult> action = null)
    {
      this._renderThread.Enqueue((RenderTaskBase) new ScreenshotTask(this._renderThread.RenderData, targetPath, action, border, width, height));
    }

     
    }
}
