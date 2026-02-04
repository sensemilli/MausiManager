using System;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.Contracts.Screen;

public interface ISimulationScreen
{
	void Render(bool skipQueuedFrames, Action<IRenderTaskResult> action = null);

	IEnumerable<Model> GetRenderedModels();

	void EnqueueTask(IRenderTaskBase task);

	void UpdateModelTransform(Model model, bool render);

	void UpdateModelGeometry(Model model, bool render);

	void UpdateModelAppearance(Model model, bool render);

	void UpdateModelGeometry(Shell shell, bool render);
}
