// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Effects.LightingBufferData
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Effects
{
  [StructLayout(LayoutKind.Sequential, Size = 48)]
  public struct LightingBufferData
  {
    public DirectionalLight _light;
    public Vector3 _ambientLight;
  }
}
