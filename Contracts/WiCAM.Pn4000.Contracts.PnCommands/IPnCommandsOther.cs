namespace WiCAM.Pn4000.Contracts.PnCommands;

public interface IPnCommandsOther
{
	F2exeReturnCode ImportUniversal(IPnCommandArg arg, IImportArg importSetting);

	F2exeReturnCode Import(IPnCommandArg arg);

	F2exeReturnCode ImportFlat(IPnCommandArg arg);

	F2exeReturnCode ImportUnfold(IPnCommandArg arg);

	F2exeReturnCode OpenView(IPnCommandArg arg);

	void Disassembly(IPnCommandArg arg, int? partId);

	void DisStepExp(IPnCommandArg arg);

	void ExportCadGeo(IPnCommandArg arg);

	F2exeReturnCode Open(IPnCommandArg arg);

	F2exeReturnCode Delete(IPnCommandArg arg);

	F2exeReturnCode Save(IPnCommandArg arg);

	void AnalyzeAssembly(IPnCommandArg arg);

	void Clean(IPnCommandArg arg);

	void CloseDocument(IPnCommandArg arg);

	void Undo(IPnCommandArg arg);

	void Redo(IPnCommandArg arg);

	void MaterialAlliance(IPnCommandArg arg);

	void UnfoldConfig(IPnCommandArg arg);

	void ShowInfo(IPnCommandArg arg);

	void ShowLegend(IPnCommandArg arg);

	void Node3DInfo(IPnCommandArg arg);

	void View3DInfo(IPnCommandArg arg);

	F2exeReturnCode DevImport(IPnCommandArg arg);

	F2exeReturnCode DevImportLowTess(IPnCommandArg arg);

	F2exeReturnCode DevSave(IPnCommandArg arg);

	void Print(IPnCommandArg arg);

	void OldPnStart(IPnCommandArg arg);

	void OldPnStart3D(IPnCommandArg arg);
}
