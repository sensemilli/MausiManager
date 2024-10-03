// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Effects.DepthEffect
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Effects
{
  public class DepthEffect : BaseEffect
  {
    public DepthEffect(Device device, RenderData renderData)
      : base(device, renderData)
    {
    }

    protected override CompilationResult InitVertexShader()
    {
      CompilationResult shaderBytecode = ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/simpledepth.hlsl", "SimpleDepthVertexShader", "vs_5_0", ShaderFlags.EnableStrictness);
      this._vertexShader = new VertexShader(this._device, (byte[]) shaderBytecode);
      return shaderBytecode;
    }

    protected override void InitPixelShader() => this._pixelShader = new PixelShader(this._device, (byte[]) ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/simpledepth.hlsl", "SimpleDepthPixelShader", "ps_5_0", ShaderFlags.EnableStrictness));

    protected override void InitRasterizerState() => this._rasterizerState = new RasterizerState(this._device, new RasterizerStateDescription()
    {
      CullMode = CullMode.Back,
      FillMode = FillMode.Solid,
      IsDepthClipEnabled = (RawBool) false,
      IsFrontCounterClockwise = (RawBool) true,
      IsMultisampleEnabled = (RawBool) false,
      IsAntialiasedLineEnabled = (RawBool) true
    });
  }
}
