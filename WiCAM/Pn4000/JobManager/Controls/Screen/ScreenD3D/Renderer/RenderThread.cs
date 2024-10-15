// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderThread
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks;

namespace WiCAM.Pn4000.ScreenD3D.Renderer
{
  public class RenderThread
  {
    private WiCAM.Pn4000.ScreenD3D.Renderer.Renderer _renderer;
    private ConcurrentQueue<RenderTaskBase> _renderQueue = new ConcurrentQueue<RenderTaskBase>();
    private Thread _renderThread;
    private Barrier _barrier = new Barrier(2);
    private EventWaitHandle _waitHandle = (EventWaitHandle) new ManualResetEvent(false);
    private StreamWriter _fs;
    private int _renderThreadNumber;
    private static int _nextRenderThreadNumber;
    private bool _abort;
    private bool _aborted;

    public RenderData RenderData => this._renderer.RenderData;

    public RenderThread(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      this._renderThreadNumber = RenderThread._nextRenderThreadNumber++;
      this._renderer = renderer;
      this._renderThread = new Thread(new ThreadStart(this.RenderLoop));
      this._renderThread.Start();
      this._barrier.SignalAndWait();
    }

    private void InitLogger()
    {
      if (this._fs != null)
        return;
      try
      {
        string str = Environment.GetEnvironmentVariable("PNHOMEDRIVE") + Path.Combine(Environment.GetEnvironmentVariable("PNHOMEPATH"), "render_log");
        if (!Directory.Exists(str))
          Directory.CreateDirectory(str);
        this._fs = new StreamWriter(Path.Combine(str, string.Format("{0}_{1}__{2}.txt", (object) Environment.UserName, (object) DateTime.Now, (object) this._renderThreadNumber).Replace(":", "_")));
      }
      catch (Exception ex)
      {
      }
    }

    public void Dispose()
    {
      this._abort = true;
      this._waitHandle.Set();
      while (!this._aborted)
        Thread.Sleep(20);
      this._fs?.Close();
    }

    public void Enqueue(RenderTaskBase task)
    {
      this._renderQueue.Enqueue(task);
      this._waitHandle.Set();
    }

    private void RenderLoop()
    {
      bool flag1 = true;
      try
      {
        this.RenderData.Initialize();
        this._renderer.Init();
      }
      catch (Exception ex)
      {
        flag1 = false;
        this.InitLogger();
        this._fs?.WriteLine(string.Format("{0} initialization failed", (object) DateTime.Now));
        this._fs?.WriteLine(ex.Message ?? "");
        this._fs?.WriteLine(ex.StackTrace ?? "");
        this._fs?.Flush();
      }
      this._barrier.SignalAndWait();
      while (!this._abort)
      {
        if (!this._renderQueue.IsEmpty)
        {
          List<RenderTaskBase> renderTaskBaseList = new List<RenderTaskBase>();
          while (this._renderQueue.Count > 0)
          {
            RenderTaskBase result;
            if (this._renderQueue.TryDequeue(out result))
              renderTaskBaseList.Add(result);
          }
          if (flag1)
          {
            int num = 0;
            foreach (RenderTaskBase renderTaskBase in renderTaskBaseList)
            {
              ++num;
              try
              {
                if (renderTaskBase is RenderTask renderTask && renderTask.SkipQueuedFrames)
                {
                  bool flag2 = false;
                  for (int index = num; index < renderTaskBaseList.Count; ++index)
                  {
                    if (renderTaskBaseList[index] is RenderTask)
                    {
                      flag2 = true;
                      break;
                    }
                  }
                  if (flag2)
                    renderTask.Skip();
                  else
                    renderTaskBase.Execute(this._renderer);
                }
                else
                  renderTaskBase.Execute(this._renderer);
              }
              catch (Exception ex)
              {
                this.InitLogger();
                this._fs?.WriteLine(string.Format("{0} error in render loop", (object) DateTime.Now));
                if (renderTaskBase is ResizeTask resizeTask)
                  this._fs?.WriteLine(string.Format("resize w {0} h {1}", (object) resizeTask.Width, (object) resizeTask.Height));
                this._fs?.WriteLine(ex.Message ?? "");
                this._fs?.WriteLine(ex.StackTrace ?? "");
                this._fs?.WriteLine(string.Format("{0}", (object) this.RenderData.Device11.DeviceRemovedReason));
                this._fs?.Flush();
              }
            }
          }
        }
        this._waitHandle.WaitOne();
        this._waitHandle.Reset();
      }
      this._renderer.Dispose();
      this._renderer = (WiCAM.Pn4000.ScreenD3D.Renderer.Renderer) null;
      this._aborted = true;
    }
  }
}
