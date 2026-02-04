using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SharpDX;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf;

public static class Extensions
{
	[CompilerGenerated]
	private sealed class _003CGetTranslatedComboboxEntries_003Ed__1<T> : IEnumerable<ComboboxEntry<T>>, IEnumerable, IEnumerator<ComboboxEntry<T>>, IEnumerator, IDisposable where T : notnull, System.Enum
	{
		private int _003C_003E1__state;

		private ComboboxEntry<T> _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string baseTranslationKey;

		public string _003C_003E3__baseTranslationKey;

		private ITranslator translator;

		public ITranslator _003C_003E3__translator;

		private IEnumerator _003C_003E7__wrap1;

		ComboboxEntry<T> IEnumerator<ComboboxEntry<T>>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetTranslatedComboboxEntries_003Ed__1(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					Type typeFromHandle = typeof(T);
					if (baseTranslationKey == null)
					{
						baseTranslationKey = "l_enum." + typeFromHandle.Name + ".";
					}
					_003C_003E7__wrap1 = System.Enum.GetValues(typeFromHandle).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					T value = (T)_003C_003E7__wrap1.Current;
					_003C_003E2__current = new ComboboxEntry<T>(translator.Translate(baseTranslationKey + value.ToString()), value);
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
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
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 is IDisposable disposable)
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
		IEnumerator<ComboboxEntry<T>> IEnumerable<ComboboxEntry<T>>.GetEnumerator()
		{
			_003CGetTranslatedComboboxEntries_003Ed__1<T> _003CGetTranslatedComboboxEntries_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetTranslatedComboboxEntries_003Ed__ = this;
			}
			else
			{
				_003CGetTranslatedComboboxEntries_003Ed__ = new _003CGetTranslatedComboboxEntries_003Ed__1<T>(0);
			}
			_003CGetTranslatedComboboxEntries_003Ed__.translator = _003C_003E3__translator;
			_003CGetTranslatedComboboxEntries_003Ed__.baseTranslationKey = _003C_003E3__baseTranslationKey;
			return _003CGetTranslatedComboboxEntries_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ComboboxEntry<T>>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetTranslatedComboboxEntriesNullable_003Ed__2<T> : IEnumerable<ComboboxEntry<T?>>, IEnumerable, IEnumerator<ComboboxEntry<T?>>, IEnumerator, IDisposable where T : struct, System.Enum
	{
		private int _003C_003E1__state;

		private ComboboxEntry<T?> _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string baseTranslationKey;

		public string _003C_003E3__baseTranslationKey;

		private string nullDesc;

		public string _003C_003E3__nullDesc;

		private ITranslator translator;

		public ITranslator _003C_003E3__translator;

		private Type _003CenumType_003E5__2;

		private IEnumerator _003C_003E7__wrap2;

		ComboboxEntry<T?> IEnumerator<ComboboxEntry<T?>>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetTranslatedComboboxEntriesNullable_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003CenumType_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003CenumType_003E5__2 = typeof(T);
					if (baseTranslationKey == null)
					{
						baseTranslationKey = "l_enum." + _003CenumType_003E5__2.Name + ".";
					}
					_003C_003E2__current = new ComboboxEntry<T?>(nullDesc, null);
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap2 = System.Enum.GetValues(_003CenumType_003E5__2).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 2:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap2.MoveNext())
				{
					T value = (T)_003C_003E7__wrap2.Current;
					_003C_003E2__current = new ComboboxEntry<T?>(translator.Translate(baseTranslationKey + value), value);
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
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
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 is IDisposable disposable)
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
		IEnumerator<ComboboxEntry<T?>> IEnumerable<ComboboxEntry<T?>>.GetEnumerator()
		{
			_003CGetTranslatedComboboxEntriesNullable_003Ed__2<T> _003CGetTranslatedComboboxEntriesNullable_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetTranslatedComboboxEntriesNullable_003Ed__ = this;
			}
			else
			{
				_003CGetTranslatedComboboxEntriesNullable_003Ed__ = new _003CGetTranslatedComboboxEntriesNullable_003Ed__2<T>(0);
			}
			_003CGetTranslatedComboboxEntriesNullable_003Ed__.translator = _003C_003E3__translator;
			_003CGetTranslatedComboboxEntriesNullable_003Ed__.baseTranslationKey = _003C_003E3__baseTranslationKey;
			_003CGetTranslatedComboboxEntriesNullable_003Ed__.nullDesc = _003C_003E3__nullDesc;
			return _003CGetTranslatedComboboxEntriesNullable_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ComboboxEntry<T?>>)this).GetEnumerator();
		}
	}

	public static bool IsCamaraInFront(this ScreenD3D11 screen)
	{
		Matrix transformation = screen.Renderer.Root.Transform ?? Matrix.Identity;
		screen.CreateRay(ref transformation, out var _, out var eyeDir, out var eyeUp, out var _);
		return (eyeDir.Y > 0.0) ^ (eyeUp.Z < 0.0);
	}

	[IteratorStateMachine(typeof(_003CGetTranslatedComboboxEntries_003Ed__1<>))]
	public static IEnumerable<ComboboxEntry<T>> GetTranslatedComboboxEntries<T>(this ITranslator translator, string? baseTranslationKey = null) where T : System.Enum
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetTranslatedComboboxEntries_003Ed__1<T>(-2)
		{
			_003C_003E3__translator = translator,
			_003C_003E3__baseTranslationKey = baseTranslationKey
		};
	}

	[IteratorStateMachine(typeof(_003CGetTranslatedComboboxEntriesNullable_003Ed__2<>))]
	public static IEnumerable<ComboboxEntry<T?>> GetTranslatedComboboxEntriesNullable<T>(this ITranslator translator, string? baseTranslationKey = null, string nullDesc = "-") where T : struct, System.Enum
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetTranslatedComboboxEntriesNullable_003Ed__2<T>(-2)
		{
			_003C_003E3__translator = translator,
			_003C_003E3__baseTranslationKey = baseTranslationKey,
			_003C_003E3__nullDesc = nullDesc
		};
	}
}
