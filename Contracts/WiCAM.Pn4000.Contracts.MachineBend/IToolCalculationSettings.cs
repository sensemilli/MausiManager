using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.ToolCalculation;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IToolCalculationSettings
{
	int BruteforceBendOrderAmount { get; set; }

	bool? BndAutoCalcBendOrder { get; set; }

	double? OrderOptionSubSimStepMaxAngle { get; set; }

	int? BendOrderCalcAmountCalculations { get; set; }

	List<IBendSequenceOrder> BendOrderStrategies { get; set; }

	List<IToolCalculationOptionOverwrite> Options { get; set; }
}
