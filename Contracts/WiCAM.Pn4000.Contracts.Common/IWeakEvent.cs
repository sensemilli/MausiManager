namespace WiCAM.Pn4000.Contracts.Common;

public interface IWeakEvent<TSender, TArg>
{
	IEventWrapper<TSender, TArg> CreateEventWrapper();

	void RemoveEventWrapper(IEventWrapper<TSender, TArg> wrapper);
}
