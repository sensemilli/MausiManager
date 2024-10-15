// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Geometry.VertexPosition
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Geometry
{
  [StructLayout(LayoutKind.Sequential, Pack = 16)]
  public struct VertexPosition
  {
    private Vector3 position;

    public VertexPosition(float a, float b, float c)
    {
      this.position.X = a;
      this.position.Y = b;
      this.position.Z = c;
    }
  }
}
