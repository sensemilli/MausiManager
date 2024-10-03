// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks.UpdateModelAppearanceTask
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks
{
  public class UpdateModelAppearanceTask : RenderTaskBase
  {
    public Model Model { get; }

    public UpdateModelAppearanceTask(Model model, Action<RenderTaskResult> callback)
      : base(callback)
    {
      this.Model = model;
    }

    public override void Execute(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      lock (this.Model)
        UpdateModelAppearanceTask.UpdateModel(this.Model, renderer);
      Action<RenderTaskResult> callback = this._callback;
      if (callback != null)
        callback(new RenderTaskResult((RenderTaskBase) this, true));
      renderer.RenderData.ShadowmapUpToDate = false;
    }

    private static void UpdateModel(Model model, WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer)
    {
      lock (model)
      {
                ShellNode shellNode3 = null;
        ModelNode modelNode;
        if (!renderer.ProtoModelMap.TryGetValue(model, out modelNode))
          return;
        foreach (Shell shell1 in model.Shells)
        {
          Shell shell = shell1;
          lock (shell)
          {
            if (modelNode.Children.FirstOrDefault<Node>((Func<Node, bool>) (n =>
            {
              if (!(n is ShellNode shellNode2))
                return false;
              int? id1 = shellNode2.Shell.RoundFaceGroups.FirstOrDefault<FaceGroup>()?.ID;
              int? id2 = shell.RoundFaceGroups.FirstOrDefault<FaceGroup>()?.ID;
              return id1.GetValueOrDefault() == id2.GetValueOrDefault() & id1.HasValue == id2.HasValue;
            })) is ShellNode)
              shellNode3.FacesNode?.UpdateAppearance(renderer.RenderData.UseOriginaColors, renderer.RenderData.OverallOpacity);
            shellNode3?.EdgesNode?.UpdateAppearance(renderer.RenderData.UseOriginaColors);
          }
        }
        foreach (Model subModel in model.SubModels)
          UpdateModelAppearanceTask.UpdateModel(subModel, renderer);
        foreach (Model model1 in model.ReferenceModel.Select<ModelInstance, Model>((Func<ModelInstance, Model>) (m => m.Reference)))
          UpdateModelAppearanceTask.UpdateModel(model1, renderer);
      }
    }
  }
}
