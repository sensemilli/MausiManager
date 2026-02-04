using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Extensions;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.uicontrols.Tooltips;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public class PnToolTipService : IPnToolTipService
{
	private SolidColorBrush _blackBr = new SolidColorBrush(Colors.Black);

	private ILanguageDictionary _languageDictionary;

	private IConfigProvider _configProvider;

	private Style _pnFunctionToolTipStyle;

	private Style _pnFunctionToolTipStyleWithPreview;

	private IPnIconsService _pnIconsService;

	public PnToolTipService(ILanguageDictionary languageDictionary, IConfigProvider configProvider, IPnIconsService pnIconsService)
	{
		this._languageDictionary = languageDictionary;
		this._configProvider = configProvider;
		this._pnIconsService = pnIconsService;
	}

	public void SetTooltip(FrameworkElement element, IPnCommand pnCommand)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (!generalUserSettingsConfig.HideToolTips)
		{
			if (this._pnFunctionToolTipStyle == null)
			{
				this._pnFunctionToolTipStyle = (Style)element.TryFindResource("PnFunctionTooltipStyle");
			}
			PnFunctionToolTip pnFunctionToolTip = new PnFunctionToolTip();
			pnFunctionToolTip.Style = this._pnFunctionToolTipStyle;
			pnFunctionToolTip.LargeImage = this._pnIconsService.GetBigIcon(pnCommand.Command);
			pnFunctionToolTip.Label1 = this._languageDictionary.GetButtonLabel(pnCommand.LabelId, pnCommand.DefaultLabel);
			if (pnCommand.Group != 100)
			{
				pnFunctionToolTip.Label2 = $"[{pnCommand.Group}] {pnCommand.Command}";
			}
			else
			{
				pnFunctionToolTip.Label2 = string.Empty;
			}
			pnFunctionToolTip.Description = this._languageDictionary.GetButtonToolTip(pnCommand.ToolTipId, pnCommand.LabelId, pnCommand.DefaultLabel);
			element.ToolTip = pnFunctionToolTip;
			pnFunctionToolTip.Foreground = this._blackBr;
			generalUserSettingsConfig.SetWpfToolTipTimes(element);
		}
	}

	public ToolTip GetToolTip(IPnCommand pnCommand)
	{
		if (this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>().HideToolTips)
		{
			return null;
		}
		if (this._pnFunctionToolTipStyle == null)
		{
			this._pnFunctionToolTipStyle = (Style)Application.Current.TryFindResource("PnFunctionTooltipStyle");
		}
		PnFunctionToolTip pnFunctionToolTip = new PnFunctionToolTip();
		pnFunctionToolTip.Style = this._pnFunctionToolTipStyle;
		pnFunctionToolTip.LargeImage = this._pnIconsService.GetBigIcon(pnCommand.Command);
		pnFunctionToolTip.Label1 = this._languageDictionary.GetButtonLabel(pnCommand.LabelId, pnCommand.DefaultLabel);
		if (pnCommand.Group != 100)
		{
			pnFunctionToolTip.Label2 = $"[{pnCommand.Group}] {pnCommand.Command}";
		}
		else
		{
			pnFunctionToolTip.Label2 = string.Empty;
		}
		pnFunctionToolTip.Description = this._languageDictionary.GetButtonToolTip(pnCommand.ToolTipId, pnCommand.LabelId, pnCommand.DefaultLabel);
		pnFunctionToolTip.Foreground = this._blackBr;
		return pnFunctionToolTip;
	}

	public void SetTooltip(FrameworkElement element, string l1, string l2, string dectription, ImageSource img)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (!generalUserSettingsConfig.HideToolTips)
		{
			if (this._pnFunctionToolTipStyle == null)
			{
				this._pnFunctionToolTipStyle = (Style)element.TryFindResource("PnFunctionTooltipStyle");
			}
			PnFunctionToolTip pnFunctionToolTip = new PnFunctionToolTip();
			pnFunctionToolTip.Style = this._pnFunctionToolTipStyle;
			pnFunctionToolTip.LargeImage = img;
			pnFunctionToolTip.Label1 = l1;
			pnFunctionToolTip.Label2 = l2;
			pnFunctionToolTip.Description = dectription;
			element.ToolTip = pnFunctionToolTip;
			generalUserSettingsConfig.SetWpfToolTipTimes(element);
		}
	}

	public void UpdateTooltipWithPreview(FrameworkElement element, ImageSource preview)
	{
		if (this._pnFunctionToolTipStyleWithPreview == null)
		{
			this._pnFunctionToolTipStyleWithPreview = (Style)element.TryFindResource("PnFunctionToolTipStyleWithPreview");
		}
		if (element.ToolTip is PnFunctionToolTip pnFunctionToolTip)
		{
			pnFunctionToolTip.Style = this._pnFunctionToolTipStyleWithPreview;
			pnFunctionToolTip.PreviewImage = preview;
		}
	}
}
