// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.EdgesNode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.ScreenD3D.Renderer.Effects;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Nodes
{
  public class EdgesNode : Node
  {
    private LineObjectConstBufferData _objectData;
    private List<Triple<Pair<WiCAM.Pn4000.BendModel.Base.Color, float>, uint, uint>> _appearanceGroupsStartEndIdx;
    private bool _isSystemModel;

    public List<Triple<FaceHalfEdge, uint, uint>> EdgeStartEndIdx { get; }

    public EdgesNode(
      List<Triple<FaceHalfEdge, uint, uint>> edgeStartEndIdx,
      Node parent,
      RenderData data)
      : base(parent)
    {
      this._objectData._world = Matrix.Identity;
      this._objectData._color = Vector4.One;
      this._objectData._width = 1f;
      this.EdgeStartEndIdx = edgeStartEndIdx;
      this._isSystemModel = (((this != null ? this.Parent : (Node) null) is ShellNode parent1 ? parent1.Parent : (Node) null) as ModelNode).Model.ModelType == ModelType.System;
      this.UpdateAppearance(data.UseOriginaColors || this._isSystemModel);
    }

    public void UpdateAppearance(bool useOriginaColors)
    {
      useOriginaColors |= this._isSystemModel;
      if (this.EdgeStartEndIdx == null)
        return;
      this._appearanceGroupsStartEndIdx = new List<Triple<Pair<WiCAM.Pn4000.BendModel.Base.Color, float>, uint, uint>>();
      Triple<FaceHalfEdge, uint, uint> triple1 = this.EdgeStartEndIdx.First<Triple<FaceHalfEdge, uint, uint>>();
      Triple<FaceHalfEdge, uint, uint> triple2 = triple1;
      foreach (Triple<FaceHalfEdge, uint, uint> triple3 in this.EdgeStartEndIdx)
      {
        if (triple1.Item1.RenderColor != triple3.Item1.RenderColor || (double) triple1.Item1.RenderWidth != (double) triple3.Item1.RenderWidth)
        {
          WiCAM.Pn4000.BendModel.Base.Color renderColor = triple1.Item1.RenderColor;
          if (useOriginaColors)
            renderColor.A = 1f;
          this._appearanceGroupsStartEndIdx.Add(new Triple<Pair<WiCAM.Pn4000.BendModel.Base.Color, float>, uint, uint>(new Pair<WiCAM.Pn4000.BendModel.Base.Color, float>(renderColor, triple1.Item1.RenderWidth), triple1.Item2, triple2.Item3));
          triple1 = triple3;
        }
        triple2 = triple3;
      }
      WiCAM.Pn4000.BendModel.Base.Color renderColor1 = triple1.Item1.RenderColor;
      if (useOriginaColors)
        renderColor1.A = 1f;
      Triple<FaceHalfEdge, uint, uint> triple4 = this.EdgeStartEndIdx.Last<Triple<FaceHalfEdge, uint, uint>>();
      this._appearanceGroupsStartEndIdx.Add(new Triple<Pair<WiCAM.Pn4000.BendModel.Base.Color, float>, uint, uint>(new Pair<WiCAM.Pn4000.BendModel.Base.Color, float>(renderColor1, triple1.Item1.RenderWidth), triple1.Item2, triple4.Item3));
    }

    public override void Render(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Matrix transform, bool renderAlpha)
    {
      if (renderer.RenderData.RenderMode == RenderData.RndMode.ShadowMap || !this.Visible)
        return;
      if (this.Transform.HasValue)
        transform *= this.Transform.Value;
      this._objectData._world = transform;
      if (renderer.RenderData.RenderMode == RenderData.RndMode.Standard)
      {
                LineEffect effect = new LineEffect(null, null);
        if (renderer.Effects[EffectType.Line] is LineEffect)
          effect.SetActive(renderAlpha);
        effect?.UpdateFrameConstantBuffer(ref renderer.LinesCamData);
        if (this._appearanceGroupsStartEndIdx != null)
        {
          foreach (Triple<Pair<WiCAM.Pn4000.BendModel.Base.Color, float>, uint, uint> triple in this._appearanceGroupsStartEndIdx)
          {
            WiCAM.Pn4000.BendModel.Base.Color color = triple.Item1.Item1;
            if (!((double) color.A < 1.0 & renderAlpha))
            {
              color = triple.Item1.Item1;
              if ((double) color.A != 1.0 || renderAlpha)
                continue;
            }
            ref LineObjectConstBufferData local = ref this._objectData;
            color = triple.Item1.Item1;
            double r = (double) color.R;
            color = triple.Item1.Item1;
            double g = (double) color.G;
            color = triple.Item1.Item1;
            double b = (double) color.B;
            color = triple.Item1.Item1;
            double a = (double) color.A;
            Vector4 vector4 = new Vector4((float) r, (float) g, (float) b, (float) a);
            local._color = vector4;
            this._objectData._width = triple.Item1.Item2;
            effect?.UpdatePerObjectConstBuffer(ref this._objectData);
            this.Geometry.Draw((int) triple.Item2, (int) triple.Item3);
          }
        }
        else if ((double) this._objectData._color.W < 1.0 & renderAlpha || (double) this._objectData._color.W == 1.0 && !renderAlpha)
          this.Geometry.Draw();
      }
      base.Render(renderer, transform, renderAlpha);
    }

    public override void Dispose()
    {
      base.Dispose();
      this.EdgeStartEndIdx.Clear();
    }
  }
}
