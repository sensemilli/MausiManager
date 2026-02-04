using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolCluster
{
	[CompilerGenerated]
	private sealed class _003CAllSections_003Ed__27 : IEnumerable<IToolSection>, IEnumerable, IEnumerator<IToolSection>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private IToolSection _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public IToolCluster _003C_003E4__this;

		private Stack<IToolCluster> _003Cclusters_003E5__2;

		private IToolCluster _003Ccluster_003E5__3;

		private List<IToolSection>.Enumerator _003C_003E7__wrap3;

		IToolSection IEnumerator<IToolSection>.Current
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
		public _003CAllSections_003Ed__27(int _003C_003E1__state)
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
			this._003Cclusters_003E5__2 = null;
			this._003Ccluster_003E5__3 = null;
			this._003C_003E7__wrap3 = default(List<IToolSection>.Enumerator);
			this._003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = this._003C_003E1__state;
				IToolCluster item = this._003C_003E4__this;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					this._003C_003E1__state = -3;
					goto IL_0099;
				}
				this._003C_003E1__state = -1;
				this._003Cclusters_003E5__2 = new Stack<IToolCluster>();
				this._003Cclusters_003E5__2.Push(item);
				goto IL_0102;
				IL_0099:
				if (this._003C_003E7__wrap3.MoveNext())
				{
					this._003C_003E2__current = this._003C_003E7__wrap3.Current;
					this._003C_003E1__state = 1;
					return true;
				}
				this._003C_003Em__Finally1();
				this._003C_003E7__wrap3 = default(List<IToolSection>.Enumerator);
				foreach (IToolCluster child in this._003Ccluster_003E5__3.Children)
				{
					this._003Cclusters_003E5__2.Push(child);
				}
				this._003Ccluster_003E5__3 = null;
				goto IL_0102;
				IL_0102:
				if (this._003Cclusters_003E5__2.Count > 0)
				{
					this._003Ccluster_003E5__3 = this._003Cclusters_003E5__2.Pop();
					this._003C_003E7__wrap3 = this._003Ccluster_003E5__3.Sections.GetEnumerator();
					this._003C_003E1__state = -3;
					goto IL_0099;
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
			((IDisposable)this._003C_003E7__wrap3/*cast due to .constrained prefix*/).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<IToolSection> IEnumerable<IToolSection>.GetEnumerator()
		{
			_003CAllSections_003Ed__27 result;
			if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				this._003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CAllSections_003Ed__27(0)
				{
					_003C_003E4__this = this._003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IToolSection>)this).GetEnumerator();
		}
	}

	double OffsetLocalX
	{
		get
		{
			return this.OffsetLocal.X;
		}
		set
		{
			Vector3d offsetLocal = this.OffsetLocal;
			offsetLocal.X = value;
			this.OffsetLocal = offsetLocal;
		}
	}

	Vector3d OffsetLocal { get; set; }

	Vector3d OffsetWorld { get; set; }

	IToolCluster? Parent { get; set; }

	List<IToolCluster> Children { get; }

	List<IToolSection> Sections { get; }

	IToolSetups Root { get; }

	int Number { get; set; }

	IEnumerable<IToolCluster> AllChildren => EnumerableExtensions.SelectManyRecursive<IToolCluster>(EnumerableExtensions.ToIEnumerable<IToolCluster>(this), (Func<IToolCluster, IEnumerable<IToolCluster>>)((IToolCluster x) => x.Children));

	double? XMin { get; }

	double? XMax { get; }

	[IteratorStateMachine(typeof(_003CAllSections_003Ed__27))]
	IEnumerable<IToolSection> AllSections()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CAllSections_003Ed__27(-2)
		{
			_003C_003E4__this = this
		};
	}
}
