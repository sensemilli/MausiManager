// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Renderer
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryGenerators;
using WiCAM.Pn4000.PN3D.Const;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Renderer.Effects;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;
using WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks;

namespace WiCAM.Pn4000.ScreenD3D.Renderer
{
  public class Renderer : IDisposable
  {
    private const float shadowNearClip = 1000f;
    private const float shadowFarClip = 40000f;
    private const float shadowCameraDistance = 20000f;
    private float range;
    private bool floorHere;
    private Matrix _view = Matrix.LookAtRH(new Vector3(0.0f, 1000f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1f));
    private Matrix _projection;
    private DirectionalLightBufferData _dirLight;
    private BillboardCameraConstBufferData _camDataBillboard;
    private PhongCameraConstBufferData _camDataBlinnPhong;
    private LineCameraConstBufferData _camDataLines;
    private ViewportBufferData _viewportData;
    private DateTime _lastMeasuredTime;
    private int largestOverflow;
    private Screen3D _screen;
    private Model _floor;
    private Vector3 sunDir;
    private Vector3d min;
    private Vector3d max;
    private Vector3d minMemory;
    private Vector3d maxMemory;
    internal float LastMouseX;
    internal float LastMouseY;
    public Texture2D _temporaryRenderTexture;
    public Dictionary<EffectType, BaseEffect> Effects = new Dictionary<EffectType, BaseEffect>();
    private Vector3d _minSceneBB;
    private Vector3d _maxSceneBB;

    public RenderData RenderData { get; private set; }

    public Device Device => this.RenderData.Device11;

    public WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node Root { get; private set; } = new WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node((WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) null);

    public int Width => this.RenderData.Width;

    public int Height => this.RenderData.Height;

    public Vector2 DistPerPix { get; set; } = new Vector2(1f, 1f);

    public float Zoom { get; set; } = 1000f;

    public float CamDistOrtho { get; set; } = 1000f;

    public ProjectionType ProjectionType { get; set; }

    public Matrix Projection
    {
      get
      {
        lock (this)
          return this._projection;
      }
    }

    public Matrix View
    {
      get
      {
        lock (this)
          return this._view;
      }
    }

    public ref DirectionalLightBufferData BlinnPhongDirectionalLight => ref this._dirLight;

    public ref PhongCameraConstBufferData BlinnPhongCamData => ref this._camDataBlinnPhong;

    public ref BillboardCameraConstBufferData BillboardCamData => ref this._camDataBillboard;

    public ref ViewportBufferData ViewportData => ref this._viewportData;

    public ref LineCameraConstBufferData LinesCamData => ref this._camDataLines;

    public Dictionary<Model, ModelNode> ProtoModelMap { get; } = new Dictionary<Model, ModelNode>();

    public Renderer(IntPtr hwnd, int width, int height, Screen3D screen)
    {
      this.RenderData = new RenderData(hwnd, width, height, this);
      this._screen = screen;
    }

    public void Init()
    {
      this.Effects.Add(EffectType.None, (BaseEffect) null);
      this.Effects.Add(EffectType.BlinnPhong, new BaseEffect(this.Device, this.RenderData));
      this.Effects.Add(EffectType.Line, (BaseEffect) new LineEffect(this.Device, this.RenderData));
      this.Effects.Add(EffectType.SimpleDepth, (BaseEffect) new DepthEffect(this.Device, this.RenderData));
      this.Effects.Add(EffectType.Textured2d, (BaseEffect) new Textured2dEffect(this.Device, this.RenderData));
      this.Effects.Add(EffectType.Billboard, (BaseEffect) new BillboardEffect(this.Device, this.RenderData));
      this.Root.Transform = new Matrix?(Matrix.Identity);
      this.sunDir = new Vector3(0.2f, 0.2f, -1f);
      this.sunDir.Normalize();
      this._dirLight._color = new Vector3(1f, 1f, 1f);
      this._dirLight._intensity = 1f;
      this._dirLight._dir = this.sunDir;
      this._dirLight._ambientColor = new Vector3(0.95f, 0.95f, 0.95f);
      PlaneModel planeModel = new PlaneModel(new Vector3d(0.0, 0.0, 0.0), new Vector3d(1.0, 0.0, 0.0), new Vector3d(0.0, 0.0, 1.0), 10000.0, 1, new WiCAM.Pn4000.BendModel.Base.Color(0.5f, 0.5f, 0.5f, 1f));
      planeModel.Enabled = true;
      planeModel.ModelType = ModelType.Static;
      this._floor = (Model) planeModel;
    }
        public void OnLoaded()
    {
            TextBillboard.TextBillboardStyle tbs = new TextBillboard.TextBillboardStyle();

            tbs.textColor = new RawColor4(0.0f, 0.0f, 0.0f, 1f);
        tbs.bgColor = new RawColor4(1f, 0.5f, 1f, 1f);
        tbs.fontSize = 50;
            tbs.roundBorder = new RoundBorder(5, 5, new RawColor4(1f, 1f, 1f, 1f));
      tbs.padding = new TextBillboard.Padding()
      {
        top = 10,
        bottom = 10,
        left = 10,
        right = 10
      };
      this.ResetFloor();
    }

    public void ResetFloor()
    {
      if (this.RenderData.ShadowMode == RenderData.Shadows.None && this.floorHere)
      {
        this._screen?.ScreenD3D.RemoveModel(this._floor);
        this.floorHere = false;
      }
      else
      {
        if (this.RenderData.ShadowMode == RenderData.Shadows.None || this.floorHere)
          return;
        this._screen?.ScreenD3D.AddModel(this._floor);
        this.floorHere = true;
      }
    }

    private Matrix ShadowMapMatrix(bool geometryChange)
    {
      Vector3 vector3 = -this.sunDir;
      vector3.Normalize();
      Vector3 result1 = vector3 * 20000f;
      Vector3 result2 = new Vector3(result1.X, result1.Z, -result1.Y);
      Vector3 result3 = new Vector3(0.0f, 0.0f, 0.0f);
      result2.Normalize();
      if (geometryChange)
      {
        ZoomExtendTask.DetermineBoundingBox(this, out this.min, out this.max, false);
        this._minSceneBB = this.min;
        this._maxSceneBB = this.max;
        if (this.min != this.minMemory || this.max != this.maxMemory)
        {
          this._view = Matrix.LookAtRH(result1, result3, result2);
          Vector3[] vector3Array = new Vector3[2]
          {
            new Vector3((float) this.min.X, (float) this.min.Y, (float) this.min.Z),
            new Vector3((float) this.max.X, (float) this.max.Y, (float) this.max.Z)
          };
          this.range = 0.0f;
          for (int index1 = 0; index1 < 2; ++index1)
          {
            for (int index2 = 0; index2 < 2; ++index2)
            {
              for (int index3 = 0; index3 < 2; ++index3)
              {
                Vector3 result4 = new Vector3(vector3Array[index1].X, vector3Array[index2].Y, vector3Array[index3].Z);
                Vector3.Transform(ref result4, ref this._view, out result4);
                if ((double) Math.Abs(result4.X) > (double) this.range)
                  this.range = Math.Abs(result4.X);
                if ((double) Math.Abs(result4.Y) > (double) this.range)
                  this.range = Math.Abs(result4.Y);
              }
            }
          }
          this.range *= 2f;
          double num = (double) this.range * 0.002;
          this._floor.Shells.First<Shell>().Transform = Matrix4d.Scale(num, num, num);
          this._floor.Transform = Matrix4d.Translation(new Vector3d(0.0, 0.0, this.min.Z - 1.0));
          this._screen?.ScreenD3D.UpdateModelAppearance(this._floor, false);
          this._screen?.ScreenD3D.UpdateModelTransform(this._floor, true);
          this.minMemory = this.min;
          this.maxMemory = this.max;
        }
      }
      Matrix transform = this.Root.Transform.Value;
      Vector3.Transform(ref result3, ref transform, out result3);
      Vector3.Transform(ref result1, ref transform, out result1);
      Vector3.Transform(ref result2, ref transform, out result2);
      Vector3 up = result2 - result3;
      this._camDataBlinnPhong._cameraPosition = result1;
      this._view = Matrix.LookAtRH(result1, result3, up);
      this._projection = Matrix.OrthoRH(this.range * 1.25f, this.range * 1.25f, 1000f, 40000f);
      this._dirLight._shadowmapScalar = this.range / 1000f;
      return this._view * this._projection;
    }

    private void GenerateShadowmap()
    {
      this.RenderData.ShadowmapUpToDate = true;
      RenderData.RndMode renderMode = this.RenderData.RenderMode;
      this.RenderData.RenderMode = RenderData.RndMode.ShadowMap;
      this.Render((RenderTargetView) null, this.RenderData._shadowMapDSV, 4096, 4096);
      this.RenderData.RenderMode = renderMode;
    }

    public void Render(
      RenderTargetView renderTargetView,
      DepthStencilView depthStencilView,
      int width,
      int height)
    {
      if (this.RenderData.AutoAdjustFloorToScreenRotation)
      {
        this.sunDir = new Vector3(0.05f, -0.5f, -1f);
        Matrix transform = this.Root.Transform.Value;
        transform.Invert();
        Vector3.TransformNormal(ref this.sunDir, ref transform, out this.sunDir);
      }
      else
        this.sunDir = new Vector3(0.2f, 0.2f, -1f);
      if (this.RenderData.ShadowMode != RenderData.Shadows.None && this.RenderData.RenderMode != RenderData.RndMode.ShadowMap)
      {
        if (this.RenderData.AutoAdjustFloorToScreenRotation || !this.RenderData.ShadowmapUpToDate)
        {
          this._dirLight._lightProjection = this.ShadowMapMatrix(true);
          this.GenerateShadowmap();
          this.ResetFloor();
        }
        else
          this._dirLight._lightProjection = this.ShadowMapMatrix(false);
      }
      if (this.RenderData.AutoAdjustFloorToScreenRotation)
      {
        Matrix matrix1 = this.Root.Transform.Value;
        Matrix matrix2 = matrix1;
        try
        {
          matrix2.Invert();
          Matrix matrix3 = Matrix.RotationX(-0.2f);
          Matrix matrix4 = Matrix.Translation(matrix1.M41, matrix1.M42, matrix1.M43 - (float) ((this._maxSceneBB - this._minSceneBB).Length * 0.5));
          ModelNode modelNode;
          if (this.ProtoModelMap.TryGetValue(this._floor, out modelNode))
            modelNode.Transform = new Matrix?(matrix4 * matrix3 * matrix2);
        }
        catch (Exception ex)
        {
          Console.WriteLine((object) ex);
        }
      }
      Matrix matrix;
      lock (this)
      {
        if (this.RenderData.RenderMode == RenderData.RndMode.ShadowMap)
          matrix = this._dirLight._lightProjection;
        else if (this.ProjectionType == ProjectionType.Perspective)
        {
          Vector3 eye = new Vector3(0.0f, this.Zoom, 0.0f);
          this._camDataBlinnPhong._cameraPosition = eye;
          this._view = Matrix.LookAtRH(eye, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1f));
          this._projection = Matrix.PerspectiveFovRH(0.7853982f, (float) width / (float) height, this.RenderData.NearClip, this.RenderData.FarClip);
          matrix = this._view * this._projection;
        }
        else
        {
          Vector3 eye = new Vector3(0.0f, this.CamDistOrtho, 0.0f);
          Vector3 target = new Vector3(0.0f, 0.0f, 0.0f);
          Vector3 up = new Vector3(0.0f, 0.0f, 1f);
          float farClip = this.RenderData.FarClip;
          float nearClip = this.RenderData.NearClip;
          this._camDataBlinnPhong._cameraPosition = new Vector3(0.0f, this.Zoom, 0.0f);
          this._view = Matrix.LookAtRH(eye, target, up);
          this._projection = Matrix.OrthoRH(this.Zoom, this.Zoom * ((float) height / (float) width), nearClip, farClip);
          this.DistPerPix = new Vector2(this.Zoom / (float) width, this.Zoom / (float) width);
          matrix = this._view * this._projection;
        }
      }
      this._camDataBlinnPhong._viewProjection = matrix;
      this._camDataBlinnPhong.viewportWidth = width;
      this._camDataBlinnPhong.viewportHeight = height;
      this._camDataBlinnPhong.minHeight = 0;
      this._camDataBlinnPhong.maxHeight = height;
      Matrix? transform1 = this.Root.Transform;
      (transform1.Value with
      {
        M41 = 0.0f,
        M42 = 0.0f,
        M43 = 0.0f
      }).Invert();
      transform1 = this.Root.Transform;
      Matrix transform2 = transform1.Value with
      {
        M41 = 0.0f,
        M42 = 0.0f,
        M43 = 0.0f
      };
      Vector3 result;
      Vector3.Transform(ref this.sunDir, ref transform2, out result);
      this._dirLight._dir = result;
      transform2.Invert();
      this._camDataBlinnPhong._worldRotationInverse = transform2;
      this._camDataBillboard._cameraPosition = this._camDataBlinnPhong._cameraPosition;
      this._camDataBillboard._viewProjection = matrix;
      this._camDataBillboard._renderTargetSize = new Vector2((float) width, (float) height);
      this._camDataBillboard._cameraMode = (int) this.ProjectionType;
      this._camDataBillboard._farClip = this.RenderData.FarClip;
      this._viewportData.w = width;
      this._viewportData.h = height;
      this._camDataLines._viewProjection = matrix;
      this._camDataLines._renderTargetSize.X = (float) width;
      this._camDataLines._renderTargetSize.Y = (float) height;
      this._camDataLines.minHeight = 0;
      this._camDataLines.maxHeight = height;
      this.Device.ImmediateContext.Rasterizer.SetViewport(0.0f, 0.0f, (float) width, (float) height);
      this.Device.ImmediateContext.OutputMerger.ResetTargets();
      if (this.RenderData.RenderMode != RenderData.RndMode.ShadowMap)
        this.Device.ImmediateContext.ClearRenderTargetView(renderTargetView, (RawColor4) new Color4(0.0f, 0.0f, 0.0f, 0.0f));
      this.Device.ImmediateContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1f, (byte) 0);
      this.Device.ImmediateContext.OutputMerger.SetTargets(depthStencilView, renderTargetView);
      UnorderedAccessView[] unorderedAccessViews = new UnorderedAccessView[3]
      {
        this.RenderData._headPointerUAV,
        this.RenderData._nodesDataUAV,
        this.RenderData._pixelCountUAV
      };
      if (this.RenderData.RenderMode != RenderData.RndMode.ShadowMap)
      {
        this.Device.ImmediateContext.PixelShader.SetShaderResource(1, this.RenderData._shadowMapSRV);
        this.Device.ImmediateContext.PixelShader.SetShaderResource(4, this.RenderData._imageLightSRV);
        this.Device.ImmediateContext.PixelShader.SetConstantBuffer(3, this.RenderData._constantBufferSettings);
        this.Device.ImmediateContext.VertexShader.SetConstantBuffer(3, this.RenderData._constantBufferSettings);
        this.Device.ImmediateContext.ClearUnorderedAccessView(this.RenderData._pixelCountUAV, new RawInt4(0, 0, 0, 0));
        this.Device.ImmediateContext.ClearUnorderedAccessView(this.RenderData._headPointerUAV, new RawInt4((int) ushort.MaxValue, (int) ushort.MaxValue, (int) ushort.MaxValue, (int) ushort.MaxValue));
        this.Device.ImmediateContext.OutputMerger.SetUnorderedAccessViews(1, unorderedAccessViews, new int[3]);
      }
      this.Root?.Render(this, Matrix.Identity, false);
      this.Root?.Render(this, Matrix.Identity, true);
      if (this.RenderData.RenderMode != RenderData.RndMode.ShadowMap)
      {
        uint counterLimit = 16777216;
        this.Device.ImmediateContext.CopySubresourceRegion((Resource) this.RenderData._pixelCountBuffer, 0, new ResourceRegion?(new ResourceRegion(0, 0, 0, 4, 1, 1)), (Resource) this.RenderData._stagingPixelCountBuffer, 0);
        uint num = this.CheckTotalCount();
        if (this.RenderData.TransparencyInitialized)
        {
          if (num > 0U && num <= counterLimit)
          {
            this.Device.ImmediateContext.OutputMerger.SetUnorderedAccessView(1, this.RenderData._headPointerUAV);
            this.Device.ImmediateContext.PixelShader.SetShaderResource(0, this.RenderData._nodesDataSRV);
            this.RenderData.FullscreenTriangleRenderer.Render(this, Matrix.Identity, width, height, 0, height);
          }
          else if (num > counterLimit)
          {
            this.Device.ImmediateContext.CopyResource((Resource) this.RenderData._pixelCountBuffer, (Resource) this.RenderData._stagingPixelCountBuffer);
            List<Vector2> segments = this.GetSegments(height, counterLimit);
            int count = segments.Count;
            if (count > this.largestOverflow)
              this.largestOverflow = count;
            if (DateTime.Now - this._lastMeasuredTime > TimeSpan.FromSeconds(1.0))
            {
              this.largestOverflow = 0;
              this._lastMeasuredTime = DateTime.Now;
            }
            this.Device.ImmediateContext.ClearUnorderedAccessView(this.RenderData._headPointerUAV, new RawInt4(0, 0, 0, 0));
            foreach (Vector2 vector2 in segments)
            {
              this.Device.ImmediateContext.OutputMerger.SetUnorderedAccessViews(1, unorderedAccessViews, new int[3]);
              int x = (int) vector2.X;
              int y = (int) vector2.Y;
              this._camDataBlinnPhong.minHeight = x;
              this._camDataBlinnPhong.maxHeight = y;
              this._camDataLines.minHeight = x;
              this._camDataLines.maxHeight = y;
              this.Root?.Render(this, Matrix.Identity, true);
              this.Device.ImmediateContext.OutputMerger.SetUnorderedAccessView(1, this.RenderData._headPointerUAV);
              this.Device.ImmediateContext.PixelShader.SetShaderResource(0, this.RenderData._nodesDataSRV);
              this.RenderData.FullscreenTriangleRenderer.Render(this, Matrix.Identity, width, height, x, y);
              this.Device.ImmediateContext.PixelShader.SetShaderResource(0, (ShaderResourceView) null);
            }
          }
        }
        else if (num != 0U)
        {
          this.RenderData.InitTransparency();
          this.Render(renderTargetView, depthStencilView, width, height);
        }
      }
      this.Device.ImmediateContext.ClearState();
      this.Device.ImmediateContext.Flush();
    }

    public uint CheckTotalCount()
    {
      DataStream stream;
      this.Device.ImmediateContext.MapSubresource(this.RenderData._stagingPixelCountBuffer, MapMode.Read, MapFlags.None, out stream);
      uint[] numArray = stream.ReadRange<uint>(1);
      stream.Dispose();
      this.Device.ImmediateContext.UnmapSubresource((Resource) this.RenderData._stagingPixelCountBuffer, 0);
      return numArray[0];
    }

    public CameraState ExportCameraState() => new CameraState()
    {
      RootTransform = this.Root.Transform,
      Zoom = this.Zoom,
      CamDistOrtho = this.CamDistOrtho
    };

    public void ImportCameraState(CameraState cameraState)
    {
      this.Zoom = cameraState.Zoom;
      this.CamDistOrtho = cameraState.CamDistOrtho;
      this.Root.Transform = cameraState.RootTransform;
    }

    public List<Vector2> GetSegments(int count, uint counterLimit)
    {
      DataStream stream;
      this.Device.ImmediateContext.MapSubresource(this.RenderData._stagingPixelCountBuffer, MapMode.Read, MapFlags.None, out stream);
      uint[] numArray = stream.ReadRange<uint>(count + 1);
      stream.Dispose();
      this.Device.ImmediateContext.UnmapSubresource((Resource) this.RenderData._stagingPixelCountBuffer, 0);
      List<Vector2> segments = new List<Vector2>();
      uint num = 0;
      int x = 0;
      for (int index = 1; index <= count; ++index)
      {
        num += numArray[index];
        if (num > counterLimit)
        {
          segments.Add(new Vector2((float) x, (float) (index - 1)));
          x = index;
          num = numArray[index];
        }
      }
      if (num != 0U)
        segments.Add(new Vector2((float) x, (float) (count - 1)));
      return segments;
    }

    public void Dispose()
    {
      lock (this)
      {
        this.Root.DisposeWithChildren();
        this.Root = (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node) null;
        this.ProtoModelMap.Clear();
        foreach (KeyValuePair<EffectType, BaseEffect> effect in this.Effects)
          effect.Value?.Dispose();
        this.Effects.Clear();
        this.RenderData.Dispose();
        this.RenderData = (RenderData) null;
      }
    }
  }
}
