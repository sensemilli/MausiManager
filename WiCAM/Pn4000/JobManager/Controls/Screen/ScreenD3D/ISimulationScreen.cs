// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.ISimulationScreen
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.ScreenD3D
{
  public interface ISimulationScreen
  {
    void Render(bool skipQueuedFrames);

    void UpdateModelTransform(Model model, bool render);

    void UpdateModelGeometry(Model model, bool render);

    void UpdateModelAppearance(Model model, bool render);
  }
}
