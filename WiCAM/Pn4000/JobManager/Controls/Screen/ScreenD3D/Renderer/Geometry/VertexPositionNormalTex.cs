// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Geometry.VertexPositionNormalTex
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Geometry
{
  [StructLayout(LayoutKind.Sequential, Pack = 16)]
  public struct VertexPositionNormalTex
  {
    public Vector3 _position;
    public Vector3 _normal;
    public Vector2 _tex;

    public VertexPositionNormalTex(Vector3 position, Vector3 normal, Vector2 tex)
    {
      this._position = position;
      this._normal = normal;
      this._tex = tex;
    }
  }
}
