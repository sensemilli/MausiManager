// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.RenderTaskResult
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class RenderTaskResult
  {
    public RenderTaskBase RenderTask { get; private set; }

    public bool IsSuccessfull { get; private set; }

    public RenderTaskResult(RenderTaskBase task, bool succsess)
    {
      this.RenderTask = task;
      this.IsSuccessfull = succsess;
    }
  }
}
