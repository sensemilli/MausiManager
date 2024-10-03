// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.UpdateModelVisibilityTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class UpdateModelVisibilityTask : RenderTaskBase
  {
    public Model Model { get; set; }

    public bool Visible { get; set; }

    public bool SubModelsVisible { get; set; }

    public UpdateModelVisibilityTask(Model model, Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.Model = model;
      this.Visible = this.Model.Enabled;
      this.SubModelsVisible = this.Model.EnabledSubModels;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      renderer.ProtoModelMap[this.Model].Enabled = this.Visible;
      renderer.ProtoModelMap[this.Model].EnabledSubNodes = this.SubModelsVisible;
      Action<RenderTaskResult> callback = this._callback;
      if (callback != null)
        callback(new RenderTaskResult((RenderTaskBase) this, true));
      renderer.RenderData.ShadowmapUpToDate = false;
    }
  }
}
