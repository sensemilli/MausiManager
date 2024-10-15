// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.TriangleMeshNode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.ScreenD3D.Renderer.Effects;

namespace WiCAM.Pn4000.ScreenD3D.Renderer.Nodes
{
  public class TriangleMeshNode : Node
  {
    private PhongObjectConstBufferData _objectData;
    private List<Triple<WiCAM.Pn4000.BendModel.Base.Color, uint, uint>> _appearanceGroupsStartEndIdx;
    private bool _isPartModel;
    private bool _isSystemModel;

    public List<Triple<Face, uint, uint>> FaceStartEndIdx { get; }

    private double Clamp(double val, double min, double max)
    {
      if (val < min)
        return min;
      return val > max ? max : val;
    }

    public TriangleMeshNode(
      List<Triple<Face, uint, uint>> faceStartEndIdx,
      Node parent,
      RenderData data)
      : base(parent)
    {
      this.FaceStartEndIdx = faceStartEndIdx;
      this._objectData._world = Matrix.Identity;
      this._objectData._material.color = Vector4.One;
      this._objectData._material.hardness = (float) Math.Pow(128.0, Math.Pow(1.0 - this.Clamp(0.4, 0.0, 1.0), 2.0));
      ModelNode parent1 = ((this != null ? this.Parent : (Node) null) is ShellNode parent2 ? parent2.Parent : (Node) null) as ModelNode;
      if (parent1.Model.PartRole == PartRole.Billboard)
        this._objectData._type = 1;
      else if (parent1.Model.PartRole == PartRole.BillboardModel)
        this._objectData._type = 4;
      else if (parent1.Model.ModelType == ModelType.Static)
      {
        this._objectData._type = 2;
        this._objectData._material.type = 0;
      }
      else if (parent1.Model.ModelType == ModelType.System)
      {
        this._objectData._type = 3;
        this._objectData._material.type = 0;
      }
      else
        this._objectData._type = 0;
      ModelType modelType = GetModelType(parent1.Model);
      this._isPartModel = modelType == ModelType.Part || modelType == ModelType.Assembly;
      this.UpdateAppearance(data.UseOriginaColors || !this._isPartModel, data.OverallOpacity);

      static ModelType GetModelType(Model m)
      {
        if (m == null)
          return ModelType.None;
        return m.ModelType == ModelType.None ? GetModelType(m.Parent) : m.ModelType;
      }
    }

    public void UpdateAppearance(bool useOriginaColors, double overallOpacity)
    {
      useOriginaColors |= !this._isPartModel;
      if (this.FaceStartEndIdx == null || this.FaceStartEndIdx.Count <= 0)
        return;
      this.GetModelInstance();
      this._appearanceGroupsStartEndIdx = new List<Triple<WiCAM.Pn4000.BendModel.Base.Color, uint, uint>>();
      Triple<Face, uint, uint> triple1 = this.FaceStartEndIdx.First<Triple<Face, uint, uint>>();
      Triple<Face, uint, uint> triple2 = triple1;
      foreach (Triple<Face, uint, uint> triple3 in this.FaceStartEndIdx)
      {
        if (triple1.Item1.RenderColor(useOriginaColors, overallOpacity) != triple3.Item1.RenderColor(useOriginaColors, overallOpacity))
        {
          this._appearanceGroupsStartEndIdx.Add(new Triple<WiCAM.Pn4000.BendModel.Base.Color, uint, uint>(triple1.Item1.RenderColor(useOriginaColors, overallOpacity), triple1.Item2, triple2.Item3));
          triple1 = triple3;
        }
        triple2 = triple3;
      }
      Triple<Face, uint, uint> triple4 = this.FaceStartEndIdx.Last<Triple<Face, uint, uint>>();
      this._appearanceGroupsStartEndIdx.Add(new Triple<WiCAM.Pn4000.BendModel.Base.Color, uint, uint>(triple1.Item1.RenderColor(useOriginaColors, overallOpacity), triple1.Item2, triple4.Item3));
    }

    public override void Render(WiCAM.Pn4000.ScreenD3D.Renderer.Renderer renderer, Matrix transform, bool renderAlpha)
    {
      if (this.Visible)
      {
        if (this.Transform.HasValue)
          transform *= this.Transform.Value;
        this._objectData._world = transform;
        this._objectData._material.type = (int) renderer.RenderData.LightingMode;
        BaseEffect effect = renderer.Effects[EffectType.BlinnPhong];
        if (renderer.RenderData.RenderMode != RenderData.RndMode.ShadowMap)
        {
          if (renderer.RenderData.RenderMode == RenderData.RndMode.Depth)
            effect = renderer.Effects[EffectType.SimpleDepth];
          else if ((((this != null ? this.Parent : (Node) null) is ShellNode parent ? parent.Parent : (Node) null) as ModelNode).Model.PartRole == PartRole.WindowControl)
            effect = renderer.Effects[EffectType.Textured2d];
        }
        effect?.SetActive(renderAlpha);
        effect?.UpdateFrameConstantBuffer(ref renderer.BlinnPhongCamData);
        effect?.UpdateViewportBuffer(ref renderer.ViewportData);
        effect?.UpdateLightingConstantBuffer(ref renderer.BlinnPhongDirectionalLight);
        if (this._appearanceGroupsStartEndIdx != null)
        {
          foreach (Triple<WiCAM.Pn4000.BendModel.Base.Color, uint, uint> triple in this._appearanceGroupsStartEndIdx)
          {
            WiCAM.Pn4000.BendModel.Base.Color color = triple.Item1;
            if (!((double) color.A < 1.0 & renderAlpha))
            {
              color = triple.Item1;
              if ((double) color.A != 1.0 || renderAlpha)
                continue;
            }
            ref Material local = ref this._objectData._material;
            color = triple.Item1;
            double r = (double) color.R;
            color = triple.Item1;
            double g = (double) color.G;
            color = triple.Item1;
            double b = (double) color.B;
            color = triple.Item1;
            double a = (double) color.A;
            Vector4 vector4 = new Vector4((float) r, (float) g, (float) b, (float) a);
            local.color = vector4;
            effect?.UpdatePerObjectConstBuffer(ref this._objectData);
            this.Geometry.Draw((int) triple.Item2, (int) triple.Item3);
          }
        }
        else if ((double) this._objectData._material.color.W < 1.0 & renderAlpha || (double) this._objectData._material.color.W == 1.0 && !renderAlpha)
          this.Geometry.Draw();
      }
      base.Render(renderer, transform, renderAlpha);
    }

    private ModelInstance GetModelInstance() => !((((this != null ? this.Parent : (Node) null) is ShellNode parent1 ? parent1.Parent : (Node) null) is ModelNode parent2 ? parent2.Parent : (Node) null) is ModelInstanceNode parent3) ? (ModelInstance) null : parent3.ModelInstance;

    public override void Dispose()
    {
      base.Dispose();
      this.FaceStartEndIdx.Clear();
    }
  }
}
