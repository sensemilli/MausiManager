// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.RemoveModelTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class RemoveModelTask : RenderTaskBase
  {
    public Model Model { get; }

    public RemoveModelTask(Model model, Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.Model = model;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      lock (renderer)
      {
        if (this.Model != null)
        {
          ModelNode n;
          if (renderer.ProtoModelMap.TryGetValue(this.Model, out n))
          {
            if (n.Parent != null)
            {
              lock (n.Parent)
                n.Parent.Children.Remove((Node) n);
            }
            n.Parent = (Node) null;
            DisposeRec((Node) n);
          }

          void DisposeRec(Node n)
          {
            if (n is ModelNode modelNode)
              renderer.ProtoModelMap.Remove(modelNode.Model);
            foreach (Node child in n.Children)
              DisposeRec(child);
            n.Dispose();
          }
        }
        else
        {
          foreach (ModelNode n in renderer.ProtoModelMap.Values.Where<ModelNode>((Func<ModelNode, bool>) (x => x.Model.ModelType != ModelType.System && x.Model.ModelType != ModelType.Static)).ToList<ModelNode>())
          {
            if (n != renderer.Root.Children.FirstOrDefault<Node>())
            {
              if (n.Parent != null)
              {
                lock (n.Parent)
                  n.Parent.Children.Remove((Node) n);
              }
              n.Parent = (Node) null;
              DisposeRec((Node) n);
            }
          }

          void DisposeRec(Node n)
          {
            if (n is ModelNode modelNode && modelNode.Model != null)
              renderer.ProtoModelMap.Remove(modelNode.Model);
            foreach (Node child in n.Children)
              DisposeRec(child);
            n.Dispose();
          }
        }
        renderer.RenderData.ShadowmapUpToDate = false;
      }
    }
  }
}
