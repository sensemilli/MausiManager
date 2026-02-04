using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Assembly;

namespace WiCAM.Pn4000.PN3D.Doc;

public interface IPnBndDocImporter
{
	IDoc3d ImportStl(string fileName, bool viewStyle);

	IDoc3d ImportSmx(string fileName, bool viewStyle, bool highTesselation);

	IDoc3d ImportC3MO(string fileName, bool viewStyle);

	IDoc3d ImportC3DO(string fileName, bool viewStyle);

	IDoc3d CreateByImportSpatial(out F2exeReturnCode code, string fileName, int licenceKey, bool checkLicense, bool viewStyle, IFactorio factorio, bool moveToCenter, bool analyze = true, int machineNum = -1, bool isDevImport = false, int materialNum = -1);

	F2exeReturnCode StartSpatial(string fileName, int licenceKey, bool checkLicense, bool viewStyle, IGlobals globals, global::WiCAM.Pn4000.PN3D.Assembly.Assembly assembly);
}
