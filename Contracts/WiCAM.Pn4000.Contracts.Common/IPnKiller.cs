namespace WiCAM.Pn4000.Contracts.Common;

public interface IPnKiller
{
	void Kill(bool initialMultiTask);

	void KillAllPnInSeparateFolder(string path, string key);

	void Unregister();
}
