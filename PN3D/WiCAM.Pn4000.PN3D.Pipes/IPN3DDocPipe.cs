using System.Windows;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.PN3D.Pipes;

public interface IPN3DDocPipe
{
	ModelViewMode ModelType { get; set; }

	bool IsShowingUnfold { get; }

	void ExportGeoCadAction(IDoc3d doc);

	void ExportStepStructure(IDoc3d doc);

	void MaterialAlliancePopupShow(IGlobals globals, IScreen3D screen3d);

	void ExportAsmAsStepExternal(string folder);

	F2exeReturnCode ImportSpatialAssembly(string fileName, bool checkLicense, bool moveToCenter, bool useBackground, out global::WiCAM.Pn4000.PN3D.Assembly.Assembly assembly, bool viewStyle = false, int machineNum = -1);

	void Print();

	void OldPnStart(string param, bool for3D);

	int OpenFile3dWithParameters(string fileName, int overrideMachine, IMaterialArt overrideMaterial, string overrideModelname, out IDoc3d? newDoc);

	F2exeReturnCode OpenFileP3DExternal(string fileName, out IDoc3d? newDoc);

	F2exeReturnCode Activate3DTab();

	F2exeReturnCode ActivateUnfoldTab();

	F2exeReturnCode UnfoldTube(IDoc3d doc, IGlobals globals, bool render = true);

	void SetMaterialByUser(IDoc3d doc, IGlobals globals);

	F2exeReturnCode UnfoldWithMessage(IGlobals globals, IDoc3d doc, bool render = true);

	void ValidateGeometry(IDoc3d doc, IGlobals globals);

	void ValidateGeometryReset(IDoc3d doc);

	void UnfoldFromSelectedFace(Triangle triangle, IGlobals globals, IDoc3d doc);

	F2exeReturnCode OpenP3D();

	F2exeReturnCode DeleteP3D();

	F2exeReturnCode SaveP3DFileWithUi(IDoc3d doc, IGlobals globals, Window mainWindow, IAutoMode autoMode);

	F2exeReturnCode SaveP3DFileWithLoop(string path, int archiveNumber, IDoc3d doc);

	F2exeReturnCode SaveDocP3D(string fileName, IDoc3d doc, int archiveNumber = 1);

	F2exeReturnCode Generate2D(IDoc3d doc, IGlobals globals);

	F2exeReturnCode Generate2D(IDoc3d doc, IGlobals globals, Model model, Face face, Model faceModel, bool removeProjectionHoles, bool includeZeroBorders);

	void Node3DInfo();

	void Show3DModelViewInfo(IDoc3d doc, IGlobals globals);

	F2exeReturnCode DevImportHighTess(IPnPathService pathService, IGlobals globals);

	F2exeReturnCode DevImportLowTess(IPnPathService pathService, IGlobals globals);

	F2exeReturnCode DevImport(IPnPathService pathService, bool viewStyle, IGlobals globals, bool highTesselation);

	F2exeReturnCode DevSave3D(IDoc3d doc);

	void SetDocNameExternally(string extModelName, IDoc3d doc);

	F2exeReturnCode SetDocNcFilenameExternally(string extNcFilename, SetNcTimestampTypes addTimestamp, IDoc3d doc);

	F2exeReturnCode Simulation(int collisionCheck, IDoc3d doc);

	void AnalyzeDisassemblyData(IDoc3d doc);

	global::WiCAM.Pn4000.PN3D.Assembly.Assembly CreateAssemblyAfterSpatial();

	bool ReconstructFromSelectedFace(Face selectedFace, Model selectedFaceModel, IDoc3d doc, IGlobals globals);
}
