using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Assembly.DisassemblyWindowHelpers;

public interface IPartAnalyzer
{
	void AnalyzeParts(DisassemblySimpleModel mainModel, IDoc3d doc, string fileName, Dictionary<string, int> materialImportToPnMatId, out List<DisassemblyPart> disassemblyParts, bool useBackgroundWorker, IScreenshotScreen screenshotScreen);

	void Analyze(DisassemblyPart part, IMessageDisplay messageDisplay, int partsCount, IDoc3d orgDoc, bool isAssembly, Dictionary<string, int> materialImportToPnMatId, out HashSet<Triple<FaceGroup, double, double>> notAdjustableBends, out Dictionary<int, BendTableReturnValues> bendTableResults, out Model entryModel3D, List<PurchasedPartsMerger.PurchasedPart> purchasedParts = null, List<Parts1dMerger.Part1d> parts1d = null, IImportArg importSettings = null);
}
