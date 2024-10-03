// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.BillboardNode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.ScreenD3D.Renderer.Effects;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Nodes
{
  public class BillboardNode : Node
  {
    private readonly SharpDX.Direct3D11.Device device;
    private readonly int billboardType;
    private readonly Texture2DBillboard billboard;
    private Vector2d extents;
    private ShaderResourceView billboardSRV;

    public override bool Enabled
    {
      get => this.billboard.Enabled;
      set => this.billboard.Enabled = value;
    }

    private void InitBillboardSRV(Texture2D billboardTexture)
    {
      ShaderResourceViewDescription.Texture2DResource texture2Dresource = new ShaderResourceViewDescription.Texture2DResource()
      {
        MostDetailedMip = 0,
        MipLevels = -1
      };
      ShaderResourceViewDescription description = new ShaderResourceViewDescription()
      {
        Format = Format.B8G8R8A8_UNorm,
        Dimension = ShaderResourceViewDimension.Texture2D,
        Texture2D = texture2Dresource
      };
      this.billboardSRV = new ShaderResourceView(this.device, (SharpDX.Direct3D11.Resource) billboardTexture, description);
      billboardTexture.Dispose();
    }

    public BillboardNode(
      Node parent,
      SharpDX.Direct3D11.Device device,
      Texture2DBillboard billboard,
      Texture2D billboardTexture)
      : base(parent)
    {
      this.extents = billboard.Extents;
      this.device = device;
      this.billboard = billboard;
      if (billboard.ScaleWithDistance)
        this.billboardType += 2;
      if (billboard.RenderOnTop)
        ++this.billboardType;
      this.InitBillboardSRV(billboardTexture);
    }

    public override void Render(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Matrix transform, bool renderAlpha)
    {
      if (renderer.RenderData.RenderMode != 0 | renderAlpha || !this.billboard.Enabled)
        return;
      if (this.Visible)
      {
        if (this.Transform.HasValue)
          transform *= this.Transform.Value;
        if (renderer.Effects[EffectType.Billboard] is BillboardEffect effect)
          effect.SetActive(renderAlpha);
        float num = (float) renderer.BlinnPhongCamData.viewportHeight / 1024f / this.billboard.PixelDensity;
        BillboardObjectConstBufferData objectData = new BillboardObjectConstBufferData()
        {
          _world = transform,
          _textureDimensions = new Vector2((float) this.extents.X * num, (float) this.extents.Y * num),
          _billboardType = this.billboardType,
          _sortingLayer = this.billboard.SortingLayer
        };
              
        //effect?.UpdateFrameConstantBuffer(ref renderer.BillboardCamData);
        //effect?.UpdatePerObjectConstBuffer(ref objectData);
        this.device.ImmediateContext.PixelShader.SetShaderResource(0, this.billboardSRV);
        this.Geometry.Draw();
      }
      base.Render(renderer, transform, renderAlpha);
    }

    public override void Dispose()
    {
      base.Dispose();
      this.billboardSRV?.Dispose();
    }
  }
}
