// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.Config.DataStructures.GeneralUserSettingsConfig
// Assembly: WiCAM.Pn4000.Config, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
// MVID: 37C17F08-198C-4824-8E0D-AD436E235F93
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.Config.dll

using System.Collections.Generic;
using System.Windows;
using WiCAM.Config;
using WiCAM.Services.ConfigProviders.Contracts.Attributes;
using WiCAM.Services.ConfigProviders.Contracts.DataType;

namespace WiCAM.Pn4000.Config.DataStructures
{
  [ConfigSection("GeneralUserSettings")]
  [ConfigScope(ConfigScope.User)]
  public class GeneralUserSettingsConfig
  {
    public GeneralUserSettingsConfig()
    {
      this.WindowHeight = SystemParameters.PrimaryScreenHeight - 100.0;
      this.WindowWidth = SystemParameters.PrimaryScreenWidth - 100.0;
      this.WindowLeft = 50.0;
      this.WindowTop = 50.0;
      this.WindowState = WindowState.Normal;
    }

    public bool RibbonCompactMode { get; set; }

    public List<int> InfoPaneSizeList { get; set; } = new List<int>();

    public bool SubMenuBigIcons { get; set; } = true;

    public bool SubMenuHideText { get; set; }

    public bool CenterMenuDeactivate { get; set; }

    public int CenterMenuIconSize { get; set; } = 32;

    public int CenterMenuTextLocation { get; set; } = 2;

    public bool CenterMenuMetroStyle { get; set; }

    public int CenterMenuX { get; set; } = 5;

    public int CenterMenuY { get; set; } = 4;

    public bool CenterMenuLocateLikeContext { get; set; }

    public bool CenterMenuAddNextPage { get; set; }

    public int CenterMenuWidth { get; set; } = -1;

    public int CenterMenuHeight { get; set; } = -1;

    public bool LowerCaseAllowed { get; set; } = true;

    public int PartPaneSize { get; set; } = 1;

    public int PartPaneScaleType { get; set; }

    public int PartPaneScale { get; set; } = 1;

    public int PartPaneWidth { get; set; } = 175;

    public bool PartPaneContentOrientation { get; set; }

    public int PartPaneMode { get; set; }

    public int PartPaneWindowX { get; set; } = -1;

    public int PartPaneWindowY { get; set; } = -1;

    public int PartPaneWindowWidth { get; set; } = -1;

    public int PartPaneWindowHeight { get; set; } = -1;

    public int ToolTipDelay { get; set; } = 1000;

    public string MonospaceFont { get; set; } = "Lucida Console";

    public int CalculatorLeft { get; set; } = 150;

    public int CalculatorTop { get; set; } = 150;

    public int CalculatorWidth { get; set; } = (int) byte.MaxValue;

    public int CalculatorHeight { get; set; } = 450;

    public int SubWindowPosX { get; set; } = (int) SystemParameters.PrimaryScreenWidth - 200;

    public int SubWindowPosY { get; set; } = 170;

    public bool ForceFlySubWindow { get; set; }

    public double WindowTop { get; set; }

    public double WindowLeft { get; set; }

    public double WindowHeight { get; set; }

    public double WindowWidth { get; set; }

    public WindowState WindowState { get; set; }

    public bool HideToolTips { get; set; }

    public int GalleryStrategy { get; set; }

    public CfgColor BackgroundColor1 { get; set; } = new CfgColor(0.0f, 0.0f, 0.3f, 1f);

    public CfgColor BackgroundColor2 { get; set; } = new CfgColor(0.0f, 0.0f, 0.6f, 1f);

    public bool PnOneColor { get; set; }

    public CfgColor BackgroundColor1_3d { get; set; } = new CfgColor(0.67f, 0.74f, 0.8f, 1f);

    public CfgColor BackgroundColor2_3d { get; set; } = new CfgColor(0.86f, 0.93f, 1f, 1f);

    public bool PnOneColor_3d { get; set; } = true;

    public bool SpecialBackgroundColorFor3D { get; set; } = true;

    public bool PnDrawThick { get; set; }

    public bool PnMainScreenTooltip { get; set; }

    public bool RibbonSplitbuttonAutoset { get; set; }

    public byte PopupButtonMode { get; set; }

    public byte PopupPosMode { get; set; } = 1;

    public byte PopupRestoreMode { get; set; }

    public int PopupPosX { get; set; } = -1;

    public int PopupPosY { get; set; } = -1;

    public int PopupPosXLM { get; set; } = -1;

    public int PopupPosYLM { get; set; } = -1;

    public string Skin4 { get; set; } = "BalancedGray";

    public bool PopupCloseOutsidePopup { get; set; } = true;

    public bool RibbonIsMinimize { get; set; }

    public bool GadgetCoordinates { get; set; } = true;

    public bool PreviewRecentlyUsed { get; set; } = true;

    public bool InformationPanels { get; set; } = true;

    public bool ActiveDebugWindow { get; set; } = true;

    public bool RBFBHelpButtonVisibility { get; set; } = true;

    public bool RBFBUserDataVisibility { get; set; } = true;

    public bool RBFBQuickLaunchVisibility { get; set; } = true;

    public bool RBFBErrorButtonVisibility { get; set; } = true;

    public bool MultiTask { get; set; }

    public bool MultiTaskRefreshDefault { get; set; } = true;

    public bool UseOnlineHelp { get; set; }

    public bool SavePool { get; set; }

    public bool InitialMultiTask { get; set; }

    public bool InitialMultiTaskRefreshDefault { get; set; }

    public int SumMenuWindowMode { get; set; } = 1;

    public double PropertyPanelWidth { get; set; } = 200.0;

    public bool P3D_PerspectiveProjection { get; set; }

    public bool P3D_InvertMouseWheel { get; set; }

    public CfgColor PreviewColor3D1 { get; set; } = new CfgColor(0.78f, 0.78f, 0.78f, 1f);

    public CfgColor PreviewColor3D2 { get; set; } = new CfgColor(0.93f, 0.93f, 0.93f, 1f);

    public int UnfoldTime { get; set; }

    public bool HideLeftTray { get; set; }

    public bool HideRightTray { get; set; }

    public bool HideTopTray { get; set; }

    public bool RibbonMode { get; set; }

    public bool GraphicShowShadow { get; set; } = true;

    public bool GraphicMetallicLook { get; set; } = true;
  }
}
