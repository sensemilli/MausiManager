using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Services;

internal class StyleProvider : IStyleProvider
{
	private readonly IConfigProvider _configProvider;

	private double _billboardSizeMultiplier;

	private readonly TextStyle _defaultBillboardTextStyle = new TextStyle
	{
		FontSize = 100f,
		Color = Color.Black
	};

	private readonly GlyphStyle _defaultBillboardGlyphStyle = new GlyphStyle
	{
		Size = 100,
		Color = Color.Black,
		Rotation = 0.0
	};

	private readonly BackgroundStyle _defaultBillboardBackground;

	private List<WeakReference<IStyleReceiver>> _listeningBillboardStyles;

	public Color AccentBackgroundColor { get; } = FromRgba(16, 110, 190, 255);

	public Color AccentBorderColor { get; } = FromRgba(16, 110, 190, 255);

	public Color AccentForegroundColor { get; } = FromRgba(255, 255, 255, 255);

	public Color AccentMouseOverBackgroundColor { get; } = FromRgba(212, 232, 248, 255);

	public Color AccentMouseOverBorderColor { get; } = FromRgba(47, 150, 237, 255);

	public Color AccentPressedBackgroundColor { get; } = FromRgba(161, 205, 237, 255);

	public Color BaseBackgroundColor { get; } = FromRgba(223, 223, 223, 255);

	public Color ButtonBackgroundColor { get; } = FromRgba(229, 229, 229, 255);

	public Color DisabledBackgroundColor { get; } = FromRgba(241, 241, 241, 255);

	public Color DisabledBorderColor { get; } = FromRgba(214, 214, 214, 255);

	public Color DisabledForegroundColor { get; } = FromRgba(214, 214, 214, 255);

	public Color DisabledIconColor { get; } = FromRgba(96, 96, 96, 255);

	public Color IconColor { get; } = FromRgba(96, 96, 96, 255);

	public Color MainBackgroundColor { get; } = FromRgba(255, 255, 255, 255);

	public Color MainBorderColor { get; } = FromRgba(172, 172, 172, 255);

	public Color MainForegroundColor { get; } = FromRgba(0, 0, 0, 255);

	public Color MouseOverBackgroundColor { get; } = FromRgba(200, 199, 198, 255);

	public Color MouseOverBorderColor { get; } = FromRgba(200, 199, 198, 255);

	public Color PressedBackgroundColor { get; } = FromRgba(179, 175, 173, 255);

	public Color ReadOnlyBackgroundColor { get; } = FromRgba(255, 255, 255, 255);

	public Color ReadOnlyBorderColor { get; } = FromRgba(172, 172, 172, 255);

	public Color SecondaryBackgroundColor { get; } = FromRgba(214, 213, 213, 255);

	public Color SecondaryForegroundColor { get; } = FromRgba(0, 0, 0, 255);

	public Color ValidationColor { get; } = FromRgba(228, 62, 0, 255);

	public double BillboardSizeMultiplier
	{
		get
		{
			return _billboardSizeMultiplier;
		}
		set
		{
			_billboardSizeMultiplier = value;
			GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
			generalUserSettingsConfig.BendOrderBillboardSize = value;
			_configProvider.Push(generalUserSettingsConfig);
			_configProvider.Save<GeneralUserSettingsConfig>();
			ImmutableArray<WeakReference<IStyleReceiver>>.Enumerator enumerator = _listeningBillboardStyles.ToImmutableArray().GetEnumerator();
			while (enumerator.MoveNext())
			{
				WeakReference<IStyleReceiver> current = enumerator.Current;
				if (current.TryGetTarget(out var target))
				{
					target.BillboardStylesChanged();
				}
				else
				{
					_listeningBillboardStyles.Remove(current);
				}
			}
		}
	}

	public BackgroundStyle BillboardBackgroundStyle
	{
		get
		{
			BackgroundStyle defaultBillboardBackground = _defaultBillboardBackground;
			defaultBillboardBackground.Padding = _defaultBillboardBackground.Padding * (float)BillboardSizeMultiplier;
			defaultBillboardBackground.BorderThickness = (int)Math.Round((double)_defaultBillboardBackground.BorderThickness * BillboardSizeMultiplier);
			return defaultBillboardBackground;
		}
	}

	public TextStyle BillboardTextStyle
	{
		get
		{
			TextStyle defaultBillboardTextStyle = _defaultBillboardTextStyle;
			defaultBillboardTextStyle.FontSize = (float)((double)_defaultBillboardTextStyle.FontSize * BillboardSizeMultiplier);
			return defaultBillboardTextStyle;
		}
	}

	public GlyphStyle BillboardGlyphStyle
	{
		get
		{
			GlyphStyle defaultBillboardGlyphStyle = _defaultBillboardGlyphStyle;
			defaultBillboardGlyphStyle.Size = (int)((double)_defaultBillboardGlyphStyle.Size * BillboardSizeMultiplier);
			return defaultBillboardGlyphStyle;
		}
	}

	public StyleProvider(IConfigProvider configProvider)
	{
		BackgroundStyle defaultBillboardBackground = new BackgroundStyle
		{
			Padding = 10f,
			BorderThickness = 5f
		};
		Color white = Color.White;
		white.A = 0.7f;
		defaultBillboardBackground.Color = white;
		defaultBillboardBackground.BorderColor = Color.Black;
		defaultBillboardBackground.MinWidth = 10f;
		defaultBillboardBackground.MinHeight = 10f;
		_defaultBillboardBackground = defaultBillboardBackground;
		_listeningBillboardStyles = new List<WeakReference<IStyleReceiver>>();
		//base._002Ector();
		_configProvider = configProvider;
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		_billboardSizeMultiplier = generalUserSettingsConfig.BendOrderBillboardSize;
	}

	private static Color FromRgba(int r, int g, int b, int a)
	{
		return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
	}

	public void RegisterBillboardStilesChanged(IStyleReceiver styleReceiver)
	{
		_listeningBillboardStyles.Add(new WeakReference<IStyleReceiver>(styleReceiver));
	}
}
