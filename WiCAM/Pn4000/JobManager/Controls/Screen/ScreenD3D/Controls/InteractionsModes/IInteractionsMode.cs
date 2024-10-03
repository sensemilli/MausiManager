// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes.IInteractionsMode
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System.Windows.Input;

namespace WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes
{
  public interface IInteractionsMode
  {
    void Activate();

    void Deactivate();

    void IgnoreMouseMove(bool value);

    void ImageHostMouseMove(object sender, MouseEventArgs e);

    void ImageHostMouseUp(object sender, MouseButtonEventArgs e);

    void ImageHostMouseDown(object sender, MouseButtonEventArgs e);
  }
}
