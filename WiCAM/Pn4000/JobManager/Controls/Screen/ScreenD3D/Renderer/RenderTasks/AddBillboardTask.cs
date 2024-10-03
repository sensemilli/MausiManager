// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.AddBillboardTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using SharpDX.Direct3D11;
using System;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.ScreenD3D.Renderer.Geometry;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class AddBillboardTask : RenderTaskBase
  {
    public Texture2DBillboard Billboard { get; }

    public Model Parent { get; }

    public AddBillboardTask(
      Texture2DBillboard billboard,
      Model parent,
      Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.Billboard = billboard;
      this.Parent = parent;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      try
      {
        lock (renderer)
          this.CreateBillboardNode(renderer, this.Billboard, this.Parent);
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

    private void CreateBillboardNode(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Texture2DBillboard billboard, Model parent)
    {
      if (billboard == null)
        return;
      lock (billboard)
      {
        BillboardNode billboardNode1 = new BillboardNode(renderer.Root, renderer.Device, billboard, billboard.Texture);
        billboardNode1.Transform = new Matrix?(new Matrix((float) billboard.Transform[0, 0], (float) billboard.Transform[0, 1], (float) billboard.Transform[0, 2], 0.0f, (float) billboard.Transform[1, 0], (float) billboard.Transform[1, 1], (float) billboard.Transform[1, 2], 0.0f, (float) billboard.Transform[2, 0], (float) billboard.Transform[2, 1], (float) billboard.Transform[2, 2], 0.0f, (float) billboard.Transform[3, 0], (float) billboard.Transform[3, 1], (float) billboard.Transform[3, 2], 1f));
        BillboardNode billboardNode2 = billboardNode1;
        Device device = renderer.Device;
        Vector3d position1 = billboard.Position;
        double x = position1.X;
        position1 = billboard.Position;
        double y = position1.Y;
        position1 = billboard.Position;
        double z = position1.Z;
        Vector4 position2 = new Vector4((float) x, (float) y, (float) z, 1f);
        uint[] indices = new uint[1];
        BillboardGeometry billboardGeometry = new BillboardGeometry(device, position2, indices);
        billboardNode2.Geometry = (IGeometry) billboardGeometry;
        BillboardNode billboardNode3 = billboardNode1;
        if (parent == null)
        {
          lock (renderer.Root)
            renderer.Root.Children.Add((Node) billboardNode3);
          billboardNode3.Parent = renderer.Root;
        }
        else
        {
          ModelNode modelNode = this.FindModelNode(renderer.Root, parent);
          if (modelNode != null)
          {
            lock (modelNode)
              modelNode.Children.Add((Node) billboardNode3);
          }
          billboardNode3.Parent = (Node) modelNode;
        }
      }
    }

    private ModelNode FindModelNode(Node root, Model model)
    {
      foreach (Node child in root.Children)
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
