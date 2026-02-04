// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.Navigate3DView
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using WiCAM.Pn4000.JobManager;

namespace WiCAM.Pn4000.ScreenD3D.Controls
{
  public partial class Navigate3DView : UserControl, IComponentConnector
  {
   

    public Navigate3DView() => this.InitializeComponent();

    private void UIElement_OnMouseEnter(object sender, MouseEventArgs e) => ((ViewModelBase) this.DataContext).MouseEnterCommand();

    private void UIElement_OnMouseLeave(object sender, MouseEventArgs e) => ((ViewModelBase) this.DataContext).MouseLeaveCommand();


   
  }
}
