using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.pn4.pn4FlowCenter;

namespace WiCAM.Pn4000;

public class StyleManager
{
	private readonly ITranslator _translator;

	private readonly ICultureService _cultureService;

	private List<Uri> _styles = new List<Uri>();

	private Uri _skinDictionaryUri;

	private byte _popupButtonMode;

	private IPKernelFlowGlobalDataService _pKernelFlowGlobalData;

	private IPnPathService _pnPathService;

	public Action ExtraThemaUpdate { get; set; }

	public StyleManager(IPKernelFlowGlobalDataService pKernelFlow, IPnPathService pathService, ITranslator translator, ICultureService cultureService)
	{
		this.AddStyleFromPn4UiControlsDll("TransparentCircleSectionButtonStyle.xaml");
		this.AddStyleFromPn4UiControlsDll("pn4RibbonStyles.xaml");
		this.AddStyleFromPn4UiControlsDll("pn4Toolbars.xaml");
		this.AddStyleFromPn4UiControlsDll("pn4PropertyPanel.xaml");
		this._pKernelFlowGlobalData = pKernelFlow;
		this._pnPathService = pathService;
		this._translator = translator;
		this._cultureService = cultureService;
	}

	private Uri GetUriFromThemasDll(string name)
	{
		return new Uri($"/WiCAM.Pn4000.Themes;Component/{name}.xaml", UriKind.Relative);
	}

	private Uri GetStyleUri(string name)
	{
		return new Uri($"/WiCAM.Pn4000.Themes;Component/Skins/{name}.xaml", UriKind.Relative);
	}

	private Uri GetPopupStyleUri(string name)
	{
		return new Uri($"/WiCAM.Pn4000.Popup;Component/Resources/{name}.xaml", UriKind.Relative);
	}

	private Uri GetStyleUriFromThatDll(string name)
	{
		return new Uri($"/pn4.uicontrols;Component/Styles/{name}", UriKind.Relative);
	}

	private Uri GetStdStyleUriFromExe(string name)
	{
		return new Uri($"/pn4UILib/Styles/{name}", UriKind.Relative);
	}

	public void ApplyPopupStyleAndMerge(byte popupButtonMode)
	{
		throw new NotImplementedException();
	}

	public void AddStyle(Uri address)
	{
		this._styles.Add(address);
	}

	public void AddStyle(string name)
	{
		this.AddStyle(this.GetStdStyleUriFromExe(name));
	}

	private void AddStyleFromPn4UiControlsDll(string name)
	{
		this.AddStyle(this.GetStyleUriFromThatDll(name));
	}

	public void ApplySkinAndMerge(string name, byte popupButtonMode)
	{
		this.ApplySkinAndMerge(this.GetStyleUri(name), popupButtonMode);
	}

	public void Update()
	{
		this.ApplySkinAndMerge(this._skinDictionaryUri, this._popupButtonMode);
	}

	private void ApplySkinAndMerge(Uri skinDictionaryUri, byte popupButtonMode)
	{
		this._skinDictionaryUri = skinDictionaryUri;
		this._popupButtonMode = popupButtonMode;
		Collection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
		mergedDictionaries.Clear();
		mergedDictionaries.Add(Application.LoadComponent(skinDictionaryUri) as ResourceDictionary);
		foreach (Uri style in this._styles)
		{
			mergedDictionaries.Add(Application.LoadComponent(style) as ResourceDictionary);
		}
		mergedDictionaries.Add(Application.LoadComponent(this.GetPopupStyleUri("PopupCommon")) as ResourceDictionary);
		mergedDictionaries.Add(Application.LoadComponent(this.GetUriFromThemasDll("Default")) as ResourceDictionary);
		switch (popupButtonMode)
		{
		case 0:
			mergedDictionaries.Add(Application.LoadComponent(this.GetPopupStyleUri("PopupRightStyle")) as ResourceDictionary);
			break;
		case 1:
			mergedDictionaries.Add(Application.LoadComponent(this.GetPopupStyleUri("PopupLeftStyle")) as ResourceDictionary);
			break;
		case 2:
			mergedDictionaries.Add(Application.LoadComponent(this.GetPopupStyleUri("PopupTopStyle")) as ResourceDictionary);
			break;
		case 3:
			mergedDictionaries.Add(Application.LoadComponent(this.GetPopupStyleUri("PopupBottomStyle")) as ResourceDictionary);
			break;
		}
		this.MergeLanguageDictionatiesWithSequenceLogic(mergedDictionaries);
		this.ExtraThemaUpdate?.Invoke();
		this._translator.RefreshResources(localizationChanged: true, themeChanged: true);
		this._cultureService.UpdateCulture();
	}

	private void MergeLanguageDictionatiesWithSequenceLogic(Collection<ResourceDictionary> mergedDicts)
	{
		mergedDicts.Add(Application.LoadComponent(new Uri("/WiCAM.Pn4000.DependencyInjection;Component/pn4_translation.xaml", UriKind.Relative)) as ResourceDictionary);
		if (this._pKernelFlowGlobalData != null && this._pKernelFlowGlobalData.PnLanguage != -1)
		{
			string arg = (string.IsNullOrEmpty(this._pnPathService.PNMASTER) ? this._pnPathService.PNDRIVE : this._pnPathService.PNMASTER);
			if (this._pKernelFlowGlobalData.PnLanguage > 2)
			{
				this.CheckAndMerge(mergedDicts, $"{arg}\\u\\pn\\lfiles\\{2:00}\\pn4_translation.xaml");
			}
			this.CheckAndMerge(mergedDicts, $"{arg}\\u\\pn\\lfiles\\{this._pKernelFlowGlobalData.PnLanguage:00}\\pn4_translation.xaml");
			this.CheckAndMerge(mergedDicts, $"{arg}\\u\\pn\\lfiles\\00\\pn4_translation.xaml");
			if (this._pKernelFlowGlobalData.PnMachine != -1)
			{
				this.CheckAndMerge(mergedDicts, $"{arg}\\u\\pn\\machine\\machine_{this._pKernelFlowGlobalData.PnMachine:0000}\\lfiles\\{this._pKernelFlowGlobalData.PnLanguage:00}\\pn4_translation.xaml");
				this.CheckAndMerge(mergedDicts, $"{arg}\\u\\pn\\machine\\machine_{this._pKernelFlowGlobalData.PnMachine:0000}\\lfiles\\00\\pn4_translation.xaml");
				this._cultureService.UpdateCulture();
			}
		}
	}

	private void CheckAndMerge(Collection<ResourceDictionary> mergedDicts, string path)
	{
		ResourceDictionary resourceDictionary = StyleManager.ReadXamlFile(path);
		if (resourceDictionary != null)
		{
			mergedDicts.Add(resourceDictionary);
		}
	}

	private static ResourceDictionary ReadXamlFile(string path)
	{
		if (!File.Exists(path))
		{
			return null;
		}
		try
		{
			return new ResourceDictionary
			{
				Source = new Uri(path)
			};
		}
		catch
		{
			return null;
		}
	}
}
