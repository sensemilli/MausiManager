using WiCAM.Pn4000.Contracts.BendSimulation;

namespace WiCAM.Pn4000.PN3D.Popup.Model;

internal class PopupValidationPendingModel
{
	public bool simulationDone;

	internal void StopEvent(ISimulationThread obj)
	{
		this.simulationDone = true;
	}
}
