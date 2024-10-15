// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.ScreenInfoView
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

namespace WiCAM.Pn4000.ScreenD3D.Controls
{
  public partial class ScreenInfoView : UserControl, IComponentConnector
  {


    public ScreenInfoView() => this.InitializeComponent();

        public void Connect(int connectionId, object target)
        {
            throw new NotImplementedException();
        }

    

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e) => ((ScreenInfoViewModel) this.DataContext).MouseEnterCommand();

    private void UIElement_OnMouseLeave(object sender, MouseEventArgs e) => ((ScreenInfoViewModel) this.DataContext).MouseLeaveCommand();

    
  }
}
