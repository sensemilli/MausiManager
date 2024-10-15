// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.RenderTaskBase
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public abstract class RenderTaskBase
  {
    private static long _nextId;
    protected Action<RenderTaskResult> _callback;

    public long ID { get; }

    public RenderTaskBase(Action<RenderTaskResult> callback)
    {
      this.ID = RenderTaskBase._nextId;
      ++RenderTaskBase._nextId;
      this._callback = callback;
    }

    public abstract void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer);

    public void Skip()
    {
      if (this._callback == null)
        return;
      this._callback(new RenderTaskResult(this, false));
    }
  }
}
