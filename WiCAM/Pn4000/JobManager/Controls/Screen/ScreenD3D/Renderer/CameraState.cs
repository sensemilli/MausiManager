// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.CameraState
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;

namespace WiCAM.Pn4000.ScreenD3D.Renderer
{
  public struct CameraState
  {
    public Matrix? RootTransform;
    public float Zoom;
    public float CamDistOrtho;
  }
}
