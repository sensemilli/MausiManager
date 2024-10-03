// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.RenderTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class RenderTask : RenderTaskBase
  {
    private RenderData _renderData;
    private D3DImage _d3d9Image;
    private static int _framesWpf;
    private static DateTime _lastMeasuredTimeWpf;

    public bool SkipQueuedFrames { get; }

    public RenderTask(
      bool skipQueuedFrames,
      RenderData renderData,
      D3DImage d3d9Image,
      Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.SkipQueuedFrames = skipQueuedFrames;
      this._renderData = renderData;
      this._d3d9Image = d3d9Image;
    }

    private void SaveDepthScreenshot(byte[] zBufData, int imgWidthPadded, int _height)
    {
      Bitmap bitmap = new Bitmap(imgWidthPadded, _height, PixelFormat.Format32bppArgb);
      BitmapData bitmapdata = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, imgWidthPadded, _height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
      Marshal.Copy(zBufData, 0, bitmapdata.Scan0, zBufData.Length);
      bitmap.UnlockBits(bitmapdata);
      bitmap.Save("C:/temp/depthscreenshot.png", ImageFormat.Png);
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      lock (renderer)
      {
        lock (renderer.RenderData)
        {
          RenderTask.RenderTripod(renderer);
          int num1 = Math.Min(200, this._renderData.Height);
          if (this._renderData.Width == 0 || this._renderData.Height == 0)
          {
            Action<RenderTaskResult> callback = this._callback;
            if (callback == null)
              return;
            callback(new RenderTaskResult((RenderTaskBase) this, true));
          }
          else
          {
            int num2 = (int) ((double) num1 * (double) this._renderData.Width / (double) this._renderData.Height);
            byte[] data;
            RenderTask.RenderOffscreen(renderer, RenderData.RndMode.Depth, num2, num1, false, out data);
            this.DetermineClippingPlanes(renderer, num2, num1, data);
            renderer.RenderData.RenderMode = RenderData.RndMode.Standard;
            renderer.Render(this._renderData.RenderTargetView, this._renderData.DepthStencilView, this._renderData.Width, this._renderData.Height);
            this._renderData.Device11.ImmediateContext.ResolveSubresource((SharpDX.Direct3D11.Resource) this._renderData.RenderTexture, 0, (SharpDX.Direct3D11.Resource) this._renderData.TextureBackBuffer, 0, SharpDX.DXGI.Format.B8G8R8A8_UNorm);
            this._renderData.Device11.ImmediateContext.CopySubresourceRegion((SharpDX.Direct3D11.Resource) this._renderData.TextureBackBuffer, 0, new ResourceRegion?(new ResourceRegion(0, 0, 0, 1, 1, 1)), (SharpDX.Direct3D11.Resource) this._renderData.StagingTexture, 0);
            this._renderData.Device11.ImmediateContext.MapSubresource(this._renderData.StagingTexture, 0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out DataStream _);
            this._renderData.Device11.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) this._renderData.StagingTexture, 0);
            try
            {
              Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, (Delegate) (() =>
              {
                this.UpdateWpfD3D9Image();
                if (DateTime.Now - RenderTask._lastMeasuredTimeWpf > TimeSpan.FromSeconds(1.0))
                {
                  RenderTask._framesWpf = 0;
                  RenderTask._lastMeasuredTimeWpf = DateTime.Now;
                }
                ++RenderTask._framesWpf;
              }));
            }
            catch (Exception ex)
            {
              Console.WriteLine(ex.Message);
              Console.WriteLine(ex.StackTrace);
            }
            Action<RenderTaskResult> callback = this._callback;
            if (callback == null)
              return;
            callback(new RenderTaskResult((RenderTaskBase) this, true));
          }
        }
      }
    }

    public static void RenderTripod(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      int num = 640;
      Matrix? transform = renderer.Root.Transform;
      float zoom = renderer.Zoom;
      float camDistOrtho = renderer.CamDistOrtho;
      float nearClip = renderer.RenderData.NearClip;
      float farClip = renderer.RenderData.FarClip;
      if (renderer.Root.Transform.HasValue)
      {
        renderer.RenderData.RenderMode = RenderData.RndMode.Tripod;
        renderer.Zoom = 200f;
        renderer.CamDistOrtho = 1000f;
        Matrix matrix = renderer.Root.Transform.Value with
        {
          M41 = 0.0f,
          M42 = 0.0f,
          M43 = 0.0f
        };
        lock (renderer.Root)
        {
          renderer.Root.Transform = new Matrix?(matrix);
          renderer.RenderData.NearClip = 1f;
          renderer.RenderData.FarClip = 3000f;
          renderer.RenderData.SetOSTargetViewSizeTripod(num, num);
          renderer.Render(renderer.RenderData.RenderTargetViewOSTripod, renderer.RenderData.DepthStencilViewOSTripod, num, num);
          renderer.Root.Transform = transform;
        }
      }
      renderer.Zoom = zoom;
      renderer.CamDistOrtho = camDistOrtho;
      renderer.RenderData.NearClip = nearClip;
      renderer.RenderData.FarClip = farClip;
    }

    public static void RenderOffscreen(
      WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer,
      RenderData.RndMode renderMode,
      int width,
      int height,
      bool multisampling,
      out byte[] data)
    {
      renderer.RenderData.NearClip = 1f;
      renderer.RenderData.FarClip = 100000f;
      if (width == 0 || height == 0)
      {
        data = new byte[0];
      }
      else
      {
        renderer.RenderData.RenderMode = renderMode;
        renderer.RenderData.SetOSTargetViewSize(width, height, multisampling);
        renderer.Render(renderer.RenderData.RenderTargetViewOS, renderer.RenderData.DepthStencilViewOS, width, height);
        if (multisampling)
        {
          renderer.RenderData.Device11.ImmediateContext.ResolveSubresource((SharpDX.Direct3D11.Resource) renderer.RenderData.RenderTextureOS, 0, (SharpDX.Direct3D11.Resource) renderer.RenderData.TextureBackBufferOS, 0, SharpDX.DXGI.Format.B8G8R8A8_UNorm);
          renderer.RenderData.Device11.ImmediateContext.CopyResource((SharpDX.Direct3D11.Resource) renderer.RenderData.TextureBackBufferOS, (SharpDX.Direct3D11.Resource) renderer.RenderData.StagingTextureOS);
        }
        else
          renderer.RenderData.Device11.ImmediateContext.CopyResource((SharpDX.Direct3D11.Resource) renderer.RenderData.RenderTextureOS, (SharpDX.Direct3D11.Resource) renderer.RenderData.StagingTextureOS);
        DataStream stream;
        DataBox dataBox = renderer.RenderData.Device11.ImmediateContext.MapSubresource(renderer.RenderData.StagingTextureOS, 0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out stream);
        int widthOs = renderer.RenderData.WidthOS;
        int heightOs = renderer.RenderData.HeightOS;
        int num = dataBox.RowPitch / 4;
        data = new byte[stream.RemainingLength];
        stream.Read(data, 0, (int) stream.RemainingLength);
        renderer.RenderData.Device11.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) renderer.RenderData.StagingTextureOS, 0);
      }
    }

    private void DetermineClippingPlanes(
      WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer,
      int imgWidth,
      int imgHeight,
      byte[] zBufferData)
    {
      if (zBufferData.Length == 0)
        return;
      int num1 = zBufferData.Length / (imgHeight * 4);
      float num2 = float.MaxValue;
      float num3 = float.MinValue;
      int x1 = -1;
      int y1 = -1;
      int x2 = -1;
      int y2 = -1;
      for (int index1 = 0; index1 < imgHeight; ++index1)
      {
        for (int index2 = 0; index2 < imgWidth; ++index2)
        {
          int index3 = index1 * num1 * 4 + index2 * 4;
          if (zBufferData[index3] != (byte) 0 || zBufferData[index3 + 1] != (byte) 0 || zBufferData[index3 + 2] != (byte) 0)
          {
            int num4 = ((int) zBufferData[index3] & 128) > 0 ? 1 : 0;
            float num5 = (float) ((int) zBufferData[index3 + 2] | (int) zBufferData[index3 + 1] << 8 | ((int) zBufferData[index3] & (int) sbyte.MaxValue) << 16);
            if (num4 != 0)
              num5 *= -1f;
            if ((double) num5 < (double) num2)
            {
              num2 = num5;
              x1 = index2;
              y1 = index1;
            }
            if ((double) num5 > (double) num3)
            {
              num3 = num5;
              x2 = index2;
              y2 = index1;
            }
          }
        }
      }
      if ((double) num2 == 3.4028234663852886E+38)
      {
        float num6 = 1f;
        float num7 = 10000f;
        this._renderData.NearClip = num6;
        this._renderData.FarClip = num7;
      }
      else
      {
        float z1 = num2 / 100f;
        float z2 = num3 / 100f;
        float num8;
        float num9;
        if (renderer.ProjectionType == ProjectionType.Isometric)
        {
          Matrix4d wicamMatrix4d1 = RenderTask.SharpDXMatrix4dToWicamMatrix4d(renderer.View * renderer.Projection);
          Matrix4d wicamMatrix4d2 = RenderTask.SharpDXMatrix4dToWicamMatrix4d(renderer.View);
          Matrix4d inverted = wicamMatrix4d1.Inverted;
          Vector3d v1 = new Vector3d((double) x1, (double) y1, (double) z1);
          Vector3d v2 = inverted.Transform(ref v1);
          Vector3d vector3d1 = wicamMatrix4d2.Transform(ref v2);
          Vector3d v3 = new Vector3d((double) x2, (double) y2, (double) z2);
          Vector3d v4 = inverted.Transform(ref v3);
          Vector3d vector3d2 = wicamMatrix4d2.Transform(ref v4);
          float num10 = (float) -vector3d1.Z;
          float num11 = (float) -vector3d2.Z;
          renderer.CamDistOrtho += 1000f - num10;
          float val2 = (float) ((double) num11 - (double) num10 + 1000.0 + 1000.0);
          num8 = 1f;
          num9 = Math.Min(1000000f, val2);
        }
        else
        {
          float num12 = z2 - z1;
          float val2_1 = z1 - 0.1f * num12;
          float val2_2 = z2 + 0.1f * num12;
          num8 = Math.Max(1f, val2_1);
          float val2_3 = Math.Min(1000000f, val2_2);
          num9 = Math.Max(num8 + 1000f, val2_3);
        }
        this._renderData.NearClip = num8;
        this._renderData.FarClip = num9;
      }
    }

    private static Matrix4d SharpDXMatrix4dToWicamMatrix4d(Matrix mat) => new Matrix4d()
    {
      M00 = (double) mat.M11,
      M01 = (double) mat.M12,
      M02 = (double) mat.M13,
      M03 = (double) mat.M14,
      M10 = (double) mat.M21,
      M11 = (double) mat.M22,
      M12 = (double) mat.M23,
      M13 = (double) mat.M24,
      M20 = (double) mat.M31,
      M21 = (double) mat.M32,
      M22 = (double) mat.M33,
      M23 = (double) mat.M34,
      M30 = (double) mat.M41,
      M31 = (double) mat.M42,
      M32 = (double) mat.M43,
      M33 = (double) mat.M44
    };

    private void UpdateWpfD3D9Image()
    {
      this._d3d9Image.Lock();
      this._renderData.IsFirstFrame = false;
      try
      {
        if (!this._renderData.IsDisposed)
        {
          using (SharpDX.Direct3D9.Surface surfaceLevel = this._renderData.SharedBackBufferAsD3D9.GetSurfaceLevel(0))
            this._d3d9Image.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surfaceLevel.NativePointer, true);
        }
        this._d3d9Image.AddDirtyRect(new Int32Rect(0, 0, this._d3d9Image.PixelWidth, this._d3d9Image.PixelHeight));
      }
      catch (Exception ex)
      {
      }
      this._d3d9Image.Unlock();
    }
  }
}
