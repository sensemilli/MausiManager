using System;
using System.Windows;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.DependencyInjection.Implementations;

public class Translator : ITranslator
{
	private class ResourcesChangedArgs : EventArgs, IResourcesChangedArgs
	{
		public bool LocalizationChanged { get; }

		public bool ThemeChanged { get; }

		public ResourcesChangedArgs(bool localizationChanged, bool themeChanged)
		{
			LocalizationChanged = localizationChanged;
			ThemeChanged = themeChanged;
		}
	}

	private readonly Action<ITranslator, IResourcesChangedArgs> _resourcesChangedInvoke;

	private ITranslator.DelegateTryFallbackTranslation _fallbackTranslation;

	public IWeakEvent<ITranslator, IResourcesChangedArgs> ResourcesChangedWeak { get; }

	public event Action<ITranslator, IResourcesChangedArgs> ResourcesChangedStrong;

	public Translator(IWeakEventManager weakEventManager)
	{
		ResourcesChangedWeak = weakEventManager.CreateEvent(out _resourcesChangedInvoke);
	}

	public bool TryTranslate(string msgKey, out string result, params object[] parameters)
	{
		if (Application.Current.TryFindResource(msgKey) is string format)
		{
			result = string.Format(format, parameters);
			return true;
		}
		ITranslator.DelegateTryFallbackTranslation fallbackTranslation = _fallbackTranslation;
		if (fallbackTranslation != null && fallbackTranslation(msgKey, out var result2))
		{
			result = string.Format(result2, parameters);
			return true;
		}
		result = null;
		return false;
	}

	public string Translate(string msgKey, params object[] parameters)
	{
		if (!TryTranslate(msgKey, out var result, parameters))
		{
			return msgKey;
		}
		return result;
	}

	public string TranslateFallback(string msgKey, string fallbackResult, params object[] parameters)
	{
		if (!TryTranslate(msgKey, out var result, parameters))
		{
			return string.Format(fallbackResult, parameters);
		}
		return result;
	}

	public void SetFallbackTranslation(ITranslator.DelegateTryFallbackTranslation fallback)
	{
		_fallbackTranslation = fallback;
	}

	public void RefreshResources(bool localizationChanged, bool themeChanged)
	{
		ResourcesChangedArgs arg = new ResourcesChangedArgs(localizationChanged, themeChanged);
		_resourcesChangedInvoke(this, arg);
		this.ResourcesChangedStrong?.Invoke(this, arg);
	}
}
