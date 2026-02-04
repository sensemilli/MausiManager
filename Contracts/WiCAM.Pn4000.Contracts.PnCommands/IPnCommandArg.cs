using System.Threading;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Doc3d;

namespace WiCAM.Pn4000.Contracts.PnCommands;

public interface IPnCommandArg
{
	IScopedFactorio ScopedFactorio { get; }

	IPnBndDoc Doc { get; }

	CancellationTokenSource CancellationToken { get; }

	bool IsReadOnly { get; set; }

	string CommandStr { get; set; }

	int CommandGroup { get; set; }

	string CommandParam { get; set; }

	ICalculationArg? CalculationArg { get; set; }
}
