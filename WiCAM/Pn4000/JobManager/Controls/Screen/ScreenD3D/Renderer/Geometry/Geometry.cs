// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Geometry.Geometry
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Runtime.InteropServices;
using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Geometry
{
  public class Geometry : IGeometry, IDisposable
  {
    private SharpDX.Direct3D11.Device _device;
    private SharpDX.Direct3D11.Buffer _vertexBuffer;
    private VertexBufferBinding _vertexBufferBinding;
    private SharpDX.Direct3D11.Buffer _indexBuffer;
    private PrimitiveTopology _topology;
    private int _size;
    private bool hasIndex;

    public Geometry(
      SharpDX.Direct3D11.Device device,
      PrimitiveTopology topology,
      VertexPositionNormalTex[] vertices,
      uint[] indices)
    {
      this._device = device;
      this._topology = topology;
      BufferDescription description = new BufferDescription(Marshal.SizeOf<VertexPositionNormalTex>() * vertices.Length, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
      this._vertexBuffer = SharpDX.Direct3D11.Buffer.Create<VertexPositionNormalTex>(this._device, vertices, description);
      this._vertexBufferBinding = new VertexBufferBinding(this._vertexBuffer, Marshal.SizeOf<VertexPositionNormalTex>(), 0);
      if (indices != null && indices.Length != 0)
      {
        this.hasIndex = true;
        this._indexBuffer = SharpDX.Direct3D11.Buffer.Create<uint>(this._device, BindFlags.IndexBuffer, indices);
        this._size = indices.Length;
      }
      else
      {
        this.hasIndex = false;
        this._size = vertices.Length;
      }
    }

    public void Dispose()
    {
      this._vertexBuffer?.Dispose();
      this._indexBuffer?.Dispose();
      this._vertexBuffer = (SharpDX.Direct3D11.Buffer) null;
      this._indexBuffer = (SharpDX.Direct3D11.Buffer) null;
    }

    public virtual void Draw()
    {
      this._device.ImmediateContext.InputAssembler.PrimitiveTopology = this._topology;
      this._device.ImmediateContext.InputAssembler.SetVertexBuffers(0, this._vertexBufferBinding);
      if (this.hasIndex)
      {
        this._device.ImmediateContext.InputAssembler.SetIndexBuffer(this._indexBuffer, Format.R32_UInt, 0);
        this._device.ImmediateContext.DrawIndexed(this._size, 0, 0);
      }
      else
        this._device.ImmediateContext.Draw(this._size, 0);
    }

    public virtual void Draw(int startIdx, int endIdx)
    {
      this._device.ImmediateContext.InputAssembler.PrimitiveTopology = this._topology;
      this._device.ImmediateContext.InputAssembler.SetVertexBuffers(0, this._vertexBufferBinding);
      if (!this.hasIndex)
        return;
      this._device.ImmediateContext.InputAssembler.SetIndexBuffer(this._indexBuffer, Format.R32_UInt, 0);
      this._device.ImmediateContext.DrawIndexed(endIdx - startIdx + 1, startIdx, 0);
    }

    public void UpdateMeshVertexBuffer(VertexPositionNormalTex[] vertices)
    {
      DataStream stream;
      this._device.ImmediateContext.MapSubresource(this._vertexBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
      stream.WriteRange<VertexPositionNormalTex>(vertices);
      stream.Dispose();
      this._device.ImmediateContext.UnmapSubresource((SharpDX.Direct3D11.Resource) this._vertexBuffer, 0);
    }

    public void UpdateLineVertexBuffer(VertexPositionNormal[] vertices) => throw new BendException("Not implemented!");
  }
}
