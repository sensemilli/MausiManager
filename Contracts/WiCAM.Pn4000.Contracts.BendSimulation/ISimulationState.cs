using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base.Enum;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ISimulationState
{
	bool Abort { get; set; }

	ISimulationStep ActiveStep { get; set; }

	bool CheckCollisions { get; set; }

	bool CheckCollisionsKeyFrames { get; set; }

	bool CheckSelfCollisions { get; set; }

	double CurrentCalculationStep { get; set; }

	double CurrentStep { get; set; }

	int DesiredFrameRate { get; set; }

	ConcurrentDictionary<(Triangle tri, Model model), HashSet<(Triangle tri, Model model)>> DetectedCollisionsCurrentStep { get; set; }

	bool HasCollisions { get; }

	bool IgnoreClampCollisions { get; set; }

	bool IgnoreOpeningCollisions { get; set; }

	bool IgnoreClosingCollisions { get; set; }

	bool IgnoreOverbendCollisions { get; set; }

	bool IgnoreLiftingAidCollisions { get; set; }

	bool IsRunning { get; set; }

	ILiftingAid LeftBackLiftingAid { get; }

	IFingerStop LeftFinger { get; }

	bool LeftFingerFixed { get; set; }

	bool LeftFingerSnap { get; set; }

	ILiftingAid LeftFrontLiftingAid { get; }

	Dictionary<AxisType, MotionLinearAxis> LinearAxesByType { get; }

	Model Machine { get; }

	IBendMachine MachineConfig { get; }

	IMaterialArt Material { get; }

	int MaxFrameRate { get; set; }

	Model Part { get; }

	FaceGroupModelMapping PartFaceGroupMapper { get; }

	bool PauseOnCollision { get; set; }

	Action<int> ResetClampCollisions { get; }

	ILiftingAid RightBackLiftingAid { get; }

	IFingerStop RightFinger { get; }

	bool RightFingerFixed { get; set; }

	bool RightFingerSnap { get; set; }

	bool PrefereAutoRetractValue { get; set; }

	ILiftingAid RightFrontLiftingAid { get; }

	Dictionary<AxisType, MotionRotationAxis> RotationAxesByType { get; }

	bool SheetHandlingVisible { get; set; }

	ISimulationCollisionManager SimulationCollisionManager { get; }

	double SpeedFactor { get; set; }

	double Thickness { get; }

	bool ToolCalculationIgnoreCollisions { get; set; }

	Dictionary<IToolPiece, Model> ToolPieceToModel { get; }

	bool UnfoldVertices { get; set; }

	bool ValidationMode { get; set; }

	IToolSetups? ToolSetupsRoot { get; }

	ITransformState CurrentTransformState { get; }

	IToolAccessor ToolAccessor { get; }

	INcValues NcValuesOut { get; }

	INcValues NcValuesIn { get; }

	event Action<double> CurrentStepChangedEvent;

	IEnumerable<IToolSection> GetLowerToolSectionsForBend(ISimulationBendInfo? bend);

	IEnumerable<IToolSection> GetUpperToolSectionsForBend(ISimulationBendInfo? bend);
}
