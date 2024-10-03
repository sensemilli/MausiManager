// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.ScreenD3D.Renderer.Geometry;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Nodes
{
  public class Node : IDisposable
  {
    protected Matrix? _transform;
    private List<Node> _children = new List<Node>();

    public virtual bool Visible { get; set; } = true;

    public virtual bool Enabled { get; set; } = true;

    public virtual bool EnabledSubNodes { get; set; } = true;

    public Node Parent { get; set; }

    public virtual List<Node> Children
    {
      get
      {
        lock (this)
          return this._children;
      }
    }

    private bool HasChildren => this.Children.Count > 0;

    public virtual Matrix? Transform
    {
      get
      {
        lock (this)
          return this._transform;
      }
      set
      {
        lock (this)
          this._transform = value;
      }
    }

    public IGeometry Geometry { get; set; }

    public Node(Node parent) => this.Parent = parent;

    public virtual void Render(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Matrix transform, bool renderAlpha)
    {
      if (this.Visible && this.Transform.HasValue)
        transform = this.Transform.Value * transform;
      if (this.EnabledSubNodes)
      {
        foreach (Node node in this.Children.Where<Node>((Func<Node, bool>) (c => c is ModelNode)))
          node.Render(renderer, transform, renderAlpha);
      }
      if (!this.Enabled)
        return;
      foreach (Node node in this.Children.Where<Node>((Func<Node, bool>) (c => !(c is ModelNode))))
        node.Render(renderer, transform, renderAlpha);
    }

    public virtual void Dispose() => this.Geometry?.Dispose();

    public virtual void DisposeWithChildren()
    {
      this.Dispose();
      foreach (Node child in this.Children)
        child.DisposeWithChildren();
      lock (this)
        this.Children.Clear();
    }
  }
}
