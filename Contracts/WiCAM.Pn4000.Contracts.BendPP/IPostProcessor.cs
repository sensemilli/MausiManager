using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.Contracts.BendPP;

public interface IPostProcessor
{
	string TranslationBaseKey { get; }

	bool SupportMultipleToolRoots { get; }

	bool SupportExcludedBends { get; }

	IPpData? CreateNC(IPnBndDocExt doc, IFactorio factorio, string targetFolder, List<ICombinedBendDescriptor> bends, string ppName, out List<IPPError> errors);

	bool WriteNC(string filename, IPpData data, IFactorio factorio);

	IPostProcessorSettings GetSettings(string machinePath);

	void SetAndSaveSettings(IPostProcessorSettings settings, string machinePath);

	IAxisTranslator GetAxisTranslator(ISimulationThread simulation, IBendMachine machine);

	IEnumerable<int> GetAllStepChangeModes();

	IEnumerable<string> GetOutputFiles(string tempFolder, IPnBndDocExt doc);

	void EmbedFileName(string fullName);
}
