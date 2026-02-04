using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.BendSimulation;

public interface ICollisionInterval
{
	List<double> ConfirmedSteps { get; set; }

	double Start { get; set; }

	double End { get; set; }

	HashSet<(Face face, Model model)> Faces { get; set; }

	double FirstStep => this.ConfirmedSteps.Min();

	double LastStep => this.ConfirmedSteps.Max();
}
