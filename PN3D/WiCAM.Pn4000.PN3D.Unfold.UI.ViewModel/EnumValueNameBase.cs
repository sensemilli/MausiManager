using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class EnumValueNameBase
{
	[CompilerGenerated]
	private sealed class _003CGetValues_003Ed__0<T> : IEnumerable<EnumValueName<T>>, IEnumerable, IEnumerator<EnumValueName<T>>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private EnumValueName<T> _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private ITranslator translator;

		public ITranslator _003C_003E3__translator;

		private IEnumerator _003C_003E7__wrap1;

		EnumValueName<T> IEnumerator<EnumValueName<T>>.Current
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
		public _003CGetValues_003Ed__0(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			this._003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = this._003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					this._003C_003Em__Finally1();
				}
			}
			this._003C_003E7__wrap1 = null;
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
					this._003C_003E7__wrap1 = global::System.Enum.GetValues(typeof(T)).GetEnumerator();
					this._003C_003E1__state = -3;
					break;
				case 1:
					this._003C_003E1__state = -3;
					break;
				}
				if (this._003C_003E7__wrap1.MoveNext())
				{
					object current = this._003C_003E7__wrap1.Current;
					string text = this.translator.Translate("Enums." + typeof(T).Name + "." + global::System.Enum.GetName(typeof(T), current));
					if (string.IsNullOrEmpty(text))
					{
						text = current.ToString();
					}
					this._003C_003E2__current = new EnumValueName<T>
					{
						Name = text,
						Value = (T)current
					};
					this._003C_003E1__state = 1;
					return true;
				}
				this._003C_003Em__Finally1();
				this._003C_003E7__wrap1 = null;
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
			if (this._003C_003E7__wrap1 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<EnumValueName<T>> IEnumerable<EnumValueName<T>>.GetEnumerator()
		{
			_003CGetValues_003Ed__0<T> _003CGetValues_003Ed__;
			if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				this._003C_003E1__state = 0;
				_003CGetValues_003Ed__ = this;
			}
			else
			{
				_003CGetValues_003Ed__ = new _003CGetValues_003Ed__0<T>(0);
			}
			_003CGetValues_003Ed__.translator = this._003C_003E3__translator;
			return _003CGetValues_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<EnumValueName<T>>)this).GetEnumerator();
		}
	}

	[IteratorStateMachine(typeof(_003CGetValues_003Ed__0<>))]
	public static IEnumerable<EnumValueName<T>> GetValues<T>(ITranslator translator)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetValues_003Ed__0<T>(-2)
		{
			_003C_003E3__translator = translator
		};
	}
}
