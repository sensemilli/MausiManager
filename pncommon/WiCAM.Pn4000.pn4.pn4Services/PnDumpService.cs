using WiCAM.Pn4000.Contracts.LogCenterServices;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class PnDumpService : IPnDumpService
{
	private readonly IPnDebug _pnDebug;

	public PnDumpService(IPnDebug pnDebug)
	{
		this._pnDebug = pnDebug;
	}

	public void Dump(string dump)
	{
		if (!this._pnDebug.IsVisible)
		{
			this._pnDebug.ShowPlus();
		}
		this._pnDebug.DebugThat(dump);
	}
}
