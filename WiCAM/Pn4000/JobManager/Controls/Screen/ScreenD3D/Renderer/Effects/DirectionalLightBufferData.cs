// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Effects.DirectionalLightBufferData
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Effects
{
  [StructLayout(LayoutKind.Sequential, Size = 112)]
  public struct DirectionalLightBufferData
  {
    public Matrix _lightProjection;
    public Vector3 _color;
    public float _intensity;
    public Vector3 _dir;
    public float _shadowmapScalar;
    public Vector3 _ambientColor;
  }
}
