namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolsAndBendsSerializerToolMapping
{
	IToolProfile? TryGetToolProfile(int? id);

	IMultiToolProfile? TryGetMultiToolProfile(int? id);
}
