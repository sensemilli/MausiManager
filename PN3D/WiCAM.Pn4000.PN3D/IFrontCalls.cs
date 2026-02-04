using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D;

public interface IFrontCalls
{
	IMessageDisplay MessageDisplay { get; }

	IFrontCalls CreateDocInstance(string docName);

	IMaterialArt GetMaterial(IDoc3d doc, IGlobals globals, IMessageDisplay messageDisplay);

	IMaterialArt GetMaterialAssembly(IDoc3d doc, IGlobals globals, IMessageDisplay messageDisplay);

	bool CreatePostprocessorIfMaterialNotByUser();

	bool CreatePostprocessorIfNoKFactor();

	bool CreatePostprocessorIfErrorsSimValidation();

	void GetDeductionValue(IDoc3d doc, Dictionary<IBendDescriptor, BendTableReturnValues> bendTableResults, IGlobals globals, bool editWithTools);

	IWaitCancel ShowWaitWithCancel(IGlobals globals);

	void CloseWaitWithCancel(IWaitCancel waitCancel);

	string OpenFolderDialog(IPnBndDoc doc, out bool? result, string initialPath);
}
