using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.DependencyInjection.Implementations;

public class WeakEventManager : IWeakEventManager
{
	private class WeakEvent<TSender, TArg> : IWeakEvent<TSender, TArg>
	{
		private class EventWrapper : IEventWrapper<TSender, TArg>
		{
			public event Action<TSender, TArg> Action;

			public void DoAction(TSender sender, TArg arg)
			{
				this.Action?.Invoke(sender, arg);
			}
		}

		private readonly List<WeakReference<EventWrapper>> delegates = new List<WeakReference<EventWrapper>>();

		public WeakEvent(out Action<TSender, TArg> invoke)
		{
			invoke = Invoke;
		}

		public IEventWrapper<TSender, TArg> CreateEventWrapper()
		{
			EventWrapper eventWrapper = new EventWrapper();
			lock (delegates)
			{
				delegates.Add(new WeakReference<EventWrapper>(eventWrapper));
				return eventWrapper;
			}
		}

		public void RemoveEventWrapper(IEventWrapper<TSender, TArg> wrapper)
		{
			HashSet<WeakReference<EventWrapper>> hashSet = new HashSet<WeakReference<EventWrapper>>();
			foreach (WeakReference<EventWrapper> @delegate in delegates)
			{
				if (!@delegate.TryGetTarget(out var target))
				{
					hashSet.Add(@delegate);
				}
				else if (target == wrapper)
				{
					hashSet.Add(@delegate);
					break;
				}
			}
			lock (delegates)
			{
				foreach (WeakReference<EventWrapper> item in hashSet)
				{
					delegates.Remove(item);
				}
			}
		}

		private void Invoke(TSender sender, TArg arg)
		{
			HashSet<WeakReference<EventWrapper>> hashSet = null;
			foreach (WeakReference<EventWrapper> @delegate in delegates)
			{
				if (!@delegate.TryGetTarget(out var target))
				{
					if (hashSet == null)
					{
						hashSet = new HashSet<WeakReference<EventWrapper>>();
					}
					hashSet.Add(@delegate);
				}
				else
				{
					target.DoAction(sender, arg);
				}
			}
			if (hashSet == null)
			{
				return;
			}
			lock (delegates)
			{
				foreach (WeakReference<EventWrapper> item in hashSet)
				{
					delegates.Remove(item);
				}
			}
		}
	}

	public IWeakEvent<TSender, TArg> CreateEvent<TSender, TArg>(out Action<TSender, TArg> invoke)
	{
		return new WeakEvent<TSender, TArg>(out invoke);
	}
}
