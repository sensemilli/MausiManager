// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.ResizeTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class ResizeTask : RenderTaskBase
  {
    public int Width { get; }

    public int Height { get; }

    public ResizeTask(int width, int height, Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.Width = width;
      this.Height = height;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer) => renderer.RenderData.Resize(Math.Max(1, this.Width), Math.Max(1, this.Height));
  }
}
