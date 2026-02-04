using System;
using System.Threading;

namespace WiCAM.Pn4000.Contracts.Doc3d;

public interface ICalculationArg
{
	string Status { get; }

	double? Progress { get; }

	CancellationToken CancellationToken { get; }

	ICalculationDebugArg? DebugInfo { get; set; }

	event Action<ICalculationArg> StatusChanged;

	void SetStatus(string status, bool update = true);

	void SetProgress(double? progress, bool update = true);

	bool TryCancelCalculation(bool update = true);
}
