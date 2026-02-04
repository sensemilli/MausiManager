using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace pncommon.WiCAM.Pn4000.Helpers.Util;

public static class WPFHelper
{
	[CompilerGenerated]
	private sealed class _003CFindVisualChildren_003Ed__2<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : DependencyObject
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private DependencyObject depObj;

		public DependencyObject _003C_003E3__depObj;

		private int _003Ci_003E5__2;

		private DependencyObject _003Cchild_003E5__3;

		private IEnumerator<T> _003C_003E7__wrap3;

		T IEnumerator<T>.Current
		{
			[DebuggerHidden]
			get
			{
				return this._003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return this._003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CFindVisualChildren_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			this._003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = this._003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					this._003C_003Em__Finally1();
				}
			}
			this._003Cchild_003E5__3 = null;
			this._003C_003E7__wrap3 = null;
			this._003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (this._003C_003E1__state)
				{
				default:
					return false;
				case 0:
					this._003C_003E1__state = -1;
					if (this.depObj == null)
					{
						break;
					}
					this._003Ci_003E5__2 = 0;
					goto IL_0107;
				case 1:
					this._003C_003E1__state = -1;
					goto IL_0090;
				case 2:
					{
						this._003C_003E1__state = -3;
						goto IL_00d6;
					}
					IL_0107:
					if (this._003Ci_003E5__2 >= VisualTreeHelper.GetChildrenCount(this.depObj))
					{
						break;
					}
					this._003Cchild_003E5__3 = VisualTreeHelper.GetChild(this.depObj, this._003Ci_003E5__2);
					if (this._003Cchild_003E5__3 != null && this._003Cchild_003E5__3 is T)
					{
						this._003C_003E2__current = (T)this._003Cchild_003E5__3;
						this._003C_003E1__state = 1;
						return true;
					}
					goto IL_0090;
					IL_0090:
					this._003C_003E7__wrap3 = WPFHelper.FindVisualChildren<T>(this._003Cchild_003E5__3).GetEnumerator();
					this._003C_003E1__state = -3;
					goto IL_00d6;
					IL_00d6:
					if (this._003C_003E7__wrap3.MoveNext())
					{
						this._003C_003E2__current = this._003C_003E7__wrap3.Current;
						this._003C_003E1__state = 2;
						return true;
					}
					this._003C_003Em__Finally1();
					this._003C_003E7__wrap3 = null;
					this._003Cchild_003E5__3 = null;
					this._003Ci_003E5__2++;
					goto IL_0107;
				}
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			this._003C_003E1__state = -1;
			if (this._003C_003E7__wrap3 != null)
			{
				this._003C_003E7__wrap3.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			_003CFindVisualChildren_003Ed__2<T> _003CFindVisualChildren_003Ed__;
			if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				this._003C_003E1__state = 0;
				_003CFindVisualChildren_003Ed__ = this;
			}
			else
			{
				_003CFindVisualChildren_003Ed__ = new _003CFindVisualChildren_003Ed__2<T>(0);
			}
			_003CFindVisualChildren_003Ed__.depObj = this._003C_003E3__depObj;
			return _003CFindVisualChildren_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	public static void ToDispatcher(Action act, DispatcherPriority prio = DispatcherPriority.Normal)
	{
		Application.Current.Dispatcher.BeginInvoke(act, prio);
	}

	public static void GuiSafeAction(Action act, DispatcherPriority prio = DispatcherPriority.Normal)
	{
		if (Application.Current.Dispatcher.CheckAccess())
		{
			act();
		}
		else
		{
			Application.Current.Dispatcher.Invoke(act, prio);
		}
	}

	[IteratorStateMachine(typeof(_003CFindVisualChildren_003Ed__2<>))]
	public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFindVisualChildren_003Ed__2<T>(-2)
		{
			_003C_003E3__depObj = depObj
		};
	}
}
