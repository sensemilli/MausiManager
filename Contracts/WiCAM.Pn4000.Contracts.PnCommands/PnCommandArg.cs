using System.Threading;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Doc3d;

namespace WiCAM.Pn4000.Contracts.PnCommands;

public class PnCommandArg : IPnCommandArg
{
	public IScopedFactorio ScopedFactorio { get; }

	public IPnBndDoc Doc { get; }

	public CancellationTokenSource CancellationToken { get; }

	public bool IsReadOnly { get; set; }

	public string CommandStr { get; set; }

	public int CommandGroup { get; set; }

	public string CommandParam { get; set; }

	public ICalculationArg? CalculationArg { get; set; }

	public PnCommandArg(IPnBndDoc doc, IScopedFactorio scopedFactorio)
	{
		this.Doc = doc;
		this.ScopedFactorio = scopedFactorio;
	}
}
