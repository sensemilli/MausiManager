// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.Base.BindingProxy
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System.Windows;

namespace WiCAM.Pn4000.ScreenD3D.Controls.Base
{
  public class BindingProxy : Freezable
  {
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof (Data), typeof (object), typeof (BindingProxy), (PropertyMetadata) new UIPropertyMetadata((PropertyChangedCallback) null));

    protected override Freezable CreateInstanceCore() => (Freezable) new BindingProxy();

    public object Data
    {
      get => this.GetValue(BindingProxy.DataProperty);
      set => this.SetValue(BindingProxy.DataProperty, value);
    }
  }
}
