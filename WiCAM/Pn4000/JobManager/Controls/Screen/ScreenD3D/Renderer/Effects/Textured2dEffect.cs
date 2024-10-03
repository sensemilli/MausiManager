// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Effects.Textured2dEffect
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Effects
{
  public class Textured2dEffect : BaseEffect
  {
    public Textured2dEffect(Device device, RenderData renderData)
      : base(device, renderData)
    {
    }

    protected override void InitBlendState()
    {
      BlendStateDescription description = new BlendStateDescription()
      {
        AlphaToCoverageEnable = (RawBool) true,
        IndependentBlendEnable = (RawBool) true
      };
      description.RenderTarget[0].IsBlendEnabled = (RawBool) true;
      description.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
      description.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
      description.RenderTarget[0].BlendOperation = BlendOperation.Add;
      description.RenderTarget[0].SourceAlphaBlend = BlendOption.SourceAlpha;
      description.RenderTarget[0].DestinationAlphaBlend = BlendOption.InverseSourceAlpha;
      description.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
      description.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
      this._blendState = new BlendState(this._device, description);
    }

    protected override CompilationResult InitVertexShader()
    {
      CompilationResult shaderBytecode = ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/textured2d.hlsl", "Textured2dVertexShader", "vs_5_0", ShaderFlags.EnableStrictness);
      this._vertexShader = new VertexShader(this._device, (byte[]) shaderBytecode);
      return shaderBytecode;
    }

    protected override void InitPixelShader() => this._pixelShader = new PixelShader(this._device, (byte[]) ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/textured2d.hlsl", "Textured2dPixelShader", "ps_5_0", ShaderFlags.EnableStrictness));

    protected override void InitRasterizerState() => this._rasterizerState = new RasterizerState(this._device, new RasterizerStateDescription()
    {
      CullMode = CullMode.None,
      FillMode = FillMode.Solid,
      IsDepthClipEnabled = (RawBool) false,
      IsFrontCounterClockwise = (RawBool) true,
      IsMultisampleEnabled = (RawBool) true,
      IsAntialiasedLineEnabled = (RawBool) true
    });

    public override void SetActive(bool renderAlpha)
    {
      base.SetActive(renderAlpha);
      this._device.ImmediateContext.PixelShader.SetShaderResource(0, this._renderData.RenderTextureViewOSTripod);
    }
  }
}
