using System;
using System.Collections.Generic;
using System.Threading;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Assembly;

public interface IAssemblyAnalysisManagement
{
	DisassemblyPart TopPriority { get; set; }

	void ImportPartList(Assembly assembly);

	void Step1LoadLowTesselation(CancellationToken cancellationToken);

	void Step2AnalyzeHd(CancellationToken cancellationImportToken);

	void AnalyzePart(DisassemblyPart part, out Model entryModel, List<PurchasedPartsMerger.PurchasedPart> purchasedPartsForAnalyze, List<Parts1dMerger.Part1d> parts1DForAnalyze);

	void Step3ExportData(string assemblyName, IEnumerable<DisassemblyPart> parts);

	void SaveAssembly();

	F2exeReturnCode Step4OpenPartAndSaveAssembly(DisassemblyPart part);

	F2exeReturnCode Step4OpenPartAndSaveAssembly(IEnumerable<DisassemblyPart> parts);

	void AnalyzeModel(IDoc3d doc, string fileName, out List<DisassemblyPart> disassemblyParts, bool useBackgroundWorker, Action backgroundCompleted, out Thread threadBackgroundAnalyze, Dictionary<string, int> materialImportToPnMatId = null);

	Model GetAssemblyGeometry(IGlobals globals);

	DisassemblySimpleModel.StructureNode GetStructureRootNode();

	void Init(Assembly assembly, IImportArg importSetting);
}
