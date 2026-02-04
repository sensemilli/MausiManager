using WiCAM.Pn4000.BendDoc.Contracts;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.BendDoc;

public class PnBndDocImporter : IPnBndDocImporter
{
	private readonly IDocSerializer _docSerializer;

	private readonly IDoc3dFactory _doc3dFactory;

	private readonly ISpatialImport _spatialImport;

	public PnBndDocImporter(IDocSerializer docSerializer, IDoc3dFactory doc3dFactory, ISpatialImport spatialImport)
	{
		this._docSerializer = docSerializer;
		this._doc3dFactory = doc3dFactory;
		this._spatialImport = spatialImport;
	}

	public IDoc3d ImportStl(string fileName, bool viewStyle)
	{
		if (viewStyle)
		{
			IDoc3d doc3d = this._doc3dFactory.CreateDoc(fileName);
			doc3d.View3DModel = StlLoader.LoadStl(fileName);
			return doc3d;
		}
		IDoc3d doc3d2 = this._doc3dFactory.CreateDoc(fileName);
		doc3d2.InputModel3D = StlLoader.LoadStl(fileName);
		return doc3d2;
	}

	public IDoc3d ImportSmx(string fileName, bool viewStyle, bool highTesselation)
	{
		if (viewStyle)
		{
			IDoc3d doc3d = this._doc3dFactory.CreateDoc(fileName);
			doc3d.View3DModel = new SmxLoader().LoadSmx(fileName, highTesselation, null);
			return doc3d;
		}
		IDoc3d doc3d2 = this._doc3dFactory.CreateDoc(fileName);
		doc3d2.InputModel3D = new SmxLoader().LoadSmx(fileName, highTesselation, null);
		return doc3d2;
	}

	public IDoc3d ImportC3MO(string fileName, bool viewStyle)
	{
		if (viewStyle)
		{
			IDoc3d doc3d = this._doc3dFactory.CreateDoc(fileName);
			doc3d.View3DModel = ModelSerializer.Deserialize(fileName);
			return doc3d;
		}
		IDoc3d doc3d2 = this._doc3dFactory.CreateDoc(fileName);
		doc3d2.SetDefautMachine(viewStyle);
		doc3d2.InputModel3D = ModelSerializer.Deserialize(fileName);
		return doc3d2;
	}

	public IDoc3d ImportC3DO(string fileName, bool viewStyle)
	{
		return this._docSerializer.DecompressAndDeserialize(fileName);
	}

	public IDoc3d CreateByImportSpatial(out F2exeReturnCode code, string fileName, int licenceKey, bool checkLicense, bool viewStyle, IFactorio factorio, bool moveToCenter, bool analyze, int machineNum, bool isDevImport, int materialNum)
	{
		return this._spatialImport.CreateByImportSpatial(out code, fileName, licenceKey, checkLicense, viewStyle, factorio, moveToCenter, analyze, machineNum, isDevImport, materialNum);
	}

	public F2exeReturnCode StartSpatial(string fileName, int licenceKey, bool checkLicense, bool viewStyle, IGlobals globals, Assembly assembly)
	{
		return this._spatialImport.StartSpatial(fileName, licenceKey, checkLicense, viewStyle, globals, assembly);
	}
}
