// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.ScreenInfoViewModel
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System.Collections.Generic;
using System.Text;
using System.Windows;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.ScreenD3D.Controls
{
  public class ScreenInfoViewModel : ViewModelBase
  {
    private Visibility _controlVisibility = Visibility.Collapsed;
    private string _infoText;
    private double _opacity = 0.6;

    public double Opacity
    {
      get => this._opacity;
      set
      {
        this._opacity = value;
        this.NotifyPropertyChanged(nameof (Opacity));
      }
    }

    public Visibility ControlVisibility
    {
      get => this._controlVisibility;
      set
      {
        this._controlVisibility = value;
        this.NotifyPropertyChanged(nameof (ControlVisibility));
      }
    }

    public string InfoText
    {
      get => this._infoText;
      set
      {
        this._infoText = value;
        this.NotifyPropertyChanged(nameof (InfoText));
      }
    }

    public void SetText(List<string> data)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < data.Count - 1; ++index)
        stringBuilder.AppendLine(data[index]);
      if (data.Count > 0)
        stringBuilder.Append(data[data.Count - 1]);
      this.InfoText = stringBuilder.ToString();
    }

    public void MouseEnterCommand() => this.Opacity = 1.0;

    public void MouseLeaveCommand() => this.Opacity = 0.6;
  }
}
