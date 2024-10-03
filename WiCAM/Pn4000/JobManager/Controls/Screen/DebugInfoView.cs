// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.DebugInfoView
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks;

namespace WiCAM.Pn4000.ScreenD3D.Controls
{
  public partial class DebugInfoView : System.Windows.Controls.UserControl, IDisposable, INotifyPropertyChanged, IComponentConnector
  {
    private Model _selectedModel;
    private string _selectedType;
    private DebugInfoView.SearchTypes _clickType = DebugInfoView.SearchTypes.Face;
    private DebugInfoView.SearchTypes _searchType = DebugInfoView.SearchTypes.Face;
    private string _selectedObjectBb;
    private Brush _backgroundSearch = (Brush) Brushes.White;
   

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    public ScreenD3D11 ScreenD3D { get; set; }

    public PropertyGrid PropertyGrid { get; private set; }

    public new bool IsVisible { get; set; }

    public string SelectedType
    {
      get => this._selectedType;
      set
      {
        if (!(this._selectedType != value))
          return;
        this._selectedType = value;
        this.OnPropertyChanged(nameof (SelectedType));
      }
    }

    public DebugInfoView.SearchTypes ClickType
    {
      get => this._clickType;
      set
      {
        if (this._clickType == value)
          return;
        this._clickType = value;
        this.OnPropertyChanged(nameof (ClickType));
      }
    }

    public DebugInfoView.SearchTypes SearchType
    {
      get => this._searchType;
      set
      {
        if (this._searchType == value)
          return;
        this._searchType = value;
        this.OnPropertyChanged(nameof (SearchType));
      }
    }

    public IEnumerable<DebugInfoView.SearchTypes> AllSearchTypes => System.Enum.GetValues(typeof (DebugInfoView.SearchTypes)).Cast<DebugInfoView.SearchTypes>();

    public string SelectedObjectBb
    {
      get => this._selectedObjectBb;
      set
      {
        if (!(this._selectedObjectBb != value))
          return;
        this._selectedObjectBb = value;
        this.OnPropertyChanged(nameof (SelectedObjectBb));
      }
    }

    public bool MoveScreenWhenSearching { get; set; } = true;

    public Brush BackgroundSearch
    {
      get => this._backgroundSearch;
      set
      {
        if (this._backgroundSearch == value)
          return;
        this._backgroundSearch = value;
        this.OnPropertyChanged(nameof (BackgroundSearch));
      }
    }

    public DebugInfoView()
    {
      this.InitializeComponent();
      PropertyGrid propertyGrid = new PropertyGrid();
      propertyGrid.Dock = DockStyle.Fill;
      propertyGrid.PropertySort = PropertySort.NoSort;
      this.PropertyGrid = propertyGrid;
    }

    public void Dispose()
    {
      this.WindowsFormsHost?.Dispose();
      this.PropertyGrid = (PropertyGrid) null;
    }

    private void TxtSearch_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key != Key.Return)
        return;
      if (this.SearchObject(this.txtSearch.Text))
        this.BackgroundSearch = (Brush) Brushes.Aquamarine;
      else
        this.BackgroundSearch = (Brush) Brushes.MistyRose;
    }

    private bool SearchObject(string searchValue)
    {
      int result;
      if (this._selectedModel != null && int.TryParse(searchValue, out result))
      {
        switch (this.SearchType)
        {
          case DebugInfoView.SearchTypes.Triangle:
            Triangle triangleById = this._selectedModel.GetTriangleById(result);
            if (triangleById != null)
            {
              this.ZoomToVertices(new List<Vertex>()
              {
                triangleById.V0,
                triangleById.V1,
                triangleById.V2
              });
              this.SelectObject((object) triangleById);
              return true;
            }
            break;
          case DebugInfoView.SearchTypes.Face:
            Face faceById = this._selectedModel.GetFaceById(result);
            if (faceById != null)
            {
              this.ZoomToVertices(faceById.BoundaryEdgesCcw.SelectMany<FaceHalfEdge, Vertex>((Func<FaceHalfEdge, IEnumerable<Vertex>>) (x => (IEnumerable<Vertex>) x.Vertices)).ToList<Vertex>());
              this.SelectObject((object) faceById);
              return true;
            }
            break;
          case DebugInfoView.SearchTypes.FaceGroup:
            FaceGroup faceGroupById = this._selectedModel.GetFaceGroupById(result);
            if (faceGroupById != null)
            {
              this.ZoomToVertices(faceGroupById.GetAllFaces().SelectMany<Face, Vertex>((Func<Face, IEnumerable<Vertex>>) (f => f.BoundaryEdgesCcw.SelectMany<FaceHalfEdge, Vertex>((Func<FaceHalfEdge, IEnumerable<Vertex>>) (x => (IEnumerable<Vertex>) x.Vertices)))).ToList<Vertex>());
              this.SelectObject((object) faceGroupById);
              return true;
            }
            break;
          case DebugInfoView.SearchTypes.FaceHalfEdge:
            FaceHalfEdge faceHalfEdgeById = this._selectedModel.GetFaceHalfEdgeById(result);
            if (faceHalfEdgeById != null)
            {
              this.ZoomToVertices(faceHalfEdgeById.Vertices);
              this.SelectObject((object) faceHalfEdgeById);
              return true;
            }
            break;
          case DebugInfoView.SearchTypes.Macro:
            Macro macroById = this._selectedModel.GetMacroById(result);
            if (macroById != null)
            {
              this.ZoomToVertices(macroById.Faces.SelectMany<Face, Vertex>((Func<Face, IEnumerable<Vertex>>) (f => f.BoundaryEdgesCcw.SelectMany<FaceHalfEdge, Vertex>((Func<FaceHalfEdge, IEnumerable<Vertex>>) (x => (IEnumerable<Vertex>) x.Vertices)))).ToList<Vertex>());
              this.SelectObject((object) macroById);
              return true;
            }
            break;
        }
      }
      return false;
    }

    private void ZoomToVertices(List<Vertex> points)
    {
      double num1 = double.PositiveInfinity;
      double num2 = double.PositiveInfinity;
      double num3 = double.PositiveInfinity;
      double num4 = double.NegativeInfinity;
      double num5 = double.NegativeInfinity;
      double num6 = double.NegativeInfinity;
      if (points == null || points.Count <= 0 || points.First<Vertex>().Faces.Count <= 0)
        return;
      Matrix4d? worldMatrix = points.First<Vertex>().Faces.First<Face>().Shell?.WorldMatrix;
      if (!worldMatrix.HasValue)
        return;
      Matrix4d valueOrDefault = worldMatrix.GetValueOrDefault();
      foreach (Vertex point in points)
      {
        Vector3d vector3d = valueOrDefault.Transform(point.Pos);
        num1 = Math.Min(num1, vector3d.X);
        num2 = Math.Min(num2, vector3d.Y);
        num3 = Math.Min(num3, vector3d.Z);
        num4 = Math.Max(num4, vector3d.X);
        num5 = Math.Max(num5, vector3d.Y);
        num6 = Math.Max(num6, vector3d.Z);
      }
      Vector3d bbMin = new Vector3d(num1, num2, num3);
      Vector3d bbMax = new Vector3d(num4, num5, num6);
      this.SelectedObjectBb = "BB World:" + Environment.NewLine + "Min " + bbMin.ToString("F1") + Environment.NewLine + "Max " + bbMax.ToString("F2");
      if (!this.MoveScreenWhenSearching)
        return;
      this.ScreenD3D.ZoomExtend(true, bbMin, bbMax, 10.0, true, (Action<RenderTaskResult>) null);
    }

    public void SelectTriangle(Triangle tri)
    {
      if (!this.IsVisible)
        return;
      this.PropertyGrid.SelectedObject = (object) tri;
      this.SelectedType = tri?.GetType()?.Name;
      this._selectedModel = tri?.Face?.Shell?.Model;
      this.SelectedObjectBb = (string) null;
      this.BackgroundSearch = (Brush) Brushes.White;
      switch (this.ClickType)
      {
        case DebugInfoView.SearchTypes.Triangle:
          this.SelectObject((object) tri);
          break;
        case DebugInfoView.SearchTypes.Face:
          this.SelectObject((object) tri?.Face);
          break;
        case DebugInfoView.SearchTypes.FaceGroup:
          this.SelectObject((object) tri?.Face?.FaceGroup);
          break;
        case DebugInfoView.SearchTypes.FaceHalfEdge:
          this.SelectObject((object) tri?.Face);
          break;
        case DebugInfoView.SearchTypes.Macro:
          this.SelectObject((object) tri?.Face?.Macro);
          break;
      }
    }

    private void SelectObject(object obj)
    {
      if (!this.IsVisible)
        return;
      this.PropertyGrid.SelectedObject = obj;
      this.SelectedType = obj?.GetType()?.Name;
    }

    private void DebugInfoView_OnLoaded(object sender, RoutedEventArgs e)
    {
      System.Windows.Forms.Panel panel1 = new System.Windows.Forms.Panel();
      panel1.Dock = DockStyle.Fill;
      System.Windows.Forms.Panel panel2 = panel1;
      panel2.Controls.Add((System.Windows.Forms.Control) this.PropertyGrid);
      this.WindowsFormsHost.Child = (System.Windows.Forms.Control) panel2;
    }

   

        public enum SearchTypes
    {
      Triangle,
      Face,
      FaceGroup,
      FaceHalfEdge,
      Macro,
    }
  }
}
