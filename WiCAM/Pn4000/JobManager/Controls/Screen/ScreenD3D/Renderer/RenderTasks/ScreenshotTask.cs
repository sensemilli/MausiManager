// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.ScreenshotTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  internal class ScreenshotTask : RenderTaskBase
  {
    private RenderData _renderData;
    private EventWaitHandle _waitFrameCompletedHandle = (EventWaitHandle) new ManualResetEvent(true);
    private string _screenshotDestPath;
    private int _border;
    private int _width;
    private int _height;
    private bool _multisampling;
    private RenderData.RndMode _renderMode;

    public Bitmap LastScreenshot { get; private set; }

    public ScreenshotTask(
      RenderData renderData,
      string screenshotDestPath,
      Action<RenderTaskResult> callback,
      int border = -1,
      int width = -1,
      int height = -1,
      RenderData.RndMode renderMode = RenderData.RndMode.Standard)
      : base(callback)
    {
      this._screenshotDestPath = screenshotDestPath;
      this._renderData = renderData;
      this._border = border;
      this._width = width == -1 ? this._renderData.Width : width;
      this._height = height == -1 ? this._renderData.Height : height;
      this._multisampling = renderMode == RenderData.RndMode.Standard;
      this._renderMode = renderMode;
    }

    public ScreenshotTask(RenderData renderData, Action<RenderTaskResult> callback)
      : base(callback)
    {
      this._renderData = renderData;
    }

    private Bitmap CropImage(Bitmap imageToCrop, int x, int y, int width, int height)
    {
      Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
      Graphics graphics = Graphics.FromImage((Image) bitmap);
      RectangleF srcRect = new RectangleF((float) x, (float) y, (float) width, (float) height);
      RectangleF destRect = new RectangleF(0.0f, 0.0f, (float) width, (float) height);
      graphics.DrawImage((Image) imageToCrop, destRect, srcRect, GraphicsUnit.Pixel);
      graphics.Dispose();
      return bitmap;
    }

    private bool DoScreenshot(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      if (this._screenshotDestPath != null)
      {
        string directoryName = Path.GetDirectoryName(this._screenshotDestPath);
        if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
          return false;
      }
      byte[] data;
      RenderTask.RenderOffscreen(renderer, this._renderMode, this._width, this._height, this._multisampling, out data);
      int width = data.Length / (this._height * 4);
      if (this._screenshotDestPath != null || this._callback != null)
      {
        this.LastScreenshot = new Bitmap(width, this._height, PixelFormat.Format32bppArgb);
        BitmapData bitmapdata = this.LastScreenshot.LockBits(new Rectangle(0, 0, width, this._height), ImageLockMode.ReadWrite, this.LastScreenshot.PixelFormat);
        Marshal.Copy(data, 0, bitmapdata.Scan0, data.Length);
        this.LastScreenshot.UnlockBits(bitmapdata);
      }
      if (this._border != -1)
      {
        int num1 = int.MaxValue;
        int val2_1 = int.MinValue;
        int num2 = int.MaxValue;
        int val2_2 = int.MinValue;
        for (int val1_1 = 0; val1_1 < this._height; ++val1_1)
        {
          for (int val1_2 = 0; val1_2 < this._width; ++val1_2)
          {
            int index = val1_1 * width * 4 + val1_2 * 4;
            if (data[index] != (byte) 0 || data[index + 1] != (byte) 0 || data[index + 2] != (byte) 0)
            {
              num1 = Math.Min(val1_2, num1);
              val2_1 = Math.Max(val1_2, val2_1);
              num2 = Math.Min(val1_1, num2);
              val2_2 = Math.Max(val1_1, val2_2);
            }
          }
        }
        if (num1 != int.MaxValue && num1 < val2_1 && num2 < val2_2)
          this.LastScreenshot = this.CropImage(this.LastScreenshot, num1, num2, val2_1 - num1, val2_2 - num2);
      }
      if (this._screenshotDestPath != null)
      {
        try
        {
          this.LastScreenshot.Save(this._screenshotDestPath, ImageFormat.Png);
        }
        catch (ExternalException ex)
        {
          return false;
        }
      }
      return true;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      renderer.RenderData.RenderMode = this._renderMode;
      bool succsess = this.DoScreenshot(renderer);
      Action<RenderTaskResult> callback = this._callback;
      if (callback == null)
        return;
      callback(new RenderTaskResult((RenderTaskBase) this, succsess));
    }
  }
}
