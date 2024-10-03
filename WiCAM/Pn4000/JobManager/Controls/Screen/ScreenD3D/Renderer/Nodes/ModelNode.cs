// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.ModelNode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Nodes
{
  public class ModelNode : Node
  {
    public Model Model { get; private set; }

    public override bool Enabled
    {
      get => this.Model.Enabled;
      set => this.Model.Enabled = value;
    }

    public override bool EnabledSubNodes
    {
      get => this.Model.EnabledSubModels;
      set => this.Model.EnabledSubModels = value;
    }

    public override Matrix? Transform { get; set; }

    public ModelNode(Model model, Node parent)
      : base(parent)
    {
      this.Model = model;
    }

    public override void Render(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Matrix transform, bool renderAlpha)
    {
      if ((renderer.RenderData.RenderMode == RenderData.RndMode.Tripod || this.Model.PartRole == PartRole.Tripod || this.Model.PartRole == PartRole.BillboardModel) && (renderer.RenderData.RenderMode != RenderData.RndMode.Tripod || this.Model.PartRole != PartRole.Tripod && this.Model.PartRole != PartRole.BillboardModel))
        return;
      base.Render(renderer, transform, renderAlpha);
    }

    public override void Dispose()
    {
      base.Dispose();
      this.Model = (Model) null;
    }
  }
}
