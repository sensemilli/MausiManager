// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.SetViewDirectionTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class SetViewDirectionTask : RenderTaskBase
  {
    public Matrix ViewDirection { get; }

    public SetViewDirectionTask(Matrix matrix, Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.ViewDirection = matrix;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      renderer.Root.Transform = new Matrix?(this.ViewDirection);
      Action<RenderTaskResult> callback = this._callback;
      if (callback == null)
        return;
      callback(new RenderTaskResult((RenderTaskBase) this, true));
    }
  }
}
