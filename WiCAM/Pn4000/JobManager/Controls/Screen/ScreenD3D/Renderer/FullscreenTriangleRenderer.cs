// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.FullscreenTriangleRenderer
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Runtime.InteropServices;
using WiCAM.Pn4000.ScreenD3D.Renderer.Effects;
using WiCAM.Pn4000.ScreenD3D.Renderer.Geometry;

namespace WiCAM.Pn4000.ScreenD3D.Renderer
{
  public class FullscreenTriangleRenderer : IDisposable
  {
    private SharpDX.Direct3D11.Device Device;
    private PixelShader _pixelShaderB;
    private VertexShader _vertexShader;
    private VertexBufferBinding _vertexBufferBinding;
    private SharpDX.Direct3D11.Buffer _indexBuffer;
    private InputLayout _vertexInputLayout;
    private BlendState _blendState;
    private SharpDX.Direct3D11.Buffer _constantBufferFrame;
    private SharpDX.Direct3D11.Buffer _vertexBuffer;
    private DepthStencilState _depthStencilState;

    public void Init(SharpDX.Direct3D11.Device device)
    {
      this.Device = device;
      this.InitVertexShader();
      this.InitPixelShaderB();
      this.InitFrameConstantBuffer();
      this.InitIndexBuffer();
      this.InitDepthStencil();
      this.InitBlendState();
    }

    protected virtual void InitDepthStencil() => this._depthStencilState = new DepthStencilState(this.Device, new DepthStencilStateDescription()
    {
      IsDepthEnabled = (RawBool) true,
      DepthWriteMask = DepthWriteMask.Zero,
      DepthComparison = Comparison.Always,
      IsStencilEnabled = (RawBool) false
    });

    private void InitPixelShaderB() => this._pixelShaderB = new PixelShader(this.Device, (byte[]) ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/transparencydraw.hlsl", "PS_RENDERFRAGMENT", "ps_5_0", ShaderFlags.EnableStrictness));

    private void InitBlendState()
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
      this._blendState = new BlendState(this.Device, description);
    }

    private void InitVertexShader()
    {
      CompilationResult shaderBytecode = ShaderBytecode.CompileFromFile(AppDomain.CurrentDomain.BaseDirectory + "shaders/transparencydraw.hlsl", "VS_PASSTHROUGH", "vs_5_0", ShaderFlags.EnableStrictness);
      this._vertexShader = new VertexShader(this.Device, (byte[]) shaderBytecode);
      this._vertexInputLayout = new InputLayout(this.Device, shaderBytecode.Bytecode.Data, new InputElement[3]
      {
        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
      });
    }

    protected virtual void InitFrameConstantBuffer() => this._constantBufferFrame = new SharpDX.Direct3D11.Buffer(this.Device, new BufferDescription()
    {
      Usage = ResourceUsage.Dynamic,
      BindFlags = BindFlags.ConstantBuffer,
      SizeInBytes = Utilities.SizeOf<PhongCameraConstBufferData>(),
      CpuAccessFlags = CpuAccessFlags.Write,
      StructureByteStride = 0,
      OptionFlags = ResourceOptionFlags.None
    });

    protected virtual void InitIndexBuffer()
    {
      uint[] data = new uint[4]{ 1U, 2U, 0U, 0U };
      BufferDescription bufferDescription = new BufferDescription()
      {
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.IndexBuffer,
        SizeInBytes = Marshal.SizeOf<uint>() * 4,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.None
      };
      this._indexBuffer = SharpDX.Direct3D11.Buffer.Create<uint>(this.Device, BindFlags.IndexBuffer, data);
    }

    protected virtual void InitVertexBuffer(float start, float end)
    {
      VertexPosition[] data = new VertexPosition[3]
      {
        new VertexPosition(3f, end, 0.0f),
        new VertexPosition(-1f, end, 0.0f),
        new VertexPosition(-1f, 2f * start - end, 0.0f)
      };
      BufferDescription description = new BufferDescription()
      {
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.VertexBuffer,
        SizeInBytes = Marshal.SizeOf<VertexPosition>() * 4,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.None
      };
      this._vertexBuffer?.Dispose();
      this._vertexBuffer = SharpDX.Direct3D11.Buffer.Create<VertexPosition>(this.Device, data, description);
      this._vertexBufferBinding = new VertexBufferBinding(this._vertexBuffer, Marshal.SizeOf<VertexPosition>(), 0);
    }

    private void SetActive(
      int width,
      int height,
      WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer,
      int minHeight,
      int maxHeight)
    {
      this.Device.ImmediateContext.OutputMerger.SetBlendState(this._blendState);
      this.Device.ImmediateContext.PixelShader.Set(this._pixelShaderB);
      this.Device.ImmediateContext.VertexShader.Set(this._vertexShader);
      this.Device.ImmediateContext.GeometryShader.Set((GeometryShader) null);
      this.Device.ImmediateContext.PixelShader.SetConstantBuffer(0, this._constantBufferFrame);
      this.Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
      this.Device.ImmediateContext.InputAssembler.InputLayout = this._vertexInputLayout;
      float end = (float) (-2.0 * (double) (minHeight - 1) / (double) height + 1.0);
      this.InitVertexBuffer((float) (-2.0 * (double) (maxHeight + 1) / (double) height + 1.0), end);
      this.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, this._vertexBufferBinding);
      this.Device.ImmediateContext.InputAssembler.SetIndexBuffer(this._indexBuffer, Format.R32_UInt, 0);
      this.Device.ImmediateContext.OutputMerger.SetDepthStencilState(this._depthStencilState);
      PhongCameraConstBufferData camData = new PhongCameraConstBufferData()
      {
        viewportWidth = width,
        viewportHeight = height,
        minHeight = minHeight,
        maxHeight = maxHeight
      };
      this.UpdateFrameConstantBuffer(ref camData);
    }

    public void Render(
      WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer,
      Matrix transform,
      int width,
      int height,
      int minHeight,
      int maxHeight)
    {
      this.SetActive(width, height, renderer, minHeight, maxHeight);
      this.Device.ImmediateContext.DrawIndexed(3, 0, 0);
    }

    public void UpdateFrameConstantBuffer(ref PhongCameraConstBufferData camData)
    {
      DataStream stream;
      this.Device.ImmediateContext.MapSubresource(this._constantBufferFrame, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
      stream.Write<PhongCameraConstBufferData>(camData);
      stream.Dispose();
      this.Device.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) this._constantBufferFrame, 0);
    }

    public void Dispose()
    {
      this._pixelShaderB?.Dispose();
      this._vertexShader?.Dispose();
      this._vertexBuffer?.Dispose();
      this._vertexInputLayout?.Dispose();
      this._indexBuffer?.Dispose();
      this._constantBufferFrame?.Dispose();
      this._depthStencilState?.Dispose();
      this._blendState?.Dispose();
    }
  }
}
