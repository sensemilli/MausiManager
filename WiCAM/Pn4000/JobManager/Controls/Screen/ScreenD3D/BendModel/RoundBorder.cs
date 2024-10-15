// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.BendModel.RoundBorder
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX.Mathematics.Interop;

namespace WiCAM.Pn4000.BendModel
{
  public class RoundBorder
  {
    public RoundBorder(int width, int radius, RawColor4 color)
    {
      this.Width = width;
      this.Radius = radius;
      this.Color = color;
    }

    public int Width { get; set; }

    public int Radius { get; set; }

    public RawColor4 Color { get; set; }
  }
}
