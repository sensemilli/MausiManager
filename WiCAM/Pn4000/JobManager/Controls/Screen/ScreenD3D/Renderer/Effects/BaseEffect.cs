// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Effects.BaseEffect
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Drawing;
using System.IO;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Effects
{
  public class BaseEffect : IDisposable
  {
    protected SharpDX.Direct3D11.Device _device;
    protected RenderData _renderData;
    protected VertexShader _vertexShader;
    protected VertexShader _vertexShaderShadowMap;
    protected PixelShader _pixelShaderSoftShadows;
    protected PixelShader _pixelShaderHardShadows;
    protected PixelShader _pixelShader;
    protected PixelShader _pixelShaderShadowMap;
    protected BlendState _blendState;
    protected DepthStencilState _depthStencilState;
    protected DepthStencilState _readonlyDepthStencilState;
    protected RasterizerState _rasterizerState;
    protected RasterizerState _rasterizerStateTransparency;
    protected SamplerState _colorSamplerState;
    protected SamplerState _imageLightSamplerState;
    protected SamplerState _shadowmapSamplerState;
    protected SharpDX.Direct3D11.Buffer _constantBufferObject;
    protected SharpDX.Direct3D11.Buffer _constantBufferFrame;
    protected SharpDX.Direct3D11.Buffer _constantDirLightBuffer;
    protected SharpDX.Direct3D11.Buffer _constantBufferViewport;
    protected InputLayout _vertexInputLayout;

    public BaseEffect(SharpDX.Direct3D11.Device device, RenderData renderData)
    {
      this._device = device;
      this._renderData = renderData;
      CompilationResult shaderByteCodeVS = this.InitVertexShader();
      this.InitPixelShader();
      this.InitBlendState();
      this.InitDepthStencil();
      this.InitRasterizerState();
      this.InitTextureSamplerState();
      this.InitPerObjectConstantBuffer();
      this.InitFrameConstantBuffer();
      this.InitLightingConstantBuffer();
      this.InitViewportConstantsBuffer();
      this.InitInputLayout(device, shaderByteCodeVS);
      this.InitImageLightSamplerState();
      this.InitShadowMapSamplerState();
    }

    protected virtual CompilationResult InitVertexShader()
    {
      CompilationResult shaderBytecode = ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/blinnphong.hlsl", "VS_BLINNPHONG", "vs_5_0", ShaderFlags.EnableStrictness);
      this._vertexShader = new VertexShader(this._device, (byte[]) shaderBytecode);
      this._vertexShaderShadowMap = new VertexShader(this._device, (byte[]) ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/passthrough.hlsl", "VS_PASSTHROUGH", "vs_5_0", ShaderFlags.EnableStrictness));
      return shaderBytecode;
    }

    protected virtual void InitPixelShader()
    {
      this._pixelShaderSoftShadows = new PixelShader(this._device, (byte[]) ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/blinnphong.hlsl", "PS_BLINN_SOFTSHADOWS", "ps_5_0", ShaderFlags.EnableStrictness));
      this._pixelShaderHardShadows = new PixelShader(this._device, (byte[]) ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/blinnphong.hlsl", "PS_BLINN_HARDSHADOWS", "ps_5_0", ShaderFlags.EnableStrictness));
      this._pixelShader = new PixelShader(this._device, (byte[]) ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/blinnphong.hlsl", "PS_BLINN", "ps_5_0", ShaderFlags.EnableStrictness));
      this._pixelShaderShadowMap = new PixelShader(this._device, (byte[]) ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/passthrough.hlsl", "PS_PASSTHROUGH", "ps_5_0", ShaderFlags.EnableStrictness));
    }

    protected virtual void InitDepthStencil()
    {
      this._depthStencilState = new DepthStencilState(this._device, new DepthStencilStateDescription()
      {
        IsDepthEnabled = (RawBool) true,
        DepthWriteMask = DepthWriteMask.All,
        DepthComparison = Comparison.LessEqual,
        IsStencilEnabled = (RawBool) false
      });
      this._readonlyDepthStencilState = new DepthStencilState(this._device, new DepthStencilStateDescription()
      {
        IsDepthEnabled = (RawBool) true,
        DepthWriteMask = DepthWriteMask.Zero,
        DepthComparison = Comparison.LessEqual,
        IsStencilEnabled = (RawBool) false
      });
    }

    public byte[] ImageToByteArray(Image imageIn)
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        imageIn.Save((Stream) memoryStream, imageIn.RawFormat);
        return memoryStream.ToArray();
      }
    }

    protected virtual void InitBlendState()
    {
      BlendStateDescription description = new BlendStateDescription()
      {
        AlphaToCoverageEnable = (RawBool) false,
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

    protected virtual void InitRasterizerState()
    {
      this._rasterizerState = new RasterizerState(this._device, new RasterizerStateDescription()
      {
        CullMode = CullMode.Back,
        FillMode = FillMode.Solid,
        IsDepthClipEnabled = (RawBool) false,
        IsFrontCounterClockwise = (RawBool) true,
        IsMultisampleEnabled = (RawBool) true,
        IsAntialiasedLineEnabled = (RawBool) true
      });
      this._rasterizerStateTransparency = new RasterizerState(this._device, new RasterizerStateDescription()
      {
        CullMode = CullMode.None,
        FillMode = FillMode.Solid,
        IsDepthClipEnabled = (RawBool) false,
        IsFrontCounterClockwise = (RawBool) true,
        IsMultisampleEnabled = (RawBool) true,
        IsAntialiasedLineEnabled = (RawBool) true
      });
    }

    protected virtual void InitTextureSamplerState() => this._colorSamplerState = new SamplerState(this._device, new SamplerStateDescription()
    {
      Filter = SharpDX.Direct3D11.Filter.Anisotropic,
      AddressU = TextureAddressMode.Wrap,
      AddressV = TextureAddressMode.Wrap,
      AddressW = TextureAddressMode.Wrap,
      BorderColor = (RawColor4) new Color4(0.0f, 0.0f, 0.0f, 0.0f)
    });

    protected virtual void InitImageLightSamplerState() => this._imageLightSamplerState = new SamplerState(this._device, new SamplerStateDescription()
    {
      Filter = SharpDX.Direct3D11.Filter.Anisotropic,
      AddressU = TextureAddressMode.Wrap,
      AddressV = TextureAddressMode.Wrap,
      AddressW = TextureAddressMode.Wrap,
      BorderColor = (RawColor4) new Color4(0.0f, 0.0f, 0.0f, 0.0f)
    });

    protected virtual void InitShadowMapSamplerState() => this._shadowmapSamplerState = new SamplerState(this._device, new SamplerStateDescription()
    {
      Filter = SharpDX.Direct3D11.Filter.ComparisonMinMagMipPoint,
      AddressU = TextureAddressMode.Clamp,
      AddressV = TextureAddressMode.Clamp,
      AddressW = TextureAddressMode.Clamp,
      BorderColor = (RawColor4) new Color4(0.0f, 0.0f, 0.0f, 0.0f),
      ComparisonFunction = Comparison.GreaterEqual
    });

    protected virtual void InitPerObjectConstantBuffer() => this._constantBufferObject = new SharpDX.Direct3D11.Buffer(this._device, new BufferDescription()
    {
      Usage = ResourceUsage.Dynamic,
      BindFlags = BindFlags.ConstantBuffer,
      SizeInBytes = Utilities.SizeOf<PhongObjectConstBufferData>(),
      CpuAccessFlags = CpuAccessFlags.Write,
      StructureByteStride = 0,
      OptionFlags = ResourceOptionFlags.None
    });

    protected virtual void InitFrameConstantBuffer() => this._constantBufferFrame = new SharpDX.Direct3D11.Buffer(this._device, new BufferDescription()
    {
      Usage = ResourceUsage.Dynamic,
      BindFlags = BindFlags.ConstantBuffer,
      SizeInBytes = Utilities.SizeOf<PhongCameraConstBufferData>(),
      CpuAccessFlags = CpuAccessFlags.Write,
      StructureByteStride = 0,
      OptionFlags = ResourceOptionFlags.None
    });

    protected virtual void InitLightingConstantBuffer() => this._constantDirLightBuffer = new SharpDX.Direct3D11.Buffer(this._device, new BufferDescription()
    {
      Usage = ResourceUsage.Dynamic,
      BindFlags = BindFlags.ConstantBuffer,
      SizeInBytes = Utilities.SizeOf<DirectionalLightBufferData>(),
      CpuAccessFlags = CpuAccessFlags.Write,
      StructureByteStride = 0,
      OptionFlags = ResourceOptionFlags.None
    });

    protected virtual void InitViewportConstantsBuffer() => this._constantBufferViewport = new SharpDX.Direct3D11.Buffer(this._device, new BufferDescription()
    {
      Usage = ResourceUsage.Dynamic,
      BindFlags = BindFlags.ConstantBuffer,
      SizeInBytes = Utilities.SizeOf<ViewportBufferData>(),
      CpuAccessFlags = CpuAccessFlags.Write,
      StructureByteStride = 0,
      OptionFlags = ResourceOptionFlags.None
    });

    protected virtual void InitInputLayout(SharpDX.Direct3D11.Device device, CompilationResult shaderByteCodeVS) => this._vertexInputLayout = new InputLayout(device, shaderByteCodeVS.Bytecode.Data, new InputElement[3]
    {
      new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
      new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
      new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
    });

    public virtual void SetActive(bool renderAlpha)
    {
      this._device.ImmediateContext.VertexShader.SetConstantBuffer(0, this._constantBufferFrame);
      this._device.ImmediateContext.VertexShader.SetConstantBuffer(1, this._constantBufferObject);
      if (this._constantDirLightBuffer != null)
        this._device.ImmediateContext.VertexShader.SetConstantBuffer(2, this._constantDirLightBuffer);
      this._device.ImmediateContext.PixelShader.SetConstantBuffer(0, this._constantBufferFrame);
      this._device.ImmediateContext.PixelShader.SetConstantBuffer(1, this._constantBufferObject);
      if (this._constantDirLightBuffer != null)
        this._device.ImmediateContext.PixelShader.SetConstantBuffer(2, this._constantDirLightBuffer);
      this._device.ImmediateContext.PixelShader.SetSampler(0, this._colorSamplerState);
      this._device.ImmediateContext.PixelShader.SetSampler(1, this._imageLightSamplerState);
      this._device.ImmediateContext.PixelShader.SetSampler(2, this._shadowmapSamplerState);
      this._device.ImmediateContext.VertexShader.Set(this._vertexShader);
      this._device.ImmediateContext.OutputMerger.SetBlendState(this._blendState);
      if (renderAlpha && this._renderData.RenderMode != RenderData.RndMode.ShadowMap)
      {
        this._device.ImmediateContext.OutputMerger.SetDepthStencilState(this._readonlyDepthStencilState);
        this._device.ImmediateContext.Rasterizer.State = this._rasterizerStateTransparency;
      }
      else
      {
        this._device.ImmediateContext.OutputMerger.SetDepthStencilState(this._depthStencilState);
        this._device.ImmediateContext.Rasterizer.State = this._rasterizerState;
      }
      if (this._renderData.RenderMode == RenderData.RndMode.ShadowMap && this._vertexShaderShadowMap != null && this._pixelShaderShadowMap != null)
      {
        this._device.ImmediateContext.VertexShader.Set(this._vertexShaderShadowMap);
        this._device.ImmediateContext.PixelShader.Set(this._pixelShaderShadowMap);
      }
      else if (this._renderData.ShadowMode == RenderData.Shadows.Soft && this._pixelShaderSoftShadows != null)
        this._device.ImmediateContext.PixelShader.Set(this._pixelShaderSoftShadows);
      else if (this._renderData.ShadowMode == RenderData.Shadows.Hard && this._pixelShaderHardShadows != null)
        this._device.ImmediateContext.PixelShader.Set(this._pixelShaderHardShadows);
      else
        this._device.ImmediateContext.PixelShader.Set(this._pixelShader);
      this._device.ImmediateContext.GeometryShader.Set((GeometryShader) null);
      this._device.ImmediateContext.InputAssembler.InputLayout = this._vertexInputLayout;
    }

    public void UpdateViewportBuffer(ref ViewportBufferData viewportData)
    {
      DataStream stream;
      this._device.ImmediateContext.MapSubresource(this._constantBufferObject, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
      stream.Write<ViewportBufferData>(viewportData);
      stream.Dispose();
      this._device.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) this._constantBufferObject, 0);
    }

    public void UpdatePerObjectConstBuffer(ref PhongObjectConstBufferData objectData)
    {
      DataStream stream;
      this._device.ImmediateContext.MapSubresource(this._constantBufferObject, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
      stream.Write<PhongObjectConstBufferData>(objectData);
      stream.Dispose();
      this._device.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) this._constantBufferObject, 0);
    }

    public void UpdateFrameConstantBuffer(ref PhongCameraConstBufferData camData)
    {
      DataStream stream;
      this._device.ImmediateContext.MapSubresource(this._constantBufferFrame, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
      stream.Write<PhongCameraConstBufferData>(camData);
      stream.Dispose();
      this._device.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) this._constantBufferFrame, 0);
    }

    public void UpdateLightingConstantBuffer(ref DirectionalLightBufferData dirLightData)
    {
      DataStream stream;
      this._device.ImmediateContext.MapSubresource(this._constantDirLightBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
      stream.Write<DirectionalLightBufferData>(dirLightData);
      stream.Dispose();
      this._device.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) this._constantDirLightBuffer, 0);
    }

    public virtual void Dispose()
    {
      this._vertexShader?.Dispose();
      this._vertexShaderShadowMap?.Dispose();
      this._pixelShader?.Dispose();
      this._pixelShaderSoftShadows?.Dispose();
      this._pixelShaderHardShadows?.Dispose();
      this._pixelShaderShadowMap?.Dispose();
      this._blendState?.Dispose();
      this._depthStencilState?.Dispose();
      this._readonlyDepthStencilState?.Dispose();
      this._rasterizerState?.Dispose();
      this._rasterizerStateTransparency?.Dispose();
      this._colorSamplerState?.Dispose();
      this._shadowmapSamplerState?.Dispose();
      this._constantBufferObject?.Dispose();
      this._constantBufferFrame?.Dispose();
      this._constantDirLightBuffer?.Dispose();
      this._imageLightSamplerState?.Dispose();
      this._constantBufferViewport?.Dispose();
      this._vertexInputLayout?.Dispose();
      this._vertexShader = (VertexShader) null;
      this._vertexShaderShadowMap = (VertexShader) null;
      this._pixelShader = (PixelShader) null;
      this._pixelShaderSoftShadows = (PixelShader) null;
      this._pixelShaderHardShadows = (PixelShader) null;
      this._pixelShaderShadowMap = (PixelShader) null;
      this._blendState = (BlendState) null;
      this._depthStencilState = (DepthStencilState) null;
      this._readonlyDepthStencilState = (DepthStencilState) null;
      this._rasterizerState = (RasterizerState) null;
      this._rasterizerStateTransparency = (RasterizerState) null;
      this._colorSamplerState = (SamplerState) null;
      this._shadowmapSamplerState = (SamplerState) null;
      this._constantBufferObject = (SharpDX.Direct3D11.Buffer) null;
      this._constantBufferFrame = (SharpDX.Direct3D11.Buffer) null;
      this._constantDirLightBuffer = (SharpDX.Direct3D11.Buffer) null;
      this._imageLightSamplerState = (SamplerState) null;
      this._constantBufferViewport = (SharpDX.Direct3D11.Buffer) null;
      this._vertexInputLayout = (InputLayout) null;
    }
  }
}
