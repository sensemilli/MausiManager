// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Geometry.IGeometry
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Geometry
{
  public interface IGeometry : IDisposable
  {
    void Draw();

    void Draw(int startIdx, int endIdx);

    void UpdateMeshVertexBuffer(VertexPositionNormalTex[] vertices);

    void UpdateLineVertexBuffer(VertexPositionNormal[] vertices);
  }
}
