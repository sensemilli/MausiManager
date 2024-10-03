// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.Base.RelayCommand`1
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WiCAM.Pn4000.ScreenD3D.Controls.Base
{
  public class RelayCommand<T> : ICommand
  {
    private readonly Action<T> _execute;
    private readonly Predicate<T> _canExecute;

    public RelayCommand(Action<T> execute)
      : this(execute, (Predicate<T>) null)
    {
    }

    public RelayCommand(Action<T> execute, Predicate<T> canExecute)
    {
      this._execute = execute != null ? execute : throw new ArgumentNullException(nameof (execute));
      this._canExecute = canExecute;
    }

    [DebuggerStepThrough]
    public bool CanExecute(object parameter) => this._canExecute == null || this._canExecute((T) parameter);

    public event EventHandler CanExecuteChanged
    {
      add => CommandManager.RequerySuggested += value;
      remove => CommandManager.RequerySuggested -= value;
    }

    public void Execute(object parameter) => this._execute((T) parameter);
  }
}
