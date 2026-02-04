namespace WiCAM.Pn4000.Contracts.Common;

public interface IResourcesChangedArgs
{
	bool LocalizationChanged { get; }

	bool ThemeChanged { get; }
}
