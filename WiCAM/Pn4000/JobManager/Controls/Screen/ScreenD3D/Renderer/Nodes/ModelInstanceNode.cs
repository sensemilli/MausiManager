// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.ModelInstanceNode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Nodes
{
  public class ModelInstanceNode : Node
  {
    public ModelInstance ModelInstance { get; private set; }

    public override bool Enabled
    {
      get => this.ModelInstance.Enabled;
      set => this.ModelInstance.Enabled = value;
    }

    public ModelInstanceNode(ModelInstance instance, Node parent)
      : base(parent)
    {
      this.ModelInstance = instance;
    }

    public override void Render(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Matrix transform, bool renderAlpha)
    {
      if (!this.Enabled || !this.ModelInstance.Reference.Enabled)
        return;
      base.Render(renderer, transform, renderAlpha);
    }

    public override void Dispose()
    {
      base.Dispose();
      this.ModelInstance = (ModelInstance) null;
    }
  }
}
