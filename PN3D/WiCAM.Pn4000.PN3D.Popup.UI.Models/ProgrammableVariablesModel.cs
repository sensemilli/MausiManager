using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataSourceModel;
using BendDataSourceModel.Enums;
using BendDataSourceModel.Geometry;
using BendDataSourceModel.Processes;
using WiCAM.Pn4000.ConfigMachine;
using WiCAM.Pn4000.ConfigMachine.Version2;
using WiCAM.Pn4000.PN3D.Popup.UI.Information;

namespace WiCAM.Pn4000.PN3D.Popup.UI.Models;

public class ProgrammableVariablesModel
{
	private BdsmDataModel pPModel;

	public ObservableCollection<BdsmBendProcess> DB { get; set; }

	public global::WiCAM.Pn4000.PN3D.Popup.UI.Information.ProgramableVariablesVisibility Visibility { get; set; }

	public bool isViewOkResult { get; set; }

	public ProgrammableVariablesModel(BendMachine bendMachine, BdsmDataModel pPModel)
	{
		global::WiCAM.Pn4000.ConfigMachine.Version2.ProgramableVariablesVisibility programableVariablesVisibility = DeSerializer.LoadProgramableVariablesVisibility(bendMachine?.MachinePath);
		if (programableVariablesVisibility == null)
		{
			this.Visibility = new global::WiCAM.Pn4000.PN3D.Popup.UI.Information.ProgramableVariablesVisibility();
			if (bendMachine != null)
			{
				DeSerializer.SaveProgramableVariablesVisibility(bendMachine?.MachinePath, new global::WiCAM.Pn4000.ConfigMachine.Version2.ProgramableVariablesVisibility
				{
					AngleDegVisibility = this.Visibility.AngleDegVisibility,
					BendVelocityVisibility = this.Visibility.BendVelocityVisibility,
					CloseVelocityVisibility = this.Visibility.CloseVelocityVisibility,
					ExampleEnumVisibility = this.Visibility.ExampleEnumVisibility,
					HoldTimePressVisibility = this.Visibility.HoldTimePressVisibility,
					LengthWithGapsVisibility = this.Visibility.LengthWithGapsVisibility,
					LowerDeadPointVisibility = this.Visibility.LowerDeadPointVisibility,
					MutePointVisibility = this.Visibility.MutePointVisibility,
					PressForceVisibility = this.Visibility.PressForceVisibility,
					RAxisPositionLeftStopVisibility = this.Visibility.RAxisPositionLeftStopVisibility,
					RAxisPositionRightStopVisibility = this.Visibility.RAxisPositionRightStopVisibility,
					RadiusVisibility = this.Visibility.RadiusVisibility,
					RelaxationVelocityVisibility = this.Visibility.RelaxationVelocityVisibility,
					ReleasePointVisibility = this.Visibility.ReleasePointVisibility,
					WAxisVisibility = this.Visibility.WAxisVisibility,
					XAxisPositionLeftStopVisibility = this.Visibility.XAxisPositionLeftStopVisibility,
					XAxisPositionRightStopVisibility = this.Visibility.XAxisPositionRightStopVisibility,
					XAxisRetractionLeftStopVisibility = this.Visibility.XAxisRetractionLeftStopVisibility,
					XAxisRetractionRightStopVisibility = this.Visibility.XAxisRetractionRightStopVisibility,
					ZAxisPositionLeftStopVisibility = this.Visibility.ZAxisPositionLeftStopVisibility,
					ZAxisPositionRightStopVisibility = this.Visibility.ZAxisPositionRightStopVisibility
				});
			}
		}
		else
		{
			this.Visibility = new global::WiCAM.Pn4000.PN3D.Popup.UI.Information.ProgramableVariablesVisibility
			{
				AngleDegVisibility = programableVariablesVisibility.AngleDegVisibility,
				BendVelocityVisibility = programableVariablesVisibility.BendVelocityVisibility,
				CloseVelocityVisibility = programableVariablesVisibility.CloseVelocityVisibility,
				ExampleEnumVisibility = programableVariablesVisibility.ExampleEnumVisibility,
				HoldTimePressVisibility = programableVariablesVisibility.HoldTimePressVisibility,
				LengthWithGapsVisibility = programableVariablesVisibility.LengthWithGapsVisibility,
				LowerDeadPointVisibility = programableVariablesVisibility.LowerDeadPointVisibility,
				MutePointVisibility = programableVariablesVisibility.MutePointVisibility,
				PressForceVisibility = programableVariablesVisibility.PressForceVisibility,
				RAxisPositionLeftStopVisibility = programableVariablesVisibility.RAxisPositionLeftStopVisibility,
				RAxisPositionRightStopVisibility = programableVariablesVisibility.RAxisPositionRightStopVisibility,
				RadiusVisibility = programableVariablesVisibility.RadiusVisibility,
				RelaxationVelocityVisibility = programableVariablesVisibility.RelaxationVelocityVisibility,
				ReleasePointVisibility = programableVariablesVisibility.ReleasePointVisibility,
				WAxisVisibility = programableVariablesVisibility.WAxisVisibility,
				XAxisPositionLeftStopVisibility = programableVariablesVisibility.XAxisPositionLeftStopVisibility,
				XAxisPositionRightStopVisibility = programableVariablesVisibility.XAxisPositionRightStopVisibility,
				XAxisRetractionLeftStopVisibility = programableVariablesVisibility.XAxisRetractionLeftStopVisibility,
				XAxisRetractionRightStopVisibility = programableVariablesVisibility.XAxisRetractionRightStopVisibility,
				ZAxisPositionLeftStopVisibility = programableVariablesVisibility.ZAxisPositionLeftStopVisibility,
				ZAxisPositionRightStopVisibility = programableVariablesVisibility.ZAxisPositionRightStopVisibility
			};
		}
		this.pPModel = pPModel;
		this.DB = new ObservableCollection<BdsmBendProcess>();
		foreach (BdsmBendProcess bendProcess in pPModel.BendProcesses)
		{
			BdsmBendProcess bdsmBendProcess = new BdsmBendProcess
			{
				BendLine = new BdsmBendLine()
			};
			bdsmBendProcess.BendLine.CommonBendFace = new BdsmCommonBendFace
			{
				Info = new BdsmBendInfo()
			};
			bdsmBendProcess.ID = pPModel.BendProcesses.IndexOf(bendProcess) + 1;
			bdsmBendProcess.ExampleEnum = BdsmExampleEnum.Bdsm3;
			this.CopySelectedProperties(bendProcess, bdsmBendProcess);
			this.DB.Add(bdsmBendProcess);
		}
	}

	public void GetDataBack()
	{
		if (this.pPModel != null && this.DB != null && this.pPModel.BendProcesses.Count == this.DB.Count)
		{
			List<BdsmBendProcess> list = this.DB.OrderBy((BdsmBendProcess x) => x.ID).ToList();
			for (int i = 0; i < this.pPModel.BendProcesses.Count; i++)
			{
				this.CopySelectedProperties(list[i], this.pPModel.BendProcesses[i]);
			}
		}
	}

	private void CopySelectedProperties(BdsmBendProcess org, BdsmBendProcess copy)
	{
		copy.BendLine.BendInfo.AngleDeg = Math.Round(org.BendLine.BendInfo.AngleDeg, 3);
		copy.BendLine.CommonBendFace.LengthWithGaps = Math.Round(org.BendLine.CommonBendFace.LengthWithGaps, 3);
		copy.BendLine.BendInfo.Radius = Math.Round(org.BendLine.BendInfo.Radius, 3);
		copy.Clamp.CloseVelocity = Math.Round(org.Clamp.CloseVelocity, 3);
		copy.Clamp.MutePoint = Math.Round(org.Clamp.MutePoint, 3);
		copy.Bend.PressForce = Math.Round(org.Bend.PressForce, 3);
		copy.Bend.PressForceCalculated = Math.Round(org.Bend.PressForceCalculated, 3);
		copy.Bend.BendVelocity = Math.Round(org.Bend.BendVelocity, 3);
		copy.Bend.LowerDeadPoint = Math.Round(org.Bend.LowerDeadPoint, 3);
		copy.Bend.HoldTimePress = Math.Round(org.Bend.HoldTimePress, 3);
		copy.Relaxation.RelaxationVelocity = Math.Round(org.Relaxation.RelaxationVelocity, 3);
		copy.Relaxation.ReleasePoint = Math.Round(org.Relaxation.ReleasePoint, 3);
		copy.Relaxation.OpeningDistance = Math.Round(org.Relaxation.OpeningDistance, 3);
		copy.Relaxation.OverBendAngle = Math.Round(org.Relaxation.OverBendAngle, 3);
		copy.SetStop.RAxisPositionLeftStop = Math.Round(org.SetStop.RAxisPositionLeftStop, 3);
		copy.SetStop.RAxisPositionRightStop = Math.Round(org.SetStop.RAxisPositionRightStop, 3);
		copy.SetStop.XAxisPositionLeftStop = Math.Round(org.SetStop.XAxisPositionLeftStop, 3);
		copy.SetStop.XAxisPositionRightStop = Math.Round(org.SetStop.XAxisPositionRightStop, 3);
		copy.SetStop.ZAxisPositionLeftStop = Math.Round(org.SetStop.ZAxisPositionLeftStop, 3);
		copy.SetStop.ZAxisPositionRightStop = Math.Round(org.SetStop.ZAxisPositionRightStop, 3);
		copy.SetStop.LeftRetractionX = Math.Round(org.SetStop.LeftRetractionX, 3);
		copy.SetStop.RightRetractionX = Math.Round(org.SetStop.RightRetractionX, 3);
		copy.SetStop.FingerStopIdLeft = org.SetStop.FingerStopIdLeft;
		copy.SetStop.FingerStopIdRight = org.SetStop.FingerStopIdRight;
		copy.ExampleEnum = org.ExampleEnum;
		copy.WAxisPos = Math.Round(org.WAxisPos, 3);
		copy.IAxisPos = Math.Round(org.IAxisPos, 3);
	}
}
