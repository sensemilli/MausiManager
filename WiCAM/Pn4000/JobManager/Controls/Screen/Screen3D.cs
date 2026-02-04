// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.Screen3D
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryGenerators;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;
using WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using WiCAM.Pn4000.ScreenD3D.Renderer.Nodes;
using WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.ScreenD3D.Controls
{
  public partial class Screen3D : UserControl, IScreenD3D11, IDisposable, IScreenshotScreen, IComponentConnector
  {
    private IConfigProvider _configProvider;
    private int _surfWidth;
    private int _surfHeight;
    private Model _clickSphere;
    private MouseRotationMode _rotationMode;
    private IInteractionsMode _interactionMode;


        public Screen3D _Screen3D { get; private set; }

        public void Init(Screen3D screen)
        {

            _Screen3D = screen;
        }

        public ScreenD3D11 ScreenD3D { get;  set; }

    public bool MouseWheelInverted { get; set; }

    public Vector3d RotationPoint { get; set; }

    public Model ClickSphere => this._clickSphere;

    public IInteractionsMode InteractionMode
    {
      get => this._interactionMode;
      set
      {
        IInteractionsMode interactionMode = this._interactionMode;
        this._interactionMode?.Deactivate();
        this._interactionMode = value;
        this._interactionMode?.Activate();
        Action<IInteractionsMode, IInteractionsMode> actionModeChanged = this.InterActionModeChanged;
        if (actionModeChanged == null)
          return;
        actionModeChanged(interactionMode, this._interactionMode);
      }
    }

    public KeyEventHandler ExternalKeyDown { get; set; }

    public EventHandler<Triangle> MouseEnterTriangle { get; set; }

    public EventHandler<Triangle> MouseLeaveTriangle { get; set; }

    public event Action<NavigateInteractionMode, Triangle, MouseButtonEventArgs, Vector3d> TriangleSelected;

    public event Action<IInteractionsMode, IInteractionsMode> InterActionModeChanged;

    public Screen3D(ScreenD3D11 screenD3D, IConfigProvider configProvider)
    {
      this.InitializeComponent();
      this._configProvider = configProvider;
      Sphere sphere = new Sphere(this.RotationPoint, 0.5, 25, 25, new WiCAM.Pn4000.BendModel.Base.Color(0.0f, 1f, 0.0f, 1f));
      sphere.Enabled = false;
      sphere.ModelType = ModelType.System;
      this._clickSphere = (Model) sphere;
      this._rotationMode = MouseRotationMode.Free;
      this.DebugInfo.ScreenD3D = this.ScreenD3D;
      this.ScreenD3D = screenD3D;
    }

    public Screen3D()
    {
      this.InitializeComponent();
      Sphere sphere = new Sphere(this.RotationPoint, 0.5, 25, 25, new WiCAM.Pn4000.BendModel.Base.Color(0.0f, 1f, 0.0f, 1f));
      sphere.Enabled = false;
      sphere.ModelType = ModelType.System;
      this._clickSphere = (Model) sphere;
      this._rotationMode = MouseRotationMode.Free;
    }

    public void NavigationKeyDown(object sender, KeyEventArgs e)
    {
            /*
      if (e.Handled || !(this.Navigate3DView?.DataContext is Navigate3DViewModel dataContext))
        return;
      e.Handled = true;
      switch (e.Key)
      {
        case Key.NumPad4:
          dataContext.LeftClick();
          break;
        case Key.NumPad5:
          dataContext.FrontClick();
          break;
        case Key.NumPad6:
          dataContext.RightClick();
          break;
        case Key.NumPad8:
          dataContext.TopClick();
          break;
        default:
          e.Handled = false;
          break;
      }
            */
    }

    public void SetConfigProviderAndApplySettings(IConfigProvider configProvider)
    {
      this._configProvider = configProvider;
      if (this.Navigate3DView?.DataContext is Navigate3DViewModel dataContext)
        dataContext.ConfigProvider = configProvider;
      this.ApplyConfig();
    }

    private void ApplyConfig()
    {
      GeneralUserSettingsConfig userSettingsConfig = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
      this.MouseWheelInverted = userSettingsConfig.P3D_InvertMouseWheel;
      if (this.ScreenD3D == null)
        return;
      this.ScreenD3D.ProjectionType = userSettingsConfig.P3D_PerspectiveProjection ? ProjectionType.Perspective : ProjectionType.Isometric;
      this.ScreenD3D.Renderer.RenderData.ShadowMode = !userSettingsConfig.GraphicShowShadow ? WiCAM.Pn4000.ScreenD3D.Renderer.RenderData.Shadows.None : WiCAM.Pn4000.ScreenD3D.Renderer.RenderData.Shadows.Soft;
      if (userSettingsConfig.GraphicMetallicLook)
        this.ScreenD3D.Renderer.RenderData.LightingMode = WiCAM.Pn4000.ScreenD3D.Renderer.RenderData.LightMode.Full;
      else
        this.ScreenD3D.Renderer.RenderData.LightingMode = WiCAM.Pn4000.ScreenD3D.Renderer.RenderData.LightMode.Simplified;
    }

    private void Screen3D_OnLoaded(object sender, RoutedEventArgs e)
    {
      if (Application.Current.MainWindow == null || this.ScreenD3D != null)
        return;
      Window window = Window.GetWindow((DependencyObject) this);
      if (window != null)
        window.Closed += (EventHandler) ((o, args) => this.Dispose());
      this.InteractionMode = (IInteractionsMode) new NavigateInteractionMode(this);
      this.ImageHost.SizeChanged += new SizeChangedEventHandler(this.ImageHostSizeChanged);
      this.ExternalKeyDown += new KeyEventHandler(this.KeyboardClick);
      double a1 = 100.0;
      double a2 = 100.0;
      if (!double.IsNaN(this.Width))
        a1 = this.Width;
      if (!double.IsNaN(this.Height))
        a2 = this.Height;
      this.ScreenD3D = new ScreenD3D11(new WindowInteropHelper(Application.Current.MainWindow).Handle, this.InteropImage, (int) Math.Ceiling(a1), (int) Math.Ceiling(a2), this);
      this.DebugInfo.ScreenD3D = this.ScreenD3D;
     this.Navigate3DView.DataContext = (object) new Navigate3DViewModel(this, this._configProvider);
     this.ScreenInfoView.DataContext = (object) new ScreenInfoViewModel();
      double num = (double) this.ScreenD3D.Renderer.Zoom * 0.01;
      if (this._clickSphere != null)
        this._clickSphere.Shells.First<Shell>().Transform = Matrix4d.Scale(num, num, num);
      this.ScreenD3D.AddModel(this._clickSphere);
    }

    public void SetScreenInfoText(List<string> data)
    {
      if (!(this.ScreenInfoView.DataContext is ScreenInfoViewModel dataContext))
        return;
      dataContext.ControlVisibility = Visibility.Visible;
      dataContext.SetText(data);
    }

    public void HideScreenInfoText() => (this.ScreenInfoView.DataContext as ScreenInfoViewModel).ControlVisibility = Visibility.Collapsed;

    public void IgnoreMouseMove(bool ignore) => this.InteractionMode?.IgnoreMouseMove(ignore);

    private void ImageHostSizeChanged(object sender, SizeChangedEventArgs e)
    {
      double num = 1.0;
      if (PresentationSource.FromVisual((Visual) Application.Current.MainWindow).CompositionTarget is HwndTarget compositionTarget)
        num = compositionTarget.TransformToDevice.M11;
      this._surfWidth = e.NewSize.Width < 1.0 ? 1 : (int) Math.Ceiling(e.NewSize.Width * num);
      this._surfHeight = e.NewSize.Height < 1.0 ? 1 : (int) Math.Ceiling(e.NewSize.Height * num);
      this.ScreenD3D?.Resize(this._surfWidth, this._surfHeight);
    }

    public void ShowClickSphere(Vector3d point, int specialColor = 0)
    {
      double num = (double) this.ScreenD3D.Renderer.Zoom * 0.008;
      this._clickSphere.Shells.First<Shell>().Transform = Matrix4d.Scale(num, num, num);
      this._clickSphere.Transform = Matrix4d.Translation(point);
      if (!this._clickSphere.Enabled)
        this._clickSphere.Enabled = true;
      switch (specialColor)
      {
        case 0:
          this._clickSphere.UnHighLightModel();
          break;
        case 1:
          this._clickSphere.HighLightModel(new WiCAM.Pn4000.BendModel.Base.Color(0.0f, 0.5f, 1f, 1f));
          break;
      }
      this.ScreenD3D.UpdateModelAppearance(this._clickSphere, false);
      this.ScreenD3D.UpdateModelTransform(this._clickSphere, true);
    }

    public void ShowNavigation(bool show) => this.Navigate3DView.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

    public void UpdateViewMode() => ((Navigate3DViewModel) this.Navigate3DView.DataContext).UpdateViewMode();

    private void KeyboardClick(object sender, KeyEventArgs e)
    {
      if (!Keyboard.IsKeyDown(Key.F6))
        return;
      this.ShowDebugView();
    }

    public void ShowDebugView()
    {
      if (this.GridSplitter.Visibility == Visibility.Collapsed && this.DebugInfo.Visibility == Visibility.Collapsed)
      {
        this.MainGrid.ColumnDefinitions[2].Width = new GridLength(1.0, GridUnitType.Star);
       this.GridSplitter.Visibility = Visibility.Visible;
        this.DebugInfo.Visibility = Visibility.Visible;
        this.DebugInfo.IsVisible = true;
      }
      else
      {
       this.MainGrid.ColumnDefinitions[2].Width = new GridLength(0.0, GridUnitType.Pixel);
       this.GridSplitter.Visibility = Visibility.Collapsed;
        this.DebugInfo.Visibility = Visibility.Collapsed;
        this.DebugInfo.IsVisible = false;
     }
    }

    public void InitWait(bool silent)
    {
      if (silent)
        return;
      Mouse.OverrideCursor = Cursors.Wait;
    }

    public void CloseWait(bool silent)
    {
      if (silent)
        return;
      Mouse.OverrideCursor = (Cursor) null;
    }

    public void ShowFacesAndWires(bool showFaces, bool showWires)
    {
      List<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node> list;
      lock (this.ScreenD3D.Renderer.Root)
        list = this.ScreenD3D.Renderer.Root.Children.Where<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>((Func<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node, bool>) (c => c is ModelNode modelNode && modelNode.Model.ModelType != ModelType.System && modelNode.Model.ModelType != ModelType.Static)).ToList<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>();
      foreach (ModelNode modelNode in list)
      {
        Screen3D.SetFaceTransparency(modelNode.Model, showFaces);
        Screen3D.SetEdgeTransparency(modelNode.Model, showWires);
      }
      this.ScreenD3D.UpdateAllModelAppearance(true);
    }

    private static void SetFaceTransparency(Model model, bool transparent)
    {
      foreach (Shell shell in model.Shells)
      {
        foreach (Face face1 in shell.Faces)
        {
          Face face2 = face1;
          double r1 = (double) face1.Color.R;
          double g1 = (double) face1.Color.G;
          WiCAM.Pn4000.BendModel.Base.Color color1 = face1.Color;
          double b1 = (double) color1.B;
          double a1 = transparent ? 1.0 : 0.0;
          WiCAM.Pn4000.BendModel.Base.Color color2 = new WiCAM.Pn4000.BendModel.Base.Color((float) r1, (float) g1, (float) b1, (float) a1);
          face2.Color = color2;
          WiCAM.Pn4000.BendModel.Base.Color? highlightColor = face1.HighlightColor;
          if (highlightColor.HasValue)
          {
            Face face3 = face1;
            highlightColor = face1.HighlightColor;
            color1 = highlightColor.Value;
            double r2 = (double) color1.R;
            highlightColor = face1.HighlightColor;
            color1 = highlightColor.Value;
            double g2 = (double) color1.G;
            highlightColor = face1.HighlightColor;
            color1 = highlightColor.Value;
            double b2 = (double) color1.B;
            double a2 = transparent ? 1.0 : 0.0;
            WiCAM.Pn4000.BendModel.Base.Color? nullable = new WiCAM.Pn4000.BendModel.Base.Color?(new WiCAM.Pn4000.BendModel.Base.Color((float) r2, (float) g2, (float) b2, (float) a2));
            face3.HighlightColor = nullable;
          }
        }
      }
      foreach (Model subModel in model.SubModels)
        Screen3D.SetFaceTransparency(subModel, transparent);
      foreach (Model model1 in model.ReferenceModel.Select<ModelInstance, Model>((Func<ModelInstance, Model>) (refModel => refModel.Reference)))
        Screen3D.SetFaceTransparency(model1, transparent);
    }

    private static void SetEdgeTransparency(Model model, bool transparent)
    {
      foreach (Shell shell in model.Shells)
      {
        foreach (Face face in shell.Faces)
        {
          foreach (FaceHalfEdge faceHalfEdge1 in face.BoundaryEdgesCcw)
          {
            FaceHalfEdge faceHalfEdge2 = faceHalfEdge1;
            double r1 = (double) faceHalfEdge1.Color.R;
            double g1 = (double) faceHalfEdge1.Color.G;
            WiCAM.Pn4000.BendModel.Base.Color color1 = faceHalfEdge1.Color;
            double b1 = (double) color1.B;
            double a1 = transparent ? 1.0 : 0.0;
            WiCAM.Pn4000.BendModel.Base.Color color2 = new WiCAM.Pn4000.BendModel.Base.Color((float) r1, (float) g1, (float) b1, (float) a1);
            faceHalfEdge2.Color = color2;
            WiCAM.Pn4000.BendModel.Base.Color? highlightColor = faceHalfEdge1.HighlightColor;
            if (highlightColor.HasValue)
            {
              FaceHalfEdge faceHalfEdge3 = faceHalfEdge1;
              highlightColor = faceHalfEdge1.HighlightColor;
              color1 = highlightColor.Value;
              double r2 = (double) color1.R;
              highlightColor = faceHalfEdge1.HighlightColor;
              color1 = highlightColor.Value;
              double g2 = (double) color1.G;
              highlightColor = faceHalfEdge1.HighlightColor;
              color1 = highlightColor.Value;
              double b2 = (double) color1.B;
              double a2 = transparent ? 1.0 : 0.0;
              WiCAM.Pn4000.BendModel.Base.Color? nullable = new WiCAM.Pn4000.BendModel.Base.Color?(new WiCAM.Pn4000.BendModel.Base.Color((float) r2, (float) g2, (float) b2, (float) a2));
              faceHalfEdge3.HighlightColor = nullable;
            }
          }
          foreach (List<FaceHalfEdge> faceHalfEdgeList in face.HoleEdgesCw)
          {
            foreach (FaceHalfEdge faceHalfEdge4 in faceHalfEdgeList)
            {
              FaceHalfEdge faceHalfEdge5 = faceHalfEdge4;
              double r3 = (double) faceHalfEdge4.Color.R;
              double g3 = (double) faceHalfEdge4.Color.G;
              WiCAM.Pn4000.BendModel.Base.Color color3 = faceHalfEdge4.Color;
              double b3 = (double) color3.B;
              double a3 = transparent ? 1.0 : 0.0;
              WiCAM.Pn4000.BendModel.Base.Color color4 = new WiCAM.Pn4000.BendModel.Base.Color((float) r3, (float) g3, (float) b3, (float) a3);
              faceHalfEdge5.Color = color4;
              WiCAM.Pn4000.BendModel.Base.Color? highlightColor = faceHalfEdge4.HighlightColor;
              if (highlightColor.HasValue)
              {
                FaceHalfEdge faceHalfEdge6 = faceHalfEdge4;
                highlightColor = faceHalfEdge4.HighlightColor;
                color3 = highlightColor.Value;
                double r4 = (double) color3.R;
                highlightColor = faceHalfEdge4.HighlightColor;
                color3 = highlightColor.Value;
                double g4 = (double) color3.G;
                highlightColor = faceHalfEdge4.HighlightColor;
                color3 = highlightColor.Value;
                double b4 = (double) color3.B;
                double a4 = transparent ? 1.0 : 0.0;
                WiCAM.Pn4000.BendModel.Base.Color? nullable = new WiCAM.Pn4000.BendModel.Base.Color?(new WiCAM.Pn4000.BendModel.Base.Color((float) r4, (float) g4, (float) b4, (float) a4));
                faceHalfEdge6.HighlightColor = nullable;
              }
            }
          }
        }
      }
      foreach (Model subModel in model.SubModels)
        Screen3D.SetEdgeTransparency(subModel, transparent);
      foreach (Model model1 in model.ReferenceModel.Select<ModelInstance, Model>((Func<ModelInstance, Model>) (refModel => refModel.Reference)))
        Screen3D.SetEdgeTransparency(model1, transparent);
    }

    public void ChangeWiresType()
    {
    }

    public void SetTransparency(float opacity)
    {
      this.ScreenD3D.Renderer.RenderData.OverallOpacity = (double) opacity;
      this.ScreenD3D.UpdateAllModelAppearance(true);
    }

    public void SetColorsToOriginal(bool value)
    {
      this.ScreenD3D.Renderer.RenderData.UseOriginaColors = value;
      this.ScreenD3D.UpdateAllModelAppearance(true);
    }

    private static void SetTransparencyForModel(Model model, float opacity)
    {
      foreach (Shell shell in model.Shells)
      {
        foreach (Face face1 in shell.Faces)
        {
          Face face2 = face1;
          double r1 = (double) face1.Color.R;
          double g1 = (double) face1.Color.G;
          WiCAM.Pn4000.BendModel.Base.Color color1 = face1.Color;
          double b1 = (double) color1.B;
          double a1 = (double) opacity;
          WiCAM.Pn4000.BendModel.Base.Color color2 = new WiCAM.Pn4000.BendModel.Base.Color((float) r1, (float) g1, (float) b1, (float) a1);
          face2.Color = color2;
          WiCAM.Pn4000.BendModel.Base.Color? highlightColor1;
          if (face1.HighlightColor.HasValue)
          {
            Face face3 = face1;
            highlightColor1 = face1.HighlightColor;
            color1 = highlightColor1.Value;
            double r2 = (double) color1.R;
            highlightColor1 = face1.HighlightColor;
            color1 = highlightColor1.Value;
            double g2 = (double) color1.G;
            highlightColor1 = face1.HighlightColor;
            color1 = highlightColor1.Value;
            double b2 = (double) color1.B;
            double a2 = (double) opacity;
            WiCAM.Pn4000.BendModel.Base.Color? nullable = new WiCAM.Pn4000.BendModel.Base.Color?(new WiCAM.Pn4000.BendModel.Base.Color((float) r2, (float) g2, (float) b2, (float) a2));
            face3.HighlightColor = nullable;
          }
          foreach (FaceHalfEdge faceHalfEdge1 in face1.BoundaryEdgesCcw)
          {
            FaceHalfEdge faceHalfEdge2 = faceHalfEdge1;
            color1 = faceHalfEdge1.Color;
            double r3 = (double) color1.R;
            color1 = faceHalfEdge1.Color;
            double g3 = (double) color1.G;
            color1 = faceHalfEdge1.Color;
            double b3 = (double) color1.B;
            double a3 = (double) opacity;
            WiCAM.Pn4000.BendModel.Base.Color color3 = new WiCAM.Pn4000.BendModel.Base.Color((float) r3, (float) g3, (float) b3, (float) a3);
            faceHalfEdge2.Color = color3;
            highlightColor1 = faceHalfEdge1.HighlightColor;
            if (highlightColor1.HasValue)
            {
              FaceHalfEdge faceHalfEdge3 = faceHalfEdge1;
              highlightColor1 = faceHalfEdge1.HighlightColor;
              color1 = highlightColor1.Value;
              double r4 = (double) color1.R;
              highlightColor1 = faceHalfEdge1.HighlightColor;
              color1 = highlightColor1.Value;
              double g4 = (double) color1.G;
              highlightColor1 = faceHalfEdge1.HighlightColor;
              color1 = highlightColor1.Value;
              double b4 = (double) color1.B;
              double a4 = (double) opacity;
              WiCAM.Pn4000.BendModel.Base.Color? nullable = new WiCAM.Pn4000.BendModel.Base.Color?(new WiCAM.Pn4000.BendModel.Base.Color((float) r4, (float) g4, (float) b4, (float) a4));
              faceHalfEdge3.HighlightColor = nullable;
            }
          }
          foreach (List<FaceHalfEdge> faceHalfEdgeList in face1.HoleEdgesCw)
          {
            foreach (FaceHalfEdge faceHalfEdge4 in faceHalfEdgeList)
            {
              FaceHalfEdge faceHalfEdge5 = faceHalfEdge4;
              double r5 = (double) faceHalfEdge4.Color.R;
              double g5 = (double) faceHalfEdge4.Color.G;
              WiCAM.Pn4000.BendModel.Base.Color color4 = faceHalfEdge4.Color;
              double b5 = (double) color4.B;
              double a5 = (double) opacity;
              WiCAM.Pn4000.BendModel.Base.Color color5 = new WiCAM.Pn4000.BendModel.Base.Color((float) r5, (float) g5, (float) b5, (float) a5);
              faceHalfEdge5.Color = color5;
              if (faceHalfEdge4.HighlightColor.HasValue)
              {
                FaceHalfEdge faceHalfEdge6 = faceHalfEdge4;
                WiCAM.Pn4000.BendModel.Base.Color? highlightColor2 = faceHalfEdge4.HighlightColor;
                color4 = highlightColor2.Value;
                double r6 = (double) color4.R;
                highlightColor2 = faceHalfEdge4.HighlightColor;
                color4 = highlightColor2.Value;
                double g6 = (double) color4.G;
                highlightColor2 = faceHalfEdge4.HighlightColor;
                color4 = highlightColor2.Value;
                double b6 = (double) color4.B;
                double a6 = (double) opacity;
                WiCAM.Pn4000.BendModel.Base.Color? nullable = new WiCAM.Pn4000.BendModel.Base.Color?(new WiCAM.Pn4000.BendModel.Base.Color((float) r6, (float) g6, (float) b6, (float) a6));
                faceHalfEdge6.HighlightColor = nullable;
              }
            }
          }
        }
      }
      foreach (Model subModel in model.SubModels)
        Screen3D.SetTransparencyForModel(subModel, opacity);
      foreach (Model model1 in model.ReferenceModel.Select<ModelInstance, Model>((Func<ModelInstance, Model>) (refModel => refModel.Reference)))
        Screen3D.SetTransparencyForModel(model1, opacity);
    }

    public void SetMouseRotationMode(MouseRotationMode rotationMode) => this._rotationMode = rotationMode;

    public void SetBackground(System.Windows.Media.Color color1) => this.Background = (Brush) new SolidColorBrush(color1);

    public void SetBackground(System.Windows.Media.Color color1, System.Windows.Media.Color color2)
    {
      LinearGradientBrush linearGradientBrush = new LinearGradientBrush()
      {
        StartPoint = new System.Windows.Point(0.5, 0.0),
        EndPoint = new System.Windows.Point(0.5, 1.0)
      };
      GradientStop gradientStop1 = new GradientStop()
      {
        Color = color1,
        Offset = 0.0
      };
      linearGradientBrush.GradientStops.Add(gradientStop1);
      GradientStop gradientStop2 = new GradientStop()
      {
        Color = color2,
        Offset = 1.0
      };
      linearGradientBrush.GradientStops.Add(gradientStop2);
      this.Background = (Brush) linearGradientBrush;
    }

    public void Dispose()
    {
      this._clickSphere = (Model) null;
      this.ScreenD3D?.Dispose();
      this.ScreenD3D = (ScreenD3D11) null;
      this.DebugInfo?.Dispose();
    }

    public Vector3d GetCamWorldPos(float mx, float my, ref SharpDX.Matrix transformation)
    {
      float x = (float) (2.0 * (double) mx / (double) this.ScreenD3D.Renderer.Width - 1.0);
      float y = (float) (1.0 - 2.0 * (double) my / (double) this.ScreenD3D.Renderer.Height);
      if (this.ScreenD3D.Renderer.ProjectionType == ProjectionType.Isometric)
      {
        SharpDX.Matrix transform = transformation * this.ScreenD3D.Renderer.View * this.ScreenD3D.Renderer.Projection;
        transform.Invert();
        Vector3 result = new Vector3(x, y, 0.0f);
        Vector3.Transform(ref result, ref transform, out result);
        return new Vector3d((double) result.X, (double) result.Y, (double) result.Z);
      }
      SharpDX.Matrix transform1 = transformation;
      transform1.Invert();
      Vector3 coordinate = new Vector3(0.0f, this.ScreenD3D.Renderer.Zoom, 0.0f);
      Vector3 result1;
      Vector3.TransformCoordinate(ref coordinate, ref transform1, out result1);
      return new Vector3d((double) result1.X, (double) result1.Y, (double) result1.Z);
    }

    public Vector2d GetScreenCoordinates(Vector3d p)
    {
      Vector3 coordinate = new Vector3((float) p.X, (float) p.Y, (float) p.Z);
      SharpDX.Matrix transform;
      lock (this.ScreenD3D.Renderer)
        transform = this.ScreenD3D.Renderer.Root.Transform.Value * this.ScreenD3D.Renderer.View * this.ScreenD3D.Renderer.Projection;
      Vector3 vector3 = Vector3.TransformCoordinate(coordinate, transform);
      return new Vector2d(0.5 * ((double) vector3.X + 1.0) * (double) this.ScreenD3D.Renderer.Width, (1.0 - 0.5 * ((double) vector3.Y + 1.0)) * (double) this.ScreenD3D.Renderer.Height);
    }

    public void PrintScreen(
      Model model,
      string targetPath,
      Matrix4d transform,
      int border = -1,
      int width = -1,
      int height = -1)
    {
      lock (this)
      {
        CameraState cameraState = this.ScreenD3D.Renderer.ExportCameraState();
        ProjectionType projectionType = this.ScreenD3D.ProjectionType;
        this.ScreenD3D.UpdateProjectionType(ProjectionType.Perspective);
        this.ScreenD3D.RemoveModel((Model) null, false);
        this.ScreenD3D.AddModel(model, false);
        this.ScreenD3D.SetViewDirectionByMatrix4d(transform, false);
        IEnumerable<Model> systemModels = this.HideSystemModels();
        this.ScreenD3D.ZoomExtend(false);
        EventWaitHandle waitHandle = (EventWaitHandle) new ManualResetEvent(false);
        this.ScreenD3D.PrintScreen(targetPath, border, width, height, (Action<RenderTaskResult>) (a => waitHandle.Set()));
        waitHandle.WaitOne();
        this.ScreenD3D.RemoveModel((Model) null, false);
        this.ShowSystemModels(systemModels);
        this.ScreenD3D.UpdateProjectionType(projectionType);
        this.ScreenD3D.Renderer.ImportCameraState(cameraState);
      }
    }

    private IEnumerable<Model> HideSystemModels()
    {
      List<Model> modelList = new List<Model>();
      List<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node> list;
      lock (this.ScreenD3D.Renderer.Root)
        list = this.ScreenD3D.Renderer.Root.Children.ToList<WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node>();
      foreach (WiCAM.Pn4000.ScreenD3D.Renderer.Nodes.Node node in list)
      {
        if (!(node.GetType() != typeof (ModelNode)))
        {
          ModelNode modelNode = (ModelNode) node;
          Model model1 = modelNode.Model;
          if ((model1 != null ? (model1.ModelType != ModelType.System ? 1 : 0) : 1) != 0)
          {
            Model model2 = modelNode.Model;
            if ((model2 != null ? (model2.ModelType != ModelType.Static ? 1 : 0) : 1) != 0)
              continue;
          }
          modelNode.Model.Enabled = false;
          modelList.Add(modelNode.Model);
          this.ScreenD3D.UpdateModelVisibility(modelNode.Model, false);
        }
      }
      return (IEnumerable<Model>) modelList;
    }

    private void ShowSystemModels(IEnumerable<Model> systemModels)
    {
      foreach (Model systemModel in systemModels)
      {
        systemModel.Enabled = true;
        this.ScreenD3D.UpdateModelVisibility(systemModel, false);
      }
    }

    public void RecalculateWpFPointToPixelPoint(System.Windows.Point p, out float X, out float Y)
    {
      System.Windows.Media.Matrix transformToDevice = PresentationSource.FromVisual((Visual) this).CompositionTarget.TransformToDevice;
      X = (float) (p.X * transformToDevice.M11);
      Y = (float) (p.Y * transformToDevice.M22);
    }

    public void ExecuteTriangleSelected(
      NavigateInteractionMode mode,
      Triangle tri,
      MouseButtonEventArgs e,
      Vector3d hitPoint)
    {
      Action<NavigateInteractionMode, Triangle, MouseButtonEventArgs, Vector3d> triangleSelected = this.TriangleSelected;
      if (triangleSelected == null)
        return;
      triangleSelected(mode, tri, e, hitPoint);
    }

     
    }
}
