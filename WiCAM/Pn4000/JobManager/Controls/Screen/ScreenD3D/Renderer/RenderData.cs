// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderData
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using WiCAM.Pn4000.ScreenD3D.Renderer.Effects;

namespace WiCAM.Pn4000.ScreenD3D.Renderer
{
  public class RenderData : IDisposable
  {
    public const int MBForTransparency = 256;
    public const int shadowMapResolution = 4096;
    public const int nodeDataBytes = 16;
    public SharpDX.Direct3D11.Buffer _headPointerUAVBuffer;
    public UnorderedAccessView _headPointerUAV;
    public SharpDX.Direct3D11.Buffer _pixelCountBuffer;
    public UnorderedAccessView _pixelCountUAV;
    public SharpDX.Direct3D11.Buffer _stagingPixelCountBuffer;
    public SharpDX.Direct3D11.Buffer _nodesDataBuffer;
    public UnorderedAccessView _nodesDataUAV;
    public ShaderResourceView _nodesDataSRV;
    public ShaderResourceView _imageLightSRV;
    public DepthStencilView _shadowMapDSV;
    public ShaderResourceView _shadowMapSRV;
    public Texture2D _shadowMapTexture;
    public Texture2D _imageLightTexture;
    internal IntPtr Hwnd;
    internal int Width;
    internal int Height;
    internal bool OffscreenTargetIsMultisampled = true;
    internal int WidthOS;
    internal int HeightOS;
    internal int WidthOSTripod;
    internal int HeightOSTripod;
    internal bool IsFirstFrame = true;
    internal DeviceEx Device9Ex;
    internal SharpDX.Direct3D9.Device Device9;
    internal SharpDX.Direct3D11.Device Device11;
    internal Texture SharedBackBufferAsD3D9;
    internal Texture2D TextureBackBuffer;
    internal Texture2D RenderTexture;
    internal Texture2D ZBufferTexture;
    internal Texture2D StagingTexture;
    internal RenderTargetView RenderTargetView;
    internal DepthStencilView DepthStencilView;
    internal Texture2D RenderTextureOS;
    internal Texture2D TextureBackBufferOS;
    internal Texture2D ZBufferTextureOS;
    internal RenderTargetView RenderTargetViewOS;
    internal DepthStencilView DepthStencilViewOS;
    internal Texture2D StagingTextureOS;
    internal Texture2D RenderTextureOSTripod;
    internal ShaderResourceView RenderTextureViewOSTripod;
    internal Texture2D ZBufferTextureOSTripod;
    internal DepthStencilView DepthStencilViewOSTripod;
    internal RenderTargetView RenderTargetViewOSTripod;
    internal SharpDX.Direct3D11.Buffer _constantBufferSettings;
    private readonly WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer;
    private RenderData.Shadows _shadowMode = RenderData.Shadows.Soft;
    public double OverallOpacity = 1.0;

    public FullscreenTriangleRenderer FullscreenTriangleRenderer { get; private set; } = new FullscreenTriangleRenderer();

    public bool TransparencyInitialized { get; set; }

    public bool ShadowmapUpToDate { get; set; }

    public bool ShowEdges { get; set; } = true;

    public bool ShowFaces { get; set; } = true;

    public bool ShowWireframe { get; set; }

    public bool UseOriginaColors { get; set; }

    public bool IsDisposed { get; private set; }

    public float NearClip { get; set; } = 1f;

    public float FarClip { get; set; } = 10000f;

    public RenderData.RndMode RenderMode { get; set; }

    public RenderData.LightMode LightingMode { get; set; } = RenderData.LightMode.Full;

    public RenderData.Shadows ShadowMode
    {
      get => this._shadowMode;
      set
      {
        this._shadowMode = value;
        this.renderer.ResetFloor();
      }
    }

    public bool AutoAdjustFloorToScreenRotation { get; set; } = true;

    public RenderData(IntPtr hwnd, int width, int height, WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      this.Hwnd = hwnd;
      this.Width = width;
      this.Height = height;
      this.renderer = renderer;
    }

    public void Dispose()
    {
      this.IsDisposed = true;
      this.Device11.ImmediateContext.ClearState();
      this.SharedBackBufferAsD3D9.Dispose();
      this.TextureBackBuffer.Dispose();
      this.RenderTargetView.Dispose();
      this.DepthStencilView.Dispose();
      this.RenderTexture.Dispose();
      this.ZBufferTexture.Dispose();
      this.StagingTexture.Dispose();
      this.RenderTargetViewOS?.Dispose();
      this.DepthStencilViewOS?.Dispose();
      this.StagingTextureOS?.Dispose();
      this.RenderTextureOS?.Dispose();
      this.TextureBackBufferOS?.Dispose();
      this.ZBufferTextureOS?.Dispose();
      this.DepthStencilViewOSTripod?.Dispose();
      this.RenderTargetViewOSTripod?.Dispose();
      this.RenderTextureOSTripod?.Dispose();
      this.RenderTextureViewOSTripod?.Dispose();
      this.ZBufferTextureOSTripod?.Dispose();
      this._headPointerUAV?.Dispose();
      this._headPointerUAVBuffer?.Dispose();
      this._nodesDataUAV?.Dispose();
      this._nodesDataBuffer?.Dispose();
      this._nodesDataSRV?.Dispose();
      this._pixelCountBuffer?.Dispose();
      this._pixelCountUAV?.Dispose();
      this._stagingPixelCountBuffer?.Dispose();
      this._shadowMapDSV?.Dispose();
      this._shadowMapTexture?.Dispose();
      this._shadowMapSRV?.Dispose();
      this._imageLightSRV?.Dispose();
      this._imageLightTexture?.Dispose();
      this._constantBufferSettings?.Dispose();
      this.FullscreenTriangleRenderer?.Dispose();
      this.Device11.ImmediateContext.Flush();
      this.Device9Ex.Dispose();
      this.Device9.Dispose();
      this.Device11.Dispose();
      this.SharedBackBufferAsD3D9 = (Texture) null;
      this.TextureBackBuffer = (Texture2D) null;
      this.RenderTargetView = (RenderTargetView) null;
      this.DepthStencilView = (DepthStencilView) null;
      this.RenderTexture = (Texture2D) null;
      this.ZBufferTexture = (Texture2D) null;
      this.StagingTexture = (Texture2D) null;
      this.RenderTargetViewOS = (RenderTargetView) null;
      this.DepthStencilViewOS = (DepthStencilView) null;
      this.StagingTextureOS = (Texture2D) null;
      this.RenderTextureOS = (Texture2D) null;
      this.TextureBackBufferOS = (Texture2D) null;
      this.ZBufferTextureOS = (Texture2D) null;
      this.DepthStencilViewOSTripod = (DepthStencilView) null;
      this.RenderTargetViewOSTripod = (RenderTargetView) null;
      this.RenderTextureOSTripod = (Texture2D) null;
      this.RenderTextureViewOSTripod = (ShaderResourceView) null;
      this.ZBufferTextureOSTripod = (Texture2D) null;
      this._headPointerUAV = (UnorderedAccessView) null;
      this._headPointerUAVBuffer = (SharpDX.Direct3D11.Buffer) null;
      this._nodesDataUAV = (UnorderedAccessView) null;
      this._nodesDataBuffer = (SharpDX.Direct3D11.Buffer) null;
      this._nodesDataSRV = (ShaderResourceView) null;
      this._pixelCountBuffer = (SharpDX.Direct3D11.Buffer) null;
      this._pixelCountUAV = (UnorderedAccessView) null;
      this._stagingPixelCountBuffer = (SharpDX.Direct3D11.Buffer) null;
      this._shadowMapDSV = (DepthStencilView) null;
      this._shadowMapTexture = (Texture2D) null;
      this._shadowMapSRV = (ShaderResourceView) null;
      this._imageLightSRV = (ShaderResourceView) null;
      this._imageLightTexture = (Texture2D) null;
      this._constantBufferSettings = (SharpDX.Direct3D11.Buffer) null;
      this.FullscreenTriangleRenderer = (FullscreenTriangleRenderer) null;
      this.Device9Ex = (DeviceEx) null;
      this.Device9 = (SharpDX.Direct3D9.Device) null;
      this.Device11 = (SharpDX.Direct3D11.Device) null;
    }

    public void Initialize()
    {
      this.InitD3D9Device();
      this.InitD3D11Device();
      this.Resize(this.Width, this.Height);
      this.InitShadowDepthStencil(4096);
      this.InitimageLightSRV();
      this.InitShaderSettingsBuffer();
      ShaderSettingsConstBufferData settings = new ShaderSettingsConstBufferData()
      {
        shadowOffset = 0.0002f,
        maxShade = 0.66f,
        penumbraBaseSize = 0.00268f,
        penumbraScalingSize = 0.0121f,
        noiseRes = 1024,
        transparencyNearClip = 0.0001f,
        onObjectShadows = 2
      };
      this.UpdateShaderSettings(ref settings);
      this.InitTransparency();
    }

    protected virtual void InitShaderSettingsBuffer() => this._constantBufferSettings = new SharpDX.Direct3D11.Buffer(this.Device11, new BufferDescription()
    {
      Usage = ResourceUsage.Dynamic,
      BindFlags = BindFlags.ConstantBuffer,
      SizeInBytes = Utilities.SizeOf<ShaderSettingsConstBufferData>(),
      CpuAccessFlags = CpuAccessFlags.Write,
      StructureByteStride = 0,
      OptionFlags = ResourceOptionFlags.None
    });

    protected virtual void InitimageLightSRV()
    {
      Bitmap bitmap = new Bitmap(Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "ShaderData/map01_256x.png"));
      BitmapData bitmapdata = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      this._imageLightTexture = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Width = bitmap.Width,
        Height = bitmap.Height,
        ArraySize = 1,
        MipLevels = 1,
        BindFlags = BindFlags.ShaderResource,
        Usage = ResourceUsage.Immutable,
        CpuAccessFlags = CpuAccessFlags.None,
        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        OptionFlags = ResourceOptionFlags.None,
        SampleDescription = new SampleDescription(1, 0)
      }, new DataRectangle[1]
      {
        new DataRectangle(bitmapdata.Scan0, bitmapdata.Stride)
      });
      bitmap.UnlockBits(bitmapdata);
      ShaderResourceViewDescription.Texture2DResource texture2Dresource = new ShaderResourceViewDescription.Texture2DResource()
      {
        MostDetailedMip = 0,
        MipLevels = -1
      };
      this._imageLightSRV = new ShaderResourceView(this.Device11, (SharpDX.Direct3D11.Resource) this._imageLightTexture, new ShaderResourceViewDescription()
      {
        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        Dimension = ShaderResourceViewDimension.Texture2D,
        Texture2D = texture2Dresource
      });
    }

    public void InitTransparency()
    {
      this.CreateNodesUAVBuffer(256);
      this.CreateNodesDataSRV(256);
      this.FullscreenTriangleRenderer.Init(this.Device11);
      this.CreateHeadPointerUAVBuffer();
      this.CreatePixelCountBuffer();
      this.TransparencyInitialized = true;
    }

    public void Resize(int width, int height)
    {
      lock (this)
      {
        this.Width = width;
        this.Height = height;
        this.IsFirstFrame = true;
        this.CreateHeadPointerUAVBuffer();
        this.CreatePixelCountBuffer();
        this.InitBackBufferTexture();
        this.InitRenderTexture();
        this.InitTextureStaging();
        this.GetSharedD3D11TextureAsD3D9Texture();
        this.InitDepthStencil();
        this.RenderTargetView?.Dispose();
        this.RenderTargetView = new RenderTargetView(this.Device11, (SharpDX.Direct3D11.Resource) this.RenderTexture);
      }
      this.ShadowmapUpToDate = false;
    }

    public void SetOSTargetViewSize(int width, int height, bool multisampling)
    {
      lock (this)
      {
        if (this.WidthOS == width && this.HeightOS == height && this.OffscreenTargetIsMultisampled == multisampling)
          return;
        this.WidthOS = width;
        this.HeightOS = height;
        this.OffscreenTargetIsMultisampled = multisampling;
        this.IsFirstFrame = true;
        this.InitRenderTextureOS();
        this.InitBackBufferTextureOS();
        this.InitTextureStagingOS();
        this.InitDepthStencilOS();
        this.RenderTargetViewOS?.Dispose();
        this.RenderTargetViewOS = new RenderTargetView(this.Device11, (SharpDX.Direct3D11.Resource) this.RenderTextureOS);
      }
    }

    public void CreateNodesDataSRV(int sizeInMB)
    {
      int num1 = 16;
      int num2 = sizeInMB * 1024 * 1024 / num1;
      ShaderResourceViewDescription.BufferResource bufferResource = new ShaderResourceViewDescription.BufferResource()
      {
        FirstElement = 0,
        ElementCount = num2
      };
      this._nodesDataSRV = new ShaderResourceView(this.Device11, (SharpDX.Direct3D11.Resource) this._nodesDataBuffer, new ShaderResourceViewDescription()
      {
        Format = SharpDX.DXGI.Format.Unknown,
        Dimension = ShaderResourceViewDimension.Buffer,
        Buffer = bufferResource
      });
    }

    public virtual void CreateNodesUAVBuffer(int sizeInMB)
    {
      int num1 = 16;
      int num2 = sizeInMB * 1024 * 1024 / num1;
      this._nodesDataBuffer = new SharpDX.Direct3D11.Buffer(this.Device11, new BufferDescription()
      {
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
        SizeInBytes = num2 * num1,
        StructureByteStride = num1,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.BufferStructured
      });
      UnorderedAccessViewDescription.BufferResource bufferResource = new UnorderedAccessViewDescription.BufferResource()
      {
        FirstElement = 0,
        ElementCount = num2,
        Flags = UnorderedAccessViewBufferFlags.Counter
      };
      this._nodesDataUAV = new UnorderedAccessView(this.Device11, (SharpDX.Direct3D11.Resource) this._nodesDataBuffer, new UnorderedAccessViewDescription()
      {
        Format = SharpDX.DXGI.Format.Unknown,
        Dimension = UnorderedAccessViewDimension.Buffer,
        Buffer = bufferResource
      });
    }

    public virtual void CreateHeadPointerUAVBuffer()
    {
      int num1 = this.Width * this.Height;
      this._headPointerUAV?.Dispose();
      this._headPointerUAVBuffer?.Dispose();
      int num2 = 4;
      this._headPointerUAVBuffer = new SharpDX.Direct3D11.Buffer(this.Device11, new BufferDescription()
      {
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.UnorderedAccess,
        SizeInBytes = num1 * num2,
        StructureByteStride = num2,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.BufferAllowRawViews
      });
      UnorderedAccessViewDescription.BufferResource bufferResource = new UnorderedAccessViewDescription.BufferResource()
      {
        FirstElement = 0,
        ElementCount = num1,
        Flags = UnorderedAccessViewBufferFlags.Raw
      };
      this._headPointerUAV = new UnorderedAccessView(this.Device11, (SharpDX.Direct3D11.Resource) this._headPointerUAVBuffer, new UnorderedAccessViewDescription()
      {
        Format = SharpDX.DXGI.Format.R32_Typeless,
        Dimension = UnorderedAccessViewDimension.Buffer,
        Buffer = bufferResource
      });
    }

    public virtual void CreatePixelCountBuffer()
    {
      this._stagingPixelCountBuffer?.Dispose();
      this._pixelCountBuffer?.Dispose();
      this._pixelCountUAV?.Dispose();
      BufferDescription description = new BufferDescription();
      description.Usage = ResourceUsage.Default;
      description.BindFlags = BindFlags.UnorderedAccess;
      description.SizeInBytes = (this.Height + 1) * 4;
      description.StructureByteStride = 4;
      description.CpuAccessFlags = CpuAccessFlags.None;
      description.OptionFlags = ResourceOptionFlags.BufferAllowRawViews;
      this._pixelCountBuffer = new SharpDX.Direct3D11.Buffer(this.Device11, description);
      description = new BufferDescription();
      description.Usage = ResourceUsage.Staging;
      description.BindFlags = BindFlags.None;
      description.SizeInBytes = (this.Height + 1) * 4;
      description.StructureByteStride = 4;
      description.CpuAccessFlags = CpuAccessFlags.Read;
      description.OptionFlags = ResourceOptionFlags.None;
      this._stagingPixelCountBuffer = new SharpDX.Direct3D11.Buffer(this.Device11, description);
      UnorderedAccessViewDescription.BufferResource bufferResource = new UnorderedAccessViewDescription.BufferResource()
      {
        FirstElement = 0,
        ElementCount = this.Height + 1,
        Flags = UnorderedAccessViewBufferFlags.Raw
      };
      this._pixelCountUAV = new UnorderedAccessView(this.Device11, (SharpDX.Direct3D11.Resource) this._pixelCountBuffer, new UnorderedAccessViewDescription()
      {
        Format = SharpDX.DXGI.Format.R32_Typeless,
        Dimension = UnorderedAccessViewDimension.Buffer,
        Buffer = bufferResource
      });
    }

    public unsafe void InitShadowDepthStencil(int res)
    {
      this._shadowMapDSV?.Dispose();
      this._shadowMapTexture?.Dispose();
      this._shadowMapSRV?.Dispose();
      this.ShadowmapUpToDate = false;
      Texture2DDescription description = new Texture2DDescription()
      {
        Width = res,
        Height = res,
        ArraySize = 1,
        MipLevels = 1,
        BindFlags = BindFlags.ShaderResource | BindFlags.DepthStencil,
        Usage = ResourceUsage.Default,
        CpuAccessFlags = CpuAccessFlags.None,
        Format = SharpDX.DXGI.Format.R32_Typeless,
        OptionFlags = ResourceOptionFlags.None,
        SampleDescription = new SampleDescription(1, 0)
      };
      ShaderResourceViewDescription.Texture2DResource texture2Dresource1 = new ShaderResourceViewDescription.Texture2DResource()
      {
        MostDetailedMip = 0,
        MipLevels = 1
      };
      DepthStencilViewDescription.Texture2DResource texture2Dresource2 = new DepthStencilViewDescription.Texture2DResource()
      {
        MipSlice = 0
      };
      byte[] numArray = new byte[res * res * 4];
      for (int index = 0; index < res * res * 4; ++index)
        numArray[index] = (byte) 0;
      fixed (byte* numPtr = &numArray[0])
      {
        DataRectangle dataRectangle = new DataRectangle(new IntPtr((void*) numPtr), res * 4);
        this._shadowMapTexture = new Texture2D(this.Device11, description, new DataRectangle[1]
        {
          dataRectangle
        });
      }
      this._shadowMapDSV = new DepthStencilView(this.Device11, (SharpDX.Direct3D11.Resource) this._shadowMapTexture, new DepthStencilViewDescription()
      {
        Format = SharpDX.DXGI.Format.D32_Float,
        Dimension = DepthStencilViewDimension.Texture2D,
        Flags = DepthStencilViewFlags.None,
        Texture2D = texture2Dresource2
      });
      this._shadowMapSRV = new ShaderResourceView(this.Device11, (SharpDX.Direct3D11.Resource) this._shadowMapTexture, new ShaderResourceViewDescription()
      {
        Format = SharpDX.DXGI.Format.R32_Float,
        Dimension = ShaderResourceViewDimension.Texture2D,
        Texture2D = texture2Dresource1
      });
    }

    public void SetOSTargetViewSizeTripod(int width, int height)
    {
      lock (this)
      {
        if (this.WidthOSTripod == width && this.HeightOSTripod == height)
          return;
        this.WidthOSTripod = width;
        this.HeightOSTripod = height;
        this.IsFirstFrame = true;
        this.InitRenderTextureOSTripod();
        this.InitDepthStencilOSTripod();
        this.RenderTargetViewOSTripod?.Dispose();
        this.RenderTargetViewOSTripod = new RenderTargetView(this.Device11, (SharpDX.Direct3D11.Resource) this.RenderTextureOSTripod);
        this.RenderTargetViewOSTripod.DebugName = "RenderTargetViewOSTripod";
      }
    }

    private void GetSharedD3D11TextureAsD3D9Texture()
    {
      this.SharedBackBufferAsD3D9?.Dispose();
      using (SharpDX.DXGI.Resource resource = this.TextureBackBuffer.QueryInterface<SharpDX.DXGI.Resource>())
      {
        IntPtr sharedHandle = resource.SharedHandle;
        this.SharedBackBufferAsD3D9 = new Texture(this.Device9, this.Width, this.Height, 1, SharpDX.Direct3D9.Usage.RenderTarget, SharpDX.Direct3D9.Format.A8R8G8B8, Pool.Default, ref sharedHandle);
      }
    }

    private void InitD3D9Device()
    {
      Direct3DEx direct3D = new Direct3DEx();
      SharpDX.Direct3D9.PresentParameters presentParameters = new SharpDX.Direct3D9.PresentParameters()
      {
        Windowed = (RawBool) true,
        SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
        PresentationInterval = PresentInterval.Immediate,
        DeviceWindowHandle = IntPtr.Zero
      };
      DisplayModeEx displayModeEx = new DisplayModeEx();
      this.Device9Ex = new DeviceEx(direct3D, 0, DeviceType.Hardware, this.Hwnd, CreateFlags.FpuPreserve | CreateFlags.Multithreaded | CreateFlags.HardwareVertexProcessing, presentParameters);
      this.Device9 = this.Device9Ex.QueryInterface<SharpDX.Direct3D9.Device>();
    }

    private void InitD3D11Device() => this.Device11 = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);

    private void InitBackBufferTexture()
    {
      this.TextureBackBuffer?.Dispose();
      this.TextureBackBuffer = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Width = this.Width,
        Height = this.Height,
        MipLevels = 1,
        ArraySize = 1,
        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        SampleDescription = new SampleDescription(1, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.Shared
      });
    }

    private void InitBackBufferTextureOS()
    {
      this.TextureBackBufferOS?.Dispose();
      this.TextureBackBufferOS = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Width = this.WidthOS,
        Height = this.HeightOS,
        MipLevels = 1,
        ArraySize = 1,
        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        SampleDescription = new SampleDescription(1, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.Shared
      });
    }

    private void InitRenderTexture()
    {
      this.RenderTexture?.Dispose();
      this.RenderTexture = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Width = this.Width,
        Height = this.Height,
        MipLevels = 1,
        ArraySize = 1,
        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        SampleDescription = new SampleDescription(4, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.Shared
      });
    }

    private void InitRenderTextureOS()
    {
      this.RenderTextureOS?.Dispose();
      this.RenderTextureOS = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Width = this.WidthOS,
        Height = this.HeightOS,
        MipLevels = 1,
        ArraySize = 1,
        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        SampleDescription = new SampleDescription(this.OffscreenTargetIsMultisampled ? 4 : 1, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.Shared
      });
    }

    private void InitRenderTextureOSTripod()
    {
      this.RenderTextureOSTripod?.Dispose();
      this.RenderTextureViewOSTripod?.Dispose();
      this.RenderTextureOSTripod = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Width = this.WidthOSTripod,
        Height = this.HeightOSTripod,
        MipLevels = 1,
        ArraySize = 1,
        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        SampleDescription = new SampleDescription(1, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.Shared
      });
      this.RenderTextureOSTripod.DebugName = "RenderTextureOSTripod";
      this.RenderTextureViewOSTripod = new ShaderResourceView(this.Device11, (SharpDX.Direct3D11.Resource) this.RenderTextureOSTripod);
      this.RenderTextureViewOSTripod.DebugName = "RenderTextureViewOSTripod";
    }

    private void InitTextureStaging()
    {
      this.StagingTexture?.Dispose();
      this.StagingTexture = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Width = this.Width,
        Height = this.Height,
        MipLevels = 1,
        ArraySize = 1,
        Format = SharpDX.DXGI.Format.B8G8R8A8_Typeless,
        SampleDescription = new SampleDescription(1, 0),
        Usage = ResourceUsage.Staging,
        BindFlags = BindFlags.None,
        CpuAccessFlags = CpuAccessFlags.Read,
        OptionFlags = ResourceOptionFlags.None
      });
    }

    private void InitTextureStagingOS()
    {
      this.StagingTextureOS?.Dispose();
      this.StagingTextureOS = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Width = this.WidthOS,
        Height = this.HeightOS,
        MipLevels = 1,
        ArraySize = 1,
        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        SampleDescription = new SampleDescription(1, 0),
        Usage = ResourceUsage.Staging,
        BindFlags = BindFlags.None,
        CpuAccessFlags = CpuAccessFlags.Read,
        OptionFlags = ResourceOptionFlags.None
      });
    }

    private void InitDepthStencil()
    {
      if (this.Height <= 0 || this.Width <= 0)
        return;
      this.DepthStencilView?.Dispose();
      this.ZBufferTexture?.Dispose();
      this.ZBufferTexture = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Format = SharpDX.DXGI.Format.D32_Float,
        ArraySize = 1,
        MipLevels = 1,
        Width = this.Width,
        Height = this.Height,
        SampleDescription = new SampleDescription(4, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.DepthStencil,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.None
      });
      this.DepthStencilView = new DepthStencilView(this.Device11, (SharpDX.Direct3D11.Resource) this.ZBufferTexture);
    }

    private void InitDepthStencilOS()
    {
      if (this.HeightOS <= 0 || this.WidthOS <= 0)
        return;
      this.DepthStencilViewOS?.Dispose();
      this.ZBufferTextureOS?.Dispose();
      this.ZBufferTextureOS = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Format = SharpDX.DXGI.Format.D32_Float,
        ArraySize = 1,
        MipLevels = 1,
        Width = this.WidthOS,
        Height = this.HeightOS,
        SampleDescription = new SampleDescription(this.OffscreenTargetIsMultisampled ? 4 : 1, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.DepthStencil,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.None
      });
      this.DepthStencilViewOS = new DepthStencilView(this.Device11, (SharpDX.Direct3D11.Resource) this.ZBufferTextureOS);
    }

    private void InitDepthStencilOSTripod()
    {
      if (this.HeightOSTripod <= 0 || this.WidthOSTripod <= 0)
        return;
      this.DepthStencilViewOSTripod?.Dispose();
      this.ZBufferTextureOSTripod?.Dispose();
      this.ZBufferTextureOSTripod = new Texture2D(this.Device11, new Texture2DDescription()
      {
        Format = SharpDX.DXGI.Format.D32_Float,
        ArraySize = 1,
        MipLevels = 1,
        Width = this.WidthOSTripod,
        Height = this.HeightOSTripod,
        SampleDescription = new SampleDescription(1, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.DepthStencil,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.None
      });
      this.ZBufferTextureOSTripod.DebugName = "ZBufferTextureOSTripod";
      this.DepthStencilViewOSTripod = new DepthStencilView(this.Device11, (SharpDX.Direct3D11.Resource) this.ZBufferTextureOSTripod);
      this.DepthStencilViewOSTripod.DebugName = "DepthStencilViewOSTripod";
    }

    public void UpdateShaderSettings(ref ShaderSettingsConstBufferData settings)
    {
      DataStream stream;
      this.Device11.ImmediateContext.MapSubresource(this._constantBufferSettings, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
      stream.Write<ShaderSettingsConstBufferData>(settings);
      stream.Dispose();
      this.Device11.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) this._constantBufferSettings, 0);
    }

    public enum RndMode
    {
      Standard,
      Depth,
      Tripod,
      ShadowMap,
    }

    public enum LightMode
    {
      Ambient,
      Simplified,
      Full,
    }

    public enum Shadows
    {
      None,
      Hard,
      Soft,
    }
  }
}
