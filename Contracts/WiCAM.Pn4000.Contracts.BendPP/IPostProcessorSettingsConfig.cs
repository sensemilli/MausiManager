namespace WiCAM.Pn4000.Contracts.BendPP;

public interface IPostProcessorSettingsConfig<T> where T : IPostProcessorSettings
{
	void SetSettings(T settings);

	T Convert();
}
