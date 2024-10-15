// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.BendModel.Texture2DBillboard
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX.Direct3D11;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.BendModel
{
  public class Texture2DBillboard
  {
    public Matrix4d Transform { get; set; } = Matrix4d.Identity;

    public Vector3d Position { get; set; } = new Vector3d(0.0, 0.0, 0.0);

    public Texture2D Texture { get; set; }

    public Vector2d Extents { get; set; } = new Vector2d(1.0, 1.0);

    public float PixelDensity { get; set; } = 1f;

    public bool Enabled { get; set; } = true;

    public bool ScaleWithDistance { get; set; } = true;

    public bool RenderOnTop { get; set; } = true;

    public int SortingLayer { get; set; }
  }
}
