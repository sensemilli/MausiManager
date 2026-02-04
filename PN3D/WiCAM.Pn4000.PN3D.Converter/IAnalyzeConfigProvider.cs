using WiCAM.Pn4000.BendModel.Config;
using WiCAM.Pn4000.Config.DataStructures;

namespace WiCAM.Pn4000.PN3D.Converter;

public interface IAnalyzeConfigProvider
{
	global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig GetAnalyzeConfig();

	global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig ConvertAnalyzeConfig(global::WiCAM.Pn4000.Config.DataStructures.AnalyzeConfig config);
}
