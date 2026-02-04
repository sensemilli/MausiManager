// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.Navigate3DViewModel
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX;
using System;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks;
using WiCAM.Services.ConfigProviders.Contracts;
using RelayCommand = WiCAM.Pn4000.ScreenD3D.Controls.Base.RelayCommand;

namespace WiCAM.Pn4000.ScreenD3D.Controls
{
  public class Navigate3DViewModel : JobManager.ViewModelBase
  {
    private ICommand _clickFreeRot;
    private ICommand _clickXRot;
    private ICommand _clickYRot;
    private ICommand _clickZRot;
    private ICommand _clickSolid;
    private ICommand _click75;
    private ICommand _click50;
    private ICommand _click25;
    private ICommand _clickOriginalColors;
    private ICommand _clickAnalysisColors;
    private ICommand _showAll;
    private ICommand _showWires;
    private ICommand _showFaces;
    private ICommand _showShadows;
    private ICommand _showMetallicLook;
    private ICommand _selectWiresMode;
    private ICommand _selectViewMode;
    private ICommand _clickRight;
    private ICommand _clickLeft;
    private ICommand _clickFront;
    private ICommand _clickBack;
    private ICommand _clickTop;
    private ICommand _clickBottom;
    private ICommand _clickTrb;
    private ICommand _clickTrf;
    private ICommand _clickTlf;
    private ICommand _clickTlb;
    private ICommand _clickExtend;
    private ICommand _openContextMenu;
    private double _opacity = 0.6;
        public static Navigate3DViewModel NaviModel;
        private Screen3D _screen3D;
    public IConfigProvider ConfigProvider;
    private Visibility _isometricVisible;
    private Visibility _percpectiveVisible;
    private bool _freeRotation;
    private bool _xRotation;
    private bool _yRotation;
    private bool _zRotation;

    public double Opacity
    {
      get => this._opacity;
      set
      {
        this._opacity = value;
        this.NotifyPropertyChanged(nameof (Opacity));
      }
    }

    public Visibility IsometricVisible
    {
      get => this._isometricVisible;
      set
      {
        this._isometricVisible = value;
        this.NotifyPropertyChanged(nameof (IsometricVisible));
      }
    }

    public Visibility PerspectiveVisible
    {
      get => this._percpectiveVisible;
      set
      {
        this._percpectiveVisible = value;
        this.NotifyPropertyChanged(nameof (PerspectiveVisible));
      }
    }

    public bool FreeRotation
    {
      get => this._freeRotation;
      set
      {
        this._freeRotation = value;
        this.NotifyPropertyChanged(nameof (FreeRotation));
      }
    }

    public bool XRotation
    {
      get => this._xRotation;
      set
      {
        this._xRotation = value;
        this.NotifyPropertyChanged(nameof (XRotation));
      }
    }

    public bool YRotation
    {
      get => this._yRotation;
      set
      {
        this._yRotation = value;
        this.NotifyPropertyChanged(nameof (YRotation));
      }
    }

    public bool ZRotation
    {
      get => this._zRotation;
      set
      {
        this._zRotation = value;
        this.NotifyPropertyChanged(nameof (ZRotation));
      }
    }

    public bool ShadowsVisible => this._screen3D.ScreenD3D.Renderer.RenderData.ShadowMode == RenderData.Shadows.Soft;

    public bool MetallicLookVisible => this._screen3D.ScreenD3D.Renderer.RenderData.LightingMode == RenderData.LightMode.Full;

    public ICommand ClickRight => this._clickRight ?? (this._clickRight = (ICommand) new RelayCommand((Action<object>) (param => this.RightClick())));

    public ICommand ClickLeft => this._clickLeft ?? (this._clickLeft = (ICommand) new RelayCommand((Action<object>) (param => this.LeftClick())));

    public ICommand ClickFront => this._clickFront ?? (this._clickFront = (ICommand) new RelayCommand((Action<object>) (param => this.FrontClick())));

    public ICommand ClickBack => this._clickBack ?? (this._clickBack = (ICommand) new RelayCommand((Action<object>) (param => this.BackClick())));

    public ICommand ClickTop => this._clickTop ?? (this._clickTop = (ICommand) new RelayCommand((Action<object>) (param => this.TopClick())));

    public ICommand ClickBottom => this._clickBottom ?? (this._clickBottom = (ICommand) new RelayCommand((Action<object>) (param => this.BottomClick())));

    public ICommand ClickTrb => this._clickTrb ?? (this._clickTrb = (ICommand) new RelayCommand((Action<object>) (param => this.TRBClick())));

    public ICommand ClickTrf => this._clickTrf ?? (this._clickTrf = (ICommand) new RelayCommand((Action<object>) (param => this.TRFClick())));

    public ICommand ClickTlf => this._clickTlf ?? (this._clickTlf = (ICommand) new RelayCommand((Action<object>) (param => this.TLFClick())));

    public ICommand ClickTlb => this._clickTlb ?? (this._clickTlb = (ICommand) new RelayCommand((Action<object>) (param => this.TLBClick())));

    public ICommand ClickExtend => this._clickExtend ?? (this._clickRight = (ICommand) new RelayCommand((Action<object>) (param => this.ExtendClick())));

    public ICommand ClickFreeRot => this._clickFreeRot ?? (this._clickFreeRot = (ICommand) new RelayCommand((Action<object>) (param => this.FreeRotClick())));

    public ICommand ClickXRot => this._clickXRot ?? (this._clickXRot = (ICommand) new RelayCommand((Action<object>) (param => this.XRotClick())));

    public ICommand ClickYRot => this._clickYRot ?? (this._clickYRot = (ICommand) new RelayCommand((Action<object>) (param => this.YRotClick())));

    public ICommand ClickZRot => this._clickZRot ?? (this._clickZRot = (ICommand) new RelayCommand((Action<object>) (param => this.ZRotClick())));

    public ICommand ClickSolid => this._clickSolid ?? (this._clickSolid = (ICommand) new RelayCommand((Action<object>) (param => this.SolidClick())));

    public ICommand Click75 => this._click75 ?? (this._click75 = (ICommand) new RelayCommand((Action<object>) (param => this.Click75Command())));

    public ICommand Click50 => this._click50 ?? (this._click50 = (ICommand) new RelayCommand((Action<object>) (param => this.Click50Command())));

    public ICommand Click25 => this._click25 ?? (this._click25 = (ICommand) new RelayCommand((Action<object>) (param => this.Click25Command())));

    public ICommand ClickOriginalColors => this._clickOriginalColors ?? (this._clickOriginalColors = (ICommand) new RelayCommand((Action<object>) (param => this.ClickOriginalColorsCommand())));

    public ICommand ClickAnalysisColors => this._clickAnalysisColors ?? (this._clickAnalysisColors = (ICommand) new RelayCommand((Action<object>) (param => this.ClickAnalysisColorsCommand())));

    public ICommand ShowAll => this._showAll ?? (this._showAll = (ICommand) new RelayCommand((Action<object>) (param => this.ShowAllClick())));

    public ICommand ShowWires => this._showWires ?? (this._showWires = (ICommand) new RelayCommand((Action<object>) (param => this.ShowWiresClick())));

    public ICommand ShowFaces => this._showFaces ?? (this._showFaces = (ICommand) new RelayCommand((Action<object>) (param => this.ShowFacesClick())));

    public ICommand ShowShadows => this._showShadows ?? (this._showShadows = (ICommand) new RelayCommand((Action<object>) (param => this.ShowShadowsClick())));

    public ICommand ShowMetallicLook => this._showMetallicLook ?? (this._showMetallicLook = (ICommand) new RelayCommand((Action<object>) (param => this.ShowMetallicLooksClick())));

    public ICommand SelectWiresMode => this._selectWiresMode ?? (this._selectWiresMode = (ICommand) new RelayCommand((Action<object>) (param => this.SelectWiresModeClick())));

    public ICommand SelectViewMode => this._selectViewMode ?? (this._selectViewMode = (ICommand) new RelayCommand((Action<object>) (param => this.SelectViewModeClick())));

    public ICommand OpenContextMenu => this._openContextMenu ?? (this._openContextMenu = (ICommand) new RelayCommand((Action<object>) (param => this.OpenContextMenuClick((FrameworkElement) param))));

    public Navigate3DViewModel()
    {
    }

    public Navigate3DViewModel(Screen3D screen3D, IConfigProvider configProvider)
    {
            NaviModel = this;
      this._screen3D = screen3D;
      this.ConfigProvider = configProvider;
      this.FreeRotation = true;
      this.XRotation = false;
      this.YRotation = false;
      this.ZRotation = false;
      if (this._screen3D.ScreenD3D.ProjectionType == ProjectionType.Perspective)
      {
        this.IsometricVisible = Visibility.Collapsed;
        this.PerspectiveVisible = Visibility.Visible;
      }
      else
      {
        this.IsometricVisible = Visibility.Visible;
        this.PerspectiveVisible = Visibility.Collapsed;
      }
      this.NotifyPropertyChanged(nameof (ShadowsVisible));
      this.NotifyPropertyChanged(nameof (MetallicLookVisible));
    }
        public Navigate3DViewModel(Screen3D screen3D, ScreenD3D11 screenD3D11)
        {
            NaviModel = this;

            this._screen3D = screen3D;
          //  this.ConfigProvider = configProvider;
            this.FreeRotation = true;
            this.XRotation = false;
            this.YRotation = false;
            this.ZRotation = false;
            if (screenD3D11.ProjectionType == ProjectionType.Perspective)
            {
                this.IsometricVisible = Visibility.Collapsed;
                this.PerspectiveVisible = Visibility.Visible;
            }
            else
            {
                this.IsometricVisible = Visibility.Visible;
                this.PerspectiveVisible = Visibility.Collapsed;
            }
       //     this.NotifyPropertyChanged(nameof(ShadowsVisible));
        //    this.NotifyPropertyChanged(nameof(MetallicLookVisible));
        }


        public void RightClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ScreenD3D.SetViewDirection(Matrix.Identity * Matrix.RotationZ(3.14159274f) * Matrix.RotationZ(-1.57079637f), false, (Action<RenderTaskResult>) (r => this._screen3D.ScreenD3D.ZoomExtend()));
      this._screen3D.IgnoreMouseMove(false);
    }

    public void LeftClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ScreenD3D.SetViewDirection(Matrix.Identity * Matrix.RotationZ(3.14159274f) * Matrix.RotationZ(1.57079637f), false, (Action<RenderTaskResult>) (r => this._screen3D.ScreenD3D.ZoomExtend()));
      this._screen3D.IgnoreMouseMove(false);
    }

    public void FrontClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ScreenD3D.SetViewDirection(Matrix.Identity * Matrix.RotationZ(3.14159274f), false, (Action<RenderTaskResult>) (r => this._screen3D.ScreenD3D.ZoomExtend()));
      this._screen3D.IgnoreMouseMove(false);
    }

    public void BackClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ScreenD3D.SetViewDirection(Matrix.Identity * Matrix.RotationZ(3.14159274f) * Matrix.RotationZ(3.14159274f), false, (Action<RenderTaskResult>) (r => this._screen3D.ScreenD3D.ZoomExtend()));
      this._screen3D.IgnoreMouseMove(false);
    }

    public void TopClick()
    {
      this._screen3D.ScreenD3D.SetViewDirection(Matrix.Identity * Matrix.RotationZ(3.14159274f) * Matrix.RotationX(-1.57079637f), false, (Action<RenderTaskResult>) (r => this._screen3D.ScreenD3D.ZoomExtend()));
      this._screen3D.IgnoreMouseMove(false);
    }

    public void BottomClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ScreenD3D.SetViewDirection(Matrix.Identity * Matrix.RotationZ(3.14159274f) * Matrix.RotationX(1.57079637f), false, (Action<RenderTaskResult>) (r => this._screen3D.ScreenD3D.ZoomExtend()));
      this._screen3D.IgnoreMouseMove(false);
    }

    private void TRBClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ScreenD3D.SetViewDirection(Matrix.Identity * Matrix.RotationZ(0.7853982f) * Matrix.RotationX(-1.04719758f), false, (Action<RenderTaskResult>) (r => this._screen3D.ScreenD3D.ZoomExtend()));
      this._screen3D.IgnoreMouseMove(false);
    }

    private void TRFClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ScreenD3D.SetViewDirection(Matrix.Identity * Matrix.RotationZ(2.3561945f) * Matrix.RotationX(-1.04719758f), false, (Action<RenderTaskResult>) (r => this._screen3D.ScreenD3D.ZoomExtend()));
      this._screen3D.IgnoreMouseMove(false);
    }

    private void TLFClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ScreenD3D.SetViewDirection(Matrix.Identity * Matrix.RotationZ(3.92699075f) * Matrix.RotationX(-1.04719758f), false, (Action<RenderTaskResult>) (r => this._screen3D.ScreenD3D.ZoomExtend()));
      this._screen3D.IgnoreMouseMove(false);
    }

    private void TLBClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ScreenD3D.SetViewDirection(Matrix.Identity * Matrix.RotationZ(-0.7853982f) * Matrix.RotationX(-1.04719758f), false, (Action<RenderTaskResult>) (r => this._screen3D.ScreenD3D.ZoomExtend()));
      this._screen3D.IgnoreMouseMove(false);
    }

    private void ExtendClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ScreenD3D.ZoomExtend();
      this._screen3D.IgnoreMouseMove(false);
    }

    private void FreeRotClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.SetMouseRotationMode(MouseRotationMode.Free);
      this.FreeRotation = true;
      this.XRotation = false;
      this.YRotation = false;
      this.ZRotation = false;
      this._screen3D.IgnoreMouseMove(false);
    }

    private void XRotClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.SetMouseRotationMode(MouseRotationMode.X);
      this.FreeRotation = false;
      this.XRotation = true;
      this.YRotation = false;
      this.ZRotation = false;
      this._screen3D.IgnoreMouseMove(false);
    }

    private void YRotClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.SetMouseRotationMode(MouseRotationMode.Y);
      this.FreeRotation = false;
      this.XRotation = false;
      this.YRotation = true;
      this.ZRotation = false;
      this._screen3D.IgnoreMouseMove(false);
    }

    private void ZRotClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.SetMouseRotationMode(MouseRotationMode.Z);
      this.FreeRotation = false;
      this.XRotation = false;
      this.YRotation = false;
      this.ZRotation = true;
      this._screen3D.IgnoreMouseMove(false);
    }

    private void SolidClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.SetTransparency(1f);
      this._screen3D.IgnoreMouseMove(false);
    }

    private void Click75Command()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.SetTransparency(0.75f);
      this._screen3D.IgnoreMouseMove(false);
    }

    private void Click50Command()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.SetTransparency(0.5f);
      this._screen3D.IgnoreMouseMove(false);
    }

    private void Click25Command()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.SetTransparency(0.25f);
      this._screen3D.IgnoreMouseMove(false);
    }

    private void ClickOriginalColorsCommand()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.SetColorsToOriginal(true);
      this._screen3D.IgnoreMouseMove(false);
    }

    private void ClickAnalysisColorsCommand()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.SetColorsToOriginal(false);
      this._screen3D.IgnoreMouseMove(false);
    }

    private void ShowAllClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ShowFacesAndWires(true, true);
      this._screen3D.IgnoreMouseMove(false);
    }

    private void ShowWiresClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ShowFacesAndWires(false, true);
      this._screen3D.IgnoreMouseMove(false);
    }

    private void ShowFacesClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.ShowFacesAndWires(true, false);
      this._screen3D.IgnoreMouseMove(false);
    }

    private void SelectWiresModeClick()
    {
      this._screen3D.IgnoreMouseMove(true);
      this._screen3D.IgnoreMouseMove(false);
    }

    private void ShowShadowsClick()
    {
      GeneralUserSettingsConfig model = this.ConfigProvider?.InjectOrCreate<GeneralUserSettingsConfig>();
      if (this._screen3D.ScreenD3D.Renderer.RenderData.ShadowMode == RenderData.Shadows.Soft)
      {
        this._screen3D.ScreenD3D.Renderer.RenderData.ShadowMode = RenderData.Shadows.None;
        if (model != null)
          model.GraphicShowShadow = false;
      }
      else
      {
        this._screen3D.ScreenD3D.Renderer.RenderData.ShadowMode = RenderData.Shadows.Soft;
        if (model != null)
          model.GraphicShowShadow = true;
      }
      this.ConfigProvider?.Push<GeneralUserSettingsConfig>(model);
      this.ConfigProvider?.Save<GeneralUserSettingsConfig>();
      this.NotifyPropertyChanged("ShadowsVisible");
      this._screen3D.ScreenD3D.Render(true);
    }

    private void ShowMetallicLooksClick()
    {
      GeneralUserSettingsConfig model = this.ConfigProvider?.InjectOrCreate<GeneralUserSettingsConfig>();
      if (this._screen3D.ScreenD3D.Renderer.RenderData.LightingMode == RenderData.LightMode.Full)
      {
        this._screen3D.ScreenD3D.Renderer.RenderData.LightingMode = RenderData.LightMode.Simplified;
        if (model != null)
          model.GraphicMetallicLook = false;
      }
      else
      {
        this._screen3D.ScreenD3D.Renderer.RenderData.LightingMode = RenderData.LightMode.Full;
        if (model != null)
          model.GraphicMetallicLook = true;
      }
      this.ConfigProvider?.Push<GeneralUserSettingsConfig>(model);
      this.ConfigProvider?.Save<GeneralUserSettingsConfig>();
      this.NotifyPropertyChanged("MetallicLookVisible");
      this._screen3D.ScreenD3D.Render(true);
    }

    public void UpdateViewMode()
    {
      if (this._screen3D.ScreenD3D.ProjectionType == ProjectionType.Perspective)
      {
        this._screen3D.ScreenD3D.ProjectionType = ProjectionType.Perspective;
        this.IsometricVisible = Visibility.Collapsed;
        this.PerspectiveVisible = Visibility.Visible;
      }
      else
      {
        this._screen3D.ScreenD3D.ProjectionType = ProjectionType.Isometric;
        this.IsometricVisible = Visibility.Visible;
        this.PerspectiveVisible = Visibility.Collapsed;
      }
      this._screen3D.ScreenD3D.Render(true);
      this._screen3D.ScreenD3D.ZoomExtend();
    }

    private void SelectViewModeClick()
    {
      if (this._screen3D.ScreenD3D.ProjectionType == ProjectionType.Perspective)
      {
        this._screen3D.ScreenD3D.ProjectionType = ProjectionType.Isometric;
        this.IsometricVisible = Visibility.Visible;
        this.PerspectiveVisible = Visibility.Collapsed;
      }
      else
      {
        this._screen3D.ScreenD3D.ProjectionType = ProjectionType.Perspective;
        this.IsometricVisible = Visibility.Collapsed;
        this.PerspectiveVisible = Visibility.Visible;
      }
      this._screen3D.ScreenD3D.Render(true);
      this._screen3D.ScreenD3D.ZoomExtend();
    }

    private void OpenContextMenuClick(FrameworkElement sender)
    {
      this._screen3D.IgnoreMouseMove(true);
      if (sender?.ContextMenu != null)
        sender.ContextMenu.IsOpen = true;
      this._screen3D.IgnoreMouseMove(false);
    }

   
  }
}
