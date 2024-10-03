// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.UpdateProjectionTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class UpdateProjectionTask : RenderTaskBase
  {
    public ProjectionType ProjectionType { get; set; }

    public UpdateProjectionTask(ProjectionType projectionType, Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.ProjectionType = projectionType;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      try
      {
        this.SetProjection(renderer);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        Console.WriteLine((object) renderer.RenderData.Device11.DeviceRemovedReason);
      }
      Action<RenderTaskResult> callback = this._callback;
      if (callback == null)
        return;
      callback(new RenderTaskResult((RenderTaskBase) this, true));
    }

    private void SetProjection(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer) => renderer.ProjectionType = this.ProjectionType;
  }
}
