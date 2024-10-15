// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.Base.TransparentCircleSectionButton
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WiCAM.Pn4000.ScreenD3D.Controls.Base
{
  public class TransparentCircleSectionButton : Button
  {
    public static readonly DependencyProperty ButtonGeometryProperty = DependencyProperty.Register(nameof (ButtonGeometry), typeof (Geometry), typeof (TransparentCircleSectionButton), (PropertyMetadata) new UIPropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty MarkGeometryProperty = DependencyProperty.Register(nameof (MarkGeometry), typeof (Geometry), typeof (TransparentCircleSectionButton), (PropertyMetadata) new UIPropertyMetadata((PropertyChangedCallback) null));

    public Geometry ButtonGeometry
    {
      get => (Geometry) this.GetValue(TransparentCircleSectionButton.ButtonGeometryProperty);
      set => this.SetValue(TransparentCircleSectionButton.ButtonGeometryProperty, (object) value);
    }

    public Geometry MarkGeometry
    {
      get => (Geometry) this.GetValue(TransparentCircleSectionButton.MarkGeometryProperty);
      set => this.SetValue(TransparentCircleSectionButton.MarkGeometryProperty, (object) value);
    }
  }
}
