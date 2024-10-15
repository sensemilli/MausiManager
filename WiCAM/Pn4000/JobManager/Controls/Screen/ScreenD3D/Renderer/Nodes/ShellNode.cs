// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.ShellNode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Nodes
{
  public class ShellNode : Node
  {
    public Shell Shell { get; private set; }

    public TriangleMeshNode FacesNode { get; set; }

    public EdgesNode EdgesNode { get; set; }

    public override Matrix? Transform => this.Shell != null ? new Matrix?(new Matrix((float) this.Shell.Transform[0, 0], (float) this.Shell.Transform[0, 1], (float) this.Shell.Transform[0, 2], 0.0f, (float) this.Shell.Transform[1, 0], (float) this.Shell.Transform[1, 1], (float) this.Shell.Transform[1, 2], 0.0f, (float) this.Shell.Transform[2, 0], (float) this.Shell.Transform[2, 1], (float) this.Shell.Transform[2, 2], 0.0f, (float) this.Shell.Transform[3, 0], (float) this.Shell.Transform[3, 1], (float) this.Shell.Transform[3, 2], 1f)) : new Matrix?(Matrix.Identity);

    public ShellNode(Shell shell, Node parent)
      : base(parent)
    {
      this.Shell = shell;
    }

    public override void Render(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Matrix transform, bool renderAlpha)
    {
      if (this.Visible && this.Transform.HasValue)
        transform = this.Transform.Value * transform;
      if (this.FacesNode != null && this.FacesNode.Enabled)
        this.FacesNode.Render(renderer, transform, renderAlpha);
      if (this.EdgesNode == null || !this.EdgesNode.Enabled)
        return;
      this.EdgesNode?.Render(renderer, transform, renderAlpha);
    }

    public override void Dispose()
    {
      base.Dispose();
      this.FacesNode?.Dispose();
      this.EdgesNode?.Dispose();
      this.Shell = (Shell) null;
    }
  }
}
