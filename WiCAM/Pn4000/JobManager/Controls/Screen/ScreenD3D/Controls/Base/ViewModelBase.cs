// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.Base.ViewModelBase
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.ScreenD3D.Controls.Base
{
  public abstract class ViewModelBase : INotifyPropertyChanged
  {
    [XmlIgnore]
    public string DisplayName;

    [Browsable(false)]
    [XmlIgnore]
    public bool IsNotifyEnabled { get; set; }

    protected ViewModelBase() => this.IsNotifyEnabled = true;

    public event PropertyChangedEventHandler PropertyChanged;
        public void MouseEnterCommand() => Navigate3DViewModel.NaviModel.Opacity = 1.0;

        public void MouseLeaveCommand() => Navigate3DViewModel.NaviModel.Opacity = 0.6;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
      if (!this.IsNotifyEnabled || this.PropertyChanged == null)
        return;
      this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    [Conditional("DEBUG")]
    private void VerifyProperty(string propertyName)
    {
      if (TypeDescriptor.GetProperties((object) this).Find(propertyName, false) != null)
        return;
      string.Format((IFormatProvider) CultureInfo.CurrentCulture, "{0} is not a public property of {1}", (object) propertyName, (object) this.GetType().FullName);
    }
  }
}
