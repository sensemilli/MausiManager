// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.UpdateModelTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.ScreenD3D.Renderer.Geometry;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class UpdateModelTask : RenderTaskBase
  {
    public Model Model { get; }

    public Model Parent { get; }

    public UpdateModelTask(Model model, Model parent, Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.Model = model;
      this.Parent = parent;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      try
      {
        this.CreateModelNode(renderer, this.Model);
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

    private void CreateModelNode(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Model model)
    {
      lock (model)
      {
        ModelNode modelNode;
        if (!renderer.ProtoModelMap.TryGetValue(model, out modelNode))
          return;
        this.CreateShellNodes(model, modelNode);
      }
    }

    private void CreateShellNodes(Model model, ModelNode modelNode)
    {
      lock (model)
      {
        foreach (Shell shell1 in model.Shells)
        {
          Shell shell = shell1;
          lock (shell)
          {
            List<VertexPositionNormalTex> positionNormalTexList1 = new List<VertexPositionNormalTex>();
            List<VertexPositionNormal> vertexPositionNormalList = new List<VertexPositionNormal>();
            List<uint> uintList = new List<uint>();
            Dictionary<Vector3d, List<Pair<uint, Face>>> dictionary = new Dictionary<Vector3d, List<Pair<uint, Face>>>();
            uint num1 = 0;
            uint num2 = 0;
            foreach (Vertex key in shell.VertexCache.Values)
            {
              List<Pair<uint, Face>> pairList = new List<Pair<uint, Face>>();
              foreach (Face face in key.Faces)
              {
                SurfaceDerivatives surfaceDerivative = face.SurfaceDerivatives[key];
                List<VertexPositionNormalTex> positionNormalTexList2 = positionNormalTexList1;
                Vector3 position = new Vector3((float) key.Pos.X, (float) key.Pos.Y, (float) key.Pos.Z);
                Vector3d normal1 = surfaceDerivative.Normal;
                double x = normal1.X;
                normal1 = surfaceDerivative.Normal;
                double y = normal1.Y;
                normal1 = surfaceDerivative.Normal;
                double z = normal1.Z;
                Vector3 normal2 = new Vector3((float) x, (float) y, (float) z);
                Vector2 tex = new Vector2((float) surfaceDerivative.UV.X, (float) surfaceDerivative.UV.Y);
                VertexPositionNormalTex positionNormalTex = new VertexPositionNormalTex(position, normal2, tex);
                positionNormalTexList2.Add(positionNormalTex);
                pairList.Add(new Pair<uint, Face>(num2, face));
                ++num2;
              }
              dictionary.Add(key.Pos, pairList);
            }
            foreach (Face face in shell.Faces)
            {
              foreach (FaceHalfEdge faceHalfEdge in face.BoundaryEdgesCcw)
              {
                ++num1;
                Vector3? nullable = new Vector3?();
                foreach (Vertex vertex in faceHalfEdge.Vertices)
                {
                  if (nullable.HasValue)
                  {
                    uintList.Add(num1 - 1U);
                    uintList.Add(num1);
                    ++num1;
                  }
                  Vector3d pos = vertex.Pos;
                  double x = pos.X;
                  pos = vertex.Pos;
                  double y = pos.Y;
                  pos = vertex.Pos;
                  double z = pos.Z;
                  nullable = new Vector3?(new Vector3((float) x, (float) y, (float) z));
                  Vector3d normal3 = face.SurfaceDerivatives[vertex].Normal;
                  Vector3 normal4 = new Vector3((float) normal3.X, (float) normal3.Y, (float) normal3.Z);
                  vertexPositionNormalList.Add(new VertexPositionNormal(new Vector4(nullable.Value, 1f), normal4));
                }
              }
              foreach (FaceHalfEdge faceHalfEdge in face.HoleEdgesCw.SelectMany<List<FaceHalfEdge>, FaceHalfEdge>((Func<List<FaceHalfEdge>, IEnumerable<FaceHalfEdge>>) (x => (IEnumerable<FaceHalfEdge>) x)))
              {
                ++num1;
                Vector3? nullable = new Vector3?();
                foreach (Vertex vertex in faceHalfEdge.Vertices)
                {
                  if (nullable.HasValue)
                  {
                    uintList.Add(num1 - 1U);
                    uintList.Add(num1);
                    ++num1;
                  }
                  Vector3d pos = vertex.Pos;
                  double x = pos.X;
                  pos = vertex.Pos;
                  double y = pos.Y;
                  pos = vertex.Pos;
                  double z = pos.Z;
                  nullable = new Vector3?(new Vector3((float) x, (float) y, (float) z));
                  Vector3d normal5 = face.SurfaceDerivatives[vertex].Normal;
                  Vector3 normal6 = new Vector3((float) normal5.X, (float) normal5.Y, (float) normal5.Z);
                  vertexPositionNormalList.Add(new VertexPositionNormal(new Vector4(nullable.Value, 1f), normal6));
                }
              }
            }
            ShellNode shellNode1 = modelNode.Children.FirstOrDefault<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>((Func<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node, bool>) (n =>
            {
              if (!(n is ShellNode shellNode3))
                return false;
              int? id1 = shellNode3.Shell.RoundFaceGroups.FirstOrDefault<FaceGroup>()?.ID;
              int? id2 = shell.RoundFaceGroups.FirstOrDefault<FaceGroup>()?.ID;
              return id1.GetValueOrDefault() == id2.GetValueOrDefault() & id1.HasValue == id2.HasValue;
            })) as ShellNode;
            shellNode1.FacesNode.Geometry.UpdateMeshVertexBuffer(positionNormalTexList1.ToArray());
            shellNode1.EdgesNode?.Geometry.UpdateLineVertexBuffer(vertexPositionNormalList.ToArray());
          }
        }
      }
    }
  }
}
