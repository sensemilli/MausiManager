using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.BendDoc.Contracts;

public interface ISpatialImport
{
	F2exeReturnCode StartSpatial(string fileName, int licenceKey, bool checkLicense, bool viewStyle, IGlobals globals, Assembly assembly);

	IDoc3d CreateByImportSpatial(out F2exeReturnCode code, string fileName, int licenceKey, bool checkLicense, bool viewStyle, IFactorio factorio, bool moveToCenter, bool analyze = true, int machineNum = -1, bool isDevImport = false, int materialNum = -1);
}
