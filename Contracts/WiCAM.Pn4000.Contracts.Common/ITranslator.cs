using System;

namespace WiCAM.Pn4000.Contracts.Common;

public interface ITranslator
{
	public delegate bool DelegateTryFallbackTranslation(string key, out string result);

	IWeakEvent<ITranslator, IResourcesChangedArgs> ResourcesChangedWeak { get; }

	event Action<ITranslator, IResourcesChangedArgs> ResourcesChangedStrong;

	bool TryTranslate(string msgKey, out string result, params object[] parameters);

	string Translate(string msgKey, params object[] parameters);

	string TranslateFallback(string msgKey, string fallbackResult, params object[] parameters);

	void RefreshResources(bool localizationChanged, bool themeChanged);

	void SetFallbackTranslation(DelegateTryFallbackTranslation fallback);
}
