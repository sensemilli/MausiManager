using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
public class BendProcess
{
	[XmlAttribute]
	public int _ID { get; set; }

	[XmlAttribute]
	public int BendSeqOrder { get; set; }

	[XmlAttribute]
	public int BendMethod { get; set; }

	[XmlAttribute]
	public int BendType { get; set; }

	[XmlAttribute]
	public int BendOperation { get; set; }

	[XmlAttribute]
	public double BendLength { get; set; }

	[XmlAttribute]
	public double BendAngle { get; set; }

	[XmlAttribute]
	public double BendAngleC { get; set; }

	[XmlAttribute]
	public double BendAngleRC { get; set; }

	[XmlAttribute]
	public double BendAngleMC { get; set; }

	[XmlAttribute]
	public int NumCycles { get; set; }

	[XmlAttribute]
	public string AxisPosition { get; set; }

	[XmlAttribute]
	public string AxisPositionC { get; set; }

	[XmlAttribute]
	public string AxisParked { get; set; }

	[XmlAttribute]
	public double HCPressure { get; set; }

	[XmlAttribute]
	public double HCPressureC { get; set; }

	[XmlAttribute]
	public double PressForce { get; set; }

	[XmlAttribute]
	public double PressForceC { get; set; }

	[XmlAttribute]
	public double PressTime { get; set; }

	[XmlAttribute]
	public int AxisDelay { get; set; }

	[XmlAttribute]
	public string ReturnTraverse { get; set; }

	[XmlAttribute]
	public double BendingSpeedDown { get; set; }

	[XmlAttribute]
	public double BendingSpeedDownC { get; set; }

	[XmlAttribute]
	public double BendingSpeedUp { get; set; }

	[XmlAttribute]
	public double BendingSpeedUpC { get; set; }

	[XmlAttribute]
	public double SwitchOverPtOffset { get; set; }

	[XmlAttribute]
	public double ClampingPoint { get; set; }

	[XmlAttribute]
	public double ClampingPointC { get; set; }

	[XmlAttribute]
	public double DecompFactor { get; set; }

	[XmlAttribute]
	public double UpperDeadCentre { get; set; }

	[XmlAttribute]
	public int NumDieInserts { get; set; }

	[XmlAttribute]
	public double ToolStationOffset { get; set; }

	[XmlAttribute]
	public int SheetHandling { get; set; }

	[XmlAttribute]
	public string BackStopEdge { get; set; }

	[XmlAttribute]
	public string BackStopPosition { get; set; }

	[XmlAttribute]
	public string BackStopSnapping { get; set; }

	[XmlAttribute]
	public string BSRefPointIndex { get; set; }

	[XmlAttribute]
	public string ElectronicContactXB { get; set; }

	[XmlAttribute]
	public int BendSeqDependency { get; set; }

	[XmlAttribute]
	public int BendEdge { get; set; }

	[XmlAttribute]
	public double DeltaXPos { get; set; }

	[XmlAttribute]
	public int TurnUT { get; set; }

	[XmlAttribute]
	public int TurnLT { get; set; }

	[XmlAttribute]
	public int AcceptableCollision { get; set; }

	[XmlAttribute]
	public int ReturnTraverseType { get; set; }

	[XmlAttribute]
	public int PeripheryStartType { get; set; }

	[XmlAttribute]
	public int BAStopSynchronisation { get; set; }

	[XmlAttribute]
	public int LDCHalt { get; set; }

	[XmlAttribute]
	public int BoxBending { get; set; }

	[XmlAttribute]
	public int WarningState { get; set; }

	[XmlAttribute]
	public string WarningValue { get; set; }

	[XmlAttribute]
	public int NoClampingPointStop { get; set; }

	[XmlAttribute]
	public int NoBeamMove { get; set; }

	[XmlAttribute]
	public int IPC_Temperature { get; set; }

	[XmlAttribute]
	public int IPC_SheetThickness { get; set; }

	[XmlAttribute]
	public int IPC_FrameDeflection { get; set; }

	[XmlAttribute]
	public int IPC_SpringBack { get; set; }

	[XmlAttribute]
	public int IPC_ToolOverLoad { get; set; }

	[XmlAttribute]
	public int IPC_DynamicCrowning { get; set; }

	[XmlAttribute]
	public int IPC_DynCroFactor { get; set; }

	[XmlAttribute]
	public int IPC_SpringBackUnloading { get; set; }

	[XmlAttribute]
	public int IPC_SpringBackOvertake { get; set; }

	[XmlAttribute]
	public int IPC_SheetThicknessOvertake { get; set; }

	[XmlAttribute]
	public int IPC_SpringBackEveryN { get; set; }

	[XmlAttribute]
	public int IPC_SheetThicknessEveryN { get; set; }

	[XmlAttribute]
	public int IPC_AMS { get; set; }

	[XmlAttribute]
	public int IPC_AMS_SpringBack { get; set; }

	[XmlAttribute]
	public int IPC_AMS_SpringBackUnloading { get; set; }

	[XmlAttribute]
	public int IPC_AMSOvertake { get; set; }

	[XmlAttribute]
	public int IPC_AMS_EveryN { get; set; }

	[XmlAttribute]
	public int IPC_AMS_SpringBackDBPresetY1 { get; set; }

	[XmlAttribute]
	public int IPC_AMS_SpringBackDBPresetY2 { get; set; }

	[XmlAttribute]
	public int IPC_AMS_SpringBackManualPresetY1 { get; set; }

	[XmlAttribute]
	public int IPC_AMS_SpringBackManualPresetY2 { get; set; }

	public List<BOSRef> BosRefs { get; set; }
}
