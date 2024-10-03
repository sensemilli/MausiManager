// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Effects.LineEffect
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Effects
{
  public class LineEffect : BaseEffect
  {
    protected GeometryShader _geometryShader;

    public LineEffect(SharpDX.Direct3D11.Device device, RenderData renderData)
      : base(device, renderData)
    {
      this.InitGeometryShader();
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
      CompilationResult shaderBytecode = ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/line.hlsl", "VS_LineV2", "vs_5_0", ShaderFlags.EnableStrictness);
      this._vertexShader = new VertexShader(this._device, (byte[]) shaderBytecode);
      return shaderBytecode;
    }

    protected virtual void InitGeometryShader() => this._geometryShader = new GeometryShader(this._device, (byte[]) ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/line.hlsl", "GS_LineV2", "gs_5_0", ShaderFlags.EnableStrictness));

    protected override void InitPixelShader()
    {
      this._pixelShader = new PixelShader(this._device, (byte[]) ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/line.hlsl", "PS_LineV2_OIT", "ps_5_0", ShaderFlags.EnableStrictness));
      ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/line.hlsl", "PS_LineV2", "ps_5_0", ShaderFlags.EnableStrictness);
    }

    protected override void InitPerObjectConstantBuffer() => this._constantBufferObject = new SharpDX.Direct3D11.Buffer(this._device, new BufferDescription()
    {
      Usage = ResourceUsage.Dynamic,
      BindFlags = BindFlags.ConstantBuffer,
      SizeInBytes = Utilities.SizeOf<LineObjectConstBufferData>(),
      CpuAccessFlags = CpuAccessFlags.Write,
      StructureByteStride = 0,
      OptionFlags = ResourceOptionFlags.None
    });

    protected override void InitFrameConstantBuffer() => this._constantBufferFrame = new SharpDX.Direct3D11.Buffer(this._device, new BufferDescription()
    {
      Usage = ResourceUsage.Dynamic,
      BindFlags = BindFlags.ConstantBuffer,
      SizeInBytes = Utilities.SizeOf<LineCameraConstBufferData>(),
      CpuAccessFlags = CpuAccessFlags.Write,
      StructureByteStride = 0,
      OptionFlags = ResourceOptionFlags.None
    });

    protected override void InitLightingConstantBuffer()
    {
    }

    protected override void InitInputLayout(SharpDX.Direct3D11.Device device, CompilationResult shaderByteCodeVS) => this._vertexInputLayout = new InputLayout(device, shaderByteCodeVS.Bytecode.Data, new InputElement[2]
    {
      new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
      new InputElement("NORMAL", 0, Format.R32G32B32_Float, 16, 0)
    });

    public override void SetActive(bool renderAlpha)
    {
      base.SetActive(renderAlpha);
      this._device.ImmediateContext.GeometryShader.SetConstantBuffer(0, this._constantBufferFrame);
      this._device.ImmediateContext.GeometryShader.SetConstantBuffer(1, this._constantBufferObject);
      this._device.ImmediateContext.GeometryShader.Set(this._geometryShader);
    }

    public void UpdatePerObjectConstBuffer(ref LineObjectConstBufferData objectData)
    {
      DataStream stream;
      this._device.ImmediateContext.MapSubresource(this._constantBufferObject, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
      stream.Write<LineObjectConstBufferData>(objectData);
      stream.Dispose();
      this._device.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) this._constantBufferObject, 0);
    }

    public void UpdateFrameConstantBuffer(ref LineCameraConstBufferData camData)
    {
      DataStream stream;
      this._device.ImmediateContext.MapSubresource(this._constantBufferFrame, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
      stream.Write<LineCameraConstBufferData>(camData);
      stream.Dispose();
      this._device.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) this._constantBufferFrame, 0);
    }

    public override void Dispose()
    {
      base.Dispose();
      this._geometryShader.Dispose();
      this._geometryShader = (GeometryShader) null;
    }
  }
}
