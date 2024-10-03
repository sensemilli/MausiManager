// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.IScreenshotScreen
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.ScreenD3D.Controls
{
  public interface IScreenshotScreen
  {
    void PrintScreen(
      Model model,
      string targetPath,
      Matrix4d transform,
      int border = -1,
      int width = -1,
      int height = -1);
  }
}
