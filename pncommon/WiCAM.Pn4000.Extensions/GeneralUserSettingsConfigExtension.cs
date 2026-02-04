using System;
using System.Windows;
using System.Windows.Controls;
using WiCAM.Pn4000.Config.DataStructures;

namespace WiCAM.Pn4000.Extensions;

public static class GeneralUserSettingsConfigExtension
{
	public static void SetWpfToolTipTimes(this GeneralUserSettingsConfig cfg, DependencyObject obj)
	{
		ToolTipService.SetInitialShowDelay(obj, cfg.ToolTipDelay);
		ToolTipService.SetShowDuration(obj, int.MaxValue);
		ToolTipService.SetBetweenShowDelay(obj, 1000);
		ToolTipService.SetHasDropShadow(obj, value: true);
	}

	public static void MainScreenPositionCorrections(this GeneralUserSettingsConfig cfg)
	{
		GeneralUserSettingsConfigExtension.SizeToFit(cfg);
		GeneralUserSettingsConfigExtension.MoveIntoView(cfg);
	}

	private static void SizeToFit(GeneralUserSettingsConfig cfgLocal)
	{
		if (cfgLocal.WindowHeight > SystemParameters.VirtualScreenHeight)
		{
			cfgLocal.WindowHeight = SystemParameters.VirtualScreenHeight;
		}
		if (cfgLocal.WindowWidth > SystemParameters.VirtualScreenWidth)
		{
			cfgLocal.WindowWidth = SystemParameters.VirtualScreenWidth;
		}
		if (Math.Abs(cfgLocal.WindowWidth) < 0.001)
		{
			cfgLocal.WindowWidth = 400.0;
		}
		if (Math.Abs(cfgLocal.WindowHeight) < 0.001)
		{
			cfgLocal.WindowHeight = 300.0;
		}
	}

	private static void MoveIntoView(GeneralUserSettingsConfig cfgLocal)
	{
		if (cfgLocal.WindowTop + cfgLocal.WindowHeight / 2.0 > SystemParameters.VirtualScreenHeight)
		{
			cfgLocal.WindowTop = SystemParameters.VirtualScreenHeight - cfgLocal.WindowHeight;
		}
		if (cfgLocal.WindowLeft + cfgLocal.WindowWidth / 2.0 > SystemParameters.VirtualScreenWidth)
		{
			cfgLocal.WindowLeft = SystemParameters.VirtualScreenWidth - cfgLocal.WindowWidth;
		}
		if (cfgLocal.WindowTop < 0.0)
		{
			cfgLocal.WindowTop = 0.0;
		}
		if (cfgLocal.WindowLeft < 0.0)
		{
			cfgLocal.WindowLeft = 0.0;
		}
	}
}
