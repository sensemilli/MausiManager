using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D;

public class FrontCallsConsole : IFrontCalls
{
	private class WaitCancelDummy : IWaitCancel
	{
		public bool IsCancel { get; set; }

		public string Message { get; set; }

		public double? Progress { get; set; }
	}

	private int _docCount;

	private readonly IFrontCalls _fallback;

	private readonly IMaterial3dFortran _material3dFortran;

	public IMessageDisplay MessageDisplay { get; }

	public FrontCallsConsole(IMessageDisplay messageDisplay, IFrontCalls fallback, IMaterial3dFortran material3dFortran)
	{
		this.MessageDisplay = messageDisplay;
		this._fallback = fallback;
		this._material3dFortran = material3dFortran;
	}

	public IFrontCalls CreateDocInstance(string docName)
	{
		this._docCount++;
		return new FrontCallsConsole(this.MessageDisplay.CreateSubMessageDisplay(this._docCount.ToString("000") + "_" + docName), this, this._material3dFortran);
	}

	public IMaterialArt GetMaterial(IDoc3d doc, IGlobals globals, IMessageDisplay messageDisplay)
	{
		return this._material3dFortran.GetActiveMaterial(isAssembly: false);
	}

	public IMaterialArt GetMaterialAssembly(IDoc3d doc, IGlobals globals, IMessageDisplay messageDisplay)
	{
		return this._material3dFortran.GetActiveMaterial(isAssembly: true);
	}

	public void GetDeductionValue(IDoc3d doc, Dictionary<IBendDescriptor, BendTableReturnValues> bendTableResults, IGlobals globals, bool editWithTools)
	{
	}

	public IWaitCancel ShowWaitWithCancel(IGlobals globals)
	{
		return new WaitCancelDummy();
	}

	public void CloseWaitWithCancel(IWaitCancel waitCancel)
	{
	}

	public string OpenFolderDialog(IPnBndDoc doc, out bool? result, string initialPath)
	{
		result = null;
		return initialPath;
	}

	public bool CreatePostprocessorIfMaterialNotByUser()
	{
		return false;
	}

	public bool CreatePostprocessorIfNoKFactor()
	{
		return false;
	}

	public bool CreatePostprocessorIfErrorsSimValidation()
	{
		return false;
	}
}
