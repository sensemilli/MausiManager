using System;

namespace WiCAM.Pn4000.Contracts.Common;

public interface IWeakEventManager
{
	IWeakEvent<TSender, TArg> CreateEvent<TSender, TArg>(out Action<TSender, TArg> invoke);
}
