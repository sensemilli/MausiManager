// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.Base.RelayCommand
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WiCAM.Pn4000.ScreenD3D.Controls.Base
{
  public class RelayCommand : ICommand
  {
    private readonly Action<object> execute;
    private readonly Predicate<object> canExecute;

    public string Name { get; set; }

    public RelayCommand(Action<object> execute)
      : this(string.Empty, execute, (Predicate<object>) null)
    {
    }

    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
      : this(string.Empty, execute, canExecute)
    {
    }

    public RelayCommand(string name, Action<object> execute, Predicate<object> canExecute)
    {
      if (execute == null)
        throw new ArgumentNullException(nameof (execute));
      this.Name = name;
      this.execute = execute;
      this.canExecute = canExecute;
    }

    [DebuggerStepThrough]
    public bool CanExecute(object parameter) => this.canExecute == null || this.canExecute(parameter);

    public event EventHandler CanExecuteChanged
    {
      add => CommandManager.RequerySuggested += value;
      remove => CommandManager.RequerySuggested -= value;
    }

    public void Execute(object parameter) => this.execute(parameter);
  }
}
