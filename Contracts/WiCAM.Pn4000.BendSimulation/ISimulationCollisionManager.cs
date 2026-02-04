using System.Collections.Concurrent;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.BendSimulation;

public interface ISimulationCollisionManager
{
	IEnumerable<double> GetRecordedSteps { get; }

	IReadOnlyList<ICollisionInterval> CollisionIntervals { get; }

	void AcceptCollision(double t);

	void AddResult(double currentStep, ConcurrentDictionary<(Triangle tri, Model model), HashSet<(Triangle tri, Model model)>> collisions, bool collisionsChecked, bool selfCollicionsChecked);

	HashSet<(Face face, Model model)> GetCollisionFaces(double start, double end);

	IEnumerable<(double start, double end, bool userAccepted)> GetCollisionIntervals();

	Dictionary<int, HashSet<Model>> GetCollisionModelsPerStep();

	bool HasRecordedStep(double t, out bool hasCollision);

	bool IsValidated();

	void Reset();

	void ResetInterval(double start, double end);
}
