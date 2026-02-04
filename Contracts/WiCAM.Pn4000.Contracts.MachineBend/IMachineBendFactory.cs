using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendPP;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IMachineBendFactory
{
	IBendMachine? CreateMachine(IPnBndDoc doc);

	IBendMachine? CreateMachine(string? machinePath, IMessageDisplay messageDisplay, IPnBndDoc doc, bool editorMode = false);

	IBendMachineSummary? CreateMachineSummary(string path);

	IEnumerable<IBendMachineSummary> GetMachineSummaries();

	void SaveMachine(IBendMachine bendMachine);

	IPostProcessor LoadPP(string path, IMessageDisplay messageDisplay);
}
