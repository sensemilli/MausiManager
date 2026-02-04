using System;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base.Enum;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ISimulationThread
{
	ISimulationScreen? Screen { get; set; }

	List<ISimulationStep> SimulationsSteps { get; }

	double MaxSimulationStep { get; }

	double CurrentTime => this.StepToTime(this.CurrentStep);

	double CurrentStep { get; }

	ISimulationState State { get; }

	bool HiddenForUser { get; }

	ITransformState EndTransformState { get; }

	bool IsMultiSim { get; }

	bool IsRunning { get; }

	event Action<IToolSetups?> NewToolSetupsEvent;

	event Action<ISimulationThread> PlayEvent;

	event Action<ISimulationThread> PauseEvent;

	event Action<ISimulationThread> StopEvent;

	event Action<ISimulationThread, bool> TickEvent;

	event Action<ISimulationThread> RenderEvent;

	void Dispose();

	void UpdateFingerPos(int bendNumber);

	void UpdateWAxesPos(int bendNumber, double pos, IAngleMeasurementSystem angleMeasurementSystem);

	void UpdateLiftingAidPos(double pos, AxisType axisType, ILiftingAid liftingAid, ICombinedBendDescriptorInternal currentBend);

	bool Play();

	bool Pause();

	bool Stop();

	void GotoStep(double step, bool visible = true, bool applyModification = true);

	void GotoStep(double step, double maxStep, bool visible = true, bool applyModification = true);

	double TimeToStep(double time);

	double StepToTime(double step);

	double MaxTime();

	void GotoTime(double time, bool visible = true);

	void GoToBend(int number, bool visible = true);

	void GotoNextStep(bool visible = true);

	void GotoPrevStep(bool visible = true);

	ISimulationStep? WindSimulation(bool forward, bool visible, Func<ISimulationStep, bool> validStep);

	void GoToEnd(bool visible = true);

	void RecalculateCurrentStep();

	void ResetClampCollisions(int bendNumber = -1);

	ISimulationBendInfo GetBendInfo(int cbdOrder);

	void SetSettings(bool CheckCollisions, bool CheckCollisionsKeyFrames, bool SheetHandlingVisible, double SpeedFactor, int MaxFrameRate, bool ValidationMode, bool PauseOnCollision, bool IgnoreClampCollisions, bool IgnoreOverbendCollisions, bool IgnoreOpeningCollisions, bool IgnoreClosingCollisions, bool IgnoreLiftingAidCollisions);

	IEnumerable<(double start, double end, bool userAccepted)> GetCollisionIntervals();
}
