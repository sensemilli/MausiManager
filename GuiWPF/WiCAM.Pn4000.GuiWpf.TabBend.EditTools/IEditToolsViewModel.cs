using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.Implementations;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.ToolCalculationGuiWpf.EditTools.Entities;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public interface IEditToolsViewModel : ISubViewModel, IPopupViewModel
{
	bool IsVisible { get; }

	EditToolSelectionModes EditToolSelectionMode { get; set; }

	bool IsSelectionModeToolPiece { get; set; }

	bool IsSelectionModeToolSection { get; set; }

	bool IsSelectionModeCluster { get; set; }

	ICommand CmdChangeToolProfile { get; }

	ICommand CmdAddToolPiece { get; }

	ICommand<IToolPieceProfile> CmdAddToolPieceLeft { get; }

	ICommand<IToolPieceProfile> CmdAddToolPieceRight { get; }

	ICommand<IAdapterProfile> CmdAddAdapterToSection { get; }

	ICommand<IToolProfile> CmdAddExtensionToSection { get; }

	ICommand CmdAddAdapter { get; }

	ICommand CmdNewSection { get; }

	ICommand CmdDeleteTools { get; }

	ICommand CmdCenterTools { get; }

	ICommand CmdMoveLeft { get; }

	ICommand CmdMoveRight { get; }

	ICommand<double> CmdMoveByDistance { get; }

	ICommand<double> CmdMoveByDistanceAndMerge { get; }

	ICommand<int> CmdMoveToIndexInSection { get; }

	ICommand CmdMoveBendLeft { get; }

	ICommand CmdMoveBendRight { get; }

	ICommand<double> CmdMoveBendByDistance { get; }

	double MoveDistance { get; set; }

	double MoveDistanceUi { get; set; }

	ICommand CmdNewSectionForBends { get; }

	ICommand CmdAdjustSectionLengthLeft { get; }

	ICommand CmdAdjustSectionLengthRight { get; }

	ICommand CmdAdjustSectionLengthCenter { get; }

	double? AdjustSectionLength { get; set; }

	double? AdjustSectionLengthUi { get; set; }

	ICommand<IDictionary<IToolSection, double>> CmdExtendSectionsLeft { get; set; }

	ICommand<IDictionary<IToolSection, double>> CmdExtendSectionsRight { get; set; }

	ICommand CmdFlipTools { get; }

	RadObservableCollection<ToolSetupsViewModel> ToolSetups { get; }

	IToolSetups? SelectedSetup { get; set; }

	ICommand CmdDeleteToolSetup { get; }

	ICommand CmdExtractToolPieces { get; }

	ICommand CmdMergeToolSections { get; }

	ICommand CmdSaveToolSetups { get; }

	ICommand CmdLoadToolSetups { get; }

	ICommand CmdUndo { get; }

	ICommand CmdRedo { get; }

	ICommand CmdCloseOk { get; }

	ICommand CmdCloseCancel { get; }

	bool IsUndoAvailable { get; }

	bool IsRedoAvailable { get; }

	double? GapLeftUi { get; set; }

	double? GapRightUi { get; set; }

	string UnitLength { get; }

	event PropertyChangedEventHandler PropertyChanged;

	void Dispose();

	void Activate();

	void DoCmdChangeToolSetups(IBendPositioning bend, IToolSetups root, IToolCluster? station);
}
