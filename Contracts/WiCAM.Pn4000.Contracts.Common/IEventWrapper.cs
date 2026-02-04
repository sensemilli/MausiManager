using System;

namespace WiCAM.Pn4000.Contracts.Common;

public interface IEventWrapper<TSender, TArg>
{
	event Action<TSender, TArg> Action;
}
