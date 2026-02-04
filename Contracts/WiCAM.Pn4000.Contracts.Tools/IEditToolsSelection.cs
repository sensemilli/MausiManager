using System;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IEditToolsSelection
{
	IToolsAndBends ToolsAndBends { get; set; }

	IToolSetups? CurrentSetups { get; set; }

	IBendMachine? CurrentMachine { get; set; }

	IPnBndDoc CurrentDoc { get; set; }

	HashSet<IToolPiece> BadTools { get; }

	EditToolSelectionModes SelectionMode { get; set; }

	IEnumerable<IToolPiece> AllPiecesInSetups { get; }

	IEnumerable<IToolPiece> SelectionAsPieces { get; }

	IEnumerable<IToolPiece> SelectedPieces { get; }

	IEnumerable<IToolSection> SelectedSections { get; }

	IEnumerable<IToolCluster> SelectedClusters { get; }

	Dictionary<Model, IToolPiece> ModelToPieces { get; }

	Dictionary<IToolPiece, Model> PiecesToModel { get; }

	event Action SelectionChanged;

	event Action DataRefreshed;

	event Action NewSetups;

	void RefreshData();

	void SetData(IPnBndDoc doc, IToolsAndBends toolsAndBends, IToolSetups? toolSetups);

	void SetDataToolSetups(IToolSetups? toolSetups);

	void SetSelection(IToolPiece piece, bool isSelected);

	void SetSelection(IEnumerable<IToolPiece> pieces, bool isSelected);

	void SetSelection(IToolSection section, bool isSelected);

	void SetSelection(IEnumerable<IToolSection> sections, bool isSelected);

	void SetSelection(IToolCluster cluster, bool isSelected);

	void SetSelection(IEnumerable<IToolCluster> clusters, bool isSelected);

	void ToggleSelection(IToolPiece piece);

	void ToggleSelection(IToolSection section);

	void ToggleSelection(IToolCluster cluster);

	void SelectAll();

	void UnselectAll();
}
