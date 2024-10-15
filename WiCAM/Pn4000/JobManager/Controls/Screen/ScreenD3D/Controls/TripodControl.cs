// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.TripodControl
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.ScreenD3D.Controls
{
  public class TripodControl : Model
  {
    private float _windowWidthFraction;

    public TripodControl(float windowWidthFraction, int windowWidthPixels, int windowHeightPixels)
    {
      this._windowWidthFraction = windowWidthFraction;
      this.ModelType = ModelType.System;
      this.PartRole = PartRole.WindowControl;
      this.Shells.Add(this.CreateShell(windowWidthFraction, windowWidthPixels, windowHeightPixels));
    }

    private Shell CreateShell(
      float windowWidthFraction,
      int windowWidthPixels,
      int windowHeightPixels)
    {
      Shell shell = new Shell((Model) this);
      Face face = new Face(shell, 0);
      face.Color = new Color(1f, 0.0f, 0.0f, 1f);
      float z = 0.0f;
      float num1 = 1f;
      if (windowHeightPixels != 0 && windowWidthPixels != 0)
        num1 = (float) windowWidthPixels / (float) windowHeightPixels;
      float num2 = windowWidthFraction * num1;
      Vector3d pos1 = new Vector3d(-1.0, -1.0, (double) z);
      Vertex vertex1 = new Vertex(ref pos1);
      Vector3d pos2 = new Vector3d((double) windowWidthFraction - 1.0, -1.0, (double) z);
      Vertex vertex2 = new Vertex(ref pos2);
      Vector3d pos3 = new Vector3d(-1.0, (double) num2 - 1.0, (double) z);
      Vertex vertex3 = new Vertex(ref pos3);
      Vector3d pos4 = new Vector3d((double) windowWidthFraction - 1.0, (double) num2 - 1.0, (double) z);
      Vertex vertex4 = new Vertex(ref pos4);
      shell.VertexCache.Add(pos1, vertex1);
      shell.VertexCache.Add(pos2, vertex2);
      shell.VertexCache.Add(pos3, vertex3);
      shell.VertexCache.Add(pos4, vertex4);
      Vector3d normal = new Vector3d(0.0, -1.0, 0.0);
      face.SurfaceDerivatives.Add(vertex1, new SurfaceDerivatives(normal, new Vector2d(0.0, 1.0)));
      face.SurfaceDerivatives.Add(vertex2, new SurfaceDerivatives(normal, new Vector2d(1.0, 1.0)));
      face.SurfaceDerivatives.Add(vertex3, new SurfaceDerivatives(normal, new Vector2d(0.0, 0.0)));
      face.SurfaceDerivatives.Add(vertex4, new SurfaceDerivatives(normal, new Vector2d(1.0, 0.0)));
      face.Mesh.Add(new Triangle(face, vertex1, vertex3, vertex2));
      face.Mesh.Add(new Triangle(face, vertex2, vertex4, vertex3));
      shell.Faces.Add(face);
      return shell;
    }

    public void Resized(int windowWidthPixels, int windowHeightPixels)
    {
      this.Shells.Clear();
      this.Shells.Add(this.CreateShell(this._windowWidthFraction, windowWidthPixels, windowHeightPixels));
    }
  }
}
