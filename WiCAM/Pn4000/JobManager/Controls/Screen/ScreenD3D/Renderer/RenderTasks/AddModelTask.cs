// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.AddModelTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.ScreenD3D.Renderer.Geometry;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class AddModelTask : RenderTaskBase
  {
    public Model Model { get; }

    public Model Parent { get; }
        public AnyCAD.Foundation.TopoShape TopoShape { get; }

        public AddModelTask(Model model, Model parent, Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.Model = model;
      this.Parent = parent;
    }

        public AddModelTask(AnyCAD.Foundation.TopoShape topoShape, Model parent, Action<RenderTaskResult> callback)
              : base(callback)
        {
            TopoShape = topoShape;
            this.Parent = parent;
        }

        public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      try
      {
        lock (renderer)
          this.CreateModelNode(renderer, this.Model, this.Parent);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        Console.WriteLine((object) renderer.RenderData.Device11.DeviceRemovedReason);
      }
      Action<RenderTaskResult> callback = this._callback;
      if (callback != null)
        callback(new RenderTaskResult((RenderTaskBase) this, true));
      renderer.RenderData.ShadowmapUpToDate = false;
    }

    private void CreateModelNode(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Model model, Model parent)
    {
      if (model == null)
        return;
      lock (model)
      {
        ModelNode modelNode1 = new ModelNode(model, (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) null);
        modelNode1.Transform = new Matrix?(new Matrix((float) model.Transform[0, 0], (float) model.Transform[0, 1], (float) model.Transform[0, 2], 0.0f, (float) model.Transform[1, 0], (float) model.Transform[1, 1], (float) model.Transform[1, 2], 0.0f, (float) model.Transform[2, 0], (float) model.Transform[2, 1], (float) model.Transform[2, 2], 0.0f, (float) model.Transform[3, 0], (float) model.Transform[3, 1], (float) model.Transform[3, 2], 1f));
        if (parent == null)
        {
          lock (renderer.Root)
            renderer.Root.Children.Add((WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) modelNode1);
          modelNode1.Parent = renderer.Root;
        }
        else
        {
          ModelNode modelNode2 = this.FindModelNode(renderer.Root, parent);
          if (modelNode2 != null)
          {
            lock (modelNode2)
              modelNode2.Children.Add((WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) modelNode1);
          }
          modelNode1.Parent = (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) modelNode2;
        }
        foreach (ModelInstance instance in model.ReferenceModel)
        {
          ModelNode modelNode3;
          if (!renderer.ProtoModelMap.TryGetValue(instance.Reference, out modelNode3))
          {
            modelNode3 = new ModelNode(instance.Reference, (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) null);
            renderer.ProtoModelMap.Add(instance.Reference, modelNode3);
            this.CreateShellNodes(renderer, instance.Reference, modelNode3);
          }
          ModelInstanceNode modelInstanceNode = new ModelInstanceNode(instance, (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) modelNode1);
          modelInstanceNode.Children.Add((WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) modelNode3);
          lock (modelNode1)
            modelNode1.Children.Add((WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) modelInstanceNode);
        }
        if (model.Shells.Count > 0)
          this.CreateShellNodes(renderer, model, modelNode1);
        renderer.ProtoModelMap.Add(model, modelNode1);
        lock (model.SubModels)
        {
          foreach (Model subModel in model.SubModels)
            this.CreateModelNode(renderer, subModel, model);
        }
      }
    }

    private void CreateShellNodes(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Model model, ModelNode modelNode)
    {
      foreach (Shell shell in model.Shells)
      {
        lock (shell)
        {
          List<VertexPositionNormalTex> positionNormalTexList1 = new List<VertexPositionNormalTex>();
          List<VertexPositionNormal> vertexPositionNormalList = new List<VertexPositionNormal>();
          List<uint> uintList1 = new List<uint>();
          List<uint> uintList2 = new List<uint>();
          Dictionary<Vector3d, List<Pair<uint, Face>>> dictionary = new Dictionary<Vector3d, List<Pair<uint, Face>>>();
          List<Triple<Face, uint, uint>> faceStartEndIdx = new List<Triple<Face, uint, uint>>();
          List<Triple<FaceHalfEdge, uint, uint>> edgeStartEndIdx = new List<Triple<FaceHalfEdge, uint, uint>>();
          uint num1 = 0;
          uint num2 = 0;
          foreach (Vertex key in shell.VertexCache.Values)
          {
            List<Pair<uint, Face>> pairList = new List<Pair<uint, Face>>();
            foreach (Face face in key.Faces)
            {
              SurfaceDerivatives surfaceDerivative = face.SurfaceDerivatives[key];
              List<VertexPositionNormalTex> positionNormalTexList2 = positionNormalTexList1;
              double x1 = key.Pos.X;
              Vector3d vector3d = key.Pos;
              double y1 = vector3d.Y;
              vector3d = key.Pos;
              double z1 = vector3d.Z;
              Vector3 position = new Vector3((float) x1, (float) y1, (float) z1);
              vector3d = surfaceDerivative.Normal;
              double x2 = vector3d.X;
              vector3d = surfaceDerivative.Normal;
              double y2 = vector3d.Y;
              vector3d = surfaceDerivative.Normal;
              double z2 = vector3d.Z;
              Vector3 normal = new Vector3((float) x2, (float) y2, (float) z2);
              Vector2 tex = new Vector2((float) surfaceDerivative.UV.X, (float) surfaceDerivative.UV.Y);
              VertexPositionNormalTex positionNormalTex = new VertexPositionNormalTex(position, normal, tex);
              positionNormalTexList2.Add(positionNormalTex);
              pairList.Add(new Pair<uint, Face>(num2, face));
              ++num2;
            }
            dictionary.Add(key.Pos, pairList);
          }
          foreach (Face face1 in shell.Faces)
          {
            Face face = face1;
            Triple<Face, uint, uint> triple1 = new Triple<Face, uint, uint>(face, (uint) uintList2.Count, 0U);
            foreach (Triangle triangle in face.Mesh)
            {
              Pair<uint, Face> pair1 = dictionary[triangle.V0.Pos].First<Pair<uint, Face>>((Func<Pair<uint, Face>, bool>) (pair => pair.Item2 == face));
              Pair<uint, Face> pair2 = dictionary[triangle.V1.Pos].First<Pair<uint, Face>>((Func<Pair<uint, Face>, bool>) (pair => pair.Item2 == face));
              Pair<uint, Face> pair3 = dictionary[triangle.V2.Pos].First<Pair<uint, Face>>((Func<Pair<uint, Face>, bool>) (pair => pair.Item2 == face));
              uintList2.Add(pair1.Item1);
              uintList2.Add(pair2.Item1);
              uintList2.Add(pair3.Item1);
            }
            triple1.Item3 = (uint) (uintList2.Count - 1);
            faceStartEndIdx.Add(triple1);
            foreach (FaceHalfEdge faceHalfEdge in face.BoundaryEdgesCcw)
            {
              Triple<FaceHalfEdge, uint, uint> triple2 = new Triple<FaceHalfEdge, uint, uint>(faceHalfEdge, (uint) uintList1.Count, 0U);
              ++num1;
              Vector3? nullable = new Vector3?();
              Vector3d vector3d = new Vector3d(1.0, 0.0, 0.0);
              foreach (Vertex vertex in faceHalfEdge.Vertices)
              {
                if (nullable.HasValue)
                {
                  uintList1.Add(num1 - 1U);
                  uintList1.Add(num1);
                  ++num1;
                }
                Vector3d pos = vertex.Pos;
                double x = pos.X;
                pos = vertex.Pos;
                double y = pos.Y;
                pos = vertex.Pos;
                double z = pos.Z;
                nullable = new Vector3?(new Vector3((float) x, (float) y, (float) z));
                try
                {
                  vector3d = face.SurfaceDerivatives[vertex].Normal;
                }
                catch
                {
                  Console.WriteLine("missing surface derivatives in " + face.Name + " at " + vertex?.ToString());
                }
                Vector3 normal = new Vector3((float) vector3d.X, (float) vector3d.Y, (float) vector3d.Z);
                vertexPositionNormalList.Add(new VertexPositionNormal(new Vector4(nullable.Value, 1f), normal));
              }
              triple2.Item3 = (uint) (uintList1.Count - 1);
              edgeStartEndIdx.Add(triple2);
            }
            foreach (FaceHalfEdge faceHalfEdge in face.HoleEdgesCw.SelectMany<List<FaceHalfEdge>, FaceHalfEdge>((Func<List<FaceHalfEdge>, IEnumerable<FaceHalfEdge>>) (x => (IEnumerable<FaceHalfEdge>) x)))
            {
              Triple<FaceHalfEdge, uint, uint> triple3 = new Triple<FaceHalfEdge, uint, uint>(faceHalfEdge, (uint) uintList1.Count, 0U);
              ++num1;
              Vector3? nullable = new Vector3?();
              foreach (Vertex vertex in faceHalfEdge.Vertices)
              {
                if (nullable.HasValue)
                {
                  uintList1.Add(num1 - 1U);
                  uintList1.Add(num1);
                  ++num1;
                }
                Vector3d pos = vertex.Pos;
                double x = pos.X;
                pos = vertex.Pos;
                double y = pos.Y;
                pos = vertex.Pos;
                double z = pos.Z;
                nullable = new Vector3?(new Vector3((float) x, (float) y, (float) z));
                Vector3d normal1 = face.SurfaceDerivatives[vertex].Normal;
                Vector3 normal2 = new Vector3((float) normal1.X, (float) normal1.Y, (float) normal1.Z);
                vertexPositionNormalList.Add(new VertexPositionNormal(new Vector4(nullable.Value, 1f), normal2));
              }
              triple3.Item3 = (uint) (uintList1.Count - 1);
              edgeStartEndIdx.Add(triple3);
            }
          }
          ShellNode parent = new ShellNode(shell, (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) modelNode);
          ShellNode shellNode = parent;
          double M11 = shell.Transform[0, 0];
          Matrix4d transform = shell.Transform;
          double M12 = transform[0, 1];
          transform = shell.Transform;
          double M13 = transform[0, 2];
          transform = shell.Transform;
          double M21 = transform[1, 0];
          transform = shell.Transform;
          double M22 = transform[1, 1];
          transform = shell.Transform;
          double M23 = transform[1, 2];
          transform = shell.Transform;
          double M31 = transform[2, 0];
          transform = shell.Transform;
          double M32 = transform[2, 1];
          transform = shell.Transform;
          double M33 = transform[2, 2];
          transform = shell.Transform;
          double M41 = transform[3, 0];
          transform = shell.Transform;
          double M42 = transform[3, 1];
          transform = shell.Transform;
          double M43 = transform[3, 2];
          Matrix? nullable1 = new Matrix?(new Matrix((float) M11, (float) M12, (float) M13, 0.0f, (float) M21, (float) M22, (float) M23, 0.0f, (float) M31, (float) M32, (float) M33, 0.0f, (float) M41, (float) M42, (float) M43, 1f));
          shellNode.Transform = nullable1;
          TriangleMeshNode triangleMeshNode1 = new TriangleMeshNode(faceStartEndIdx, (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) parent, renderer.RenderData);
          triangleMeshNode1.Geometry = (IGeometry) new WiCAM.Pn4000.ScreenD3D.Renderer.Geometry.Geometry(renderer.Device, PrimitiveTopology.TriangleList, positionNormalTexList1.ToArray(), uintList2.ToArray());
          TriangleMeshNode triangleMeshNode2 = triangleMeshNode1;
          EdgesNode edgesNode1 = (EdgesNode) null;
          if (vertexPositionNormalList.Count > 0)
          {
            EdgesNode edgesNode2 = new EdgesNode(edgeStartEndIdx, (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) parent, renderer.RenderData);
            edgesNode2.Geometry = (IGeometry) new LineGeometry(renderer.Device, vertexPositionNormalList.ToArray(), uintList1.ToArray());
            edgesNode1 = edgesNode2;
          }
          parent.FacesNode = triangleMeshNode2;
          parent.EdgesNode = edgesNode1;
          lock (modelNode)
            modelNode.Children.Add((WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) parent);
        }
      }
    }

    private ModelNode FindModelNode(WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node root, Model model)
    {
      foreach (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node child in root.Children)
      {
        if (child is ModelNode modelNode1 && modelNode1.Model == model)
          return modelNode1;
        ModelNode modelNode2 = this.FindModelNode(child, model);
        if (modelNode2 != null)
          return modelNode2;
      }
      return (ModelNode) null;
    }
  }
}
