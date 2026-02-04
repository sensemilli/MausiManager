using System;
using WiCAM.Pn4000.Contracts.Doc3d;

namespace WiCAM.Pn4000.GuiWpf.Services;

public interface ICurrentCalculation
{
	ICalculationArg? CurrentCalculationOption { get; }

	event Action CalculationWaiting;

	event Action<ICalculationArg?> CurrentCalculationChanged;

	bool TryStartNewCalculation(ICalculationArg? option);

	void EndCalculation(ICalculationArg option);
}
