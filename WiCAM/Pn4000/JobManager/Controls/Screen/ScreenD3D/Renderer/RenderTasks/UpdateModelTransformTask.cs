// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.UpdateModelTransformTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class UpdateModelTransformTask : RenderTaskBase
  {
    public Matrix4d Transform { get; set; }

    public Model Model { get; set; }

    public UpdateModelTransformTask(Model model, Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.Model = model;
      if (model == null)
        return;
      lock (model)
        this.Transform = model.Transform;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      Matrix matrix = new Matrix();
      ref Matrix local = ref matrix;
      Matrix4d transform = this.Transform;
      double M11 = transform[0, 0];
      transform = this.Transform;
      double M12 = transform[0, 1];
      transform = this.Transform;
      double M13 = transform[0, 2];
      transform = this.Transform;
      double M21 = transform[1, 0];
      transform = this.Transform;
      double M22 = transform[1, 1];
      transform = this.Transform;
      double M23 = transform[1, 2];
      transform = this.Transform;
      double M31 = transform[2, 0];
      transform = this.Transform;
      double M32 = transform[2, 1];
      transform = this.Transform;
      double M33 = transform[2, 2];
      transform = this.Transform;
      double M41 = transform[3, 0];
      transform = this.Transform;
      double M42 = transform[3, 1];
      transform = this.Transform;
      double M43 = transform[3, 2];
      local = new Matrix((float) M11, (float) M12, (float) M13, 0.0f, (float) M21, (float) M22, (float) M23, 0.0f, (float) M31, (float) M32, (float) M33, 0.0f, (float) M41, (float) M42, (float) M43, 1f);
      renderer.ProtoModelMap[this.Model].Transform = new Matrix?(matrix);
      Action<RenderTaskResult> callback = this._callback;
      if (callback != null)
        callback(new RenderTaskResult((RenderTaskBase) this, true));
      if (this.Model.ModelType == ModelType.System)
        return;
      renderer.RenderData.ShadowmapUpToDate = false;
    }
  }
}
